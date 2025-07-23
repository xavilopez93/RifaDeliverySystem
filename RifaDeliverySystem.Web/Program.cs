using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using RifaDeliverySystem.Web.Data;
using RifaDeliverySystem.Web.Hubs;
using RifaDeliverySystem.Web.Services.Pdf;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
   opt.UseNpgsql(
        conn,
        npgsql => npgsql
            .SetPostgresVersion(new Version(9, 6))    
            .EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)
    ));
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddScoped<IReceiptPdfService, ReceiptPdfService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Ajusta políticas de contraseña, bloqueo, etc.
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();


builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();



builder.Services.AddAntiforgery(o => {
o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; o.Cookie.SameSite = SameSiteMode.Lax; 
    // sigue protegiendo CSRF
    });
builder.Services.ConfigureApplicationCookie(o => { o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; });


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options
        .UseNpgsql(conn, npgsql => npgsql.SetPostgresVersion(new Version(9, 6)))
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors()
);
var app = builder.Build();
//using(var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//    DbInitializer.Initialize(db);
//}


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();  
    await db.Database.MigrateAsync();            // applies all pending migrations
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
    DbInitializer.Initialize(db);     // just seeds data, no EnsureCreated
}   


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

//var supportedCultures = new[] { new CultureInfo("es-PY") };

//app.UseRequestLocalization(new RequestLocalizationOptions
//{
//    DefaultRequestCulture = new RequestCulture("es-PY"),
//    SupportedCultures = supportedCultures,
//    SupportedUICultures = supportedCultures
//});


var defaultCulture = new CultureInfo("es-PY");
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;


app.UseRouting();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Dashboard}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapHub<DashboardHub>("/dashboardHub");


using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
}
app.Run();
