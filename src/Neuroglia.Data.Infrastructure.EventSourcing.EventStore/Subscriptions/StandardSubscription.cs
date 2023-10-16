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

using EventStore.Client;

namespace Neuroglia.Data.Infrastructure.EventSourcing.EventStore.Subscriptions;

/// <summary>
/// Represents a standard <see cref="EventStoreSubscription"/>
/// </summary>
public class StandardSubscription
    : EventStoreSubscription
{

    /// <summary>
    /// Initializes a new <see cref="StandardSubscription"/>
    /// </summary>
    /// <param name="id">The <see cref="StandardSubscription"/>'s id</param>
    /// <param name="source">The underlying <see cref="StreamSubscription"/></param>
    public StandardSubscription(string id, object source) : base(id, source) { }

    /// <summary>
    /// Gets the underlying <see cref="StreamSubscription"/>
    /// </summary>
    protected new StreamSubscription Source => (StreamSubscription)base.Source;

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.Source?.Dispose();
            base.Source = null!;
        }
        base.Dispose(disposing);
    }

}
