using System.Security.Claims;
using System.Linq;

namespace ASC.WEB.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static (string Name, string Email) GetCurrentUserDetails(this ClaimsPrincipal user)
        {
            if (user == null) return (null, null);

            var name = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var email = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            return (name, email);
        }
    }
}
