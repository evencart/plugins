using System.Threading.Tasks;
using EvenCart.Infrastructure.Mvc;
using EvenCart.Infrastructure.Mvc.Attributes;
using EvenCart.Infrastructure.Routing;
using Microsoft.AspNetCore.Mvc;
using Authentication.Facebook.Helpers;
using EvenCart.Core;
using EvenCart.Data.Extensions;
using EvenCart.Infrastructure;
using EvenCart.Infrastructure.Social;
using Microsoft.AspNetCore.Authentication;

namespace Authentication.Facebook.Controllers
{
    [Route("authentication")]
    [PluginType(PluginType = typeof(FacebookAuthPlugin))]
    public class FacebookController : FoundationPluginController
    {
        private readonly IConnectionAccountant _connectionAccountant;
        public FacebookController(IConnectionAccountant connectionAccountant)
        {
            _connectionAccountant = connectionAccountant;
        }

        [HttpGet("facebook/login", Name = FacebookConfig.FacebookLoginRouteName)]
        public IActionResult Login(bool connectOnly = false)
        {
            var returnUrl = WebHelper.GetReferrerUrl();
            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = ApplicationEngine.RouteUrl(FacebookConfig.FacebookRedirectUrl, new { returnUrl })
            };
            return Challenge(authenticationProperties,  FacebookConfig.FacebookAuthenticationScheme);
        }

        [HttpGet("facebook/redirect", Name = FacebookConfig.FacebookRedirectUrl)]
        public async Task<IActionResult> HandleRedirect(string returnUrl)
        {
            if (returnUrl.IsNullEmptyOrWhiteSpace())
                returnUrl = ApplicationEngine.RouteUrl(RouteNames.Home);
            var request = await FacebookHelper.CreateConnectedAccountRequestAsync();
            if (_connectionAccountant.Connect(request))
                return Redirect(returnUrl);
            return RedirectToRoute(RouteNames.Login);
        }
    }
}