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

namespace Neuroglia.Data.Infrastructure.EventSourcing;

/// <summary>
/// Defines the fundamentals of an object used to describe a recorded event
/// </summary>
public interface IEventRecord
{

    /// <summary>
    /// Gets the id of the recorded event
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the offset of the recorded event
    /// </summary>
    ulong Offset { get; }

    /// <summary>
    /// Gets the date and time at which the event has been recorded
    /// </summary>
    DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets the type of the recorded event. Should be a non-versioned reverse uri made out alphanumeric, '-' and '.' characters
    /// </summary>
    string Type { get; }

    /// <summary>
    /// Gets the data of the recorded event
    /// </summary>
    object? Data { get; }

    /// <summary>
    /// Gets the metadata of the recorded event
    /// </summary>
    IDictionary<string, object>? Metadata { get; }

}
