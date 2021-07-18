using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Zixoan.Cuber.Server.Config;
using Zixoan.Cuber.Server.Web.Middleware;

namespace Zixoan.Cuber.Server.Web
{
    public class CuberWebStartup
    {
        private readonly IConfiguration configuration;

        public CuberWebStartup(IConfiguration configuration)
            => this.configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CuberOptions>(this.configuration.GetSection("Cuber"));

            services.AddControllers();
            services.AddApiVersioning();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseMiddleware<HeaderApiKeyMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
