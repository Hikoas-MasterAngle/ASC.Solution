using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace ASC.WEB.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        protected string CurrentUserName
        {
            get
            {
                return User?.Identity?.Name ?? "Unknown";
            }
        }

        protected void SetSession<T>(string key, T value)
        {
            HttpContext.Session.SetString(key, System.Text.Json.JsonSerializer.Serialize(value));
        }

        protected T GetSession<T>(string key)
        {
            var data = HttpContext.Session.GetString(key);
            return string.IsNullOrEmpty(data) ? default(T) : System.Text.Json.JsonSerializer.Deserialize<T>(data);
        }

        protected void SetTempMessage(string message)
        {
            TempData["Message"] = message;
        }
    }
}
