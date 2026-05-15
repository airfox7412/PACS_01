using Database.Core.Helper;
using Database.Core.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Api.Library;
using Api.Repositories;
using Api.Security;
using Mapster;
using NLog;
using Quartz;
using FastExpressionCompiler;
using Swashbuckle.AspNetCore.SwaggerGen;
using Api.Jobs;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

internal class Program
{
    private static void Main(string[] args)
    {
        // Mapster config
        TypeAdapterConfig.GlobalSettings.Compiler = exp => exp.CompileFast();
        TypeAdapterConfig.GlobalSettings.Default.AvoidInlineMapping(true); //跳過內聯映射

        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        //設置資料保護服務
        services.AddDataProtection();
        services.AddSingleton<IDataProtectionService, DataProtectionService>();
        //設置Hub通知服務
        //services.AddTransient<NotificationService>();

        services.AddCors(options =>
            options.AddPolicy(name: "CorsPolicy",
                policy =>
                {
                    policy.WithOrigins(
                        "http://localhost",
                        "http://192.168.28.23",
                        "capacitor://localhost");
                    policy.WithMethods("GET", "POST");
                    policy.AllowAnyHeader();
                    policy.AllowCredentials();
                }));

        services.AddMvc();
        services.AddControllers(
            options =>
            {
                options.Filters.Add(new TokenFilter());
                options.OutputFormatters.RemoveType<StringOutputFormatter>();  //swagger 文件中不要出現 text/plain
            })
            //Take the controller as a service
            .AddControllersAsServices()
            .AddJsonOptions(options =>
            {
                //// serialize enums as strings in api responses (e.g. Role)
                //options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

                // ignore omitted parameters on models to enable optional params (e.g. User update)
                options.JsonSerializerOptions.DefaultIgnoreCondition =
                    JsonIgnoreCondition.WhenWritingNull;
            })
            .AddNewtonsoftJson(options =>
            {
                options.UseMemberCasing();
                //options.SerializerSettings.Converters.Add(new StringEnumConverter());
            }); //參數區分大小寫（使用ODATA查詢必要條件）
        services.AddHttpContextAccessor(); //稽核記錄取得登入帳號及用戶端IP用
        services.AddScoped<IAuditEventService, AuditEventRepository>(); //稽核記錄用

        // Add OpenAPI v3 document
        services.AddScoped<SwaggerGenerator>()
            .AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ReportAPI系統平臺",
                    Version = "v1.0",
                    TermsOfService = null,
                    Contact = new OpenApiContact
                    {
                        Name = "OData V4.0 協定參考",
                        Email = "",

                        Url = new Uri("http://localhost/UploadFiles/OpenData通訊協定V4.pdf")
                    }
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Bearer",
                    BearerFormat = "JWT",
                    Scheme = "bearer",
                    Description = "Specify the authorization token.",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] { }
            }
                });
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Database.Core.xml"), true);
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Api.xml"), true);

                c.OperationFilter<ODataQueryOptionOperationFilter>();
            })
            .AddSwaggerGenNewtonsoftSupport()
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // 當驗證失敗時，回應標頭會包含 WWW-Authenticate 標頭，這裡會顯示失敗的詳細錯誤原因
                options.IncludeErrorDetails = true; // 預設值為 true，有時會特別關閉
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // 透過這項宣告，就可以從 "sub" 取值並設定給 User.Identity.Name
                    NameClaimType = ClaimTypes.NameIdentifier,
                    // 透過這項宣告，就可以從 "roles" 取值，並可讓 [Authorize] 判斷角色
                    //RoleClaimType = ClaimTypes.Role,

                    // 一般我們都會驗證 Issuer
                    ValidateIssuer = false,
                    //ValidIssuer = TokenManager.Issuer,

                    // 通常不太需要驗證 Audience
                    ValidateAudience = false,
                    //ValidAudience = "JwtAuthDemo", // 不驗證就不需要填寫

                    // 一般我們都會驗證 Token 的有效期間
                    ValidateLifetime = true,

                    // 如果 Token 中包含 key 才需要驗證，一般都只有簽章而已
                    ValidateIssuerSigningKey = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TokenManager.Secret))
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        if (context.Request.Path.Value != null &&
                            context.Request.Path.Value.ToLower() != "/auth/login" &&
                            context.Request.Headers.Authorization.Any())
                        {

                            if (!CheckAuthorization(context.Request.Headers.Authorization))
                            {
                                context.Response.StatusCode = 401;
                                var byteMessage = Encoding.UTF8.GetBytes("此登入帳號已從其他瀏覽器登入!");
                                context.Response.ContentType = "application/json; charset=utf-8";
                                context.Response.Body.WriteAsync(byteMessage, 0,
                                    byteMessage.Length);
                            }
                        }

                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        if (context.HttpContext.Connection.RemoteIpAddress != null)
                        {
                            if (context.HttpContext.Connection.RemoteIpAddress.ToString() != "::1" &&
                                !context.HttpContext.Connection.RemoteIpAddress.ToString().StartsWith("192.168.") &&
                                !context.HttpContext.Connection.RemoteIpAddress.ToString().StartsWith("127.0.0."))
                            {
                                if (context.Request.Path.ToString().StartsWith("/swagger/"))
                                {
                                    LogManager.GetCurrentClassLogger().Info(
                                        $"Using Swagger, IP:{context.HttpContext.Connection.RemoteIpAddress}");
                                    throw new Exception();
                                }
                            }
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        //每?小時執行一次
        services.AddQuartz(q =>
        {
            q.UseSimpleTypeLoader();
            q.UseInMemoryStore();
            var jobKey1 = new JobKey("GetHistoryJob");
            q.AddJob<GetHistoryJob>(opts => opts.WithIdentity(jobKey1));
            q.AddTrigger(opts => opts
                .ForJob(jobKey1)
                .WithIdentity("GetHistoryJob-trigger")
                .WithCronSchedule($"0 0 0/{SystemConfig.HistoryTimes} * * ?")
            );
        });
        //每?分鐘執行一次
        services.AddQuartz(q =>
        {
            q.UseSimpleTypeLoader();
            q.UseInMemoryStore();
            var jobKey1 = new JobKey("GetGeminiAIJob");
            q.AddJob<GetGeminiAIJob>(opts => opts.WithIdentity(jobKey1));
            q.AddTrigger(opts => opts
                    .ForJob(jobKey1)
                    .WithIdentity("GetGeminiAIJob-trigger")
                    .WithCronSchedule($"0 0/{SystemConfig.AITimes} * * * ?")
            );
        });
        // Add the Quartz.NET hosted service
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        // Gemini AI 呼叫定義策略
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(new[] {
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(8)
            });
        // 註冊服務
        builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("GeminiSettings"));
        builder.Services.AddHttpClient<IBoneDensityAnalyzer, GeminiBoneDensityAnalyzer>((sp, client) => {
            var settings = sp.GetRequiredService<IOptions<GeminiSettings>>().Value;
            client.BaseAddress = new Uri(settings.EndpointUrl);
        }).AddPolicyHandler(retryPolicy);
        // Gemini AI 呼叫定義策略

        var app = builder.Build();
        if (builder.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            //app.UseSwagger();
            //app.UseSwaggerUI();
        }

        //強制執行 HTTPS
        //app.UseHttpsRedirection();
        //app.MapSwagger().RequireAuthorization();

        //啟用預設 cookie 同意功能
        app.UseCookiePolicy(
            new CookiePolicyOptions
            {
                Secure = CookieSecurePolicy.Always
            });
        //常見的錯誤狀態碼啟用預設的純文字處理常式
        //app.UseStatusCodePages(); 

        //提供預設文件
        app.UseDefaultFiles();
        //靜態檔案設定
        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                if (ctx.Context.Request.Path.Value != null &&
                    ctx.Context.Request.Path.Value.ToLower() == "/index.html")
                    ctx.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            }
        });

        var dir = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "UploadFiles"));
        if (!dir.Exists)
            dir.Create();

        app.UseFileServer(new FileServerOptions
        {
            FileProvider = new PhysicalFileProvider(dir.FullName),
            RequestPath = "/UploadFiles",
        });

        dir = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "Temp"));
        if (!dir.Exists)
            dir.Create();
        app.UseFileServer(new FileServerOptions
        {
            FileProvider = new PhysicalFileProvider(dir.FullName),
            RequestPath = "/Temp"
        });

        // global cors policy
        app.UseCors(x => x
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
        app.UseCors("CorsPolicy");
        app.UseRouting();

        app.Use(
            (context, next) =>
            {
                // 確保 HTTP Request 可以多次讀取
                context.Request.EnableBuffering();

                return next();
            });

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
            endpoints.MapControllers();
        });

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("v1/swagger.json", "ReportAPI平臺 API V2");
        });
        app.UseReDoc(options =>
        {
            options.DocumentTitle = "ReportAPI平臺 API V2";
            options.SpecUrl = "/swagger/v1/swagger.json";
        });

        app.Run();

        bool CheckAuthorization(string teken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            teken = teken.Replace("Bearer ", "");
            var jwtTeken = (JwtSecurityToken)tokenHandler.ReadToken(teken);
            if (jwtTeken.Payload.ContainsKey("nameid"))
            {
                var name = (string)jwtTeken.Payload["nameid"];
                if (name == "assist")   //協助人員可多重登入
                    return true;
            }

            return true;
        }
    }
}