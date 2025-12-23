HOTEL RESERVATION PLATFORM

Bu proje, .NET 10, Razor Pages, MSSQL ve Entity Framework Core kullanılarak geliştirilmiş bir rezervasyon platformudur. 
Sistem; Admin, Otel Yöneticisi ve Müşteri rollerine göre farklı yetkiler içerir.

-------------------------------------------------------------
1) GEREKSİNİMLER
-------------------------------------------------------------
Aşağıdaki yazılımlar bilgisayarda kurulu olmalıdır:

- .NET SDK 8.0 veya üzeri
- Visual Studio 2022
- SQL Server (Express veya Developer Edition)
- SQL Server Management Studio (SSMS)

-------------------------------------------------------------
2) PROJE DOSYALARINI İNDİRME
-------------------------------------------------------------
Projeyi GitHub’dan klonlayın veya ZIP dosyasını açın.

GitHub Repo Linki: https://github.com/ErayErtac/Hotel_Reservation_Platform

Klasör yapısı şu şekildedir:

HotelReservationPlatform/
    HotelReservation.Core
    HotelReservation.Data
    HotelReservation.Web
    Database/HotelReservationDb.bak

-------------------------------------------------------------
3) VERİTABANINI .BAK DOSYASINDAN KURMA (ÖNEMLİ)
-------------------------------------------------------------
Proje ile birlikte gelen "HotelReservationDb.bak" dosyası sayesinde veritabanı SQL Server’a yüklenebilir.

ADIM 1: SSMS’i açın.
ADIM 2: "Databases" üzerine sağ tıklayın → "Restore Database…" seçin.
ADIM 3: "Source" bölümünde "Device" seçeneğini işaretleyin.
ADIM 4: .bak dosyasını seçin: HotelReservationDb.bak
ADIM 5: Restore edilecek veritabanı adı: HotelReservationDb
ADIM 6: "Options" sekmesinde gerekirse "Overwrite the existing database (WITH REPLACE)" işaretlenebilir.
ADIM 7: Restore işlemini başlatın.

İşlem tamamlandığında veritabanı kullanıma hazır olacaktır.

-------------------------------------------------------------
4) UYGULAMA AYARLARI (CONNECTION STRING)
-------------------------------------------------------------
HotelReservation.Web projesi içindeki appsettings.json dosyasını açın.
Aşağıdaki bağlantı bilgilerini kendi SQL Server adınıza göre düzenleyin:

"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=HotelReservationDb;Trusted_Connection=True;TrustServerCertificate=True;"
}

Eğer SQL Server adınız farklıysa değiştirilmelidir:
Örnek:
Server=DESKTOP-ADINIZ\SQLEXPRESS;

-------------------------------------------------------------
5) PROJEYİ ÇALIŞTIRMA
-------------------------------------------------------------
1. Visual Studio’da çözümü açın.
2. HotelReservation.Web projesine sağ tıklayıp "Set as Startup Project" seçin.
3. IIS Express veya Kestrel ile uygulamayı çalıştırın.

-------------------------------------------------------------
6) TEST KULLANICILARI
-------------------------------------------------------------
Veritabanı yedeği ile birlikte gelen hazır kullanıcılar:

ADMIN:
Email: admin@test.com
Şifre: 123456

OTEL YÖNETİCİSİ:
Email: manager@test.com
Şifre: 123456

MÜŞTERİ:
Email: customer@test.com
Şifre: 123456

-------------------------------------------------------------
7) PROJE ÖZELLİKLERİ
-------------------------------------------------------------
- Admin tarafından otel onaylama sistemi
- Otel ve oda ekleme / güncelleme / silme
- Müsait oda arama (şehir + tarih aralığı)
- Rezervasyon oluşturma, onaylama, reddetme
- Otel fotoğraf yükleme
- Otellere yorum yazma ve puanlama
- Müşterinin kendi rezervasyon geçmişi

-------------------------------------------------------------
8) SIK KARŞILAŞILAN HATALAR ve ÇÖZÜMLERİ
-------------------------------------------------------------

1) HATA: "Login failed for user"
ÇÖZÜM: SQL Server Authentication modunu kontrol edin. Windows Authentication tavsiye edilir.

2) HATA: "Database restore failed"
ÇÖZÜM: Restore sırasında dosya yolları çakışabilir. "Relocate all files" seçeneğini işaretleyin.

3) HATA: "localhost bağlanmayı reddetti"
ÇÖZÜM: Port çakışması olabilir. LaunchSettings içindeki port ayarlarını değiştirin.
