﻿// Copyright © 2021-Present Neuroglia SRL. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.Extensions.Options;
using Neuroglia.Data.Infrastructure.EventSourcing.Configuration;
using Neuroglia.Serialization;
using StackExchange.Redis;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;

namespace Neuroglia.Data.Infrastructure.EventSourcing.Services;

/// <summary>
/// Represents a <see href="https://stackexchange.github.io/StackExchange.Redis/">StackExchange Redis</see> implementation of the <see cref="IEventStore"/> interface
/// </summary>
public class RedisEventStore
    : IEventStore
{

    /// <summary>
    /// Gets the key used of the global event stream
    /// </summary>
    public const string GlobalStreamKey = "$all";

    /// <summary>
    /// Initializes a new <see cref="RedisEventStore"/>
    /// </summary>
    /// <param name="options">The current <see cref="EventStoreOptions"/></param>
    /// <param name="redis">The service used to connect to the Redis Server</param>
    /// <param name="serializerProvider">The service used to provide <see cref="ISerializer"/>s</param>
    public RedisEventStore(IOptions<EventStoreOptions> options, IConnectionMultiplexer redis, ISerializerProvider serializerProvider)
    {
        this.Options = options.Value;
        this.Serializer = serializerProvider.GetSerializers().First(s => this.Options.SerializerType == null || s.GetType() == this.Options.SerializerType);
        this.Redis = redis;
        this.Database = redis.GetDatabase();
        this.Subscriber = redis.GetSubscriber();
    }

    /// <summary>
    /// Gets the current <see cref="EventStoreOptions"/>
    /// </summary>
    protected EventStoreOptions Options { get; }

    /// <summary>
    /// Gets the service used to serialize and deserialize stored events
    /// </summary>
    protected ISerializer Serializer { get; }

    /// <summary>
    /// Gets the service used to connect to the Redis Server
    /// </summary>
    protected IConnectionMultiplexer Redis { get; }

    /// <summary>
    /// Gets the Redis database to store events into
    /// </summary>
    protected IDatabase Database { get; }

    /// <summary>
    /// Gets the service used to subscribe to a redis pub/sub channel
    /// </summary>
    protected ISubscriber Subscriber { get; }

    /// <inheritdoc/>
    public virtual async Task AppendAsync(string streamId, IEnumerable<IEventDescriptor> events, long? expectedVersion = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(streamId)) throw new ArgumentNullException(nameof(streamId));
        if (events == null || !events.Any()) throw new ArgumentNullException(nameof(events));

        var keys = (await this.Database.HashKeysAsync(streamId).ConfigureAwait(false))?.Order().ToList();
        var actualversion = keys == null || !keys.Any() ? (long?)null : (long)keys.Order().LastOrDefault();

        if (expectedVersion.HasValue)
        {
            if (expectedVersion.Value == Infrastructure.EventSourcing.StreamPosition.EndOfStream)
            {
                if (actualversion != null) throw new OptimisticConcurrencyException(expectedVersion, actualversion);
            }
            else if (actualversion == null || actualversion != expectedVersion) throw new OptimisticConcurrencyException(expectedVersion, actualversion);
        }

        var offset = actualversion.HasValue ? (ulong)actualversion.Value + 1 : StreamPosition.StartOfStream;
        foreach (var e in events)
        {
            var record = new EventRecord(streamId, Guid.NewGuid().ToShortString(), offset, DateTimeOffset.Now, e.Type, e.Data, e.Metadata);
            record.Metadata ??= new Dictionary<string, object>();
            record.Metadata[EventRecordMetadata.ClrTypeName] = e.Data?.GetType().AssemblyQualifiedName!;
            var entryValue = this.Serializer.SerializeToByteArray(record);
            await this.Database.HashSetAsync(streamId, new HashEntry[] { new(offset, entryValue) }).ConfigureAwait(false);
            await this.AppendToGlobalStreamAsync(record, cancellationToken).ConfigureAwait(false);
            await this.Database.PublishAsync(this.GetRedisChannel(streamId), entryValue).ConfigureAwait(false);
            await this.Database.PublishAsync(this.GetRedisChannel(), entryValue).ConfigureAwait(false);
            offset++;
        }

    }

    /// <summary>
    /// Appends the specified <see cref="IEventRecord"/> to the global event stream
    /// </summary>
    /// <param name="e">The <see cref="IEventRecord"/> to append to the global event stream</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task AppendToGlobalStreamAsync(IEventRecord e, CancellationToken cancellationToken = default)
    {
        var keys = (await this.Database.HashKeysAsync(GlobalStreamKey).ConfigureAwait(false))?.Order().ToList();
        var offset = keys == null || !keys.Any() ? StreamPosition.StartOfStream : (long)keys.Last() + 1;
        var entryValue = this.Serializer.SerializeToByteArray(new EventReference(e.StreamId, e.Offset));
        await this.Database.HashSetAsync(GlobalStreamKey, new HashEntry[] { new(offset, entryValue) }).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task<IEventStreamDescriptor> GetAsync(string streamId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(streamId)) throw new ArgumentNullException(nameof(streamId));
        if (!await this.Database.KeyExistsAsync(streamId).ConfigureAwait(false)) throw new StreamNotFoundException(streamId);

        var keys = (await this.Database.HashKeysAsync(streamId).ConfigureAwait(false)).Order().ToList();
        DateTimeOffset? firstEventAt = null;
        DateTimeOffset? lastEventAt = null;
        if(keys.Any())
        {
            firstEventAt = this.Serializer.Deserialize<EventRecord>((byte[])(await this.Database.HashGetAsync(streamId, keys.First()).ConfigureAwait(false))!)!.Timestamp;
            lastEventAt = this.Serializer.Deserialize<EventRecord>((byte[])(await this.Database.HashGetAsync(streamId, keys.Last()).ConfigureAwait(false))!)!.Timestamp;
        }

        return new EventStreamDescriptor(streamId, keys.Count, firstEventAt, lastEventAt);
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<IEventRecord> ReadAsync(string? streamId, StreamReadDirection readDirection, long offset, ulong? length = null, CancellationToken cancellationToken = default)
    {
        if(string.IsNullOrWhiteSpace(streamId)) return this.ReadFromAllAsync(readDirection, offset, length, cancellationToken);
        else return this.ReadFromStreamAsync(streamId, readDirection, offset, length, cancellationToken);
    }

    /// <summary>
    /// Reads events recorded on the specified stream
    /// </summary>
    /// <param name="streamId">The id of the stream to read events from</param>
    /// <param name="readDirection">The direction in which to read the stream</param>
    /// <param name="offset">The offset starting from which to read events</param>
    /// <param name="length">The amount of events to read</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IAsyncEnumerable{T}"/> containing the events read from the store</returns>
    protected virtual async IAsyncEnumerable<IEventRecord> ReadFromStreamAsync(string streamId, StreamReadDirection readDirection, long offset, ulong? length = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(streamId)) throw new ArgumentNullException(nameof(streamId));
        if (!await this.Database.KeyExistsAsync(streamId).ConfigureAwait(false)) throw new StreamNotFoundException(streamId);
        if (offset < StreamPosition.EndOfStream) throw new ArgumentOutOfRangeException(nameof(offset));

        var hashKeys = (await this.Database.HashKeysAsync(streamId).ConfigureAwait(false)).Order().ToList();

        int skip;
        switch (readDirection)
        {
            case StreamReadDirection.Forwards:
                if (offset < StreamPosition.StartOfStream) yield break;
                skip = hashKeys.IndexOf(offset);
                if (skip < 0) yield break;
                hashKeys = hashKeys.Skip(skip).ToList();
                break;
            case StreamReadDirection.Backwards:
                if (offset <= StreamPosition.StartOfStream && offset != StreamPosition.EndOfStream) yield break;
                hashKeys.Reverse();
                if (offset != StreamPosition.EndOfStream)
                {
                    skip = hashKeys.IndexOf(offset);
                    if (skip < 0) yield break;
                    hashKeys = hashKeys.Skip(skip).ToList();
                }
                break;
            default: throw new NotSupportedException($"The specified {nameof(StreamReadDirection)} '{readDirection}' is not supported");
        }

        if (length.HasValue) hashKeys = hashKeys.Take((int)length.Value).ToList();

        foreach (var hashKey in hashKeys) yield return this.DeserializeEventRecord((byte[])(await this.Database.HashGetAsync(streamId, hashKey).ConfigureAwait(false))!);
    }

    /// <summary>
    /// Reads recorded events accross all streams
    /// </summary>
    /// <param name="readDirection">The direction in which to read events</param>
    /// <param name="offset">The offset starting from which to read events</param>
    /// <param name="length">The amount of events to read</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IAsyncEnumerable{T}"/> containing the events read from the store</returns>
    protected virtual async IAsyncEnumerable<IEventRecord> ReadFromAllAsync(StreamReadDirection readDirection, long offset, ulong? length = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var hashKeys = (await this.Database.HashKeysAsync(GlobalStreamKey).ConfigureAwait(false)).Order().ToList();

        int skip;
        switch (readDirection)
        {
            case StreamReadDirection.Forwards:
                if (offset < StreamPosition.StartOfStream) yield break;
                skip = hashKeys.IndexOf(offset);
                if (skip < 0) yield break;
                hashKeys = hashKeys.Skip(skip).ToList();
                break;
            case StreamReadDirection.Backwards:
                if (offset <= StreamPosition.StartOfStream && offset != StreamPosition.EndOfStream) yield break;
                hashKeys.Reverse();
                if (offset != StreamPosition.EndOfStream)
                {
                    skip = hashKeys.IndexOf(offset);
                    if (skip < 0) yield break;
                    hashKeys = hashKeys.Skip(skip).ToList();
                }
                break;
            default: throw new NotSupportedException($"The specified {nameof(StreamReadDirection)} '{readDirection}' is not supported");
        }

        if (length.HasValue) hashKeys = hashKeys.Take((int)length.Value).ToList();

        foreach (var hashKey in hashKeys)
        {
            var reference = this.Serializer.Deserialize<EventReference>((byte[])(await this.Database.HashGetAsync(GlobalStreamKey, hashKey).ConfigureAwait(false))!)!;
            yield return this.DeserializeEventRecord((byte[])(await this.Database.HashGetAsync(reference.StreamId, reference.Offset.ToString()).ConfigureAwait(false))!);
        }
    }

    /// <inheritdoc/>
    public virtual Task<IObservable<IEventRecord>> SubscribeAsync(string? streamId, long offset = StreamPosition.EndOfStream, string? consumerGroup = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(streamId)) return this.SubscribeToAllAsync(offset, consumerGroup, cancellationToken);
        else return this.SubscribeToStreamAsync(streamId, offset, consumerGroup, cancellationToken);
    }

    /// <summary>
    /// Subscribes to events of the specified stream
    /// </summary>
    /// <param name="streamId">The id of the stream, if any, to subscribe to. If not set, subscribes to all events</param>
    /// <param name="offset">The offset starting from which to receive events. Defaults to <see cref="StreamPosition.EndOfStream"/></param>
    /// <param name="consumerGroup">The name of the consumer group, if any, in case the subscription is persistent</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IObservable{T}"/> used to observe events</returns>
    public virtual async Task<IObservable<IEventRecord>> SubscribeToStreamAsync(string streamId, long offset = StreamPosition.EndOfStream, string? consumerGroup = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(streamId)) throw new ArgumentNullException(nameof(streamId));
        if (!await this.Database.KeyExistsAsync(streamId).ConfigureAwait(false)) throw new StreamNotFoundException(streamId);
        if (offset < StreamPosition.EndOfStream) throw new ArgumentOutOfRangeException(nameof(offset));

        var events = offset == StreamPosition.EndOfStream ? Array.Empty<IEventRecord>().ToList() : await (this.ReadAsync(streamId, StreamReadDirection.Forwards, offset, cancellationToken: cancellationToken)).ToListAsync(cancellationToken).ConfigureAwait(false);
        var messageQueue = await this.Subscriber.SubscribeAsync(this.GetRedisChannel(streamId));
        var subject = new Subject<IEventRecord>();
        var redisSubscription = new RedisSubscription(messageQueue);
        var subscription = messageQueue.ToObservable().Select(m => this.DeserializeEventRecord(m.Message)).Subscribe(e => this.OnEventConsumed(subject, e, streamId, consumerGroup));
        var observable = Observable.Using(() => new CompositeDisposable(subscription, redisSubscription), _ => subject);

        return Observable.StartWith(observable, events);
    }

    /// <summary>
    /// Subscribes to all events
    /// </summary>
    /// <param name="offset">The offset starting from which to receive events. Defaults to <see cref="StreamPosition.EndOfStream"/></param>
    /// <param name="consumerGroup">The name of the consumer group, if any, in case the subscription is persistent</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new <see cref="IObservable{T}"/> used to observe events</returns>
    public virtual async Task<IObservable<IEventRecord>> SubscribeToAllAsync(long offset = StreamPosition.EndOfStream, string? consumerGroup = null, CancellationToken cancellationToken = default)
    {
        if (offset < StreamPosition.EndOfStream) throw new ArgumentOutOfRangeException(nameof(offset));

        var events = offset == StreamPosition.EndOfStream ? Array.Empty<IEventRecord>().ToList() : await (this.ReadAsync(null, StreamReadDirection.Forwards, offset, cancellationToken: cancellationToken)).ToListAsync(cancellationToken).ConfigureAwait(false);
        var messageQueue = await this.Subscriber.SubscribeAsync(this.GetRedisChannel());
        var subject = new ReplaySubject<IEventRecord>();
        var redisSubscription = new RedisSubscription(messageQueue);
        var subscription = messageQueue.ToObservable().Select(m => this.DeserializeEventRecord(m.Message)).Subscribe(m => this.OnEventConsumed(subject, m, null, consumerGroup));
        var observable = Observable.Using(() => new CompositeDisposable(subscription, redisSubscription), _ => subject);

        return Observable.StartWith(observable, events);
    }

    /// <inheritdoc/>
    public virtual async Task TruncateAsync(string streamId, ulong? beforeVersion = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(streamId)) throw new ArgumentNullException(nameof(streamId));
        if (!await this.Database.KeyExistsAsync(streamId).ConfigureAwait(false)) throw new StreamNotFoundException(streamId);
        if (beforeVersion.HasValue && beforeVersion < StreamPosition.StartOfStream) throw new ArgumentOutOfRangeException(nameof(beforeVersion));

        var hashKeys = (await this.Database.HashKeysAsync(streamId).ConfigureAwait(false)).Order().ToList();
        if (!hashKeys.Any()) return;

        var beforeElement = hashKeys.Select(k => (ulong?)k).FirstOrDefault(o => o >= beforeVersion);
        if (beforeElement != null)
        {
            var index = hashKeys.Select(k => (ulong?)k).ToList().IndexOf(beforeElement);
            hashKeys = hashKeys.Take(index).ToList();
        }
        await this.Database.HashDeleteAsync(streamId, hashKeys.ToArray()).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task DeleteAsync(string streamId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(streamId)) throw new ArgumentNullException(nameof(streamId));
        if (!await this.Database.KeyExistsAsync(streamId).ConfigureAwait(false)) throw new StreamNotFoundException(streamId);

        await this.Database.KeyDeleteAsync(streamId).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the offset persisted for the specified consumer group and stream
    /// </summary>
    /// <param name="consumerGroup">The consumer group to the stored offset for</param>
    /// <param name="streamId">The id of the stream, if any, to get the stored offset for</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    protected virtual async Task<long?> GetOffsetAsync(string consumerGroup, string? streamId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(consumerGroup)) throw new ArgumentNullException(nameof(consumerGroup));
        var offsetRaw = (string?)await this.Database.StringGetAsync(this.GetConsumerGroupCacheKey(consumerGroup, streamId)).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(offsetRaw)) return null;
        else return long.Parse(offsetRaw);
    }

    /// <inheritdoc/>
    public virtual async Task SetOffsetAsync(string consumerGroup, long offset, string? streamId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(consumerGroup)) throw new ArgumentNullException(nameof(consumerGroup));
        if (offset < StreamPosition.EndOfStream) throw new ArgumentOutOfRangeException(nameof(offset));
        await this.Database.StringSetAsync(this.GetConsumerGroupCacheKey(consumerGroup, streamId), offset.ToString());
    }

    /// <summary>
    /// Deserializes the specified <see cref="RedisValue"/> into a new <see cref="IEventRecord"/>
    /// </summary>
    /// <param name="value">The <see cref="RedisValue"/> to deserialize</param>
    /// <returns>The deserialized <see cref="IEventRecord"/></returns>
    protected virtual IEventRecord DeserializeEventRecord(RedisValue value)
    {
        var byteArray = (byte[])value!;
        var record = this.Serializer.Deserialize<EventRecord>(byteArray)!;
        var clrTypeName = record.Metadata![EventRecordMetadata.ClrTypeName].ToString()!;
        var clrType = Type.GetType(clrTypeName) ?? throw new Exception();
        record.Data = this.Serializer.Convert(record.Data, clrType);
        record.Metadata.Remove(EventRecordMetadata.ClrTypeName);
        if (!record.Metadata.Any()) record.Metadata = null;
        return record;
    }

    /// <summary>
    /// Gets the <see cref="RedisChannel"/> for the specified stream id, if any
    /// </summary>
    /// <param name="streamId">The id of the stream, if any, to get the <see cref="RedisChannel"/> for</param>
    /// <returns>The <see cref="RedisChannel"/> for the specified stream id</returns>
    protected virtual RedisChannel GetRedisChannel(string? streamId = null) => string.IsNullOrWhiteSpace(streamId) ? new($"rx-s:{streamId}", RedisChannel.PatternMode.Literal) : new($"rx-s:$all", RedisChannel.PatternMode.Literal);

    /// <summary>
    /// Gets the cache key for the specified consumer group and stream
    /// </summary>
    /// <param name="consumerGroup">The consumer group to the cache key for</param>
    /// <param name="streamId">The id of the stream, if any, to get the cache key for</param>
    /// <returns>The cache key for the specified consumer group and stream</returns>
    protected virtual string GetConsumerGroupCacheKey(string consumerGroup, string? streamId = null) => string.IsNullOrWhiteSpace(streamId) ? $"rx-cg:{consumerGroup}-$all" : $"rx-cg::{consumerGroup}-{streamId}";

    /// <summary>
    /// Handles the consumption of a <see cref="IEventRecord"/> on a subscription
    /// </summary>
    /// <param name="subject">The <see cref="ISubject{T}"/> used to stream <see cref="IEventRecord"/>s</param>
    /// <param name="streamId">The id of the stream <see cref="IEventRecord"/> belongs to</param>
    /// <param name="e">The <see cref="IEventRecord"/> to handle</param>
    /// <param name="consumerGroup">The name of the group, if any, that consumes the <see cref="IEventRecord"/></param>
    protected void OnEventConsumed(ISubject<IEventRecord> subject, IEventRecord e, string? streamId, string? consumerGroup)
    {
        var ackDelegate = () => string.IsNullOrWhiteSpace(consumerGroup) ? null : this.SetOffsetAsync(consumerGroup, (long)e.Offset, streamId);
        var nackDelegate = (string? reason) => string.IsNullOrWhiteSpace(consumerGroup) ? null : Task.Run(() => this.OnEventConsumed(subject, e, null, consumerGroup));
        var record = string.IsNullOrEmpty(consumerGroup) ? e : new AckableEventRecord(e.StreamId, e.Id, e.Offset, e.Timestamp, e.Type, e.Data, e.Metadata, ackDelegate, nackDelegate);
        subject.OnNext(record);
    }

    /// <summary>
    /// Exposes constants about event related metadata used by the <see cref="RedisEventStore"/>
    /// </summary>
    protected static class EventRecordMetadata
    {

        /// <summary>
        /// Gets the name of the event record metadata used to store the event CLR type's assembly qualified name
        /// </summary>
        public const string ClrTypeName = "clr-type";

    }

    class RedisSubscription
        : IDisposable
    {

        private readonly ChannelMessageQueue _queue;

        public RedisSubscription(ChannelMessageQueue queue)
        {
            this._queue = queue;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this._queue.Unsubscribe();
            GC.SuppressFinalize(this);
        }

    }

    record EventReference
    {

        public EventReference(string streamId, ulong offset)
        {
            this.StreamId = streamId;
            this.Offset = offset;
        }

        public string StreamId { get; }

        public ulong Offset { get; }

    }

}
