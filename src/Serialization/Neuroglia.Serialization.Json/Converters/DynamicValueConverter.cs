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
using System.Collections.Generic;
using System.Dynamic;

namespace System.Text.Json.Serialization
{

    /// <summary>
    /// Represents a <see cref="JsonConverter"/> used to convert objects from/into JSON
    /// </summary>
    public class DynamicValueConverter
        : JsonConverter<object>
    {

        /// <inheritdoc/>
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.StartObject => this.ReadObject(ref reader, options),
                JsonTokenType.StartArray => this.ReadArray(ref reader, options),
                _ => this.ReadValue(ref reader, options),
            };
        }

        /// <summary>
        /// Reads a complex object from the current token
        /// </summary>
        /// <param name="reader">The <see cref="Utf8JsonReader"/> used to read the current token</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> to use</param>
        /// <returns>The object that has been read</returns>
        protected virtual object ReadObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException($"The specified token is not of the expected '{nameof(JsonTokenType.StartObject)}' type");
            var obj = new ExpandoObject();
            var dictionary = (IDictionary<string, object>)obj;
            while (reader.Read()
                && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException($"The specified token is not of the expected '{nameof(JsonTokenType.PropertyName)}' type");
                var propertyName = reader.GetString();
                if (string.IsNullOrWhiteSpace(propertyName))
                    throw new JsonException($"The property name cannot be null or empty");
                reader.Read();
                dictionary.Add(propertyName, this.ReadValue(ref reader, options));
            }
            return obj;
        }

        /// <summary>
        /// Reads an array from the current token
        /// </summary>
        /// <param name="reader">The <see cref="Utf8JsonReader"/> used to read the current token</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> to use</param>
        /// <returns>The array that has been read</returns>
        protected virtual object ReadArray(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException($"The specified token is not of the expected '{nameof(JsonTokenType.StartArray)}' type");
            var array = new List<object>();
            while (reader.Read()
                && reader.TokenType != JsonTokenType.EndArray)
            {
                array.Add(this.ReadValue(ref reader, options));
            }
            return array;
        }

        /// <summary>
        /// Reads a primitive value from the current token
        /// </summary>
        /// <param name="reader">The <see cref="Utf8JsonReader"/> used to read the current token</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> to use</param>
        /// <returns>The primitive value that has been read</returns>
        protected virtual object ReadValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    if (reader.TryGetDateTimeOffset(out var dateTimeOffset))
                        return dateTimeOffset;
                    if (reader.TryGetDateTime(out var dateTime))
                        return dateTime;
                    return reader.GetString()!;
                case JsonTokenType.False:
                    return false;
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.Null:
                    return null!;
                case JsonTokenType.Number:
                    if (reader.TryGetInt64(out var result))
                        return result;
                    return reader.GetDecimal();
                case JsonTokenType.StartObject:
                    return this.ReadObject(ref reader, options);
                case JsonTokenType.StartArray:
                    return this.ReadArray(ref reader, options);
                default:
                    throw new JsonException($"'{reader.TokenType}' is not supported");

            }
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            System.Text.Json.JsonSerializer.Serialize(writer, value, options);
        }

    }

}
