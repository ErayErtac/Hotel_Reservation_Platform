namespace HotelReservation.Core.Entities
{
    public class HotelAmenity
    {
        // Otele ait imkanlarý (amenities) temsil eden entity sýnýfý

        public int Id { get; set; }
        public int HotelId { get; set; }
        public Hotel? Hotel { get; set; }
        public string Name { get; set; } = null!; // WiFi, Pool, Gym, Spa, etc.
        public string? Icon { get; set; }
    }
}