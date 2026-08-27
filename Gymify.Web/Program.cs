using Gymify.Application.Extensions;
using Gymify.Application.Services.Interfaces;
using Gymify.Data.Entities;
using Gymify.Persistence;
using Gymify.Persistence.SeedData;
using Gymify.Web.Hubs;
using Gymify.Web.Seed;
using Gymify.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System.Globalization;
using Gymify.Web;
using Gymify.Application.Services.Implementation;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en-US"),
        new CultureInfo("uk-UA")
        {
            NumberFormat = { NumberDecimalSeparator = ".", CurrencyDecimalSeparator = "." }
        },
    };

    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

services.Configure<SeedDataOptions>(configuration.GetSection("SeedDataOptions"));

services
    .AddPersistence(configuration)
    .AddApplication();

services.AddSignalR();
services.AddSingleton<IUserIdProvider, CustomUserIdProviderService>();
services.AddSingleton<ChatMembersTrackerService>();

services.AddScoped<INotifierService, SignalRNotifierService>();

services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedAccount = true;
    options.SignIn.RequireConfirmedEmail = true;  

    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
})
.AddEntityFrameworkStores<GymifyDbContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<LocalizedIdentityErrorDescriber>();

services.AddTransient<IEmailSender, EmailSenderService>();

services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login";
    options.LogoutPath = "/Logout";
    options.AccessDeniedPath = "/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});

services.AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(SharedResource));
    });

var app = builder.Build();

var localizationOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>()!.Value;
app.UseRequestLocalization(localizationOptions);

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await IdentitySeeder.SeedRolesAndAdminAsync(roleManager, userManager);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Main}/{action=Index}/{id?}"
);

app.MapHub<NotificationHub>("/notificationHub");
app.MapHub<ChatHub>("/chatHub");

app.Run();