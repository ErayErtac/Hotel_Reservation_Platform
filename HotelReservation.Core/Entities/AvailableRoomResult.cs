using System;
using System.Collections.Generic;
using System.Text;

namespace HotelReservation.Core.Entities
{
    // sp_SearchAvailableRooms sonucunu temsil eden "read only" model
    public class AvailableRoomResult
    {
        public int HotelId { get; set; }
        public string HotelName { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Address { get; set; } = null!;

        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = null!;
        public int Capacity { get; set; }
        public decimal PricePerNight { get; set; }
    }
}
