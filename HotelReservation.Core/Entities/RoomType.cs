namespace HotelReservation.Core.Entities
{
    public class RoomType
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // Standard, Deluxe, Suite, etc.
        public string? Description { get; set; }
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}

