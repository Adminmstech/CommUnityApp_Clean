using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.InfrastructureLayer.Repositories;
using CommUnityApp.InfrastructureLayer.Services;
using CommUnityApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.OpenApi;
using Stripe;


var builder = WebApplication.CreateBuilder(args);

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// MVC + API
builder.Services.AddControllersWithViews();

// Swagger
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


Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBMAY9C3t2U1hhQlJBfV5CQmdWfFN0QXNYflRxfF9CaEwxOX1dQl9nSXdTckdgXHtac3FWRGM=");
builder.Services.AddHttpClient();
builder.Services.AddControllersWithViews();
builder.Services.AddControllers(); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddControllers();


builder.Services.AddHttpContextAccessor();

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


builder.Services.AddSession();
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
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CommUnityApp API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();                     

app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Account}/{action=Login}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
