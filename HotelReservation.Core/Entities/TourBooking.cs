using HotelReservation.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HotelReservation.Core.Entities
{
    public class TourBooking
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public AppUser Customer { get; set; } = null!;

        public int TourId { get; set; }
        public Tour Tour { get; set; } = null!;

        public int ParticipantCount { get; set; } // Kaç kişi için rezervasyon

        public decimal TotalPrice { get; set; }

        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        public bool IsPaid { get; set; } = false;
        public DateTime? PaidAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
    }
}

