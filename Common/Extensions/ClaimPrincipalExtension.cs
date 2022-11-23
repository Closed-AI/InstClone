using System.Security.Claims;

namespace Common.Extentions
{
    public static class ClaimPrincipalExt
    {
        public static T? GetClaimValue<T>(this ClaimsPrincipal user, string claim)
        {
            var value = user.Claims.FirstOrDefault(x => x.Type == claim)?.Value;
            if (value != null)
                return Utils.Convert<T>(value);
            else
                return default;
        }
    }
}