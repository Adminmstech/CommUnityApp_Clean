using System;
using System.IO;
using System.Text;
using CommUnityApp.ApplicationCore.BAL;
using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.InfrastructureLayer.Repositories;
using CommUnityApp.InfrastructureLayer.Services;
using CommUnityApp.Hubs;
using CommUnityApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Stripe;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;


ClearBrokenLocalProxyEnvironment();

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

InitializeFirebase(builder.Configuration, builder.Environment.ContentRootPath);

var stripeSecretKey = builder.Configuration["Stripe:SecretKey"];
if (!string.IsNullOrWhiteSpace(stripeSecretKey))
{
    StripeConfiguration.ApiKey = stripeSecretKey;
}

// MVC + API
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CommUnityApp API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter only the JWT token returned from api/User/Login."
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });

    c.DocInclusionPredicate((_, api) => !api.ActionDescriptor.EndpointMetadata
        .OfType<ApiExplorerSettingsAttribute>()
        .Any(x => x.IgnoreApi));
    c.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    c.OrderActionsBy(apiDescription =>
        $"{apiDescription.ActionDescriptor.RouteValues["controller"]}_{apiDescription.RelativePath}_{apiDescription.HttpMethod}");
});

// Http Context & HttpClient
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// SignalR
builder.Services.AddSignalR();

var syncfusionLicenseKey = builder.Configuration["Syncfusion:LicenseKey"];
if (!string.IsNullOrWhiteSpace(syncfusionLicenseKey))
{
    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionLicenseKey);
}

builder.Services.AddTransient<IDapperWrapper, DapperWrapper>();
builder.Services.AddTransient<ICommunityRepository, CommunityRepository>();
builder.Services.AddTransient<IEventRepository, EventRepository>();
builder.Services.AddTransient<IBrandGameRepository, BrandGameRepository>();
builder.Services.AddTransient<IScratchWinRepository, ScratchWinRepository>();
builder.Services.AddTransient<IBusinessRepository, BusinessRepository>();
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<CommUnityApp.ApplicationCore.Interfaces.IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddTransient<IAuctionRepository, AuctionRepository>();
builder.Services.AddTransient<IForgotPasswordRepository, ForgotPasswordRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IProductRepository, ProductRepository>();
builder.Services.AddTransient<IRewardsRepository, RewardsRepository>();
builder.Services.AddTransient<IOrderRepository, OrderRepository>();
builder.Services.AddTransient<IServiceRepository, ServiceRepository>();
builder.Services.AddTransient<IVolunteerRepository, VolunteerRepository>();
builder.Services.AddTransient<INotificationRepository, NotificationRepository>();
builder.Services.AddTransient<IPushNotificationsDal, PushNotificationsDal>();
builder.Services.AddTransient<IPushNotificationsBal, PushNotificationsBal>();
builder.Services.AddTransient<IGameResultsRepository, GameResultsRepository>();
builder.Services.AddTransient<ICareConnectRepository, CareConnectRepository>();
builder.Services.AddTransient<IJobRepository, JobRepository>();
builder.Services.AddTransient<ICampaignRepository, CampignRepository>();
builder.Services.AddScoped<IPushNotificationService, PushNotificationService>();
builder.Services.AddTransient<ISmartQuizRepository, SmartQuizRepository>();
builder.Services.AddTransient<ITextQuizRepository,TextQuizRepository>();
builder.Services.AddTransient<ITalentShowRepository, TalentShowRepository>();
builder.Services.AddTransient<ICommunityHelpRepository,CommunityHelpRepository>();
builder.Services.AddTransient<ISpinGameRepository>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var dapper = provider.GetRequiredService<IDapperWrapper>();
    Func<System.Data.IDbConnection> connectionFactory = () =>
        new Microsoft.Data.SqlClient.SqlConnection(
            configuration.GetConnectionString("DefaultConnection")
        );

    return new SpinGameRepository(connectionFactory, dapper,configuration);
});

builder.Services.AddTransient<IQuizGameRepository>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var dapper = provider.GetRequiredService<IDapperWrapper>();

    Func<System.Data.IDbConnection> connectionFactory = () =>
        new Microsoft.Data.SqlClient.SqlConnection(
            configuration.GetConnectionString("DefaultConnection")
        );

    return new QuizGameRepository(connectionFactory, dapper);
});

