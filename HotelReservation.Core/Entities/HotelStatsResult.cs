using System;
using System.Collections.Generic;
using System.Text;

namespace HotelReservation.Core.Entities
{
    // sp_GetHotelStats için C# modeli
    public class HotelStatsResult
    {
        public int HotelId { get; set; }
        public string Name { get; set; } = null!;
        public string City { get; set; } = null!;
        public int RoomCount { get; set; }
        public int ReservationCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal? AverageRating { get; set; }
    }
}
