using application;
using application.middleware;
using application.services;
using CSRedis;
using domain.configs;
using domain.repository;
using infrastructure.extensions;
using infrastructure.utils;
using lfexApi.MessageHandlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace yoyoApi
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
            services.AddHttpClient();
            services.AddHttpClient("Coin");

            services.AddHttpClient("JPushSMS", client =>
            {
                String BaseStr = SecurityUtil.Base64Encode($"{Constants.JpushKey}:{Constants.JpushSecret}");
                String AuthStr = $"Basic {BaseStr}";
                client.DefaultRequestHeaders.Add("Authorization", AuthStr);
                client.Timeout = new TimeSpan(0, 0, 0, 1, 500);
            });
            #region 系统配置注入
            services.Configure<application.Models.AppSetting>(Configuration.GetSection("AppSetting"));
            services.Configure<application.Models.YoBangConfig>(Configuration.GetSection("YoBangConfig"));
            services.Configure<application.Models.EquityConfig>(Configuration.GetSection("EquityConfig"));
            services.Configure<application.Models.TicketConfig>(Configuration.GetSection("TicketConfig"));
            services.Configure<application.Models.CoinConfig>(Configuration.GetSection("CoinConfig"));
            #endregion
            #region 注入微信配置
            IConfigurationSection WeChatConf = Configuration.GetSection("WeChatConfig");
            services.Configure<application.Models.WeChatConfig>(WeChatConf);
            application.Models.WeChatConfig WeChatConfig = WeChatConf.Get<application.Models.WeChatConfig>();
            services.AddHttpClient(WeChatConfig.ClientName);
            services.AddScoped<IWechatPlugin, WeChatPlugin>();
            #endregion
            #region 支付宝配置
            IConfigurationSection AlipayConf = Configuration.GetSection("AlipayConfig");
            services.Configure<application.Models.AlipayConfig>(AlipayConf);
            application.Models.AlipayConfig AlipayConfig = AlipayConf.Get<application.Models.AlipayConfig>();
            services.AddHttpClient(AlipayConfig.ClientName);
            services.AddScoped<IAlipay, Alipay>();
            #endregion
            #region 实名认证配置
            IConfigurationSection RealVerifyConf = Configuration.GetSection("RealVerifyConfig");
            services.Configure<application.Models.RealVerifyConfig>(RealVerifyConf);
            application.Models.RealVerifyConfig RealVerifyConfig = RealVerifyConf.Get<application.Models.RealVerifyConfig>();
            services.AddHttpClient(RealVerifyConfig.ClientName);
            services.AddScoped<IRealVerify, RealVerify>();
            #endregion
            #region 腾讯云配置
            IConfigurationSection QCloudConf = Configuration.GetSection("QCloudConfig");
            services.Configure<application.Models.QCloudConfig>(QCloudConf);
            application.Models.QCloudConfig QCloudConfig = QCloudConf.Get<application.Models.QCloudConfig>();
            services.AddHttpClient(QCloudConfig.ClientName);
            services.AddScoped<IQCloudPlugin, QCloudPlugin>();
            #endregion
            #region 注入福禄充值
            IConfigurationSection FuluConf = Configuration.GetSection("FuluConfig");
            services.Configure<application.Models.FuluConfig>(FuluConf);
            application.Models.FuluConfig FuluConfig = FuluConf.Get<application.Models.FuluConfig>();
            services.AddHttpClient(FuluConfig.ClientName);
            services.AddScoped<IFuluRecharge, FuluRecharge>();
            #endregion
            #region Coin


            #endregion
            #region 注入数据库
            services.AddSingleton(o => new CSRedisClient(Configuration.GetConnectionString("RedisConnection")));
            services.Configure<ConnectionStringList>(Configuration.GetSection("ConnectionStrings"));
            services.AddDbContextPool<domain.lfexentitys.lfex_serviceContext>((serviceProvider, option) =>
            {
                option.UseMySql(Configuration.GetConnectionString("yoyoServiceConStr"), myop =>
                {
                    myop.ServerVersion(new Version(5, 7, 18), Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MySql)
                        .UnicodeCharSet(Pomelo.EntityFrameworkCore.MySql.Infrastructure.CharSet.Utf8mb4);
                });
            });
            #endregion

            #region 系统服务
            services.AddScoped<IYoyoUserSerivce, YoyoUserSerivce>();
            services.AddScoped<IUserAddress, UserAddress>();
            services.AddScoped<IAliPayAction, AliPayAction>();
            services.AddScoped<ISystemService, SystemService>();
            services.AddScoped<ITradeService, TradeService>();
            services.AddScoped<IGameService, GameService>();
            services.AddScoped<IUserWalletAccountService, UserWalletAccountService>();
            services.AddScoped<IRechargeService, RechargeService>();
            services.AddScoped<ICityPartnerService, CityPartnerService>();
            services.AddScoped<IEquityService, EquityService>();
            services.AddScoped<ICandyService, CandyService>();
            services.AddScoped<ITicketService, TicketService>();
            services.AddScoped<ICoinService, CoinService>();

            #endregion
            //Gzip压缩
            services.AddResponseCompression();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddJsonOptions(setupAction =>
            {
                setupAction.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                setupAction.SerializerSettings.DateFormatString = "yyyy/MM/d HH:mm:ss";
                setupAction.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                setupAction.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });
            services.AddMemoryCache();
#if DEBUG
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(
                    name: "v1",
                    info: new OpenApiInfo
                    {
                        Title = "LFEXAPI",
                        Description = "小鱼交易所测试文档",
                        Version = "v1.0.0"
                    }
                );
                var yoyoApiFilePath = Path.Combine(AppContext.BaseDirectory, "Api.xml");
                var domainFilePath = Path.Combine(AppContext.BaseDirectory, "domain.xml");
                c.IncludeXmlComments(yoyoApiFilePath);
                c.IncludeXmlComments(domainFilePath);
            });
#endif
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddDistributedRedisCache(option =>
            {
                option.Configuration = Configuration.GetConnectionString("SessionRedisConnection");
                option.InstanceName = "Y_Session:";
            });
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60); //session活期时间
                options.Cookie.HttpOnly = true;//设为httponly
            });
            //WEB SOCKET
            services.AddWebSocketManager();
            services.RegisterService();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
#if DEBUG
            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {
                ContentTypeProvider = new FileExtensionContentTypeProvider(new Dictionary<string, string>() { { ".apk", "application/vnd.android.package-archive" } })
            });
#else
            app.UseStaticFiles();
#endif
            app.UseErrorHandlerMiddleware();
            app.UseCors(t =>
            {
                t.WithMethods("POST", "PUT", "GET");
                t.WithHeaders("X-Requested-With", "Content-Type", "User-Agent", "token");
                t.WithOrigins("*");
            });

            //启用Gzip
            app.UseResponseCompression();
            // app.UseHttpsRedirection();
#if DEBUG
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(
                    url: "/swagger/v1/swagger.json",
                    name: "v1.0.0"
                );
            });
#endif
            app.UseSession();
            //websocket
            
            app.MapWebSocketManager("/notifications", provider.GetService<NotificationsMessageHandler>());

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
