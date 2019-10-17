using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;

namespace Lxdn.Sso.Serialization
{
    public class Base64UrlEncoderConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => typeof(byte[]) == objectType;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => writer.WriteValue(Base64UrlEncoder.Encode((byte[])value));
    }
}
