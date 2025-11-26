using System;
using System.Collections.Generic;
using System.Text;

namespace HotelReservation.Core.Entities
{
    public class Hotel
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string? Description { get; set; }

        public bool IsApproved { get; set; } = false; // Admin onayı
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Otel yöneticisi
        public int ManagerId { get; set; }
        public AppUser Manager { get; set; } = null!;

        // Navigation
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
        public ICollection<HotelReview> Reviews { get; set; } = new List<HotelReview>();
    }
}
