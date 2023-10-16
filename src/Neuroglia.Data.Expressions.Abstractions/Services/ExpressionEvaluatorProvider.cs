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

namespace Neuroglia.Data.Expressions.Services;

/// <summary>
/// Represents the default implementation of the <see cref="IExpressionEvaluator"/> interface
/// </summary>
public class ExpressionEvaluatorProvider
    : IExpressionEvaluatorProvider
{

    /// <summary>
    /// Initializes a new <see cref="ExpressionEvaluatorProvider"/>
    /// </summary>
    /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
    public ExpressionEvaluatorProvider(IServiceProvider serviceProvider) { this.ServiceProvider = serviceProvider; }

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    protected IServiceProvider ServiceProvider { get; }

    /// <inheritdoc/>
    public virtual IExpressionEvaluator? GetEvaluator(string language)
    {
        if (string.IsNullOrEmpty(language)) throw new ArgumentNullException(nameof(language));
        return this.GetEvaluators(language).FirstOrDefault();
    }

    /// <inheritdoc/>
    public virtual IEnumerable<IExpressionEvaluator> GetEvaluators(string language) => string.IsNullOrWhiteSpace(language) ? throw new ArgumentNullException(nameof(language)) : this.ServiceProvider.GetServices<IExpressionEvaluator>().Where(s => s.Supports(language));

}
