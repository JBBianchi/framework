using Microsoft.EntityFrameworkCore;
using Neuroglia.Data.Infrastructure.EntityFramework.EventSourcing.Models;
using EFEventRecord = Neuroglia.Data.Infrastructure.EntityFramework.EventSourcing.Models.EventRecord;

namespace Neuroglia.Data.Infrastructure.EventSourcing.Services;

/// <summary>
/// Represents the <see cref="DbContext"/> used to manage event sourcing related data
/// </summary>
public class EventSourcingDbContext
    : DbContext
{

    /// <inheritdoc/>
    public EventSourcingDbContext(DbContextOptions<EventSourcingDbContext> options) : base(options) { }

    /// <summary>
    /// Gets the <see cref="DbSet{TEntity}"/> used to manage <see cref="EFEventRecord"/>s
    /// </summary>
    public virtual DbSet<EFEventRecord> Events { get; protected set; }

    /// <summary>
    /// Gets the <see cref="DbSet{TEntity}"/> used to manage <see cref="EventStream"/>s
    /// </summary>
    public virtual DbSet<EventStream> Streams { get; protected set; }

    /// <summary>
    /// Gets the <see cref="DbSet{TEntity}"/> used to manage <see cref="PersistentSubscription"/>s
    /// </summary>
    public virtual DbSet<PersistentSubscription> Subscriptions { get; protected set; }

}
