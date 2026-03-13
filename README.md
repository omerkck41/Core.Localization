# Core.Localization 🌍

Modern, modüler ve .NET 10.0 mimarisi için optimize edilmiş, **Yapay Zeka Destekli** ve **Tip Güvenli** yerelleştirme kütüphanesi.

## ✨ Öne Çıkan Özellikler

- **💎 Tip Güvenliği (Source Generator):** "Magic string" hatalarına son! IntelliSense desteğiyle `L.Welcome` gibi kod tamamlamalı kullanım.
- **🤖 AI Otomatik Çeviri:** Google Gemini entegrasyonu ile dil dosyalarınızı saniyeler içinde tüm dünya dillerine otomatik çevirin.
- **🌍 Dünya Standartlarında Çoğullaştırma:** CLDR standartlarında; Türkçe, İngilizce, Arapça, Rusça gibi dillerin karmaşık çoğul kurallarını (Zero, One, Few, Many, Other) otomatik yönetir.
- **⚡ Yüksek Performans:** Akıllı in-memory caching ve düşük bellek tüketimi.
- **🧩 Database Agnostic:** Mevcut veri tabanı mimarinize (EF Core, Dapper vb.) saniyeler içinde entegre edilebilir.
- **📦 Universal:** Standart .NET `IStringLocalizer` arayüzü ile %100 uyumludur.

---

## 🚀 Hızlı Başlama

### 1. Kurulum
```bash
dotnet add package Core.Localization
```

### 2. Yapılandırma (Program.cs)
```csharp
builder.Services.AddCoreLocalization(options =>
{
    options.DefaultCulture = new CultureInfo("tr-TR");
    options.SupportedCultures = new List<CultureInfo> { new CultureInfo("tr-TR"), new CultureInfo("en-US") };
    options.ResourcePaths = new List<string> { "Resources" };
});
```

### 3. Tip Güvenli Kullanım (Source Generator)
Projenizdeki `.json` dosyalarını `.csproj` içinde `AdditionalFiles` olarak işaretleyin:
```xml
<ItemGroup>
  <AdditionalFiles Include="Resources\*.json" />
</ItemGroup>
```
Artık kodunuzda anahtarlara güvenle erişebilirsiniz:
```csharp
string msg = _localization.GetString(L.Welcome, new { Name = "Ahmet" });
```

---

## 🤖 AI Otomatik Çeviri (Gemini)

Elinizdeki Türkçe dil dosyasını saniyeler içinde İngilizceye (veya 100+ farklı dile) anlamlı bir şekilde çevirebilirsiniz:

```csharp
var translator = new GeminiTranslatorService("YOUR_GEMINI_API_KEY", logger);
var translations = await translator.TranslateAsync(sourceResources, 
    new CultureInfo("tr-TR"), 
    new CultureInfo("en-US"));
```

---

## 🌍 Gelişmiş Çoğullaştırma (Pluralization)

Karmaşık diller (Arapça, Rusça vb.) için ICU/CLDR standartlarında çoğul desteği:

```json
// resources.ru-RU.json (Rusça Örneği)
{
  "BookCount": "{count, plural, one{1 книга} few{# книги} many{# книг} other{# книги}}"
}
```
```csharp
// Otomatik olarak doğru formu seçer
_localization.GetPluralString(L.BookCount, 5); // Çıktı: "5 книг"
```

---

## 🏗️ Clean Architecture: Kendi Veri Tabanını Bağla

Mevcut e-ticaret veri tabanınızı kütüphaneye bağlamak için `IResourceProvider` arayüzünü uygulamanız yeterlidir:

```csharp
public class MyDbProvider : IResourceProvider
{
    public int Priority => 500;
    public string? GetString(string key, CultureInfo culture) => // DB'den getir...
    // ...
}

// Program.cs
builder.Services.AddSingleton<IResourceProvider, MyDbProvider>();
```

## 📄 Lisans
Bu proje [MIT](LICENSE) lisansı ile korunmaktadır.
