using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;

namespace ewms_common.Authentications
{
    public static class DynamicAuth
    {
        public const string AuthenticationScheme = "EWMS";

        public static string OnForwardDefault(HttpContext context)
        {
            return context.Request.Headers.ContainsKey("Authorization")
                ? JwtBearerDefaults.AuthenticationScheme
                : CookieAuthenticationDefaults.AuthenticationScheme;
        }
    }
}
