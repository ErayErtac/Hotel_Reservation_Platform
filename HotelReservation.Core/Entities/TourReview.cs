using System;
using System.Collections.Generic;
using System.Text;

namespace HotelReservation.Core.Entities
{
    public class TourReview
    {
        public int Id { get; set; }

        public int TourId { get; set; }
        public Tour Tour { get; set; } = null!;

        public int CustomerId { get; set; }
        public AppUser Customer { get; set; } = null!;

        public int Rating { get; set; } // 1-5 arası
        public string? Comment { get; set; }

        public int? TourBookingId { get; set; } // Hangi rezervasyon için yorum yapıldı
        public TourBooking? TourBooking { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}

