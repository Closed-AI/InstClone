using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace API.Configs
{
    public class AuthConfig
    {
        public static string Position = "Auth";

        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public int LifeTime { get; set; }

        public SymmetricSecurityKey GetSymmetricSecurityKey()
            => new(Encoding.UTF8.GetBytes(Key));
    }
}
