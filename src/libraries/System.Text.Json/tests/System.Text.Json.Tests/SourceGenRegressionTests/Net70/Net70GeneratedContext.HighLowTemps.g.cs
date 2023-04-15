﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Source files represent a source generated JsonSerializerContext as produced by the .NET 7 SDK.
// Used to validate correctness of contexts generated by previous SDKs against the current System.Text.Json runtime components.
// Unless absolutely necessary DO NOT MODIFY any of these files -- it would invalidate the purpose of the regression tests.

// <auto-generated/>

#nullable enable annotations
#nullable disable warnings

// Suppress warnings about [Obsolete] member usage in generated code.
#pragma warning disable CS0618

namespace System.Text.Json.Tests.SourceGenRegressionTests.Net70
{
    public partial class Net70GeneratedContext
    {
        private global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::System.Text.Json.Tests.SourceGenRegressionTests.Net70.HighLowTemps>? _HighLowTemps;
        /// <summary>
        /// Defines the source generated JSON serialization contract metadata for a given type.
        /// </summary>
        public global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::System.Text.Json.Tests.SourceGenRegressionTests.Net70.HighLowTemps> HighLowTemps
        {
            get => _HighLowTemps ??= Create_HighLowTemps(Options, makeReadOnly: true);
        }
        
        private global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::System.Text.Json.Tests.SourceGenRegressionTests.Net70.HighLowTemps> Create_HighLowTemps(global::System.Text.Json.JsonSerializerOptions options, bool makeReadOnly)
        {
            global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::System.Text.Json.Tests.SourceGenRegressionTests.Net70.HighLowTemps>? jsonTypeInfo = null;
            global::System.Text.Json.Serialization.JsonConverter? customConverter;
            if (options.Converters.Count > 0 && (customConverter = GetRuntimeProvidedCustomConverter(options, typeof(global::System.Text.Json.Tests.SourceGenRegressionTests.Net70.HighLowTemps))) != null)
            {
                jsonTypeInfo = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreateValueInfo<global::System.Text.Json.Tests.SourceGenRegressionTests.Net70.HighLowTemps>(options, customConverter);
            }
            else
            {
                global::System.Text.Json.Serialization.Metadata.JsonObjectInfoValues<global::System.Text.Json.Tests.SourceGenRegressionTests.Net70.HighLowTemps> objectInfo = new global::System.Text.Json.Serialization.Metadata.JsonObjectInfoValues<global::System.Text.Json.Tests.SourceGenRegressionTests.Net70.HighLowTemps>()
                {
                    ObjectCreator = static () => new global::System.Text.Json.Tests.SourceGenRegressionTests.Net70.HighLowTemps(),
                    ObjectWithParameterizedConstructorCreator = null,
                    PropertyMetadataInitializer = _ => HighLowTempsPropInit(options),
                    ConstructorParameterMetadataInitializer = null,
                    NumberHandling = default,
                    SerializeHandler = HighLowTempsSerializeHandler
                };
        
                jsonTypeInfo = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreateObjectInfo<global::System.Text.Json.Tests.SourceGenRegressionTests.Net70.HighLowTemps>(options, objectInfo);
            }
        
            if (makeReadOnly)
            {
                jsonTypeInfo.MakeReadOnly();
            }
        
            return jsonTypeInfo;
        }
        
        private static global::System.Text.Json.Serialization.Metadata.JsonPropertyInfo[] HighLowTempsPropInit(global::System.Text.Json.JsonSerializerOptions options)
        {
            global::System.Text.Json.Serialization.Metadata.JsonPropertyInfo[] properties = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfo[2];
        
            global::System.Text.Json.Serialization.Metadata.JsonPropertyInfoValues<global::System.Int32> info0 = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfoValues<global::System.Int32>()
            {
                IsProperty = true,
                IsPublic = true,
                IsVirtual = false,
                DeclaringType = typeof(global::System.Text.Json.Tests.SourceGenRegressionTests.Net70.HighLowTemps),
                Converter = null,
                Getter = static (obj) => ((global::System.Text.Json.Tests.SourceGenRegressionTests.Net70.HighLowTemps)obj).High,
                Setter = static (obj, value) => ((global::System.Text.Json.Tests.SourceGenRegressionTests.Net70.HighLowTemps)obj).High = value!,
                IgnoreCondition = null,
                HasJsonInclude = false,
                IsExtensionData = false,
                NumberHandling = default,
                PropertyName = "High",
                JsonPropertyName = null
            };
        
            global::System.Text.Json.Serialization.Metadata.JsonPropertyInfo propertyInfo0 = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreatePropertyInfo<global::System.Int32>(options, info0);
            properties[0] = propertyInfo0;
        
            global::System.Text.Json.Serialization.Metadata.JsonPropertyInfoValues<global::System.Int32> info1 = new global::System.Text.Json.Serialization.Metadata.JsonPropertyInfoValues<global::System.Int32>()
            {
                IsProperty = true,
                IsPublic = true,
                IsVirtual = false,
                DeclaringType = typeof(global::System.Text.Json.Tests.SourceGenRegressionTests.Net70.HighLowTemps),
                Converter = null,
                Getter = static (obj) => ((global::System.Text.Json.Tests.SourceGenRegressionTests.Net70.HighLowTemps)obj).Low,
                Setter = static (obj, value) => ((global::System.Text.Json.Tests.SourceGenRegressionTests.Net70.HighLowTemps)obj).Low = value!,
                IgnoreCondition = null,
                HasJsonInclude = false,
                IsExtensionData = false,
                NumberHandling = default,
                PropertyName = "Low",
                JsonPropertyName = null
            };
        
            global::System.Text.Json.Serialization.Metadata.JsonPropertyInfo propertyInfo1 = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreatePropertyInfo<global::System.Int32>(options, info1);
            properties[1] = propertyInfo1;
        
            return properties;
        }
        
        // Intentionally not a static method because we create a delegate to it. Invoking delegates to instance
        // methods is almost as fast as virtual calls. Static methods need to go through a shuffle thunk.
        private void HighLowTempsSerializeHandler(global::System.Text.Json.Utf8JsonWriter writer, global::System.Text.Json.Tests.SourceGenRegressionTests.Net70.HighLowTemps? value)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }
        
            writer.WriteStartObject();
            writer.WriteNumber(PropName_High, value.High);
            writer.WriteNumber(PropName_Low, value.Low);
        
            writer.WriteEndObject();
        }
    }
}
