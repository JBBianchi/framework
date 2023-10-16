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
/// Represents the default implementation of the <see cref="IQuery{TResult}"/> interface
/// </summary>
/// <typeparam name="T">The type of result wrapped by the resulting <see cref="IOperationResult{T}"/></typeparam>
public abstract class Query<T>
   : IQuery<IOperationResult<T>, T>
{



}
