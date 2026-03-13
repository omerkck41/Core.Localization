# Core.Localization 🌍

Modern, modüler ve .NET 10.0 mimarisi için optimize edilmiş, **Yapay Zeka Destekli** ve **Tip Güvenli** yerelleştirme kütüphanesi. Clean Architecture projelerine tam uyumludur ve hiçbir veri tabanı bağımlılığı taşımaz.

---

## ✨ Öne Çıkan Özellikler

- **💎 Tip Güvenliği (Source Generator):** "Magic string" hatalarına son! IntelliSense desteğiyle `L.Welcome` gibi kod tamamlamalı kullanım.
- **🤖 AI Otomatik Çeviri:** Google Gemini entegrasyonu ile dil dosyalarınızı saniyeler içinde tüm dünya dillerine otomatik çevirin.
- **🌍 Dünya Standartlarında Çoğullaştırma:** CLDR standartlarında; Türkçe, İngilizce, Arapça, Rusça gibi dillerin karmaşık çoğul kurallarını (Zero, One, Few, Many, Other) otomatik yönetir.
- **⚡ Yüksek Performans:** Akıllı in-memory caching ve düşük bellek tüketimi.
- **🧩 Database Agnostic:** Mevcut veri tabanı mimarinize (EF Core, Dapper vb.) saniyeler içinde entegre edilebilir.
- **📦 Universal:** Standart .NET `IStringLocalizer` arayüzü ile %100 uyumludur.

---

## 🚀 1. Kurulum ve Yapılandırma

### Paket Kurulumu
```bash
dotnet add package Core.Localization
```

### Program.cs Yapılandırması
```csharp
using System.Globalization;
using Core.Localization.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Servis Kaydı
builder.Services.AddCoreLocalization(options =>
{
    options.DefaultCulture = new CultureInfo("tr-TR");
    options.FallbackCulture = new CultureInfo("en-US");
    options.SupportedCultures = new List<CultureInfo> 
    { 
        new CultureInfo("tr-TR"), 
        new CultureInfo("en-US") 
    };
    // Dil dosyalarınızın yolu
    options.ResourcePaths = new List<string> { "Resources" };
    options.EnableCaching = true;
    options.CacheExpiration = TimeSpan.FromHours(1);
});

var app = builder.Build();

// 2. Middleware Kaydı (Dil tespiti için zorunludur)
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("tr-TR")
    .AddSupportedCultures("tr-TR", "en-US")
    .AddSupportedUICultures("tr-TR", "en-US"));
```

---

## 💎 2. Tip Güvenli Kullanım (Source Generator)

Bu özellik, JSON dosyalarınızdaki anahtarları otomatik olarak bir C# sınıfına dönüştürür.

### Adım 1: .csproj Ayarı
Proje dosyanıza dil dosyalarını `AdditionalFiles` olarak ekleyin:
```xml
<ItemGroup>
  <AdditionalFiles Include="Resources\*.json" />
</ItemGroup>
```

### Adım 2: Kod İçinde Kullanım
Artık string yazmak yerine IntelliSense desteğiyle `L` sınıfını kullanabilirsiniz:
```csharp
public class HomeController : Controller
{
    private readonly ILocalizationService _loc;

    public HomeController(ILocalizationService loc) => _loc = loc;

    public IActionResult Index()
    {
        // Hata payı sıfır!
        ViewBag.Message = _loc.GetString(L.WelcomeMessage); 
        return View();
    }
}
```

---

## 🌍 3. Gelişmiş Çoğullaştırma (Pluralization)

Kütüphanemiz ICU formatını ve CLDR kurallarını destekler. Özellikle karmaşık çoğul kuralları olan dillerde (Rusça, Arapça vb.) hayat kurtarır.

### JSON Dosyası (resources.tr-TR.json)
```json
{
  "CartSummary": "Sepetinizde {count, plural, zero{ürün yok} one{1 ürün var} other{# ürün var}}"
}
```

### C# Kullanımı
```csharp
// Adet 0 ise: "Sepetinizde ürün yok"
// Adet 1 ise: "Sepetinizde 1 ürün var"
// Adet 5 ise: "Sepetinizde 5 ürün var"
string msg = _loc.GetPluralString(L.CartSummary, 5);
```

---

## 🤖 4. AI Otomatik Çeviri (Gemini)

Elinizdeki ana dil dosyasını (örneğin Türkçe) kullanarak diğer tüm dilleri Gemini AI ile saniyeler içinde oluşturabilirsiniz.

```csharp
public async Task AutoTranslate(IAiTranslatorService translator)
{
    var sourceResources = new Dictionary<string, string> { { "Welcome", "Hoş Geldiniz" } };
    
    var englishResult = await translator.TranslateAsync(
        sourceResources, 
        new CultureInfo("tr-TR"), 
        new CultureInfo("en-US"));
        
    // Çıktı: { "Welcome": "Welcome" }
}
```

---

## 🏗️ 5. Clean Architecture: Kendi Veri Tabanını Bağla

Eğer hazır bir e-ticaret veri tabanınız varsa, kütüphaneyi kendi tablolarınıza bağlayabilirsiniz.

### Provider Oluşturma
```csharp
public class MyDbProvider : IResourceProvider
{
    private readonly MyDbContext _db;
    public MyDbProvider(MyDbContext db) => _db = db;

    public int Priority => 500; // Dosyalardan daha öncelikli olması için

    public string? GetString(string key, CultureInfo culture)
    {
        return _db.Translations
            .FirstOrDefault(t => t.Key == key && t.LanguageCode == culture.Name)?.Value;
    }

    public IEnumerable<string> GetAllKeys(CultureInfo culture) => _db.Translations.Select(x => x.Key);
    public Task ReloadAsync(CancellationToken ct = default) => Task.CompletedTask;
}
```

### Kayıt (Program.cs)
```csharp
builder.Services.AddSingleton<IResourceProvider, MyDbProvider>();
```

---

## 🔢 6. Formatlama (Date, Number, Currency)

Kütüphane içinde gelen `IFormatterService` ile kültür uyumlu formatlama yapabilirsiniz.

```csharp
// Para birimi formatlama (₺1.250,75 veya $1,250.75)
string price = _formatter.FormatCurrency(1250.75m);

// Tarih formatlama
string date = _formatter.FormatDate(DateTime.Now);
```

---

## 📄 Dil Dosyası Formatları

### JSON (`resources.en-US.json`)
```json
{
  "Hello": "Hello World",
  "Welcome": "Welcome {Name}!"
}
```

### YAML (`resources.en-US.yaml`)
```yaml
Hello: "Hello World"
Welcome: "Welcome {Name}!"
```

---

## 📄 Lisans
Bu proje [MIT](LICENSE) lisansı ile korunmaktadır.
