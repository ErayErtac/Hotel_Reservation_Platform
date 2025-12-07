using System;
using System.Collections.Generic;
using System.Text;

namespace HotelReservation.Core.Entities
{
    public class Tour
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string City { get; set; } = null!;
        public string? Description { get; set; }
        public string? Itinerary { get; set; } // Tur programı

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal Price { get; set; }
        public int Capacity { get; set; } // Maksimum katılımcı sayısı
        public int CurrentBookings { get; set; } = 0; // Mevcut rezervasyon sayısı

        public bool IsApproved { get; set; } = false; // Admin onayı
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false; // Ana sayfada önerilen olarak gösterilsin mi

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Tur yöneticisi
        public int ManagerId { get; set; }
        public AppUser? Manager { get; set; } = null!;

        // Navigation
        public ICollection<TourBooking> Bookings { get; set; } = new List<TourBooking>();
        public ICollection<TourImage> Images { get; set; } = new List<TourImage>();
        public ICollection<TourReview> Reviews { get; set; } = new List<TourReview>();
    }
}

