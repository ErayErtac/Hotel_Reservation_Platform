using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using HotelReservation.Core.Entities;
using HotelReservation.Core.Enums;
using HotelReservation.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Data
{
    public static class DbInitializer
    {
        public static void Seed(HotelDbContext context)
        {
            // Migration'lar uygulanmamışsa burayı atla
            context.Database.Migrate();

            // Zaten kullanıcı varsa seed çalıştırma (bir kere çalışsın)
            if (context.Users.Any())
                return;

            // 1) Kullanıcılar (Admin, Otel Yöneticileri, Müşteriler)

            var admin = new AppUser
            {
                FullName = "System Admin",
                Email = "admin@hotelplatform.com",
                PasswordHash = PasswordHasher.HashPassword("admin123"),
                Role = UserRole.Admin,
                IsActive = true
            };

            var manager1 = new AppUser
            {
                FullName = "Ali Otelci",
                Email = "ali.manager@hotelplatform.com",
                PasswordHash = PasswordHasher.HashPassword("manager1"),
                Role = UserRole.HotelManager,
                IsActive = true
            };

            var manager2 = new AppUser
            {
                FullName = "Ayşe Otelci",
                Email = "ayse.manager@hotelplatform.com",
                PasswordHash = PasswordHasher.HashPassword("manager2"),
                Role = UserRole.HotelManager,
                IsActive = true
            };

            var customer1 = new AppUser
            {
                FullName = "Mehmet Müşteri",
                Email = "mehmet.customer@hotelplatform.com",
                PasswordHash = PasswordHasher.HashPassword("customer1"),
                Role = UserRole.Customer,
                IsActive = true
            };

            var customer2 = new AppUser
            {
                FullName = "Zeynep Müşteri",
                Email = "zeynep.customer@hotelplatform.com",
                PasswordHash = PasswordHasher.HashPassword("customer2"),
                Role = UserRole.Customer,
                IsActive = true
            };

            context.Users.AddRange(admin, manager1, manager2, customer1, customer2);
            context.SaveChanges();

            // 2) Oteller

            var hotel1 = new Hotel
            {
                Name = "Deniz Manzaralı Otel",
                City = "Antalya",
                Address = "Konyaaltı, Antalya",
                Description = "Deniz manzaralı, aile dostu bir otel.",
                IsApproved = true,
                IsActive = true,
                ManagerId = manager1.Id, // az önce eklediğimiz manager
                CreatedAt = DateTime.UtcNow
            };

            var hotel2 = new Hotel
            {
                Name = "Şehir Merkezi Otel",
                City = "İstanbul",
                Address = "Taksim, İstanbul",
                Description = "Merkeze yakın, iş seyahatleri için ideal.",
                IsApproved = true,
                IsActive = true,
                ManagerId = manager2.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Hotels.AddRange(hotel1, hotel2);
            context.SaveChanges();

            // 3) Odalar

            var room1 = new Room
            {
                HotelId = hotel1.Id,
                RoomNumber = "101",
                Capacity = 2,
                PricePerNight = 1200,
                Description = "Deniz manzaralı çift kişilik oda.",
                IsActive = true
            };

            var room2 = new Room
            {
                HotelId = hotel1.Id,
                RoomNumber = "102",
                Capacity = 3,
                PricePerNight = 1500,
                Description = "Aile odası.",
                IsActive = true
            };

            var room3 = new Room
            {
                HotelId = hotel2.Id,
                RoomNumber = "201",
                Capacity = 1,
                PricePerNight = 900,
                Description = "Tek kişilik ekonomik oda.",
                IsActive = true
            };

            var room4 = new Room
            {
                HotelId = hotel2.Id,
                RoomNumber = "202",
                Capacity = 2,
                PricePerNight = 1300,
                Description = "Çift kişilik oda.",
                IsActive = true
            };

            context.Rooms.AddRange(room1, room2, room3, room4);
            context.SaveChanges();

            // 4) Örnek bir kaç rezervasyon + yorum da ekleyebiliriz (şimdilik 1-2 tane)

            var reservation1 = new Reservation
            {
                CustomerId = customer1.Id,
                RoomId = room1.Id,
                CheckIn = DateTime.Today.AddDays(7),
                CheckOut = DateTime.Today.AddDays(10),
                GuestCount = 2,
                TotalPrice = 1200 * 3, // 3 gece
                Status = ReservationStatus.Confirmed,
                CreatedAt = DateTime.UtcNow
            };

            var reservation2 = new Reservation
            {
                CustomerId = customer2.Id,
                RoomId = room3.Id,
                CheckIn = DateTime.Today.AddDays(3),
                CheckOut = DateTime.Today.AddDays(5),
                GuestCount = 1,
                TotalPrice = 900 * 2, // 2 gece
                Status = ReservationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            context.Reservations.AddRange(reservation1, reservation2);
            context.SaveChanges();

            var review1 = new HotelReview
            {
                HotelId = hotel1.Id,
                CustomerId = customer1.Id,
                Rating = 5,
                Comment = "Oda çok temizdi ve manzara harikaydı.",
                CreatedAt = DateTime.UtcNow
            };

            var review2 = new HotelReview
            {
                HotelId = hotel2.Id,
                CustomerId = customer2.Id,
                Rating = 4,
                Comment = "Konum süperdi ama oda biraz küçüktü.",
                CreatedAt = DateTime.UtcNow
            };

            context.HotelReviews.AddRange(review1, review2);
            context.SaveChanges();
        }
    }
}
