﻿namespace Neuroglia.Plugins.Services;

/// <summary>
/// Defines the fundamentals of a service used to provide <see cref="IPlugin"/>s
/// </summary>
public interface IPluginProvider
{

    /// <summary>
    /// Gets all sourced <see cref="IPlugin"/>s
    /// </summary>
    /// <returns>A new <see cref="IEnumerable{T}"/> containing all sourced <see cref="IPlugin"/>s</returns>
    IEnumerable<IPlugin> GetPlugins();

    /// <summary>
    /// Gets all sourced plugins that implement the specified contract
    /// </summary>
    /// <param name="serviceType">The type of the contract implemented by sourced plugins. Must be an interface</param>
    /// <param name="sourceName">The name of the plugin source, if any, to get plugin implementations from</param>
    /// <param name="tags">An <see cref="IEnumerable{T}"/> containing the tags, if any, the plugins to get must define</param>
    /// <returns>A new <see cref="IEnumerable{T}"/> containing the plugins that implement the specified contract</returns>
    IEnumerable<object> GetPlugins(Type serviceType, string? sourceName = null, IEnumerable<string>? tags = null);

    /// <summary>
    /// Gets all sourced plugins that implement the specified contract
    /// </summary>
    /// <typeparam name="TService">The type of the contract implemented by sourced plugins. Must be an interface</typeparam>
    /// <param name="sourceName">The name of the plugin source, if any, to get plugin implementations from</param>
    /// <param name="tags">An <see cref="IEnumerable{T}"/> containing the tags, if any, the plugins to get must define</param>
    /// <returns>A new <see cref="IEnumerable{T}"/> containing the plugins that implement the specified contract</returns>
    IEnumerable<TService> GetPlugins<TService>(string? sourceName = null, IEnumerable<string>? tags = null)
        where TService : class;

}