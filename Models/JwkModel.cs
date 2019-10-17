using Lxdn.Sso.Serialization;
using Newtonsoft.Json;

namespace Lxdn.Sso.Models
{
    public class JwkModel
    {
        [JsonProperty("kty")]
        public string KeyType { get; set; }

        [JsonProperty("d"), JsonConverter(typeof(Base64UrlEncoderConverter))]
        public byte[] D  { get; set; } // private key

        [JsonProperty("dp"), JsonConverter(typeof(Base64UrlEncoderConverter))]
        public byte[] DP { get; set; }

        [JsonProperty("dq"), JsonConverter(typeof(Base64UrlEncoderConverter))]
        public byte[] DQ { get; set; }

        [JsonProperty("e"), JsonConverter(typeof(Base64UrlEncoderConverter))]
        public byte[] E { get; set; } // exponent

        [JsonProperty("use")]
        public string Use { get; set; }

        [JsonProperty("kid")]
        public string KeyId { get; set; }

        [JsonProperty("alg")]
        public string Algorithm { get; set; }

        [JsonProperty("n"), JsonConverter(typeof(Base64UrlEncoderConverter))]
        public byte[] N { get; set; } // modulus

        [JsonProperty("p"), JsonConverter(typeof(Base64UrlEncoderConverter))]
        public byte[] P { get; set; }

        [JsonProperty("q"), JsonConverter(typeof(Base64UrlEncoderConverter))]
        public byte[] Q { get; set; }

        [JsonProperty("qi"), JsonConverter(typeof(Base64UrlEncoderConverter))]
        public byte[] QI { get; set; } // inverse q
    }
}
