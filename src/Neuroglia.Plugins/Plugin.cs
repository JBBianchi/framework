﻿using System.Reflection;
using System.Runtime.Loader;

namespace Neuroglia.Plugins;

/// <summary>
/// Represents the default implementation of the <see cref="IPlugin"/> interface
/// </summary>
public class Plugin
    : IPlugin
{

    /// <summary>
    /// Initializes a new <see cref="Plugin"/>
    /// </summary>
    protected Plugin() { }

    /// <summary>
    /// Initializes a new <see cref="Plugin"/>
    /// </summary>
    /// <param name="name">The name of the <see cref="IPlugin"/></param>
    /// <param name="version">The version of the <see cref="IPlugin"/></param>
    /// <param name="type">The type of the <see cref="IPlugin"/></param>
    /// <param name="assembly">The assembly of the <see cref="IPlugin"/></param>
    /// <param name="assemblyLoadContext">The <see cref="IPlugin"/>'s <see cref="System.Runtime.Loader.AssemblyLoadContext"/></param>
    public Plugin(string name, Version version, Type type, Assembly assembly, AssemblyLoadContext assemblyLoadContext)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        this.Name = name;
        this.Version = version ?? new(1, 0, 0);
        this.Type = type ?? throw new ArgumentNullException(nameof(type));
        this.Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        this.AssemblyLoadContext = assemblyLoadContext ?? throw new ArgumentNullException(nameof(assemblyLoadContext));
    }

    /// <inheritdoc/>
    public virtual string Name { get; protected set; } = null!;

    /// <inheritdoc/>
    public virtual Version Version { get; protected set; } = null!;

    /// <inheritdoc/>
    public virtual Type Type { get; protected set; } = null!;

    /// <inheritdoc/>
    public virtual Assembly Assembly { get; protected set; } = null!;

    /// <inheritdoc/>
    public AssemblyLoadContext AssemblyLoadContext { get; protected set; } = null!;
}