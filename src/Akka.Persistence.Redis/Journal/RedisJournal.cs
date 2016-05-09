using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.Persistence.Redis.Journal
{
    using System.Collections.Immutable;

    using Akka.Actor;
    using Akka.Persistence.Journal;

    /// <summary>
    /// The Redis write journal
    /// </summary>
    public class RedisJournal : AsyncWriteJournal
    {
        /// <summary>
        /// Asynchronously reads the highest stored sequence number for provided <paramref name="persistenceId" />.
        /// The persistent actor will use the highest sequence number after recovery as the starting point when
        /// persisting new events.
        /// This sequence number is also used as `toSequenceNr` in subsequent calls to
        /// <see cref="M:Akka.Persistence.Journal.IAsyncRecovery.ReplayMessagesAsync(Akka.Actor.IActorContext,System.String,System.Int64,System.Int64,System.Int64,System.Action{Akka.Persistence.IPersistentRepresentation})" /> unless the user has specified a lower `toSequenceNr`.
        /// Journal must maintain the highest sequence number and never decrease it.
        /// This call is protected with a circuit-breaker.
        /// Please also not that requests for the highest sequence number may be made concurrently
        /// to writes executing for the same <paramref name="persistenceId" />, in particular it is
        /// possible that a restarting actor tries to recover before its outstanding writes have completed.
        /// </summary>
        /// <param name="persistenceId">Persistent actor identifier</param>
        /// <param name="fromSequenceNr">Hint where to start searching for the highest sequence number.
        /// When a persistent actor is recovering this <paramref name="fromSequenceNr" /> will the the sequence
        /// number of the used snapshot, or `0L` if no snapshot is used.</param>
        /// <returns></returns>
        public override Task<long> ReadHighestSequenceNrAsync(string persistenceId, long fromSequenceNr)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously replays persistent messages. Implementations replay
        /// a message by calling <paramref name="recoveryCallback" />. The returned task must be completed
        /// when all messages (matching the sequence number bounds) have been replayed.
        /// The task must be completed with a failure if any of the persistent messages
        /// could not be replayed.
        /// The <paramref name="toSequenceNr" /> is the lowest of what was returned by
        /// <see cref="M:Akka.Persistence.Journal.IAsyncRecovery.ReadHighestSequenceNrAsync(System.String,System.Int64)" /> and what the user specified as recovery
        /// <see cref="T:Akka.Persistence.Recovery" /> parameter.
        /// This does imply that this call is always preceded by reading the highest sequence number
        /// for the given <paramref name="persistenceId" />.
        /// This call is NOT protected with a circuit-breaker because it may take a long time
        /// to replay all events. The plugin implementation itself must protect against an
        /// unresponsive backend store and make sure that the returned <see cref="T:System.Threading.Tasks.Task" />
        /// is completed with success or failure within reasonable time. It is not allowed to
        /// ignore completing the <see cref="T:System.Threading.Tasks.Task" />.
        /// </summary>
        /// <param name="persistenceId">Persistent actor identifier</param>
        /// <param name="fromSequenceNr">Inclusive sequence number where replay should start</param>
        /// <param name="toSequenceNr">Inclusive sequence number where replay should end</param>
        /// <param name="max">Maximum number of messages to be replayed</param>
        /// <param name="recoveryCallback">Called to replay a message, may be called from any thread.</param>
        /// <returns></returns>
        public override Task ReplayMessagesAsync(
                    IActorContext context,
            string persistenceId,
            long fromSequenceNr,
            long toSequenceNr,
            long max,
            Action<IPersistentRepresentation> recoveryCallback)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously deletes all persistent messages up to inclusive <paramref name="toSequenceNr" />
        /// bound.
        /// </summary>
        protected override Task DeleteMessagesToAsync(string persistenceId, long toSequenceNr)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Plugin API: asynchronously writes a batch of persistent messages to the
        /// journal.
        /// The batch is only for performance reasons, i.e. all messages don't have to be written
        /// atomically. Higher throughput can typically be achieved by using batch inserts of many
        /// records compared to inserting records one-by-one, but this aspect depends on the
        /// underlying data store and a journal implementation can implement it as efficient as
        /// possible. Journals should aim to persist events in-order for a given `persistenceId`
        /// as otherwise in case of a failure, the persistent state may be end up being inconsistent.
        /// Each <see cref="T:Akka.Persistence.AtomicWrite" /> message contains the single <see cref="T:Akka.Persistence.Persistent" />
        /// that corresponds to the event that was passed to the
        /// <see cref="!:PersistentActor.Persist&lt;TEvent&gt;(TEvent,Action&lt;TEvent&gt;)" /> method of the
        /// <see cref="T:Akka.Persistence.PersistentActor" />, or it contains several <see cref="T:Akka.Persistence.Persistent" />
        /// that correspond to the events that were passed to the
        /// <see cref="!:PersistentActor.PersistAll&lt;TEvent&gt;(IEnumerable&lt;TEvent&gt;,Action&lt;TEvent&gt;)" />
        /// method of the <see cref="T:Akka.Persistence.PersistentActor" />. All <see cref="T:Akka.Persistence.Persistent" /> of the
        /// <see cref="T:Akka.Persistence.AtomicWrite" /> must be written to the data store atomically, i.e. all or none must
        /// be stored. If the journal (data store) cannot support atomic writes of multiple
        /// events it should reject such writes with a <see cref="T:System.NotSupportedException" />
        /// describing the issue. This limitation should also be documented by the journal plugin.
        /// If there are failures when storing any of the messages in the batch the returned
        /// <see cref="T:System.Threading.Tasks.Task" /> must be completed with failure. The <see cref="T:System.Threading.Tasks.Task" /> must only be completed with
        /// success when all messages in the batch have been confirmed to be stored successfully,
        /// i.e. they will be readable, and visible, in a subsequent replay. If there is
        /// uncertainty about if the messages were stored or not the <see cref="T:System.Threading.Tasks.Task" /> must be completed
        /// with failure.
        /// Data store connection problems must be signaled by completing the <see cref="T:System.Threading.Tasks.Task" /> with
        /// failure.
        /// The journal can also signal that it rejects individual messages (<see cref="T:Akka.Persistence.AtomicWrite" />) by
        /// the returned <see cref="T:System.Threading.Tasks.Task" />. It is possible but not mandatory to reduce
        /// number of allocations by returning null for the happy path,
        /// i.e. when no messages are rejected. Otherwise the returned list must have as many elements
        /// as the input <paramref name="messages" />. Each result element signals if the corresponding
        /// <see cref="T:Akka.Persistence.AtomicWrite" /> is rejected or not, with an exception describing the problem. Rejecting
        /// a message means it was not stored, i.e. it must not be included in a later replay.
        /// Rejecting a message is typically done before attempting to store it, e.g. because of
        /// serialization error.
        /// Data store connection problems must not be signaled as rejections.
        /// It is possible but not mandatory to reduce number of allocations by returning
        /// null for the happy path, i.e. when no messages are rejected.
        /// Calls to this method are serialized by the enclosing journal actor. If you spawn
        /// work in asyncronous tasks it is alright that they complete the futures in any order,
        /// but the actual writes for a specific persistenceId should be serialized to avoid
        /// issues such as events of a later write are visible to consumers (query side, or replay)
        /// before the events of an earlier write are visible.
        /// A <see cref="T:Akka.Persistence.PersistentActor" /> will not send a new <see cref="T:Akka.Persistence.WriteMessages" /> request before
        /// the previous one has been completed.
        /// Please not that the <see cref="P:Akka.Persistence.IPersistentRepresentation.Sender" /> of the contained
        /// <see cref="T:Akka.Persistence.Persistent" /> objects has been nulled out (i.e. set to <see cref="F:Akka.Actor.ActorRefs.NoSender" />
        /// in order to not use space in the journal for a sender reference that will likely be obsolete
        /// during replay.
        /// Please also note that requests for the highest sequence number may be made concurrently
        /// to this call executing for the same `persistenceId`, in particular it is possible that
        /// a restarting actor tries to recover before its outstanding writes have completed.
        /// In the latter case it is highly desirable to defer reading the highest sequence number
        /// until all outstanding writes have completed, otherwise the <see cref="T:Akka.Persistence.PersistentActor" />
        /// may reuse sequence numbers.
        /// This call is protected with a circuit-breaker.
        /// </summary>
        protected override Task<IImmutableList<Exception>> WriteMessagesAsync(IEnumerable<AtomicWrite> messages)
        {
            throw new NotImplementedException();
        }
    }
}