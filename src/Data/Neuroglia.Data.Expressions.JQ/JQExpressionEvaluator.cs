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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neuroglia.Data.Expressions.JQ.Configuration;
using Neuroglia.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Diagnostics;
using System.Dynamic;
using System.Reactive.Joins;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Neuroglia.Data.Expressions.JQ
{

    /// <summary>
    /// Represents the default, JQ implementation of the <see cref="IExpressionEvaluator"/> interface
    /// </summary>
    public class JQExpressionEvaluator
        : IExpressionEvaluator
    {

        /// <summary>
        /// Initializes a new <see cref="JQExpressionEvaluator"/>
        /// </summary>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="options">The service used to access the current <see cref="JQExpressionEvaluatorOptions"/></param>
        /// <param name="serializerProvider">The service used to provide <see cref="ISerializer"/>s</param>
        public JQExpressionEvaluator(ILogger<JQExpressionEvaluator> logger, IOptions<JQExpressionEvaluatorOptions> options, ISerializerProvider serializerProvider)
        {
            this.Logger = logger;
            this.Options = options.Value;
            this.JsonSerializer = (IJsonSerializer)serializerProvider.GetSerializer(this.Options.SerializerType);
            if (this.JsonSerializer == null)
                throw new NullReferenceException($"Failed to find an {nameof(IJsonSerializer)} implementation of type '{this.Options.SerializerType}'");
        }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the service used to access the current <see cref="JQExpressionEvaluatorOptions"/>
        /// </summary>
        protected JQExpressionEvaluatorOptions Options { get; }

        /// <summary>
        /// Gets the service used to serialize and deserialize json
        /// </summary>
        protected IJsonSerializer JsonSerializer { get; }

        /// <inheritdoc/>
        public virtual bool Supports(string language)
        {
            if (string.IsNullOrWhiteSpace(language))
                throw new ArgumentNullException(nameof(language));
            return language.Equals("jq", StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public virtual object? Evaluate(string expression, object data, Type expectedType, IDictionary<string, object>? args = null)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentNullException(nameof(expression));
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            var serializerSettings = new JsonSerializerSettings() { ContractResolver = new DefaultContractResolver(), NullValueHandling = NullValueHandling.Ignore };
            var inputJson = JsonConvert.SerializeObject(data, serializerSettings);
            var serializedArgs = args?.ToDictionary(a => a.Key, a => JsonConvert.SerializeObject(JToken.FromObject(a.Value), Formatting.None, serializerSettings));
            var jsonArgs = string.Empty;
            var jqExpression = this.BuildJQExpression(expression);
            string fileName;
            string processArgs;
            var files = new List<string>();
            using Process process = new();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if(serializedArgs != null)
                    jsonArgs = string.Join(" ", serializedArgs.Select(a => @$"--argjson {a.Key} ""{this.EscapeDoubleQuotes(a.Value)}"""));
                fileName = "cmd.exe";
                processArgs = @$"/c echo {inputJson} | jq.exe ""{jqExpression}"" {jsonArgs}";
                if (processArgs.Length > 8000)
                {
                    var inputJsonFile = Path.GetTempFileName();
                    File.WriteAllText(inputJsonFile, inputJson);
                    files.Add(inputJsonFile);
                    var filterFile = Path.GetTempFileName();
                    jqExpression = this.BuildJQExpression(expression, false);
                    File.WriteAllText(filterFile, jqExpression);
                    files.Add(filterFile);
                    processArgs = @$"/c type {inputJsonFile} | jq.exe -f {filterFile}";
                    if(serializedArgs != null)
                    {
                        foreach (var arg in serializedArgs)
                        {
                            var argFile = Path.GetTempFileName();
                            File.WriteAllText(argFile, arg.Value);
                            files.Add(argFile);
                            processArgs += $" --argfile {arg.Key} {argFile}";
                        }
                    }
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (serializedArgs != null)
                    jsonArgs = string.Join(" ", serializedArgs.Select(a => @$"--argjson {a.Key} $'{this.EscapeDoubleQuotes(this.EscapeSingleQuotes(a.Value))}'"));
                fileName = "bash";
                processArgs = @$"-c ""echo '{this.EscapeDoubleQuotes(this.EscapeSingleQuotes(inputJson))}' | jq $'{this.EscapeDoubleQuotes(this.EscapeSingleQuotes(jqExpression))}' {jsonArgs}""";
                if (processArgs.Length > 200000)
                {
                    var inputJsonFile = Path.GetTempFileName();
                    File.WriteAllText(inputJsonFile, inputJson);
                    files.Add(inputJsonFile);
                    var filterFile = Path.GetTempFileName();
                    jqExpression = this.BuildJQExpression(expression, false);
                    File.WriteAllText(filterFile, jqExpression);
                    files.Add(filterFile);
                    processArgs = @$"-c ""cat {inputJsonFile} | jq -f {filterFile}";
                    if (serializedArgs != null)
                    {
                        foreach (var arg in serializedArgs)
                        {
                            var argFile = Path.GetTempFileName();
                            File.WriteAllText(argFile, arg.Value);
                            files.Add(argFile);
                            processArgs += $" --argfile {arg.Key} {argFile}";
                        }
                    }
                    processArgs += @"""";
                }
            }
            else
                throw new PlatformNotSupportedException();
            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = processArgs;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            var started = process.Start();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            foreach (var file in files)
            {
                try { File.Delete(file); } catch { }
            }
            if (process.ExitCode != 0)
            {
                this.Logger.LogError("An error occured while evaluting the specified expression: {error}", error);
                throw new Exception($"An error occured while evaluting the specified expression: {error}");
            }
            if (string.IsNullOrWhiteSpace(output))
                return null;
            else
                return this.JsonSerializer.Deserialize(output, expectedType); 
        }

        /// <inheritdoc/>
        public virtual T? Evaluate<T>(string expression, object data, IDictionary<string, object>? args = null)
        {
            return (T?)this.Evaluate(expression, data, typeof(T), args);
        }

        /// <inheritdoc/>
        public virtual object? Evaluate(string expression, object data, IDictionary<string, object>? args = null)
        {
            var result = this.Evaluate(expression, data, typeof(object), args);
            if (result is JToken jtoken)
                result =  jtoken.ToObject();
            return result;
        }

        /// <summary>
        /// Builds a jq compliant expression from the specified expression
        /// </summary>
        /// <param name="expression">The expression to build a jq compliant expression for</param>
        /// <param name="escape">A boolean indicating whether to escape '"' and '&' chgaracters in the resulting JQ expression</param>
        /// <returns>A new jq compliant expression built from the specified expression</returns>
        protected virtual string BuildJQExpression(string expression, bool escape = true)
        {
            var jqExpression = expression.Trim();
            if (jqExpression.StartsWith("${"))
                jqExpression = jqExpression[2..^1].Trim();
            if (!escape)
                return jqExpression;
            if (!jqExpression.Contains(@"\"""))
                jqExpression = jqExpression.Replace("\"", @"\""");
            if (!jqExpression.Contains("^&"))
                jqExpression = jqExpression.Replace("&", "^&");
            return jqExpression;
        }

        /// <summary>
        /// Escapes double quotes in the specified string
        /// </summary>
        /// <param name="input">The string for which to escape double quotes</param>
        /// <returns>The string with escaped double quotes</returns>
        protected virtual string EscapeDoubleQuotes(string input) => Regex.Replace(input, "([\"\\\\])", @"\$1", RegexOptions.Compiled);

        /// <summary>
        /// Escapes single quotes in the specified string
        /// </summary>
        /// <param name="input">The string for which to escape single quotes</param>
        /// <returns>The string with escaped single quotes</returns>
        protected virtual string EscapeSingleQuotes(string input)
        {
            if (!input.Contains("\\'"))
                input = input.Replace("'", "\\'");
            return input;
        }

    }

}
