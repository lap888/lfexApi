using System.Collections.Generic;
using application;
using application.middleware;
using application.services;
using CSRedis;
using domain.configs;
using domain.repository;
using infrastructure.extensions;
using infrastructure.mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace webAdmin
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region 系统配置注入
            services.Configure<application.Models.AppSetting>(Configuration.GetSection("AppSetting"));
            services.Configure<application.Models.YoBangConfig>(Configuration.GetSection("YoBangConfig"));
            services.Configure<application.Models.EquityConfig>(Configuration.GetSection("EquityConfig"));
            #endregion

            #region 福禄注入
            IConfigurationSection FuluConf = Configuration.GetSection("FuluConfig");
            services.Configure<application.Models.FuluConfig>(FuluConf);
            application.Models.FuluConfig FuluConfig = FuluConf.Get<application.Models.FuluConfig>();
            services.AddHttpClient(FuluConfig.ClientName);
            services.AddScoped<IFuluRecharge, FuluRecharge>();
            #endregion

            services.AddMemoryCache();
            services.AddTransient<IPermissionService, PermissionService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ISystemService, SystemService>();
            services.AddScoped<IYoyoUserSerivce, YoyoUserSerivce>();
            services.AddScoped<IYoyoWebService, YoyoWebService>();
            services.AddScoped<ITradeService, TradeService>();
            services.AddScoped<IUserWalletAccountService, UserWalletAccountService>();
            services.AddScoped<IRechargeService, RechargeService>();
            services.AddScoped<ICityPartnerService, CityPartnerService>();
            services.AddScoped<IEquityService, EquityService>();
            services.AddScoped<ICandyService, CandyService>();


            #region MyRegion
            IConfigurationSection WeChatConf = Configuration.GetSection("WeChatConfig");
            services.Configure<application.Models.WeChatConfig>(WeChatConf);
            application.Models.WeChatConfig WeChatConfig = WeChatConf.Get<application.Models.WeChatConfig>();
            services.AddHttpClient(WeChatConfig.ClientName);
            services.AddScoped<IWechatPlugin, WeChatPlugin>();
            #endregion

            #region 支付宝
            IConfigurationSection AlipayConf = Configuration.GetSection("AlipayConfig");
            services.Configure<application.Models.AlipayConfig>(AlipayConf);
            application.Models.AlipayConfig AlipayConfig = AlipayConf.Get<application.Models.AlipayConfig>();
            services.AddHttpClient(AlipayConfig.ClientName);
            services.AddScoped<IAlipay, Alipay>();
            #endregion
            #region 实名认证
            IConfigurationSection RealVerifyConf = Configuration.GetSection("RealVerifyConfig");
            services.Configure<application.Models.RealVerifyConfig>(RealVerifyConf);
            application.Models.RealVerifyConfig RealVerifyConfig = RealVerifyConf.Get<application.Models.RealVerifyConfig>();
            services.AddHttpClient(RealVerifyConfig.ClientName);
            services.AddScoped<IRealVerify, RealVerify>();
            #endregion
            #region 腾讯云
            IConfigurationSection QCloudConf = Configuration.GetSection("QCloudConfig");
            services.Configure<application.Models.QCloudConfig>(QCloudConf);
            application.Models.QCloudConfig QCloudConfig = QCloudConf.Get<application.Models.QCloudConfig>();
            services.AddHttpClient(QCloudConfig.ClientName);
            services.AddScoped<IQCloudPlugin, QCloudPlugin>();
            #endregion
            #region 注入数据库
            services.AddSingleton(o => new CSRedisClient(Configuration.GetConnectionString("RedisConnection")));
            services.Configure<ConnectionStringList>(Configuration.GetSection("ConnectionStrings"));
            #endregion
            services.AddMvcCustomer(Constants.WEBSITE_AUTHENTICATION_SCHEME, mvcOptions =>
             {
                 mvcOptions.AuthorizationSchemes = new List<MvcAuthorizeOptions>
                 {
                    new MvcAuthorizeOptions(){
                         ReturnUrlParameter="from",
                         AccessDeniedPath="/Denied",
                         AuthenticationScheme=Constants.WEBSITE_AUTHENTICATION_SCHEME,
                         LoginPath="/",
                         LogoutPath="/Logout"
                    }
                 };
             });
            services.AddDbContextPool<domain.lfexentitys.lfex_serviceContext>((serviceProvider, option) =>
            {
                option.UseMySql(Configuration.GetConnectionString("yoyoServiceConStr"), myop =>
                {
                    myop.ServerVersion(new Version(5, 7, 18), Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MySql)
                        .UnicodeCharSet(Pomelo.EntityFrameworkCore.MySql.Infrastructure.CharSet.Utf8mb4);
                });
            });
            services.AddOptions();
            services.RegisterService();
        }
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseAuthentication();
            app.UseStaticFiles();
            // app.UseStaticFiles(new StaticFileOptions
            // {
            //     //FileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory()),
            //     //设置不限制content-type 该设置可以下载所有类型的文件，但是不建议这么设置，因为不安全
            //     //下面设置可以下载apk和nupkg类型的文件
            //     ServeUnknownFileTypes = true,
            //     ContentTypeProvider = new FileExtensionContentTypeProvider(new Dictionary<string, string>
            //     {
            //           { ".apk", "application/vnd.android.package-archive" }
            //     })
            // });
            // using Microsoft.Extensions.FileProviders;
            // using System.IO;
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(env.ContentRootPath, "LfexCom")),
                RequestPath = "/lfex",
                OnPrepareResponse = ctx =>
                {
                    // using Microsoft.AspNetCore.Http;
                    ctx.Context.Response.Headers.Append(
                        "Cache-Control", $"public, max-age={604800}");
                }
            });
            app.UseErrorHandlerMiddleware();
            app.UseCors(t =>
            {
                t.WithMethods("POST", "PUT", "GET");
                t.WithHeaders("X-Requested-With", "Content-Type", "User-Agent");
                t.WithOrigins("*");
            });
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Down}/{action=Index}/{id?}");
            });
        }
    }
}
