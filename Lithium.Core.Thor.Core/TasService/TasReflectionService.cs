using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Thor.Core;
using UnityEngine;

namespace Lithium.Core.Thor.Core
{
    public class TasReflectionService : ITasReflectionService, ITasService
    {
        public bool Initialize()
        {
            return true;
        }

        public string Name => "ReflectionService";
        public float LoadProgress => 1f;

        public bool GetFieldValue<T>(object obj, string fieldName, out T value)
        {
            if (obj == null)
            {
                value = default;
                return false;
            }
            
            var type = obj.GetType();
            var field = type.GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
            
            if (field != null)
            {
                value = (T)field.GetValue(obj);
                return true;
            }

            value = default;
            return false;
        }

        public bool SetFieldValue<T>(object obj, string fieldName, T value)
        {
            if (obj == null)
                return false;
            
            var type = obj.GetType();
            var field = type.GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(obj, value);
                return true;
            }

            return false;
        }

        public bool GetFunctionDelegate<T>(object obj, string functionName, out T functionDelegate) where T : Delegate
        {
            if (obj == null)
            {
                functionDelegate = null;
                return false;
            }
            
            var type = obj.GetType();
            var method = type.GetMethod(functionName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                functionDelegate = (T)Delegate.CreateDelegate(typeof(T), obj, method);
                return true;
            }

            functionDelegate = null;
            return false;
        }

        public bool GetFunctionDelegate<T>(Type type, string functionName, out T functionDelegate) where T : Delegate
        {
            var method = type.GetMethod(functionName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (method != null)
            {
                functionDelegate = (T)Delegate.CreateDelegate(typeof(T), method);
                return true;
            }

            functionDelegate = null;
            return false;
        }

        public bool CopyFieldValues(object source, string fieldName, object destination, string destinationFieldName)
        {
            if (source == null || destination == null)
                return false;
            
            var sourceType = source.GetType();
            var sourceField = sourceType.GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (sourceField == null)
                return false;

            var destinationType = destination.GetType();
            var destinationField = destinationType.GetField(destinationFieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (destinationField == null)
                return false;

            var value = sourceField.GetValue(source);
            destinationField.SetValue(destination, value);
            return true;
        }

        public bool DeepCopyAssetReference<T>(T source, out T destination) where T : IAssetReference
        {
            destination = default;
            if (source == null)
                return false;

            try
            {
                var serialized = SerializeObject(source, maxDepth: 10);
                // Copy the serialized JSON into the destination object
                JObject jsonObject = JObject.Parse(serialized);
                destination = (T)jsonObject.DeepClone().ToObject(typeof(T));
            }
            catch (Exception e)
            {
                ServicesTas.Log.Log(
                    $"[{ServicesTas.TasReflection.GetType().Name}]: Failed to deep copy AssetReference of type {typeof(T).Name}. Exception: {e}");
                return false;
            }
            
            return true;
        }

        private class CustomJsonTextWriter : JsonTextWriter
        {
            public CustomJsonTextWriter(TextWriter textWriter) : base(textWriter)
            {
            }

            public int CurrentDepth { get; private set; }

            public override void WriteStartObject()
            {
                CurrentDepth++;
                base.WriteStartObject();
            }

            public override void WriteEndObject()
            {
                CurrentDepth--;
                base.WriteEndObject();
            }
        }

        private class AssetReferenceJsonConverter : JsonConverter
        {
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException("ReadJson is not implemented");
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType.IsGenericType &&
                       (objectType.GetGenericTypeDefinition() == typeof(AssetReference<>) ||
                        objectType.GetGenericTypeDefinition() == typeof(PrefabReference<>));
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var valueType = value as IAssetReference;
                // Assuming IAssetReference has string guid and int guidHash properties
                if (!ServicesTas.TasReflection.GetFieldValue<string>(valueType, "guid", out var guidProperty))
                {
                    throw new InvalidOperationException(
                        $"[{ServicesTas.TasReflection.GetType().Name}]: Failed to get 'guid' field value for JSON serialization.");
                }

                if (!ServicesTas.TasReflection.GetFieldValue<int>(valueType, "guidHash", out var guidHashProperty))
                {
                    throw new InvalidOperationException(
                        $"[{ServicesTas.TasReflection.GetType().Name}]: Failed to get 'guidHash' field value for JSON serialization.");
                }

                writer.WriteStartObject();
                writer.WritePropertyName("guid");
                writer.WriteValue(guidProperty);
                writer.WritePropertyName("guidHash");
                writer.WriteValue(guidHashProperty);
                writer.WriteEndObject();
            }
        }

        private class CustomContractResolver : DefaultContractResolver
        {
            private readonly Func<bool> _includeProperty;

            public CustomContractResolver(Func<bool> includeProperty)
            {
                _includeProperty = includeProperty;
            }

            protected override JsonProperty CreateProperty(
                MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);
                var shouldSerialize = property.ShouldSerialize;
                property.ShouldSerialize = obj => _includeProperty() &&
                                                  (shouldSerialize == null ||
                                                   shouldSerialize(obj));
                return property;
            }
        }

        private static string SerializeObject(object obj, int maxDepth)
        {
            using (var strWriter = new StringWriter())
            {
                using (var jsonWriter = new CustomJsonTextWriter(strWriter))
                {
                    bool Include() => jsonWriter.CurrentDepth <= maxDepth;
                    var resolver = new CustomContractResolver(Include);
                    var serializer = new JsonSerializer
                    {
                        ContractResolver = resolver,
                        Formatting = Formatting.Indented,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };
                    serializer.Converters.Add(new AssetReferenceJsonConverter()); // Add the custom converter
                    serializer.Serialize(jsonWriter, obj);
                }

                return strWriter.ToString();
            }
        }
        
        // IService
        public IEnumerator InitializeAsync()
        {
            yield return null;
        }

        public void CollectDebugState(Dictionary<string, object> debugStateProperties)
        {
        }

        public void Shutdown()
        {
        }
    }
}