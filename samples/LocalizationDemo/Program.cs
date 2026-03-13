using System.Globalization;
using Core.Localization.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Core.Localization
builder.Services.AddCoreLocalization(options =>
{
    options.DefaultCulture = new CultureInfo("en-US");
    options.FallbackCulture = new CultureInfo("en-US");
    options.SupportedCultures = new List<CultureInfo>
    {
        new CultureInfo("en-US"),
        new CultureInfo("tr-TR"),
        new CultureInfo("fr-FR")
    };
    options.ResourcePaths = new List<string> 
    { 
        Path.Combine(builder.Environment.ContentRootPath, "..", "..", "src", "Core.Localization", "Resources") 
    };
    options.EnableCaching = true;
    options.EnableResourceFileWatching = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Use request localization middleware
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-US"),
    SupportedCultures = new List<CultureInfo>
    {
        new CultureInfo("en-US"),
        new CultureInfo("tr-TR"),
        new CultureInfo("fr-FR")
    },
    SupportedUICultures = new List<CultureInfo>
    {
        new CultureInfo("en-US"),
        new CultureInfo("tr-TR"),
        new CultureInfo("fr-FR")
    }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
