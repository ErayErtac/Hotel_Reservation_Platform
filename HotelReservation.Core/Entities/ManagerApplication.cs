using System;

namespace HotelReservation.Core.Entities
{
    public class ManagerApplication
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        public AppUser User { get; set; } = null!;
        
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
        
        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedDate { get; set; }
        public DateTime? RejectedDate { get; set; }
        
        public int? ApprovedByUserId { get; set; }
        public AppUser? ApprovedByUser { get; set; }
        
        public string? RejectionReason { get; set; }
        public string? Notes { get; set; } // Admin notları
    }
}

