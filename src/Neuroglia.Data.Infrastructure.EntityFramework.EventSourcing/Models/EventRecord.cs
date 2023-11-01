using Neuroglia.Data.Infrastructure.EventSourcing;
using Neuroglia.Serialization.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;

namespace Neuroglia.Data.Infrastructure.EntityFramework.EventSourcing.Models;

/// <summary>
/// Represents the record of an event
/// </summary>
public class EventRecord
    : IEventRecord
{

    object? _data;
    IDictionary<string, object>? _metadata;

    /// <summary>
    /// Initializes a new <see cref="EventRecord"/>
    /// </summary>
    protected EventRecord() { }

    /// <summary>
    /// Initializes a new <see cref="EventRecord"/>
    /// </summary>
    /// <param name="streamId">The id of the stream the recorded event belongs to</param>
    /// <param name="id">The id of the recorded event</param>
    /// <param name="offset">The offset of the recorded event</param>
    /// <param name="timestamp">The date and time at which the event has been recorded</param>
    /// <param name="type">The type of the recorded event. Should be a non-versioned reverse uri made out alphanumeric, '-' and '.' characters</param>
    /// <param name="data">The data of the recorded event</param>
    /// <param name="metadata">The metadata of the recorded event</param>
    public EventRecord(string streamId, string id, ulong offset, DateTimeOffset timestamp, string type, object? data = null, IDictionary<string, object>? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(streamId)) throw new ArgumentNullException(nameof(streamId));
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
        if (string.IsNullOrWhiteSpace(type)) throw new ArgumentNullException(nameof(type));

        this.StreamId = streamId;
        this.Id = id;
        this.Offset = offset;
        this.Timestamp = timestamp;
        this.Type = type;
        this.DataJson = data == null ? null : JsonSerializer.Default.SerializeToText(data);
        this.MetadataJson = metadata == null ? null : JsonSerializer.Default.SerializeToText(metadata);
    }

    /// <summary>
    /// Gets the <see cref="EventRecord"/>'s global offset
    /// </summary>
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual ulong GlobalOffset { get; protected set; }

    /// <inheritdoc/>
    public virtual string StreamId { get; protected set; } = null!;

    /// <inheritdoc/>
    public virtual string Id { get; protected set; } = null!;

    /// <inheritdoc/>
    public virtual ulong Offset { get; protected set; }

    /// <inheritdoc/>
    public virtual DateTimeOffset Timestamp { get; protected set; }

    /// <inheritdoc/>
    public virtual string Type { get; protected set; } = null!;

    /// <inheritdoc/>
    [NotMapped]
    public virtual object? Data
    {
        get
        {
            if(this._data == null && !string.IsNullOrWhiteSpace(this.DataJson)) this._data = JsonSerializer.Default.Deserialize<ExpandoObject>(this.DataJson);
            return this._data;
        }
    }

    /// <summary>
    /// Gets the event's data, if any, serialized in JSON
    /// </summary>
    [Column(nameof(Data))]
    protected virtual string? DataJson { get; set; }

    /// <inheritdoc/>
    [NotMapped]
    public virtual IDictionary<string, object>? Metadata
    {
        get
        {
            if (this._metadata == null && !string.IsNullOrWhiteSpace(this.DataJson)) this._data = JsonSerializer.Default.Deserialize<IDictionary<string, object>>(this.DataJson);
            return this._metadata;
        }
    }

    /// <summary>
    /// Gets the event's metadata, if any, serialized in JSON
    /// </summary>
    [Column(nameof(Data))]
    protected virtual string? MetadataJson { get; set; }

}