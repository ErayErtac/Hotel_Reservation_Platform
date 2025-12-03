using System;
using System.Collections.Generic;
using System.Text;

namespace HotelReservation.Core.Entities
{
    // vw_HotelSummary için C# karşılığı
    public class HotelSummary
    {
        public int HotelId { get; set; }
        public string Name { get; set; } = null!;
        public string City { get; set; } = null!;
        public int RoomCount { get; set; }
        public decimal? AverageRating { get; set; }
        public int TotalReservations { get; set; }
        public decimal? TotalRevenue { get; set; }
    }
}
