using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebSockets.Helpers
{
    public static class JwtHelper
    {
        /// <summary>
        /// Reads the JWT token from Authorization header or query string.
        /// </summary>
        public static string GetToken(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var token = authHeader.FirstOrDefault()?.Split(" ").Last();
                if (!string.IsNullOrWhiteSpace(token))
                    return token;
            }

            var accessToken = context.Request.Query["access_token"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(accessToken))
                return accessToken;

            return null;
        }

        /// <summary>
        /// Validates the JWT using the provided key, issuer, and audience.
        /// Returns ClaimsPrincipal if valid, null otherwise.
        /// </summary>
        public static ClaimsPrincipal ValidateJwt(
            string token,
            string secretKey,
            string[] validAudiences)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = true,
                ValidAudiences = validAudiences,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a claim value by type from a ClaimsPrincipal.
        /// </summary>
        public static string GetClaimValue(ClaimsPrincipal principal, string claimType)
        {
            return principal?.FindFirst(claimType)?.Value;
        }
    }
}
