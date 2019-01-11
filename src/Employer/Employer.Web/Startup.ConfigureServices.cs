using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Esfa.Recruit.Employer.Web
{
    public partial class Startup
    {
        private readonly bool _isAuthEnabled = true;
        private IConfiguration _configuration { get; }
        private IHostingEnvironment _hostingEnvironment { get; }
        private AuthenticationConfiguration _authConfig { get; }

        private readonly ILoggerFactory _loggerFactory;

        public Startup(IConfiguration config, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            _configuration = config;
            _hostingEnvironment = env;
            _authConfig = _configuration.GetSection("Authentication").Get<AuthenticationConfiguration>();
            _loggerFactory = loggerFactory;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIoC(_configuration);

            var redis = ConnectionMultiplexer.Connect(_configuration.GetConnectionString("StorageRedis"));
            services.AddDataProtection().PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");

            // Routing has to come before adding Mvc
            services.AddRouting(opt =>
            {
                opt.AppendTrailingSlash = true;
            });

            services.Configure<RazorViewEngineOptions>(o =>
            {
                o.ViewLocationFormats.Add("/Views/Part1/{1}/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Part2/{1}/{0}" + RazorViewEngine.ViewExtension);
            });

            services.AddMvcService(_hostingEnvironment, _isAuthEnabled, _loggerFactory);

            if (_isAuthEnabled)
            {
                //A service provider for resolving services configured in IoC
                var sp = services.BuildServiceProvider();

                services.AddAuthenticationService(_authConfig, sp.GetService<IEmployerVacancyClient>(), sp.GetService<IHostingEnvironment>());
                services.AddAuthorizationService();
            }
        }
    }
}
