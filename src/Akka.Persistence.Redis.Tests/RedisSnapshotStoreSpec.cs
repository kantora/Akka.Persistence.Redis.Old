namespace Akka.Persistence.Redis.Tests
{
    using System.Configuration;
    using System.Linq;

    using Akka.Configuration;
    using Akka.Persistence.Redis.Snapshot;
    using Akka.Persistence.TestKit.Snapshot;

    using StackExchange.Redis;

    using Xunit.Abstractions;

    /// <summary>
    /// Tests for redis persistence plugin - snapshot storage
    /// </summary>
    public class RedisSnapshotStoreSpec : SnapshotStoreSpec
    {
        /// <summary>
        /// Initializes new instance of <see cref="RedisJournalSpec"/>
        /// </summary>
        /// <param name="output">Default xunit output</param>
        public RedisSnapshotStoreSpec(ITestOutputHelper output)
            : base(CreateSpecConfig(), "RedisJournalSpec", output)
        {
            this.Initialize();
        }

        /// <summary>
        /// Clears all temporary test data
        /// </summary>
        /// <param name="disposing">Whether method is called by <see cref="Dispose"/></param>
        protected override void Dispose(bool disposing)
        {
            var redisConnection = ConnectionMultiplexer.Connect(this.Sys.Settings.Config.GetString("akka.persistence.snapshot-store.redis.connection-string"));
            var keyPrefix = this.Sys.Settings.Config.GetString("akka.persistence.snapshot-store.redis.key-prefix");
            var database = this.Sys.Settings.Config.GetInt("akka.persistence.snapshot-store.redis.database");

            var server = redisConnection.GetServer(redisConnection.GetEndPoints().First());
            var db = redisConnection.GetDatabase(database);
            foreach (var key in server.Keys(database: database, pattern: (string)RedisSnapshotStore.GetSnapshotKey(keyPrefix, "*")))
            {
                db.KeyDelete(key);
            }

            foreach (var key in server.Keys(database: database, pattern: (string)RedisSnapshotStore.GetSnapshotMetadataKey(keyPrefix, "*")))
            {
                db.KeyDelete(key);
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Creates the default test system config
        /// </summary>
        /// <returns>The akka system config</returns>
        private static Config CreateSpecConfig()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["redis"].ConnectionString;
            var database = ConfigurationManager.AppSettings["redisDatabase"];

            return ConfigurationFactory.ParseString(@"
                akka.persistence {
                    publish-plugin-commands = on
                    snapshot-store {
                        plugin = ""akka.persistence.snapshot-store.redis""
                        redis {
                            class = ""Akka.Persistence.Redis.Snapshot.RedisSnapshotStore, Akka.Persistence.Redis""
                            connection-string = """ + connectionString + @"""
                            plugin-dispatcher = ""akka.actor.default-dispatcher""
                            ttl = 1h
                            database = """ + database + @"""
                            key-prefix = ""akka:presistance:snapshots""
                        }
                    }
                }");
        }
    }
}