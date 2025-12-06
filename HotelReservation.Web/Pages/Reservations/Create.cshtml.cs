using HotelReservation.Core.Entities;
using HotelReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HotelReservation.Web.Pages.Reservations
{
    public class CreateModel : PageModel
    {
        private readonly HotelDbContext _context;
        public CreateModel(HotelDbContext context)
    {
        _context = context;
    }

        // Normalde login olan kullanicidan alacagiz, simdilik sabit bir musteri
        private int CurrentUserId =>
                int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);


        [BindProperty(SupportsGet = true)]
    public int RoomId { get; set; }

    [BindProperty]
    public DateTime? CheckIn { get; set; }

    [BindProperty]
    public DateTime? CheckOut { get; set; }

    [BindProperty]
    public int GuestCount { get; set; } = 1;

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public void OnGet(int roomId, DateTime? checkIn, DateTime? checkOut, int? guestCount)
    {
        RoomId = roomId;
        CheckIn = checkIn ?? DateTime.Today.AddDays(1);
        CheckOut = checkOut ?? DateTime.Today.AddDays(2);
        GuestCount = guestCount ?? 1;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (RoomId <= 0)
        {
            ModelState.AddModelError(string.Empty, "Gecersiz oda bilgisi.");
        }

        if (!CheckIn.HasValue || !CheckOut.HasValue)
        {
            ModelState.AddModelError(string.Empty, "Lutfen giris ve cikis tarihlerini secin.");
        }

        if (GuestCount <= 0)
        {
            ModelState.AddModelError(nameof(GuestCount), "Kisi sayisi en az 1 olmalidir.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.sp_CreateReservation";
            command.CommandType = CommandType.StoredProcedure;

                // Parametreler
        var pCustomerId = new SqlParameter("@CustomerId", SqlDbType.Int)
            {
                Value = CurrentUserId
            };
                var pRoomId = new SqlParameter("@RoomId", SqlDbType.Int)
            {
                Value = RoomId
            };
            var pCheckIn = new SqlParameter("@CheckIn", SqlDbType.Date)
            {
                Value = CheckIn!.Value.Date
            };
            var pCheckOut = new SqlParameter("@CheckOut", SqlDbType.Date)
            {
                Value = CheckOut!.Value.Date
            };
            var pGuestCount = new SqlParameter("@GuestCount", SqlDbType.Int)
            {
                Value = GuestCount
            };
            var pReservationId = new SqlParameter("@ReservationId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            command.Parameters.AddRange(new[]
            {
                    pCustomerId, pRoomId, pCheckIn, pCheckOut, pGuestCount, pReservationId
                });

            await command.ExecuteNonQueryAsync();

            var newId = (int)pReservationId.Value;
            return RedirectToPage("/Reservations/Confirmation", new { id = newId });
        }
        catch (Exception ex)
        {
            // SP icindeki RAISERROR'lar da buraya dusuyor
            ErrorMessage = "Rezervasyon olusturulurken hata olustu: " + ex.Message;
        }

        return Page();
    }
}
}
