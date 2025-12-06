using HotelReservation.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HotelReservation.Core.Entities
{
    public class Reservation
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public AppUser Customer { get; set; } = null!;

        public int RoomId { get; set; }
        public Room Room { get; set; } = null!;

        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }

        public int GuestCount { get; set; }

        public decimal TotalPrice { get; set; }

        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        public bool IsPaid { get; set; } = false;
        public DateTime? PaidAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
    }
}
