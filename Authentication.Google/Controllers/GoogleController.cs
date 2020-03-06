using System.Threading.Tasks;
using EvenCart.Infrastructure.Mvc;
using EvenCart.Infrastructure.Mvc.Attributes;
using EvenCart.Infrastructure.Routing;
using Microsoft.AspNetCore.Mvc;
using Authentication.Google.Helpers;
using EvenCart.Core;
using EvenCart.Data.Extensions;
using EvenCart.Infrastructure;
using EvenCart.Infrastructure.Social;
using Microsoft.AspNetCore.Authentication;

namespace Authentication.Google.Controllers
{
    [Route("authentication")]
    [PluginType(PluginType = typeof(GoogleAuthPlugin))]
    public class GoogleController : FoundationPluginController
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
                RedirectUri = ApplicationEngine.RouteUrl(GoogleConfig.GoogleRedirectUrl, new { returnUrl })
            };
            return Challenge(authenticationProperties,  GoogleConfig.GoogleAuthenticationScheme);
        }

        [HttpGet("google/redirect", Name = GoogleConfig.GoogleRedirectUrl)]
        public async Task<IActionResult> HandleRedirect(string returnUrl)
        {
            if (returnUrl.IsNullEmptyOrWhiteSpace())
                returnUrl = ApplicationEngine.RouteUrl(RouteNames.Home);
            var request = await GoogleHelper.CreateConnectedAccountRequestAsync();
            if (_connectionAccountant.Connect(request))
                return Redirect(returnUrl);
            return RedirectToRoute(RouteNames.Login);
        }
    }
}