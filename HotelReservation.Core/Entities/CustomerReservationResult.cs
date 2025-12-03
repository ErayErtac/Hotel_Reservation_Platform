using System;
using System.Collections.Generic;
using System.Text;
using HotelReservation.Core.Enums;

namespace HotelReservation.Core.Entities
{
    public class CustomerReservationResult
    {
        public int ReservationId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int GuestCount { get; set; }
        public decimal TotalPrice { get; set; }
        public ReservationStatus Status { get; set; }

        public string HotelName { get; set; } = null!;
        public string City { get; set; } = null!;
        public string RoomNumber { get; set; } = null!;
    }
}
