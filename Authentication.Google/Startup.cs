using System.Collections.Generic;
using EvenCart.Core.Infrastructure;
using EvenCart.Core.Startup;
using EvenCart.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Authentication.Google
{
    public class Startup : IAppStartup
    {
        public void ConfigureServices(IServiceCollection services, IHostingEnvironment hostingEnvironment)
        {
            services.AddAuthentication(GoogleConfig.GoogleAuthenticationScheme)
                .AddGoogle(GoogleConfig.GoogleAuthenticationScheme, options =>
                    {
                        var settings = DependencyResolver.Resolve<GoogleSettings>();
                        options.ClientId = settings.ClientId;
                        options.ClientSecret = settings.ClientSecret;
                        options.SaveTokens = true;
                        options.SignInScheme = ApplicationConfig.ExternalAuthenticationScheme;
                    });
        }

        public void Configure(IApplicationBuilder app)
        {

        }

        public int Priority { get; }
    }
}