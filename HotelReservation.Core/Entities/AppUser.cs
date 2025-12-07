using System;
using System.Collections.Generic;
using System.Text;
using HotelReservation.Core.Enums;


namespace HotelReservation.Core.Entities
{
    public class AppUser
    {
        public int Id { get; set; }

        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;

        // Proje için basit bir alan bırakıyoruz, gerçek hayatta hashing şart :)
        public string PasswordHash { get; set; } = null!;

        public UserRole Role { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<Hotel> ManagedHotels { get; set; } = new List<Hotel>();
        public ICollection<Tour> ManagedTours { get; set; } = new List<Tour>();
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public ICollection<TourBooking> TourBookings { get; set; } = new List<TourBooking>();
        public ICollection<HotelReview> Reviews { get; set; } = new List<HotelReview>();
        public ICollection<TourReview> TourReviews { get; set; } = new List<TourReview>();
    }
}
