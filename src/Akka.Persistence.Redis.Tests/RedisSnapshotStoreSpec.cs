namespace Akka.Persistence.Redis.Tests
{
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
            : base(CreateSpecConfig("192.168.99.100:6379"), "RedisJournalSpec", output)
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
            var server = redisConnection.GetServer(redisConnection.GetEndPoints().First());
            var db = redisConnection.GetDatabase();
            foreach (var key in server.Keys(pattern: (string)RedisSnapshotStore.GetSnapshotKey("*")))
            {
                db.KeyDelete(key);
            }

            foreach (var key in server.Keys(pattern: (string)RedisSnapshotStore.GetSnapshotDateKey("*")))
            {
                db.KeyDelete(key);
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Creates the default test system config
        /// </summary>
        /// <param name="connectionString">Redis connection string</param>
        /// <returns>The akka system config</returns>
        private static Config CreateSpecConfig(string connectionString)
        {
            return ConfigurationFactory.ParseString(@"
                akka.persistence {
                    publish-plugin-commands = on
                    snapshot-store {
                        plugin = ""akka.persistence.snapshot-store.redis""
                        redis {
                            class = ""Akka.Persistence.Redis.Snapshot.RedisSnapshotStore, Akka.Persistence.Redis""
                            connection-string = """ + connectionString + @"""
                            plugin-dispatcher = ""akka.actor.default-dispatcher""
                        }
                    }
                }");
        }
    }
}