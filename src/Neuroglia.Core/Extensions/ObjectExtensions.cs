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

using System.ComponentModel;
using System.Dynamic;

namespace Neuroglia;

/// <summary>
/// Defines extensions for <see cref="object"/>s
/// </summary>
public static class ObjectExtensions
{

    /// <summary>
    /// Creates a new <see cref="IDictionary{TKey, TValue}"/> representing a name/value mapping of the object's properties
    /// </summary>
    /// <param name="source">The source object</param>
    /// <returns>A new <see cref="IDictionary{TKey, TValue}"/> representing a name/value mapping of the object's properties</returns>
    public static IDictionary<string, object>? ToDictionary(this object? source) => source == null ? null : source is IDictionary<string, object> dictionary ? dictionary : TypeDescriptor.GetProperties(source).OfType<PropertyDescriptor>().ToDictionary(p => p.Name, p => p.GetValue(source)!);

    /// <summary>
    /// Converts the object into a new <see cref="ExpandoObject"/>
    /// </summary>
    /// <param name="source">The object to convert</param>
    /// <returns>A new <see cref="ExpandoObject"/></returns>
    public static ExpandoObject? ToExpandoObject(this object? source)
    {
        if (source == null) return null;
        if (source is ExpandoObject expando) return expando;
        expando = new ExpandoObject();
        var inputProperties = source.ToDictionary()!;
        var outputProperties = expando as IDictionary<string, object>;
        
        foreach(var kvp in inputProperties) outputProperties[kvp.Key] = kvp.Value;

        return expando;
    }

}