﻿/*
 * Copyright © 2021 Neuroglia SPRL. All rights reserved.
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */
using System.Threading.Tasks;

namespace Neuroglia.Mediation
{

    /// <summary>
    /// Represents an async continuation for the next task to execute in a <see cref="IRequestHandler{TRequest, TResult}"/> pipeline
    /// </summary>
    /// <typeparam name="TResult">The type of expected <see cref="IOperationResult"/></typeparam>
    /// <returns>The resulting <see cref="IOperationResult"/></returns>
    public delegate Task<TResult> RequestHandlerDelegate<TResult>()
        where TResult : IOperationResult;

}
