﻿namespace Neuroglia.Plugins.Services;

/// <summary>
/// Represents the service used to build <see cref="PluginTypeFilter"/>s
/// </summary>
public class PluginTypeFilterBuilder
    : IPluginTypeFilterBuilder
{

    /// <summary>
    /// Gets the <see cref="PluginTypeFilter"/> to build
    /// </summary>
    protected PluginTypeFilter Filter { get; } = new();

    /// <inheritdoc/>
    public IPluginTypeFilterBuilder AssignableFrom(Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (!type.IsInterface) throw new ArgumentException($"The specified type must be an interface", nameof(type));
        this.Filter.Criteria.Add(new(PluginTypeFilterCriterionType.Implements, type.AssemblyQualifiedName!));
        return this;
    }

    /// <inheritdoc/>
    public virtual IPluginTypeFilterBuilder Implements(Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (!type.IsInterface) throw new ArgumentException($"The specified type must be an interface", nameof(type));
        this.Filter.Criteria.Add(new(PluginTypeFilterCriterionType.Implements, type.AssemblyQualifiedName!));
        return this;
    }

    /// <inheritdoc/>
    public virtual IPluginTypeFilterBuilder Inherits(Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (!type.IsInterface) throw new ArgumentException($"The specified type must be an interface", nameof(type));
        this.Filter.Criteria.Add(new(PluginTypeFilterCriterionType.Inherits, type.AssemblyQualifiedName!));
        return this;
    }

    /// <inheritdoc/>
    public virtual IPluginTypeFilterBuilder AssignableFrom<T>() where T : class => this.AssignableFrom(typeof(T));

    /// <inheritdoc/>
    public virtual IPluginTypeFilterBuilder Implements<T>() where T : class => this.Implements(typeof(T));

    /// <inheritdoc/>
    public virtual IPluginTypeFilterBuilder Inherits<T>() where T : class => this.Inherits(typeof(T));

    /// <inheritdoc/>
    public virtual PluginTypeFilter Build() => this.Filter;

}
