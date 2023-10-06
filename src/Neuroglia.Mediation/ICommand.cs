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
namespace Neuroglia.Mediation
{

    /// <summary>
    /// Defines the fundamentals of a CQRS command
    /// </summary>
    public interface ICommand
    {

        /// <summary>
        /// Gets a key/value mapping of the <see cref="ICommand"/>'s context data
        /// </summary>
        IDictionary<string, object> ContextData { get; }

    }

    /// <summary>
    /// Defines the fundamentals of a CQRS command 
    /// </summary>
    /// <typeparam name="TResult">The expected <see cref="IOperationResult"/> type</typeparam>
    public interface ICommand<TResult>
        : ICommand, IRequest<TResult>
        where TResult : IOperationResult
    {



    }

    /// <summary>
    /// Defines the fundamentals of a command
    /// </summary>
    /// <typeparam name="TResult">The expected <see cref="IOperationResult"/> type</typeparam>
    /// <typeparam name="T">The type of data wrapped by the <see cref="IOperationResult"/></typeparam>
    public interface ICommand<TResult, T>
        : ICommand<TResult>
        where TResult : IOperationResult<T>
    {




    }

}
