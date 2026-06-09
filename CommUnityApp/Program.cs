using System;
using System.IO;
using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.InfrastructureLayer.Repositories;
using CommUnityApp.InfrastructureLayer.Services;
using CommUnityApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.OpenApi;
using Stripe;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

var builder = WebApplication.CreateBuilder(args);

// Resolve firebase credential path: config -> env var -> content root
string firebasePath = builder.Configuration["Firebase:ServiceAccountPath"]
                      ?? Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS")
                      ?? Path.Combine(builder.Environment.ContentRootPath ?? Directory.GetCurrentDirectory(), "firebase-service-account.json");

if (Directory.Exists(firebasePath))
{
    Console.Error.WriteLine($"Firebase service account path is a directory: '{firebasePath}'. Remove or rename that folder, or provide a valid file path.");
}
else if (System.IO.File.Exists(firebasePath))
{
    try
    {
        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile(firebasePath)
        });
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Failed to initialize Firebase with '{firebasePath}': {ex.Message}");
    }
}
else
{
    Console.Error.WriteLine($"Firebase service account file not found: '{firebasePath}'. Set 'Firebase:ServiceAccountPath' in configuration or 'GOOGLE_APPLICATION_CREDENTIALS' env var, or add the file to the content root.");
}

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
builder.Services.AddTransient<IGameResultsRepository, GameResultsRepository>();
builder.Services.AddTransient<ICareConnectRepository, CareConnectRepository>();
builder.Services.AddTransient<IJobRepository, JobRepository>();
builder.Services.AddTransient<IDapperWrapper, DapperWrapper>();
builder.Services.AddTransient<ICampaignRepository, CampignRepository>();
builder.Services.AddTransient<ISpinGameRepository>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var dapper = provider.GetRequiredService<IDapperWrapper>();
    Func<System.Data.IDbConnection> connectionFactory = () =>
        new Microsoft.Data.SqlClient.SqlConnection(
            configuration.GetConnectionString("DefaultConnection")
        );

    return new SpinGameRepository(connectionFactory, dapper);
});

builder.Services.AddSession();
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

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CommUnityApp API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllers();

app.MapControllerRoute(
    name: "area_default",
    pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
