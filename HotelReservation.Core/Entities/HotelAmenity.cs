namespace HotelReservation.Core.Entities
{
    public class HotelAmenity
    {
        public int Id { get; set; }
        public int HotelId { get; set; }
        public Hotel? Hotel { get; set; }
        public string Name { get; set; } = null!; // WiFi, Pool, Gym, Spa, etc.
        public string? Icon { get; set; } // Optional icon class name
    }
}

