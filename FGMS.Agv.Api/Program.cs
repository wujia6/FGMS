using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using FGMS.Agv.Api.Hubs;
using FGMS.Agv.Api.Middleware;
using FGMS.Core.EfCore.Implements;
using FGMS.Core.EfCore.Interfaces;
using FGMS.Utils;
using MapsterMapper;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver(); //json小驼峰
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; //避免循环引用
    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; //忽略为属性NULL的值
    options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss"; //格式化时间
});

// Swagger
builder.Services.AddEndpointsApiExplorer().AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "FGMS AGV API",
        Description = "生产管理系统-AGV服务"
    });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename), true);
});

// CORS跨域配置
builder.Services.AddCors(options => 
{
    // 允许所有来源的跨域请求
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
    // 仅允许特定来源的跨域请求（适用于需要发送凭据的情况）
    options.AddPolicy("SignalRPolicy", policy => 
    {
        policy.WithOrigins("http://10.39.0.99", "https://another-example.com") // 允许的特定来源
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // 允许发送凭据（如Cookies）
    });
});

// SignalR
builder.Services.AddSignalR();

// 添加 HttpClient 支持
builder.Services.AddHttpClient();

// Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory()).ConfigureContainer<ContainerBuilder>(builder =>
{
    //注册数据库上下文服务
    builder.RegisterType<FgmsDbContext>().As<IFgmsDbContext>().InstancePerLifetimeScope();

    //注册泛型仓储服务
    builder.RegisterGeneric(typeof(FgmsDbRepository<>)).As(typeof(IFgmsDbRepository<>)).InstancePerLifetimeScope();

    //注册实体仓储服务
    builder.RegisterAssemblyTypes(ApplicationFactory.GetAssembly("FGMS.Repositories"))
        .Where(tp => tp.Name.EndsWith("Repository") && !tp.IsInterface && !tp.IsAbstract)
        .AsImplementedInterfaces()
        .InstancePerLifetimeScope();

    //注册实体应用服务
    builder.RegisterAssemblyTypes(ApplicationFactory.GetAssembly("FGMS.Services"))
        .Where(tp => tp.Name.EndsWith("Service") && !tp.IsInterface && !tp.IsAbstract)
        .AsImplementedInterfaces()
        .InstancePerLifetimeScope();

    //mapster注入
    builder.RegisterInstance(MapsterAdaptConifg.Initial());
    builder.RegisterType<ServiceMapper>().As<IMapper>().InstancePerLifetimeScope();

    //httpcontext注入
    builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().InstancePerLifetimeScope();

    //httpclient帮助类
    builder.RegisterType<HttpClientHelper>().AsSelf().SingleInstance();

    //appsettings帮助类
    builder.RegisterType<ConfigHelper>().AsSelf().InstancePerLifetimeScope();
});

// 配置Serilog - 输出到本地文件
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // 设置最小日志级别
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)  //过滤
    .MinimumLevel.Override("System", LogEventLevel.Warning) //过滤
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

// 使用Serilog替代默认日志系统
builder.Host.UseSerilog();  

var app = builder.Build();
app.UseCors("SignalRPolicy");
app.UseSwagger().UseSwaggerUI();
app.UseAuthorization();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.MapControllers();
app.MapHub<AgvHubService>("hubs/agvHubService");
app.Run();