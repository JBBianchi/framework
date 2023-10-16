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

namespace Neuroglia;

/// <summary>
/// Defines the fundamentals of an object that is uniquely identifiable
/// </summary>
public interface IIdentifiable
{

    /// <summary>
    /// Gets the object's unique identifier
    /// </summary>
    object Id { get; }

}

/// <summary>
/// Defines the fundamentals of an object that is uniquely identifiable
/// </summary>
/// <typeparam name="TKey">The type of key used to uniquely identify the object</typeparam>
public interface IIdentifiable<TKey>
    : IIdentifiable, IEquatable<IIdentifiable<TKey>>
    where TKey : IEquatable<TKey>
{

    /// <summary>
    /// Gets the object's unique identifier
    /// </summary>
    new TKey Id { get; }

}
