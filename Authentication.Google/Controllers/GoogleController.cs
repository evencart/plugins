using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Authentication.Google.Helpers;
using Genesis;
using Genesis.Extensions;
using Genesis.Infrastructure.Mvc;
using Genesis.Infrastructure.Mvc.Attributes;
using Genesis.Modules.Users;
using Genesis.Routing;
using Microsoft.AspNetCore.Authentication;

namespace Authentication.Google.Controllers
{
    [Route("authentication")]
    [PluginType(PluginType = typeof(GoogleAuthPlugin))]
    public class GoogleController : GenesisPluginController
    {
        private readonly IConnectionAccountant _connectionAccountant;
        public GoogleController(IConnectionAccountant connectionAccountant)
        {
            _connectionAccountant = connectionAccountant;
        }

        [HttpGet("google/login", Name = GoogleConfig.GoogleLoginRouteName)]
        public IActionResult Login(bool connectOnly = false)
        {
            var returnUrl = WebHelper.GetReferrerUrl();
            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = GenesisEngine.Instance.RouteUrl(GoogleConfig.GoogleRedirectUrl, new { returnUrl })
            };
            return Challenge(authenticationProperties,  GoogleConfig.GoogleAuthenticationScheme);
        }

        [HttpGet("google/redirect", Name = GoogleConfig.GoogleRedirectUrl)]
        public async Task<IActionResult> HandleRedirect(string returnUrl)
        {
            if (returnUrl.IsNullEmptyOrWhiteSpace())
                returnUrl = GenesisEngine.Instance.RouteUrl(RouteNames.Home);
            var request = await GoogleHelper.CreateConnectedAccountRequestAsync();
            if (_connectionAccountant.Connect(request))
                return Redirect(returnUrl);
            return RedirectToRoute(RouteNames.Login);
        }
    }
}