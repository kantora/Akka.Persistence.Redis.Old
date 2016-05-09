using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.Persistence.Redis.Tests
{
    using Akka.Configuration;
    using Akka.Persistence.TestKit.Snapshot;
    using Akka.Util.Internal;

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
            : base(CreateSpecConfig("redis"), "RedisJournalSpec", output)
        {
            this.Initialize();
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