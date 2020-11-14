using FreeRedis;
using FreeSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

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
            BaseEntity.Initialization(fsql, null);

            Fsql = fsql;
            Configuration = configuration;

            Redis = new RedisClient(configuration["redis"]);
            Redis.Serialize = obj => JsonConvert.SerializeObject(obj);
            Redis.Deserialize = (json, type) => JsonConvert.DeserializeObject(json, type);
            DDZ.GamePlay.OnSaveData = (id, d) => Redis.HSet($"DDZrdb", id, d);
            DDZ.GamePlay.OnGetData = id => Redis.HGet<GameInfo>("DDZrdb", id);

            Newtonsoft.Json.JsonConvert.DefaultSettings = () => {
                var st = new Newtonsoft.Json.JsonSerializerSettings();
                st.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                st.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
                st.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.RoundtripKind;
                return st;
            };
        }

        public static IFreeSql Fsql { get; private set; }
        public static RedisClient Redis { get; private set; }
        public static IConfiguration Configuration { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Redis);
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
                Redis = Redis,
                Servers = Configuration["imserver"].Split(';')
            });

            ImHelper.Instance.OnSend += (s, e) =>
                Console.WriteLine($"ImClient.SendMessage(server={e.Server},data={JsonConvert.SerializeObject(e.Message)})");
        }
    }
}
