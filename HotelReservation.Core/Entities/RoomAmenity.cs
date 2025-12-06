namespace HotelReservation.Core.Entities
{
    public class RoomAmenity
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public Room? Room { get; set; }
        public string Name { get; set; } = null!; // TV, Mini Bar, Balcony, etc.
        public string? Icon { get; set; } // Optional icon class name
    }
}

