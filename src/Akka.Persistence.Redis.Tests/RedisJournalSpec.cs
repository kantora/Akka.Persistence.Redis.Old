﻿namespace Akka.Persistence.Redis.Tests
{
    using System.Linq;

    using Akka.Configuration;
    using Akka.Persistence.Redis.Journal;
    using Akka.Persistence.TestKit.Journal;

    using StackExchange.Redis;

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

        /// <summary>
        /// Clears all temporary test data
        /// </summary>
        /// <param name="disposing">Whether method is called by <see cref="Dispose"/></param>
        protected override void Dispose(bool disposing)
        {
            var redisConnection = ConnectionMultiplexer.Connect(this.Sys.Settings.Config.GetString("akka.persistence.journal.redis.connection-string"));
            var server = redisConnection.GetServer(redisConnection.GetEndPoints().First());
            var db = redisConnection.GetDatabase();
            foreach (var key in server.Keys(pattern: (string)RedisJournal.GetJournalKey("*")))
            {
                db.KeyDelete(key);
            }

            foreach (var key in server.Keys(pattern: (string)RedisJournal.GetJournalSkippedKey("*")))
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