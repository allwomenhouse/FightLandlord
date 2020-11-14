using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BetGame.DDZ.WebHost
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

			Newtonsoft.Json.JsonConvert.DefaultSettings = () => {
				var st = new Newtonsoft.Json.JsonSerializerSettings();
				st.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
				st.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
				st.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.RoundtripKind;
				return st;
			};
		}

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
			services.AddScoped<CustomExceptionFilter>();
            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        public void Configure(IApplicationBuilder app)
        {
			app.UseDeveloperExceptionPage();

            app.UseRouting();
            app.UseEndpoints(config => config.MapControllers());
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }
    }
}
