using Neuroglia.Data.Infrastructure.EventSourcing;
using System.ComponentModel.DataAnnotations;

namespace Neuroglia.Data.Infrastructure.EntityFramework.EventSourcing.Models;

/// <summary>
/// Represents a persistent subscription
/// </summary>
public class PersistentSubscription
{

    /// <summary>
    /// Initializes a new <see cref="PersistentSubscription"/>
    /// </summary>
    protected PersistentSubscription() { }

    /// <summary>
    /// Initializes a new <see cref="PersistentSubscription"/>
    /// </summary>
    /// <param name="name">The <see cref="PersistentSubscription"/>'s name</param>
    /// <param name="streamId">The id of the stream, if any, the <see cref="PersistentSubscription"/> consumes events of</param>
    /// <param name="offset">The offset, if any, to start consuming events at. Defaults to <see cref="StreamPosition.StartOfStream"/></param>
    public PersistentSubscription(string name, string? streamId = null, ulong offset = StreamPosition.StartOfStream)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        this.Id = Guid.NewGuid().ToString("N");
        this.Name = name;
        this.StreamId = streamId;
        this.Offset = offset;
    }

    /// <summary>
    /// Gets the <see cref="PersistentSubscription"/>'s id
    /// </summary>
    public virtual string Id { get; protected set; } = null!;

    /// <summary>
    /// Gets the <see cref="PersistentSubscription"/>'s name
    /// </summary>
    [Required, MinLength(3)]
    public virtual string Name { get; protected set; } = null!;

    /// <summary>
    /// Gets the id of the stream, if any, the <see cref="PersistentSubscription"/> consumes events of
    /// </summary>
    public virtual string? StreamId { get; protected set; } = null!;

    /// <summary>
    /// Gets the <see cref="PersistentSubscription"/>'s offset in the stream
    /// </summary>
    public virtual ulong Offset { get; protected set; }

    /// <summary>
    /// Sets the <see cref="PersistentSubscription"/>'s offset
    /// </summary>
    /// <param name="offset">The <see cref="PersistentSubscription"/>'s offset</param>
    public virtual void SetOffset(ulong offset) => this.Offset = offset;

}