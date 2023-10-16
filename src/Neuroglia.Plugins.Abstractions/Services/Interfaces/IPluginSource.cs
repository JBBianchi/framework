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

namespace Neuroglia.Plugins.Services;

/// <summary>
/// Defines the fundamentals of a plugin source
/// </summary>
public interface IPluginSource
{

    /// <summary>
    /// Gets the source's name, if any
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Gets a boolean indicating whether or not the <see cref="IPluginSource"/> has been loaded
    /// </summary>
    bool IsLoaded { get; }

    /// <summary>
    /// Gets a list containing of sourced plugins
    /// </summary>
    IReadOnlyList<IPluginDescriptor> Plugins { get; }

    /// <summary>
    /// Loads the <see cref="IPluginSource"/>
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task LoadAsync(CancellationToken cancellationToken = default);

}
