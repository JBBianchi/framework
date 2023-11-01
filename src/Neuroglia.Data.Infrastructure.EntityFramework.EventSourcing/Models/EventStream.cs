using System.ComponentModel.DataAnnotations;

namespace Neuroglia.Data.Infrastructure.EntityFramework.EventSourcing.Models;

/// <summary>
/// Represents a stream of events
/// </summary>
public class EventStream
{

    /// <summary>
    /// Initializes a new <see cref="EventStream"/>
    /// </summary>
    protected EventStream() { }

    /// <summary>
    /// Initializes a new <see cref="EventStream"/>
    /// </summary>
    /// <param name="id">The <see cref="EventStream"/>'s id of the</param>
    /// <param name="events">A collection of the stream's <see cref="EventRecord"/>s</param>
    public EventStream(string id, IEnumerable<EventRecord>? events = null)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

        this.Id = id;
        this.Events = events?.ToList() ?? new List<EventRecord>();
    }

    /// <summary>
    /// Gets the <see cref="EventStream"/>'s id of the
    /// </summary>
    [Required, MinLength(3)]
    public virtual string Id { get; protected set; } = null!;

    /// <summary>
    /// Gets a collection of the stream's <see cref="EventRecord"/>s
    /// </summary>
    public virtual ICollection<EventRecord> Events { get; protected set; } = null!;

}
