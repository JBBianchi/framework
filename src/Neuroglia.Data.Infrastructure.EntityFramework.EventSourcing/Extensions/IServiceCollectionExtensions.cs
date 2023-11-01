// Copyright © 2021-Present Neuroglia SRL. All rights reserved.
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

using Microsoft.Extensions.DependencyInjection;
using Neuroglia.Data.Infrastructure.EventSourcing.Services;

namespace Neuroglia.Data.Infrastructure.EventSourcing;

/// <summary>
/// Defines extensions for <see cref="IServiceCollection"/>s
/// </summary>
public static class IServiceCollectionExtensions
{

    /// <summary>
    /// Adds and configures a <see cref="EntityFrameworkEventStore{TDbContext}"/>
    /// </summary>
    /// <typeparam name="TDbContext">The type of <see cref="EventSourcingDbContext"/> to use</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
    /// <param name="setup">An <see cref="Action{T}"/> used to configure the <see cref="EntityFrameworkEventStore{TDbContext}"/></param>
    /// <returns>The configured <see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddEntityFrameworkEventStore<TDbContext>(this IServiceCollection services, Action<IEventStoreOptionsBuilder>? setup = null)
        where TDbContext : EventSourcingDbContext
    {
        services.AddEventStore<EntityFrameworkEventStore<TDbContext>>(setup);
        return services;
    }

    /// <summary>
    /// Adds and configures a <see cref="EntityFrameworkEventStore{TDbContext}"/>
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
    /// <param name="setup">An <see cref="Action{T}"/> used to configure the <see cref="EntityFrameworkEventStore{TDbContext}"/></param>
    /// <returns>The configured <see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddEntityFrameworkEventStore(this IServiceCollection services, Action<IEventStoreOptionsBuilder>? setup = null) => services.AddEntityFrameworkEventStore<EventSourcingDbContext>(setup);

}
