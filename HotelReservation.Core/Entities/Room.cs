using System;
using System.Collections.Generic;
using System.Text;

namespace HotelReservation.Core.Entities
{
    public class Room
    {
        public int Id { get; set; }

        public int HotelId { get; set; }
        public Hotel? Hotel { get; set; } = null!;

        public string RoomNumber { get; set; } = null!;
        public int Capacity { get; set; }
        public decimal PricePerNight { get; set; }
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
