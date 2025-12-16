using System.Reflection;
using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using FGMS.Models;
using FGMS.Mx.Core;
using FGMS.PC.Api.Middleware;
using FGMS.Utils;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver(); //json小驼峰
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore; //避免循环引用
    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; //忽略为属性NULL的值
    options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss"; //格式化时间
});

// swagger
builder.Services.AddEndpointsApiExplorer().AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "FGMS PCAPI",
        Description = "富兰地砂轮管理系统"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Description = "在下框中输入请求头中需要添加Jwt授权Token：Bearer Token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme{ Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer"}}, Array.Empty<string>() }
    });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename), true);
});

// cors
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// 路由小写
//builder.Services.AddRouting(options => options.LowercaseUrls = true);

// 配置 JWT 认证
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtTokenOption:Issuer"],
        ValidAudience = builder.Configuration["JwtTokenOption:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtTokenOption:SecurityKey"]))
    };
});

// 添加 Cookie 身份验证
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

// 添加 HttpClient 支持
builder.Services.AddHttpClient();

// autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory()).ConfigureContainer<ContainerBuilder>(builder =>
{
    //autofac模块注入
    builder.RegisterModule<AutofacModule>();
    //mapster注入
    builder.RegisterInstance(MapsterAdaptConifg.Initial());
    builder.RegisterType<ServiceMapper>().As<IMapper>().InstancePerLifetimeScope();
    //httpcontext注入
    builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().InstancePerLifetimeScope();
    //httpclient帮助类
    builder.RegisterType<HttpClientHelper>().AsSelf().SingleInstance();
    //appsettings帮助类
    builder.RegisterType<ConfigHelper>().AsSelf().InstancePerLifetimeScope();
    //在线用户 InstancePerLifetimeScope() 通常对应每个 HTTP 请求一个实例
    builder.RegisterType<UserOnline>().AsSelf().InstancePerLifetimeScope();
    //二维码帮助类
    builder.RegisterType<QRCoderHelper>().AsSelf().SingleInstance();
    //随机数类
    builder.RegisterType<GenerateRandomNumber>().AsSelf().SingleInstance();
});

// 配置Serilog - 输出到控制台和本地文件
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // 设置最小日志级别
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)  //过滤
    .MinimumLevel.Override("System", LogEventLevel.Warning) //过滤
    .WriteTo.Console()           // 同时写控制台（可选，方便开发调试）
    .WriteTo.File(
        path: "Logs/log-.txt",  // 文件路径和命名，"-"表示日期滚动
        rollingInterval: RollingInterval.Day,   // 按天滚动日志
        retainedFileCountLimit: 7,               // 最多保留文件数量，避免占磁盘
        fileSizeLimitBytes: 10_000_000,          // 单文件最大10MB
        rollOnFileSizeLimit: true,                // 超过大小自动新建文件
        shared: true,                             // 让多个进程也可写该文件
        flushToDiskInterval: TimeSpan.FromSeconds(1)  // 写日志到磁盘频率
    )
    .CreateLogger();

builder.Host.UseSerilog();  // 使用Serilog替代默认日志系统

// 中间件
var app = builder.Build();
app.UseCors();
app.UseSwagger().UseSwaggerUI();
//app.UseStaticFiles(); // 允许对 wwwroot 下的文件进行访问
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.MapControllers();
app.Run();