// ========================
// COOKIE AUTHENTICATION
// ========================

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/";   // Redirect if not logged in
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("v1/swagger.json", "CommUnityApp API v1");
    c.RoutePrefix = "swagger";   // Swagger at /swagger
    c.EnableDeepLinking();
    c.DisplayRequestDuration();
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    c.DefaultModelsExpandDepth(-1);
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();


app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.MapControllerRoute(
    name: "area_default",
    pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);
app.MapHub<AuctionHub>("/auctionHub");
app.MapHub<TalentShowHub>("/talentShowHub");

app.Run();

static void ClearBrokenLocalProxyEnvironment()
{
    var proxyVariables = new[]
    {
        "HTTP_PROXY",
        "HTTPS_PROXY",
        "ALL_PROXY",
        "http_proxy",
        "https_proxy",
        "all_proxy"
    };

    foreach (var variable in proxyVariables)
    {
        var value = Environment.GetEnvironmentVariable(variable);
        if (value?.Contains("127.0.0.1:9", StringComparison.OrdinalIgnoreCase) == true ||
            value?.Contains("localhost:9", StringComparison.OrdinalIgnoreCase) == true)
        {
            Environment.SetEnvironmentVariable(variable, null);
        }
    }
}

static void InitializeFirebase(IConfiguration configuration, string contentRootPath)
{
    if (FirebaseApp.DefaultInstance != null)
        return;

    var httpClientFactory = new NoProxyHttpClientFactory();
    GoogleCredential? credential = null;
    string source = string.Empty;

    var firebaseJson = configuration["Firebase:ServiceAccountJson"]
                       ?? Environment.GetEnvironmentVariable("FIREBASE_SERVICE_ACCOUNT_JSON");

    var firebaseBase64 = configuration["Firebase:ServiceAccountBase64"]
                         ?? Environment.GetEnvironmentVariable("FIREBASE_SERVICE_ACCOUNT_BASE64");

    var appRoot = contentRootPath ?? Directory.GetCurrentDirectory();
    var configuredFirebasePath = configuration["Firebase:ServiceAccountPath"]
                                 ?? Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS")
                                 ?? "firebase-service-account.json";
    var firebasePath = Path.IsPathRooted(configuredFirebasePath)
        ? configuredFirebasePath
        : Path.Combine(appRoot, configuredFirebasePath);

    try
    {
        if (!string.IsNullOrWhiteSpace(firebaseJson))
        {
            credential = GoogleCredential.FromJson(firebaseJson);
            source = "Firebase:ServiceAccountJson";
        }
        else if (!string.IsNullOrWhiteSpace(firebaseBase64))
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(firebaseBase64));
            credential = GoogleCredential.FromJson(json);
            source = "Firebase:ServiceAccountBase64";
        }
        else if (Directory.Exists(firebasePath))
        {
            Console.Error.WriteLine($"Firebase service account path is a directory: '{firebasePath}'. Remove or rename that folder, or provide a valid file path.");
            return;
        }
        else if (System.IO.File.Exists(firebasePath))
        {
            credential = GoogleCredential.FromFile(firebasePath);
            source = firebasePath;
        }
        else
        {
            Console.Error.WriteLine($"Firebase service account file not found: '{firebasePath}'. Configure Firebase:ServiceAccountPath, Firebase:ServiceAccountJson, Firebase:ServiceAccountBase64, GOOGLE_APPLICATION_CREDENTIALS, FIREBASE_SERVICE_ACCOUNT_JSON, FIREBASE_SERVICE_ACCOUNT_BASE64, or add firebase-service-account.json to the content root.");
            return;
        }

        FirebaseApp.Create(new AppOptions
        {
            Credential = credential.CreateWithHttpClientFactory(httpClientFactory),
            HttpClientFactory = httpClientFactory
        });

        Console.WriteLine($"Firebase initialized using {source}.");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Failed to initialize Firebase using {source}: {ex.Message}");
    }
}

internal sealed class NoProxyHttpClientFactory : HttpClientFactory
{
    protected override HttpMessageHandler CreateHandler(CreateHttpClientArgs args)
    {
        return new HttpClientHandler
        {
            UseProxy = false
        };
    }
}
