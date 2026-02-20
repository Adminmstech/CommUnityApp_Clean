using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.InfrastructureLayer.Repositories;
using CommUnityApp.InfrastructureLayer.Services;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddScoped<IEmailService, EmailService>();


builder.Services.AddSession();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

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
