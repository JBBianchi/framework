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

namespace Neuroglia.Json.Schema.Generation;

/// <summary>
/// Represents the <see cref="ISchemaGenerator"/> used to handle <see cref="DateTimeOffset"/>s
/// </summary>
public class DateTimeOffsetSchemaGenerator
    : ISchemaGenerator
{

    /// <inheritdoc/>
    public virtual void AddConstraints(SchemaGenerationContextBase context)
    {
        context.Intents.Add(new TypeIntent(SchemaValueType.String));
        context.Intents.Add(new FormatIntent(new("date-time")));
    }

    /// <inheritdoc/>
    public virtual bool Handles(Type type) => type == typeof(DateTimeOffset);

}
