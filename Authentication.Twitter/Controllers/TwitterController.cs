using System;
using System.Threading.Tasks;
using EvenCart.Services.Payments;
using EvenCart.Services.Purchases;
using EvenCart.Infrastructure.Mvc;
using EvenCart.Infrastructure.Mvc.Attributes;
using EvenCart.Infrastructure.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Authentication.Twitter.Helpers;
using Authentication.Twitter.Models;
using EvenCart.Core;
using EvenCart.Data.Extensions;
using EvenCart.Infrastructure;
using EvenCart.Infrastructure.Social;
using Microsoft.AspNetCore.Authentication;

namespace Authentication.Twitter.Controllers
{
    [Route("authentication")]
    [PluginType(PluginType = typeof(TwitterAuthPlugin))]
    public class TwitterController : FoundationPluginController
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
                RedirectUri = ApplicationEngine.RouteUrl(TwitterConfig.TwitterRedirectUrl, new { returnUrl })
            };
            return Challenge(authenticationProperties,  TwitterConfig.TwitterAuthenticationScheme);
        }

        [HttpGet("twitter/redirect", Name = TwitterConfig.TwitterRedirectUrl)]
        public async Task<IActionResult> HandleRedirect(string returnUrl)
        {
            if (returnUrl.IsNullEmptyOrWhiteSpace())
                returnUrl = ApplicationEngine.RouteUrl(RouteNames.Home);
            var request = await TwitterHelper.CreateConnectedAccountRequestAsync();
            if (_connectionAccountant.Connect(request))
                return Redirect(returnUrl);
            return RedirectToRoute(RouteNames.Login);
        }
    }
}