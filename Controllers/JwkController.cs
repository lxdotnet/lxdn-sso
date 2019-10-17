
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Lxdn.Sso.Models;

namespace Lxdn.Sso.Controllers
{
    /// <summary>
    /// Helps generate a Json Web Key (JWK) containing private and public keys needed for token protection
    /// </summary>
    [Route("jwk")]
    [ApiController]
    public class JwkController : ControllerBase
    {
        [HttpPost]
        public JwkModel GenerateJwk([FromBody] JwkRequest request)
        {
            var rsa = new RSACryptoServiceProvider(1024);
            var parameters = rsa.ExportParameters(true); // include private key

            var jwk = new JwkModel
            {
                KeyType = "RSA",
                Algorithm = "RS256", // https://stackoverflow.com/questions/39239051/rs256-vs-hs256-whats-the-difference
                //KeyId = "lxdn",
                Use = "sig", // for verifying signatures

                D = parameters.D, // injection won't work 'cause these are fields, not properties
                DP = parameters.DP,
                DQ = parameters.DQ,
                P = parameters.P,
                Q = parameters.Q,
                QI = parameters.InverseQ,
                E = parameters.Exponent,
                N = parameters.Modulus
            };

            return jwk;
        }
    }
}
