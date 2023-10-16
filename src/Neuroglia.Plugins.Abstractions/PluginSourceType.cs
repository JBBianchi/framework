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
using System.Runtime.Serialization;

namespace Neuroglia.Plugins;

/// <summary>
/// Enumerates supported plugin source types
/// </summary>
[TypeConverter(typeof(EnumMemberTypeConverter))]
public enum PluginSourceType
{
    /// <summary>
    /// Indicates a file system directory based plugin source
    /// </summary>
    [EnumMember(Value = "directory")]
    Directory,
    /// <summary>
    /// Indicates an assembly based plugin source
    /// </summary>
    [EnumMember(Value = "assembly")]
    Assembly,
    /// <summary>
    /// Indicates a Nuget package plugin source
    /// </summary>
    [EnumMember(Value = "nuget")]
    Nuget
}