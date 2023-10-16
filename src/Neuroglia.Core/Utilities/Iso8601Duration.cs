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

using System.Xml;

namespace Neuroglia;

/// <summary>
/// Represents an helper class for handling ISO 8601 durations
/// </summary>
public static partial class Iso8601Duration
{

    /// <summary>
    /// Parses the specified input into a new <see cref="TimeSpan"/>
    /// </summary>
    /// <param name="input">The input string to parse</param>
    /// <returns>The parsed <see cref="TimeSpan"/></returns>
    public static TimeSpan Parse(string input) => XmlConvert.ToTimeSpan(input);

    /// <summary>
    /// Formats the specified <see cref="TimeSpan"/>
    /// </summary>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> to format</param>
    /// <returns>The parsed <see cref="TimeSpan"/></returns>
    public static string Format(TimeSpan timeSpan) => XmlConvert.ToString(timeSpan);

}