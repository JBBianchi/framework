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

namespace Neuroglia.Mediation;


/// <summary>
/// Defines the fundamentals of a service used to surround an inner <see cref="IRequestHandler{TRequest, TResult}"/> and to provide additional behavior before awaiting the next <see cref="RequestHandlerDelegate{TResult}"/>
/// </summary>
/// <typeparam name="TRequest">The type of </typeparam>
/// <typeparam name="TResult"></typeparam>
public interface IMiddleware<TRequest, TResult>
    where TRequest : IRequest<TResult>
    where TResult : IOperationResult
{

    /// <summary>
    /// Performs additional behavior and awaits the next <see cref="RequestHandlerDelegate{TResult}"/> in the pipeline
    /// </summary>
    /// <param name="request">The <see cref="IRequest"/> to handle</param>
    /// <param name="next">The next <see cref="RequestHandlerDelegate{TResult}"/> in the pipeline</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The resulting <see cref="IOperationResult"/></returns>
    Task<TResult> HandleAsync(TRequest request, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken = default);

}
