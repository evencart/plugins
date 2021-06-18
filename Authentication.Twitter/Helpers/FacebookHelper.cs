using System.Security.Claims;
using System.Threading.Tasks;
using EvenCart.Genesis;
using Genesis;
using Genesis.Modules.Users;
using Microsoft.AspNetCore.Authentication;

namespace Authentication.Twitter.Helpers
{
    public class TwitterHelper
    {
        public static async Task<ConnectedAccountRequest> CreateConnectedAccountRequestAsync()
        {
            var authenticationResult = await GenesisEngine.Instance.CurrentHttpContext.AuthenticateAsync(TwitterConfig.TwitterAuthenticationScheme);
            if (!authenticationResult.Succeeded)
                return null;
            var userPrincipal = authenticationResult.Principal;
            return new ConnectedAccountRequest()
            {
                Email = userPrincipal.FindFirstValue(ClaimTypes.Email),
                FirstName = userPrincipal.FindFirstValue(ClaimTypes.GivenName),
                LastName = userPrincipal.FindFirstValue(ClaimTypes.Surname),
                ProviderUserId = userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier),
                ProviderName = "Twitter",
                AccessToken = await GenesisEngine.Instance.CurrentHttpContext.GetTokenAsync("access_token")
            };
        }

    }
}