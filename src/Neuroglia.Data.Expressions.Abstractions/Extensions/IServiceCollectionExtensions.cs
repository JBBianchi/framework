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
using Neuroglia.Data.Expressions.Services;

namespace Neuroglia.Data.Expressions;

/// <summary>
/// Defines extensions for <see cref="IServiceCollection"/>s
/// </summary>
public static class IServiceCollectionExtensions
{

    /// <summary>
    /// Adds and configures an <see cref="IExpressionEvaluator"/> of the specified type
    /// </summary>
    /// <typeparam name="TEvaluator">The type of <see cref="IExpressionEvaluator"/> to register</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/>. Defaults to <see cref="ServiceLifetime.Transient"/></param>
    /// <returns>The configured <see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddExpressionEvaluator<TEvaluator>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TEvaluator : class, IExpressionEvaluator
    {
        services.TryAddSingleton<IExpressionEvaluatorProvider, ExpressionEvaluatorProvider>();
        services.TryAdd(new ServiceDescriptor(typeof(TEvaluator), typeof(TEvaluator), lifetime));
        services.Add(new ServiceDescriptor(typeof(IExpressionEvaluator), provider => provider.GetRequiredService<TEvaluator>(), lifetime));
        return services;
    }

}
