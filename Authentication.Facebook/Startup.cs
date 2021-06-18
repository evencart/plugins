using Genesis;
using Genesis.Extensions;
using Genesis.Startup;
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
                        var settings = D.Resolve<FacebookSettings>();
                        var clientId = settings.ClientId.IsNullEmptyOrWhiteSpace() ? "XXXX" : settings.ClientId;
                        var clientSecret = settings.ClientSecret.IsNullEmptyOrWhiteSpace() ? "XXXX" : settings.ClientSecret;
                        options.AppId = clientId;
                        options.AppSecret = clientSecret;
                        options.SaveTokens = true;
                        options.Fields.Add("email");
                        options.Fields.Add("birthday");
                        options.Fields.Add("picture");
                        options.Fields.Add("name");
                        options.SignInScheme = GenesisApp.Current.ApplicationConfig.ExternalAuthenticationScheme;

                    });
        }

        public void Configure(IApplicationBuilder app)
        {

        }

        public int Priority { get; }
    }
}