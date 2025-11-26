using System;
using System.Collections.Generic;
using System.Text;

namespace HotelReservation.Core.Enums
{
    public enum ReservationStatus
    {
        Pending = 1,     // Beklemede / onaylanmamış
        Confirmed = 2,   // Onaylanmış
        Cancelled = 3    // İptal edilmiş
    }
}
