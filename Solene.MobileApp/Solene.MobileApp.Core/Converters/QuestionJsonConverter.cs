using Newtonsoft.Json;
using Solene.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Solene.MobileApp.Core.Converters
{
    public class QuestionJsonConverter : JsonConverter
    {
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Question);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {            
            if (reader.TokenType == JsonToken.StartObject)
            {
                existingValue = existingValue ?? serializer.ContractResolver.ResolveContract(objectType).DefaultCreator();
                serializer.Populate(reader, existingValue);
                if (existingValue is Question questionValue)
                {
                    questionValue.Text = questionValue.Text.Replace("\\n", Environment.NewLine);
                    return questionValue;
                }
                return existingValue;
            }
            else if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else
            {
                throw new JsonSerializationException();
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
