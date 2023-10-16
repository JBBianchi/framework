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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Neuroglia.Data.Expressions.JQ.Configuration;

namespace Neuroglia.Data.Expressions.JQ;

/// <summary>
/// Defines extensions for <see cref="IServiceCollection"/>s
/// </summary>
public static class IServiceCollectionExtensions
{

    /// <summary>
    /// Adds and configures a new <see cref="JQExpressionEvaluator"/>
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
    /// <param name="setup">An <see cref="Action{T}"/> used to configure the <see cref="JQExpressionEvaluator"/></param>
    /// <param name="lifetime">The service's lifetime. Defaults to <see cref="ServiceLifetime.Transient"/></param>
    /// <returns>The configured <see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddJQExpressionEvaluator(this IServiceCollection services, Action<IJQExpressionEvaluatorOptionsBuilder>? setup = null, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        IJQExpressionEvaluatorOptionsBuilder builder = new JQExpressionEvaluatorOptionsBuilder();
        setup?.Invoke(builder);
        services.TryAddSingleton(Options.Create(builder.Build()));
        services.AddExpressionEvaluator<JQExpressionEvaluator>(lifetime);
        return services;
    }

}
