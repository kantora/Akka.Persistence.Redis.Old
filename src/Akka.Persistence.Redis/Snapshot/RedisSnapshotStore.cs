using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.Persistence.Redis.Snapshot
{
    using Akka.Persistence.Snapshot;

    /// <summary>
    /// Stores snapshots in redis
    /// </summary>
    public class RedisSnapshotStore : SnapshotStore
    {
        /// <summary>
        /// Deletes the snapshot identified by <paramref name="metadata" />.
        /// This call is protected with a circuit-breaker
        /// </summary>
        protected override Task DeleteAsync(SnapshotMetadata metadata)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes all snapshots matching provided <paramref name="criteria" />.
        /// This call is protected with a circuit-breaker
        /// </summary>
        protected override Task DeleteAsync(string persistenceId, SnapshotSelectionCriteria criteria)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously loads a snapshot.
        /// This call is protected with a circuit-breaker
        /// </summary>
        protected override Task<SelectedSnapshot> LoadAsync(string persistenceId, SnapshotSelectionCriteria criteria)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously saves a snapshot.
        /// This call is protected with a circuit-breaker
        /// </summary>
        protected override Task SaveAsync(SnapshotMetadata metadata, object snapshot)
        {
            throw new NotImplementedException();
        }
    }
}