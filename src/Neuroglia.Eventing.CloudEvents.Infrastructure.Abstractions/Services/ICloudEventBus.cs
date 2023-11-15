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

using Neuroglia.CloudEvents;
using System.Reactive.Subjects;

namespace Neuroglia.Eventing.CloudEvents.Infrastructure.Services;

/// <summary>
/// Defines the fundamentals of a service used to manage incoming and outgoing streams of <see cref="CloudEvent"/>s
/// </summary>
public interface ICloudEventBus
    : IDisposable
{

    /// <summary>
    /// Gets the stream of events ingested by the application
    /// </summary>
    ISubject<CloudEvent> InputStream { get; }

    /// <summary>
    /// Gets the stream of events published by the application
    /// </summary>
    ISubject<CloudEvent> OutputStream { get; }

}