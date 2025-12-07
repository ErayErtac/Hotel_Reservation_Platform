using System;
using System.Collections.Generic;
using System.Text;

namespace HotelReservation.Core.Entities
{
    public class HotelReview
    {
        public int Id { get; set; }

        public int HotelId { get; set; }
        public Hotel Hotel { get; set; } = null!;

        public int CustomerId { get; set; }
        public AppUser Customer { get; set; } = null!;

        public int Rating { get; set; } // 1-5 arası
        public string? Comment { get; set; }

        public int? ReservationId { get; set; } // Hangi rezervasyon için yorum yapıldı
        public Reservation? Reservation { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<ReviewReply> Replies { get; set; } = new List<ReviewReply>();
    }
}
