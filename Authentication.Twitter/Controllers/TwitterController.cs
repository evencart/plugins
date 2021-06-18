using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Authentication.Twitter.Helpers;
using Genesis;
using Genesis.Extensions;
using Genesis.Infrastructure.Mvc;
using Genesis.Infrastructure.Mvc.Attributes;
using Genesis.Modules.Users;
using Genesis.Routing;
using Microsoft.AspNetCore.Authentication;

namespace Authentication.Twitter.Controllers
{
    [Route("authentication")]
    [PluginType(PluginType = typeof(TwitterAuthPlugin))]
    public class TwitterController : GenesisPluginController
    {
        private readonly IConnectionAccountant _connectionAccountant;
        public TwitterController(IConnectionAccountant connectionAccountant)
        {
            _connectionAccountant = connectionAccountant;
        }

        [HttpGet("twitter/login", Name = TwitterConfig.TwitterLoginRouteName)]
        public IActionResult Login(bool connectOnly = false)
        {
            var returnUrl = WebHelper.GetReferrerUrl();
            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = GenesisEngine.Instance.RouteUrl(TwitterConfig.TwitterRedirectUrl, new { returnUrl })
            };
            return Challenge(authenticationProperties,  TwitterConfig.TwitterAuthenticationScheme);
        }

        [HttpGet("twitter/redirect", Name = TwitterConfig.TwitterRedirectUrl)]
        public async Task<IActionResult> HandleRedirect(string returnUrl)
        {
            if (returnUrl.IsNullEmptyOrWhiteSpace())
                returnUrl = GenesisEngine.Instance.RouteUrl(RouteNames.Home);
            var request = await TwitterHelper.CreateConnectedAccountRequestAsync();
            if (_connectionAccountant.Connect(request))
                return Redirect(returnUrl);
            return RedirectToRoute(RouteNames.Login);
        }
    }
}