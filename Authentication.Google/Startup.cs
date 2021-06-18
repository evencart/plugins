using Genesis;
using Genesis.Startup;
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
                        var settings = D.Resolve<GoogleSettings>();
                        options.ClientId = settings.ClientId;
                        options.ClientSecret = settings.ClientSecret;
                        options.SaveTokens = true;
                        options.SignInScheme = GenesisApp.Current.ApplicationConfig.ExternalAuthenticationScheme;
                    });
        }

        public void Configure(IApplicationBuilder app)
        {

        }

        public int Priority { get; }
    }
}