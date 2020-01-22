using System.Security.Claims;
using System.Threading.Tasks;
using EvenCart.Data.Entity.Social;
using EvenCart.Infrastructure;
using Microsoft.AspNetCore.Authentication;

namespace Authentication.Google.Helpers
{
    public class GoogleHelper
    {
        public static async Task<ConnectedAccountRequest> CreateConnectedAccountRequestAsync()
        {
            var authenticationResult = await ApplicationEngine.CurrentHttpContext.AuthenticateAsync(GoogleConfig.GoogleAuthenticationScheme);
            if (!authenticationResult.Succeeded)
                return null;
            var userPrincipal = authenticationResult.Principal;
            return new ConnectedAccountRequest()
            {
                Email = userPrincipal.FindFirstValue(ClaimTypes.Email),
                FirstName = userPrincipal.FindFirstValue(ClaimTypes.GivenName),
                LastName = userPrincipal.FindFirstValue(ClaimTypes.Surname),
                ProviderUserId = userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier),
                ProviderName = "Google",
                AccessToken = await ApplicationEngine.CurrentHttpContext.GetTokenAsync("access_token")
            };
        }

    }
}