/*
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
using Newtonsoft.Json.Linq;
using System;
using System.Dynamic;
using System.Linq;

namespace Newtonsoft.Json
{

    /// <summary>
    /// Represents a <see cref="JsonConverter"/> used to deserialize dynamic values
    /// </summary>
    public class DynamicValueConverter
        : JsonConverter
    {

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = JToken.Load(reader).ToObject(objectType);
            if (value is JObject
                || value is JArray)
                value = ((JToken)value).ToObject();
            return value;
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    
    }

    /// <summary>
    /// Defines extensions for <see cref="JToken"/>s
    /// </summary>
    public static class JTokenExtensions
    {

        /// <summary>
        /// Converts the specified <see cref="JToken"/> into a new object
        /// </summary>
        /// <param name="token">The <see cref="JToken"/> to convert</param>
        /// <returns>The converted object</returns>
        public static object ToObject(this JToken token)
        {
            return token switch
            {
                JObject => token.ToObject<ExpandoObject>(),
                JArray array => array.Select(t => t.ToObject()).ToList(),
                _ => token.ToObject<object>()
            };
        }

    }

}
