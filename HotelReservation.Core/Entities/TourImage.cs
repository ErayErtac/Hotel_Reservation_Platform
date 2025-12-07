using System;
using System.Collections.Generic;
using System.Text;

namespace HotelReservation.Core.Entities
{
    public class TourImage
    {
        public int Id { get; set; }

        public int TourId { get; set; }

        public string ImagePath { get; set; } = null!; // /uploads/tours/{tourId}/abc.jpg gibi
        public bool IsMain { get; set; }
        public DateTime UploadedAt { get; set; }

        public Tour? Tour { get; set; }
    }
}

