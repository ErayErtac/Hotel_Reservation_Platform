using System;

namespace HotelReservation.Core.Entities
{
    public class ReviewReply
    {
        public int Id { get; set; }
        
        public int ReviewId { get; set; }
        public HotelReview Review { get; set; } = null!;
        
        public int ManagerId { get; set; }
        public AppUser Manager { get; set; } = null!;
        
        public string ReplyText { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

