using System.Globalization;
using Core.Localization;
using Core.Localization.Abstractions;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace LocalizationDemo.Controllers;

public class HomeController : Controller
{
    private readonly ILocalizationService _localization;
    private readonly IFormatterService _formatter;

    public HomeController(ILocalizationService localization, IFormatterService formatter)
    {
        _localization = localization;
        _formatter = formatter;
    }

    public IActionResult Index()
    {
        var currentCulture = CultureInfo.CurrentCulture;

        // 1. Strongly Typed Resources Demo (Source Generator)
        // Using L.Welcome instead of "Welcome"
        ViewBag.WelcomeMessage = _localization.GetString(L.Welcome, new
        {
            StoreName = "CoreShop",
            UserName = "Omerkck41"
        });

        // 2. Pluralization Demo
        ViewBag.Cart1 = _localization.GetPluralString(L.CartSummary, 0);
        ViewBag.Cart2 = _localization.GetPluralString(L.CartSummary, 1);
        ViewBag.Cart3 = _localization.GetPluralString(L.CartSummary, 5);

        // 3. Formatting Demo
        ViewBag.CurrentDate = _formatter.FormatDate(DateTime.Now, culture: currentCulture);
        ViewBag.Price = _formatter.FormatCurrency(1250.75m, culture: currentCulture);

        ViewBag.SupportedCultures = _localization.GetSupportedCultures();
        return View();
    }

    public IActionResult Demo()
    {
        var currentCulture = CultureInfo.CurrentCulture;

        var model = new DemoViewModel
        {
            UserName = "Admin",
            StoreName = "Global E-Commerce",
            CartItemCount = 3,
            StockCount = 1,
            TotalAmount = 4500.00m
        };

        // Pre-localized messages for the view using tip-safe L class
        ViewBag.Welcome = _localization.GetString(L.Welcome, new { model.StoreName, model.UserName });
        ViewBag.CartText = _localization.GetPluralString(L.CartSummary, model.CartItemCount);
        ViewBag.StockText = _localization.GetPluralString(L.StockStatus, model.StockCount);
        ViewBag.OrderSuccessText = _localization.GetString(L.OrderSuccess, new
        {
            model.UserName,
            Amount = _formatter.FormatCurrency(model.TotalAmount, culture: currentCulture)
        });

        ViewBag.SupportedCultures = _localization.GetSupportedCultures();
        return View(model);
    }

    [HttpPost]
    public IActionResult SetLanguage(string culture, string returnUrl)
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
        );

        return LocalRedirect(returnUrl);
    }
}

public class DemoViewModel
{
    public string UserName { get; set; } = "";
    public string StoreName { get; set; } = "";
    public int CartItemCount { get; set; }
    public int StockCount { get; set; }
    public decimal TotalAmount { get; set; }
}
