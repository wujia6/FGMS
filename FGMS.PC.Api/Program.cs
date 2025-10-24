using System.Reflection;
using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using FGMS.Models;
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
    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver(); //jsonС�շ�
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore; //����ѭ������
    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; //����Ϊ����NULL��ֵ
    options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss"; //��ʽ��ʱ��
});

// swagger
builder.Services.AddEndpointsApiExplorer().AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "FGMS PCAPI",
        Description = "������ɰ�ֹ���ϵͳ"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Description = "���¿�����������ͷ����Ҫ���Jwt��ȨToken��Bearer Token",
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

// ·��Сд
//builder.Services.AddRouting(options => options.LowercaseUrls = true);

// ���� JWT ��֤
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

// ��� Cookie �����֤
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

// ��� HttpClient ֧��
builder.Services.AddHttpClient();

// autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory()).ConfigureContainer<ContainerBuilder>(builder =>
{
    //autofacģ��ע��
    builder.RegisterModule<AutofacModule>();
    //mapsterע��
    builder.RegisterInstance(MapsterAdaptConifg.Initial());
    builder.RegisterType<ServiceMapper>().As<IMapper>().InstancePerLifetimeScope();
    //httpcontextע��
    builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().InstancePerLifetimeScope();
    //httpclient������
    builder.RegisterType<HttpClientHelper>().AsSelf().SingleInstance();
    //appsettings������
    builder.RegisterType<ConfigHelper>().AsSelf().InstancePerLifetimeScope();
    //�����û� InstancePerLifetimeScope() ͨ����Ӧÿ�� HTTP ����һ��ʵ��
    builder.RegisterType<UserOnline>().AsSelf().InstancePerLifetimeScope();
    //��ά�������
    builder.RegisterType<QRCoderHelper>().AsSelf().SingleInstance();
    //�������
    builder.RegisterType<GenerateRandomNumber>().AsSelf().SingleInstance();
});

// ����Serilog - ���������̨�ͱ����ļ�
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // ������С��־����
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)  //����
    .MinimumLevel.Override("System", LogEventLevel.Warning) //����
    .WriteTo.Console()           // ͬʱд����̨����ѡ�����㿪�����ԣ�
    .WriteTo.File(
        path: "Logs/log-.txt",  // �ļ�·����������"-"��ʾ���ڹ���
        rollingInterval: RollingInterval.Day,   // ���������־
        retainedFileCountLimit: 7,               // ��ౣ���ļ�����������ռ����
        fileSizeLimitBytes: 10_000_000,          // ���ļ����10MB
        rollOnFileSizeLimit: true,                // ������С�Զ��½��ļ�
        shared: true,                             // �ö������Ҳ��д���ļ�
        flushToDiskInterval: TimeSpan.FromSeconds(1)  // д��־������Ƶ��
    )
    .CreateLogger();

builder.Host.UseSerilog();  // ʹ��Serilog���Ĭ����־ϵͳ

// �м��
var app = builder.Build();
app.UseCors();
app.UseSwagger().UseSwaggerUI();
//app.UseStaticFiles(); // ����� wwwroot �µ��ļ����з���
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.MapControllers();
app.Run();