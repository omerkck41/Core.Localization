# Core.Localization 🌍

Modern, modüler ve .NET 10.0 mimarisi için optimize edilmiş yerelleştirme kütüphanesi. Clean Architecture projelerine tam uyumludur ve hiçbir veri tabanı bağımlılığı taşımaz.

## 🚀 Hızlı Başlangıç

Bu kütüphaneyi projenize entegre etmek için aşağıdaki 3 adımı izlemeniz yeterlidir.

### 1. Kurulum
NuGet paketini projenize ekleyin:
```bash
dotnet add package Core.Localization
```

### 2. Yapılandırma (Dependency Injection)
Clean Architecture yapısında **WebAPI** veya **Web** katmanındaki `Program.cs` dosyasına aşağıdaki kodları ekleyin:

```csharp
using System.Globalization;
using Core.Localization.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Core.Localization Servis Kaydı
builder.Services.AddCoreLocalization(options =>
{
    // Varsayılan dil
    options.DefaultCulture = new CultureInfo("tr-TR");
    
    // Desteklenen diller
    options.SupportedCultures = new List<CultureInfo> 
    { 
        new CultureInfo("tr-TR"), 
        new CultureInfo("en-US") 
    };
    
    // Dil dosyalarının (JSON/YAML) bulunduğu klasör
    options.ResourcePaths = new List<string> { "Resources" };
});

// ASP.NET Core Middleware Kaydı (Dil tespiti için)
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("tr-TR")
    .AddSupportedCultures("tr-TR", "en-US")
    .AddSupportedUICultures("tr-TR", "en-US"));
```

### 3. Kullanım
Servislerinize veya Controller sınıflarınıza `ILocalizationService` enjekte ederek kullanmaya başlayın:

```csharp
public class ProductService
{
    private readonly ILocalizationService _localization;

    public ProductService(ILocalizationService localization)
    {
        _localization = localization;
    }

    public void ProcessOrder()
    {
        // 1. Basit Çeviri
        string msg = _localization.GetString("Welcome");

        // 2. İsimli Parametreler (E-Ticaret Senaryosu)
        string welcome = _localization.GetString("UserWelcome", new { Name = "Ahmet", Store = "CoreShop" });
        // Çıktı: "Merhaba Ahmet, CoreShop mağazasına hoş geldin!"

        // 3. Çoğullaştırma (Pluralization)
        string cartMsg = _localization.GetPluralString("CartItems", count: 5);
        // Çıktı: "Sepetinizde 5 adet ürün var."
    }
}
```

---

## 🏗️ Clean Architecture Entegrasyonu

Eğer projenizde mevcut bir veri tabanı mimarisi (EF Core, Dapper vb.) varsa, kütüphaneyi kendi tablolarınıza bağlamak çok basittir:

1. **Infrastructure** katmanında bir Provider sınıfı oluşturun:
```csharp
public class MyCustomDbProvider : IResourceProvider
{
    private readonly MyDbContext _db; // Mevcut DbContext'iniz
    public MyCustomDbProvider(MyDbContext db) => _db = db;

    public int Priority => 500; // JSON dosyalarından daha öncelikli olması için

    public string? GetString(string key, CultureInfo culture)
    {
        // Kendi tablonuzdan veriyi çekin
        return _db.Translations
            .FirstOrDefault(t => t.Key == key && t.LanguageCode == culture.Name)?.Value;
    }

    public IEnumerable<string> GetAllKeys(CultureInfo culture) => ...;
    public Task ReloadAsync(CancellationToken ct = default) => Task.CompletedTask;
}
```

2. **Program.cs** içinde bu sağlayıcıyı tanıtın:
```csharp
builder.Services.AddCoreLocalization();
builder.Services.AddScoped<IResourceProvider, MyCustomDbProvider>();
```

---

## 📄 Dil Dosyası Formatı (JSON)
`Resources/resources.tr-TR.json` dosyanız şu yapıda olmalıdır:

```json
{
  "Welcome": "Hoş Geldiniz",
  "UserWelcome": "Merhaba {Name}, {Store} mağazasına hoş geldin!",
  "CartItems": "{count, plural, zero{Sepetiniz boş} one{Sepetinizde 1 ürün var} other{Sepetinizde # ürün var}}"
}
```

## 🛠️ Özellikler
- **IStringLocalizer Uyumu:** .NET'in standart `IStringLocalizer` yapısıyla %100 uyumludur.
- **Yüksek Performans:** Akıllı Cache mekanizması ile en hızlı çeviri deneyimi.
- **Esnek:** JSON, YAML, Resx desteği varsayılan olarak gelir; veri tabanı desteği saniyeler içinde eklenebilir.

## 📄 Lisans
Bu proje [MIT](LICENSE) lisansı ile korunmaktadır.
