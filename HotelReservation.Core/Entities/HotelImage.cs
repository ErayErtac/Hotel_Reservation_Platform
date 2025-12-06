using System;
using System.Collections.Generic;
using System.Text;

namespace HotelReservation.Core.Entities
{
    public class HotelImage
    {
        public int Id { get; set; }

        public int HotelId { get; set; }

        public string ImagePath { get; set; } = null!; // /uploads/hotels/{hotelId}/abc.jpg gibi
        public bool IsMain { get; set; }
        public DateTime UploadedAt { get; set; }

        public Hotel? Hotel { get; set; }
    }
}
