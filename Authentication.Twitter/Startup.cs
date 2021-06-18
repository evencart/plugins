using Genesis;
using Genesis.Startup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Authentication.Twitter
{
    public class Startup : IAppStartup
    {
        public void ConfigureServices(IServiceCollection services, IHostingEnvironment hostingEnvironment)
        {
            services.AddAuthentication(TwitterConfig.TwitterAuthenticationScheme)
                .AddTwitter(TwitterConfig.TwitterAuthenticationScheme, options =>
                    {
                        var settings = D.Resolve<TwitterSettings>();
                        options.ConsumerKey = settings.ClientId;
                        options.ConsumerSecret = settings.ClientSecret;
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