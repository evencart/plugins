using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Authentication.Facebook.Helpers;
using Genesis;
using Genesis.Extensions;
using Genesis.Infrastructure.Mvc;
using Genesis.Infrastructure.Mvc.Attributes;
using Genesis.Modules.Users;
using Genesis.Routing;
using Microsoft.AspNetCore.Authentication;

namespace Authentication.Facebook.Controllers
{
    [Route("authentication")]
    [PluginType(PluginType = typeof(FacebookAuthPlugin))]
    public class FacebookController : GenesisPluginController
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
                RedirectUri = GenesisEngine.Instance.RouteUrl(FacebookConfig.FacebookRedirectUrl, new { returnUrl })
            };
            return Challenge(authenticationProperties,  FacebookConfig.FacebookAuthenticationScheme);
        }

        [HttpGet("facebook/redirect", Name = FacebookConfig.FacebookRedirectUrl)]
        public async Task<IActionResult> HandleRedirect(string returnUrl)
        {
            if (returnUrl.IsNullEmptyOrWhiteSpace())
                returnUrl = GenesisEngine.Instance.RouteUrl(RouteNames.Home);
            var request = await FacebookHelper.CreateConnectedAccountRequestAsync();
            if (_connectionAccountant.Connect(request))
                return Redirect(returnUrl);
            return RedirectToRoute(RouteNames.Login);
        }
    }
}