namespace Akka.Persistence.Redis.Snapshot
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Akka.Persistence.Snapshot;

    using JetBrains.Annotations;

    using StackExchange.Redis;

    /// <summary>
    /// Stores snapshots in redis
    /// </summary>
    [UsedImplicitly]
    public class RedisSnapshotStore : SnapshotStore
    {
        /// <summary>
        /// Actual connection to redis database
        /// </summary>
        private IConnectionMultiplexer redisConnection;

        /// <summary>
        /// Creates key for redis snapshot datetime
        /// </summary>
        /// <param name="persistenceId">Akka actor persistence identification</param>
        /// <returns>The redis key</returns>
        public static RedisKey GetSnapshotDateKey(string persistenceId)
        {
            return $"akka:presistance:snapshots:dates:{persistenceId}";
        }

        /// <summary>
        /// Creates key for redis snapshot data
        /// </summary>
        /// <param name="persistenceId">Akka actor persistence identification</param>
        /// <returns>The redis key</returns>
        public static RedisKey GetSnapshotKey(string persistenceId)
        {
            return $"akka:presistance:snapshots:data:{persistenceId}";
        }

        /// <summary>
        /// Deletes the snapshot identified by <paramref name="metadata" />.
        /// This call is protected with a circuit-breaker
        /// </summary>
        protected override async Task DeleteAsync(SnapshotMetadata metadata)
        {
            var db = this.redisConnection.GetDatabase();
            var transaction = db.CreateTransaction();
#pragma warning disable 4014
            transaction.HashDeleteAsync(GetSnapshotDateKey(metadata.PersistenceId), metadata.SequenceNr);
            transaction.HashDeleteAsync(GetSnapshotKey(metadata.PersistenceId), metadata.SequenceNr);
#pragma warning restore 4014
            await transaction.ExecuteAsync();
        }

        /// <summary>
        /// Deletes all snapshots matching provided <paramref name="criteria" />.
        /// This call is protected with a circuit-breaker
        /// </summary>
        protected override async Task DeleteAsync(string persistenceId, SnapshotSelectionCriteria criteria)
        {
            var storedSnapshots = await this.GetStoredSnapshotsMetadata(persistenceId);
            var metadata =
                storedSnapshots.Where(
                    m => m.SequenceNr <= criteria.MaxSequenceNr && m.Timestamp <= criteria.MaxTimeStamp);

            var db = this.redisConnection.GetDatabase();
            var transaction = db.CreateTransaction();
            foreach (var snapshotMetadata in metadata)
            {
#pragma warning disable 4014
                transaction.HashDeleteAsync(GetSnapshotDateKey(persistenceId), snapshotMetadata.SequenceNr);
                transaction.HashDeleteAsync(GetSnapshotKey(persistenceId), snapshotMetadata.SequenceNr);
#pragma warning restore 4014
            }

            await transaction.ExecuteAsync();
        }

        /// <summary>
        /// Asynchronously loads a snapshot.
        /// This call is protected with a circuit-breaker
        /// </summary>
        protected override async Task<SelectedSnapshot> LoadAsync(string persistenceId, SnapshotSelectionCriteria criteria)
        {
            var storedSnapshots = await this.GetStoredSnapshotsMetadata(persistenceId);
            var metadata =
                storedSnapshots.Where(
                    m => m.SequenceNr <= criteria.MaxSequenceNr && m.Timestamp <= criteria.MaxTimeStamp)
                    .OrderByDescending(m => m.SequenceNr)
                    .FirstOrDefault();

            if (metadata == null)
            {
                return null;
            }

            var db = this.redisConnection.GetDatabase();
            var serializer = new Wire.Serializer();
            var snapshotData = await db.HashGetAsync(GetSnapshotKey(persistenceId), metadata.SequenceNr);

            if (!snapshotData.HasValue)
            {
                return null;
            }

            using (var stream = new MemoryStream())
            {
                var bytes = (byte[])snapshotData;
                stream.Write(bytes, 0, bytes.Length);
                stream.Position = 0;
                var snapshot = serializer.Deserialize(stream);
                return new SelectedSnapshot(metadata, snapshot);
            }
        }

        /// <summary>
        /// Reads and deserializes stored snapshots metadata
        /// </summary>
        /// <param name="persistenceId">Akka actor persistence identification</param>
        /// <returns>The list of stored snapshots metadata </returns>
        private async Task<List<SnapshotMetadata>> GetStoredSnapshotsMetadata(string persistenceId)
        {
            var db = this.redisConnection.GetDatabase();
            var serializer = new Wire.Serializer();
            var storedSnapshots = (await db.HashGetAllAsync(GetSnapshotDateKey(persistenceId))).Select(
                data =>
                    {
                        using (var stream = new MemoryStream())
                        {
                            var bytes = (byte[])data.Value;
                            stream.Write(bytes, 0, bytes.Length);
                            stream.Position = 0;
                            return serializer.Deserialize<SnapshotMetadata>(stream);
                        }
                    }).ToList();
            return storedSnapshots;
        }

        /// <summary>
        ///     User overridable callback.
        ///     <p />
        ///     Is called when an Actor is started.
        ///     Actors are automatically started asynchronously when created.
        ///     Empty default implementation.
        /// </summary>
        protected override void PreStart()
        {
            base.PreStart();
            this.redisConnection = ConnectionMultiplexer.Connect(Context.System.Settings.Config.GetString("akka.persistence.snapshot-store.redis.connection-string"));
        }

        /// <summary>
        /// Asynchronously saves a snapshot.
        /// This call is protected with a circuit-breaker
        /// </summary>
        protected override async Task SaveAsync(SnapshotMetadata metadata, object snapshot)
        {
            var db = this.redisConnection.GetDatabase();
            var transaction = db.CreateTransaction();
            var serializer = new Wire.Serializer();
#pragma warning disable 4014
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(snapshot, stream);
                transaction.HashSetAsync(GetSnapshotKey(metadata.PersistenceId), metadata.SequenceNr, stream.ToArray());
            }

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(metadata, stream);
                transaction.HashSetAsync(GetSnapshotDateKey(metadata.PersistenceId), metadata.SequenceNr, stream.ToArray());
            }

#pragma warning restore 4014

            var result = await transaction.ExecuteAsync();
            if (!result)
            {
                throw new Exception("Error while saving snapshot to redis");
            }
        }
    }
}