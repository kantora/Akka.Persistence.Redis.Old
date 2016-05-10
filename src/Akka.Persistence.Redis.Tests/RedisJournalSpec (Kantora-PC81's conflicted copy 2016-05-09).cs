namespace Akka.Persistence.Redis.Tests
{
    using Akka.Actor;
    using Akka.Configuration;
    using Akka.Persistence.Redis.Journal;
    using Akka.Persistence.TestKit.Journal;

    using StackExchange.Redis;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Tests for redis persistence plugin - event journal
    /// </summary>
    public class RedisJournalSpec : JournalSpec
    {
        /// <summary>
        /// Initializes new instance of <see cref="RedisJournalSpec"/>
        /// </summary>
        /// <param name="output">Default xunit output</param>
        public RedisJournalSpec(ITestOutputHelper output)
            : base(CreateSpecConfig("192.168.99.100:6379"), "RedisJournalSpec", output)
        {
            this.Initialize();
        }

        protected override void Dispose(bool disposing)
        {
            var redisConnection = ConnectionMultiplexer.Connect(this.Sys.Settings.Config.GetString("akka.persistence.journal.redis.connection-string"));
            redisConnection.GetServer("192.168.99.100", 6379).FlushDatabase();

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
                    journal {
                        plugin = ""akka.persistence.journal.redis""
                        redis {
                            class = ""Akka.Persistence.Redis.Journal.RedisJournal, Akka.Persistence.Redis""
                            connection-string = """ + connectionString + @"""
                            plugin-dispatcher = ""akka.actor.default-dispatcher""
                        }
                    }
                }");
        }
    }
}