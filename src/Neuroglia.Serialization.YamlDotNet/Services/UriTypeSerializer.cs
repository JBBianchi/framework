﻿using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Neuroglia.Serialization.Yaml;

/// <summary>
/// Represents the <see cref="IYamlTypeConverter"/> used to serialize <see cref="Uri"/>s
/// </summary>
public class UriTypeSerializer
    : IYamlTypeConverter
{

    /// <inheritdoc/>
    public virtual bool Accepts(Type type) => typeof(Uri).IsAssignableFrom(type);

    /// <inheritdoc/>
    public virtual object ReadYaml(IParser parser, Type type)
    {
        var scalar = (Scalar)parser.Current!;
        parser.MoveNext();
        return new Uri(scalar.Value, UriKind.RelativeOrAbsolute);
    }

    /// <inheritdoc/>
    public virtual void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        if (value == null) return;
        emitter.Emit(new Scalar(((Uri)value).ToString()));
    }

}