using System.Collections.Generic;
using EvenCart.Core.Infrastructure;
using EvenCart.Core.Startup;
using EvenCart.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Authentication.Facebook
{
    public class Startup : IAppStartup
    {
        public void ConfigureServices(IServiceCollection services, IHostingEnvironment hostingEnvironment)
        {
            services.AddAuthentication(FacebookConfig.FacebookAuthenticationScheme)
                .AddFacebook(FacebookConfig.FacebookAuthenticationScheme, options =>
                    {
                        var settings = DependencyResolver.Resolve<FacebookSettings>();
                        options.AppId = "832147880558350"; // settings.ClientId;
                        options.AppSecret = "ab6f62461b856dddfa0d2d256ad7ac33"; // settings.ClientSecret;
                        options.SaveTokens = true;
                        options.Fields.Add("email");
                        options.Fields.Add("birthday");
                        options.Fields.Add("picture");
                        options.Fields.Add("name");
                        options.SignInScheme = ApplicationConfig.ExternalAuthenticationScheme;
                    });
        }

        public void Configure(IApplicationBuilder app)
        {

        }

        public int Priority { get; }
    }
}