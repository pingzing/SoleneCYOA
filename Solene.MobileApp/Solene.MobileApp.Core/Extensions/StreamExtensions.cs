using Newtonsoft.Json;
using System.IO;

namespace Solene.MobileApp.Core.Extensions
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Serializes the given value to JSON, and then flushes it into this stream.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> that the serialized JSON data will be written to.</param>
        /// <param name="value">The value to serialize to JSON.</param>
        /// <param name="serializationSettings">Optional. A <see cref="JsonSerializerSettings>"/> that descrbes how the object should be serialized.</param>
        public static void SerializeJsonToStream(this Stream stream, object value, JsonSerializerSettings serializationSettings = null)
        {
            JsonSerializer ser = JsonSerializer.Create(serializationSettings);

            using (var writer = new StreamWriter(stream))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                ser.Serialize(jsonWriter, value);
                jsonWriter.Flush();
            }
        }

        /// <summary>
        /// Deserializes the contents of a stream of JSON text to an <see cref="object"/>.
        /// </summary>
        /// <param name="stream">The stream containing the JSON text.</param>
        /// <param name="serializationSettings">Optional. A <see cref="JsonSerializerSettings>"/> that descrbes how the object should be deserialized.</param>
        /// <returns>The JSON object deserialized to a .NET <see cref="object"/>.</returns>
        public static object DeserializeJsonFromStream(this Stream stream, JsonSerializerSettings serializationSettings = null)
        {
            JsonSerializer serializer = JsonSerializer.Create(serializationSettings);

            object deserializedValue;
            using (stream)
            {
                using (var sr = new StreamReader(stream))
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    deserializedValue = serializer.Deserialize(jsonTextReader);
                }

                return deserializedValue;
            }
        }

        /// <summary>
        /// Deserializes the contents of a stream of JSON text to a <see cref="{T}"/>.
        /// </summary>
        /// <typeparam name="T">The to deserialize the JSON object to.</typeparam>
        /// <param name="stream">The stream containing the JSON text.</param>
        /// <param name="serializationSettings">Optional. A <see cref="JsonSerializerSettings>"/> that descrbes how the object should be deserialized.</param>
        /// <returns></returns>
        public static T DeserializeJsonFromStream<T>(this Stream stream, JsonSerializerSettings serializationSettings = null)
        {
            JsonSerializer serializer = JsonSerializer.Create(serializationSettings);

            T deserializedValue;
            using (stream)
            {
                using (var sr = new StreamReader(stream))
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    deserializedValue = serializer.Deserialize<T>(jsonTextReader);
                }

                return deserializedValue;
            }
        }
    }
}
