## Akka.Persistence.Redis

Akka Persistence journal and snapshot store backed by Redis database.
Based on https://github.com/StackExchange/StackExchange.Redis library.

### Configuration

Both journal and snapshot store share the same configuration keys (however they resides in separate scopes, so they are defined distinctly for either journal or snapshot store):

Remember that connection string must be provided separately to Journal and Snapshot Store.

```hocon
akka.persistence{

    journal {
        redis {

            # qualified type name of the Redis persistence journal actor
            class = "Akka.Persistence.Redis.Journal.RedisJournal, Akka.Persistence.Redis"

            #connection string, as described here: https://github.com/StackExchange/StackExchange.Redis/blob/master/Docs/Configuration.md#basic-configuration-strings
            connection-string = ""

            # dispatcher used to drive journal actor
            plugin-dispatcher = ""akka.actor.default-dispatcher""

            # You can use this directive so set TTL for journals. 
            # This is set for the whole journal (the whole journal for the particular persistent object, not records itself). 
            # It is renewed every time journal is accessed or modified.
            # In case it is unspecified, TTL will be set to infinity (so records will live forever)

            ttl = 1h

            # The Redis database number. Leave it unspecified for default value or clustered redis intstances            
            #database = 0

            #Redis journals key prefixes. Leave it for default or change it to appropriate value. WARNING: don't change it on production instances.
            key-prefix = ""akka:presistance:journal""
        }
    }    


    snapshot-store {
        redis {
            # qualified type name of the Redis persistence snapshot storage actor
            class = "Akka.Persistence.Redis.Snapshot.RedisSnapshotStore, Akka.Persistence.Redis"

            #connection string, as described here: https://github.com/StackExchange/StackExchange.Redis/blob/master/Docs/Configuration.md#basic-configuration-strings
            connection-string = ""

            # dispatcher used to drive snapshot storage actor
            plugin-dispatcher = ""akka.actor.default-dispatcher""

            # You can use this directive so set TTL for snapshot buckets. 
            # This is set for the whole bucket (whole bucket for the particular persistent object, not snapshot itself). 
            # It is renewed every time snapshot bucket is accessed or modified.
            # In case it is unspecified, TTL will be set to infinity (so snapshots will live forever)
            
            ttl = 30d

            # The Redis database number. Leave it unspecified for default value or clustered redis instances
            
            #database = 0

            #Redis storage key prefixes. Leave it for default or change it to appropriate value. WARNING: don't change it on production instances.
            #key-prefix = ""akka:presistance:snapshots""
        }
    }


}
```

# Storage description

Journal actor creates 
* `akka:presistance:journal:lists:[persistenceId]` list of serialized events 
* `akka:presistance:journal:skippedKeys:[persistenceId]` just int value - count of previosly deleted events

`akka:presistance:journal` can be overwritten via config

It uses `Wire` for serialization purposes.



Snapshot storage actor creates two hashes for each persitened entity:
* `akka:presistance:snapshots:data:[persistenceId]`, with sequence number as key and serialized snapshot as value
* `akka:presistance:snapshots:metadata:[persistenceId]`, with sequence number as key and snapshot metadata as value

`akka:presistance:snapshots` can be overwritten via config

It uses `Wire` for serialization purposes.
