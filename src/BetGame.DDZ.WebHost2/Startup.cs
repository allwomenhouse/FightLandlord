using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FreeSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace BetGame.DDZ.WebHost2
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Console.WriteLine(configuration["connectionString"].ToString());
            var fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.Sqlite, configuration["connectionString"].ToString())
                .UseAutoSyncStructure(true)
                .UseNoneCommandParameter(true)
                .Build();
            fsql.Aop.ConfigEntityProperty += new EventHandler<FreeSql.Aop.ConfigEntityPropertyEventArgs>((_, e) =>
            {
                if (fsql.Ado.DataType == FreeSql.DataType.MySql || fsql.Ado.DataType == FreeSql.DataType.OdbcMySql) return;
                if (e.Property.PropertyType.IsEnum == false) return;
                e.ModifyResult.MapType = typeof(string);
            });
            fsql.Aop.CurdBefore += new EventHandler<FreeSql.Aop.CurdBeforeEventArgs>((_, e) => Trace.WriteLine(e.Sql));
            BaseEntity.Initialization(fsql);

            Fsql = fsql;
            Configuration = configuration;

            RedisHelper.Initialization(new CSRedis.CSRedisClient(configuration["redis"]));

            Newtonsoft.Json.JsonConvert.DefaultSettings = () => {
                var st = new Newtonsoft.Json.JsonSerializerSettings();
                st.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                st.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
                st.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.RoundtripKind;
                return st;
            };
        }

        public IFreeSql Fsql { get; }
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<CustomExceptionFilter>();
            services.AddControllersWithViews().AddNewtonsoftJson();
            services.AddRazorPages();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();

            app.UseRouting();
            app.UseEndpoints(config => config.MapControllers());
            app.UseDefaultFiles();
            app.UseStaticFiles();

            ImHelper.Initialization(new ImClientOptions
            {
                Redis = RedisHelper.Instance,
                Servers = Configuration["imserver"].Split(';')
            });

            ImHelper.Instance.OnSend += (s, e) =>
                Console.WriteLine($"ImClient.SendMessage(server={e.Server},data={JsonConvert.SerializeObject(e.Message)})");
        }
    }
}
