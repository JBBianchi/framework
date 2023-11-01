using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using EFEventRecord = Neuroglia.Data.Infrastructure.EntityFramework.EventSourcing.Models.EventRecord;

namespace Neuroglia.Data.Infrastructure.EventSourcing.Services;

/// <summary>
/// Represents an Entity Framework based implementation of the <see cref="IEventStore"/> interface
/// </summary>
/// <typeparam name="TDbContext">The type of <see cref="Microsoft.EntityFrameworkCore.DbContext"/> to use</typeparam>
public class EntityFrameworkEventStore<TDbContext>
    : IEventStore
    where TDbContext : EventSourcingDbContext
{

    /// <summary>
    /// Initializes a new <see cref="EntityFrameworkEventStore"/>
    /// </summary>
    /// <param name="dbContext">The current <see cref="EventSourcingDbContext"/></param>
    public EntityFrameworkEventStore(TDbContext dbContext)
    {
        this.DbContext = dbContext;
    }

    /// <summary>
    /// Gets the current <see cref="EventSourcingDbContext"/>
    /// </summary>
    protected TDbContext DbContext { get; }

    /// <inheritdoc/>
    public virtual async Task AppendAsync(string streamId, IEnumerable<IEventDescriptor> events, long? expectedVersion = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(streamId)) throw new ArgumentNullException(nameof(streamId));
        if (events == null || !events.Any()) throw new ArgumentNullException(nameof(events));
        if (expectedVersion < StreamPosition.EndOfStream) throw new ArgumentOutOfRangeException(nameof(expectedVersion));

        var stream = (await this.DbContext.Streams.FindAsync(new object[] { streamId }, cancellationToken).ConfigureAwait(false)) ?? (await this.DbContext.Streams.AddAsync(new(streamId), cancellationToken).ConfigureAwait(false)).Entity;
        
        var offset = await this.DbContext.Events.Where(e => e.StreamId == streamId).OrderBy(e => e.Offset).Select(e => e.Offset).LastOrDefaultAsync(cancellationToken).ConfigureAwait(false) + 1;
        foreach(var e in events)
        {
            await this.DbContext.Events.AddAsync(new(streamId, Guid.NewGuid().ToString("N"), offset, DateTimeOffset.Now, e.Type, e.Data, e.Metadata), cancellationToken).ConfigureAwait(false);
            offset++;
        }
        await this.DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task<IEventStreamDescriptor> GetAsync(string streamId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(streamId)) throw new ArgumentNullException(nameof(streamId));

        var events = this.DbContext.Events.Where(e => e.StreamId == streamId).OrderBy(e => e.Offset);
        return new EventStreamDescriptor(streamId, events.Count(), (await events.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false))?.Timestamp, (await events.LastOrDefaultAsync(cancellationToken).ConfigureAwait(false))?.Timestamp);
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
        if (offset < StreamPosition.EndOfStream) throw new ArgumentOutOfRangeException(nameof(offset));
        if (length.HasValue && length < 1) yield break;

        if (!await this.DbContext.Streams.AnyAsync(s => s.Id == streamId, cancellationToken).ConfigureAwait(false)) throw new StreamNotFoundException(streamId);

        var events = this.DbContext.Events.Where(e => e.StreamId == streamId);
        if (readDirection == StreamReadDirection.Backwards) events = events.OrderByDescending(e => e.Offset);
        else events = events.OrderBy(e => e.Offset);
        events = events.SkipWhile(e => readDirection == StreamReadDirection.Forwards ? e.Offset < (ulong)offset : e.Offset > (ulong)offset);

        await foreach(var e in events.AsAsyncEnumerable()) yield return e;
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
        if (offset < StreamPosition.EndOfStream) throw new ArgumentOutOfRangeException(nameof(offset));
        if (length.HasValue && length < 1) yield break;

        var events = this.DbContext.Events.AsQueryable();
        if (readDirection == StreamReadDirection.Backwards) events = events.OrderByDescending(e => e.GlobalOffset);
        else events = events.OrderBy(e => e.GlobalOffset);
        events = events.SkipWhile(e => readDirection == StreamReadDirection.Forwards ? e.Offset < (ulong)offset : e.Offset > (ulong)offset);

        await foreach (var e in events.AsAsyncEnumerable()) yield return e;
    }

    /// <inheritdoc/>
    public virtual async Task SetOffsetAsync(string consumerGroup, long offset, string? streamId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(streamId)) throw new ArgumentNullException(nameof(streamId));
        if (offset < StreamPosition.StartOfStream) throw new ArgumentOutOfRangeException(nameof(offset));

        var subscription = await this.DbContext.Subscriptions.FirstOrDefaultAsync(s => s.Name == consumerGroup && s.StreamId == streamId, cancellationToken).ConfigureAwait(false) ?? (await this.DbContext.Subscriptions.AddAsync(new(consumerGroup, streamId), cancellationToken).ConfigureAwait(false)).Entity;
        subscription.SetOffset((ulong)offset);
        await this.DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task<IObservable<IEventRecord>> SubscribeAsync(string? streamId = null, long offset = -1, string? consumerGroup = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public virtual async Task TruncateAsync(string streamId, ulong? beforeVersion = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public virtual async Task DeleteAsync(string streamId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

}
