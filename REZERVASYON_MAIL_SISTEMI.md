# Rezervasyon Otomatik Mail Sistemi

Bu sistem, kütüphane koltuk rezervasyonları için otomatik mail bildirimleri gönderir.

## Özellikler

### 1. Rezervasyon Onay Maili
- Kullanıcı rezervasyon oluşturduğunda hemen gönderilir
- Rezervasyon detaylarını içerir (koltuk numarası, tarih, saat)
- HTML formatında güzel tasarımlı mail

### 2. Rezervasyon Hatırlatma Maili
- Rezervasyon bitişinden 15 dakika önce gönderilir
- Kalan süreyi belirtir
- Kullanıcıyı uyarır

### 3. Rezervasyon Sona Erme Maili
- Rezervasyon süresi dolduğunda gönderilir
- Rezervasyon otomatik olarak sona erdirilir
- Koltuk başka kullanıcılar için serbest bırakılır

## Kurulum

### 1. SMTP Ayarları
`Library.Web/appsettings.json` dosyasında SMTP ayarlarını güncelleyin:

```json
{
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "User": "your-email@gmail.com",
    "Pass": "your-app-password",
    "From": "your-email@gmail.com"
  }
}
```

### 2. Gmail App Password Oluşturma
1. Google Hesabınıza gidin
2. Güvenlik > 2 Adımlı Doğrulama'yı açın
3. Uygulama Şifreleri > Diğer > "Kütüphane Sistemi" yazın
4. Oluşturulan 16 haneli şifreyi `Pass` alanına yazın

### 3. Uygulamayı Çalıştırma
```bash
cd Library.Web
dotnet run
```

## Sistem Çalışma Mantığı

### Rezervasyon Oluşturma
1. Kullanıcı rezervasyon oluşturur
2. `SeatReservationJobService.ScheduleReservationJobs()` çağrılır
3. Hangfire ile job'lar planlanır:
   - Onay maili: Hemen gönderilir
   - Hatırlatma maili: 15 dakika önce
   - Sona erdirme: Bitiş zamanında

### Job'ların Çalışması
- **Onay Maili**: Rezervasyon oluşturulduğunda hemen gönderilir
- **Hatırlatma Maili**: 15 dakika önce kullanıcıyı uyarır
- **Sona Erdirme**: Süre dolduğunda rezervasyonu sona erdirir ve bildirim gönderir

## Mail Şablonları

### Onay Maili
- Mavi renk teması
- Rezervasyon detayları
- Güvenlik bilgisi

### Hatırlatma Maili
- Turuncu renk teması
- Kalan süre bilgisi
- Uyarı mesajı

### Sona Erme Maili
- Kırmızı renk teması
- Rezervasyon sona erdi bilgisi
- Yeni rezervasyon önerisi

## Teknik Detaylar

### Kullanılan Teknolojiler
- **Hangfire**: Background job scheduling
- **MailKit**: SMTP mail gönderimi
- **SignalR**: Real-time güncellemeler
- **Entity Framework**: Veritabanı işlemleri

### Dosya Yapısı
```
Library.Web/Services/
├── SeatReservationNotificationService.cs  # Mail şablonları
├── SeatReservationJobService.cs          # Job scheduling
└── MailService.cs                        # SMTP işlemleri
```

### Veritabanı
- `SeatReservations` tablosu kullanılır
- Rezervasyon ID'leri job'lar için kullanılır
- Otomatik temizlik yapılır

## Hata Yönetimi

### Mail Gönderimi Hataları
- Try-catch blokları ile yakalanır
- Console'a log yazılır
- Uygulama çökmeye devam eder

### Job Hataları
- Hangfire dashboard'da görüntülenir
- Otomatik retry mekanizması
- Manuel müdahale imkanı

## Test Etme

### 1. Test Rezervasyonu
1. Sisteme giriş yapın
2. Koltuk rezervasyonu oluşturun
3. Mail kutunuzu kontrol edin

### 2. Job Testi
1. Hangfire Dashboard'a gidin: `/hangfire`
2. Job'ları inceleyin
3. Manuel job tetikleme

### 3. Mail Testi
```csharp
// Test mail gönderimi
var mailService = serviceProvider.GetService<MailService>();
await mailService.SendMailAsync("test@example.com", "Test", "Test mail");
```

## Güvenlik

### SMTP Güvenliği
- TLS/SSL kullanımı
- App password ile güvenli erişim
- Rate limiting

### Veri Güvenliği
- Kullanıcı bilgileri şifrelenir
- Mail içeriği HTML escape edilir
- SQL injection koruması

## Performans

### Optimizasyonlar
- Asenkron mail gönderimi
- Background job kullanımı
- Veritabanı indeksleri

### Monitoring
- Hangfire dashboard
- Console logları
- Mail gönderim durumu

## Sorun Giderme

### Mail Gönderilmiyor
1. SMTP ayarlarını kontrol edin
2. Gmail app password'ü doğru mu?
3. Firewall ayarlarını kontrol edin

### Job Çalışmıyor
1. Hangfire dashboard'u kontrol edin
2. Veritabanı bağlantısını test edin
3. Log dosyalarını inceleyin

### Rezervasyon Sona Ermiyor
1. Job'ların planlandığını kontrol edin
2. Zaman dilimi ayarlarını kontrol edin
3. Veritabanı işlemlerini test edin 