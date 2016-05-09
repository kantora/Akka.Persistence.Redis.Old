namespace Akka.Persistence.Redis.Tests
{
    using Akka.Configuration;
    using Akka.Persistence.TestKit.Journal;

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