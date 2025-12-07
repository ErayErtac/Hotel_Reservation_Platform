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

        // Giriş yapmış kullanıcının ID'sini al
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return null;
            return userId;
        }

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

        public IActionResult OnGet(int roomId, DateTime? checkIn, DateTime? checkOut, int? guestCount)
        {
            // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToPage("/Account/Login", new { returnUrl = Request.Path + Request.QueryString });
            }

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                ErrorMessage = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                return RedirectToPage("/Account/Login");
            }

            RoomId = roomId;
            CheckIn = checkIn ?? DateTime.Today.AddDays(1);
            CheckOut = checkOut ?? DateTime.Today.AddDays(2);
            GuestCount = guestCount ?? 1;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToPage("/Account/Login", new { returnUrl = Request.Path });
            }

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                ErrorMessage = "Kullanıcı bilgisi alınamadı. Lütfen tekrar giriş yapın.";
                return RedirectToPage("/Account/Login");
            }

            if (RoomId <= 0)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz oda bilgisi.");
            }

            if (!CheckIn.HasValue || !CheckOut.HasValue)
            {
                ModelState.AddModelError(string.Empty, "Lütfen giriş ve çıkış tarihlerini seçin.");
            }

            if (CheckIn.HasValue && CheckOut.HasValue && CheckIn.Value >= CheckOut.Value)
            {
                ModelState.AddModelError(string.Empty, "Çıkış tarihi giriş tarihinden sonra olmalıdır.");
            }

            if (CheckIn.HasValue && CheckIn.Value.Date < DateTime.Today)
            {
                ModelState.AddModelError(nameof(CheckIn), "Giriş tarihi bugünden önce olamaz.");
            }

            if (GuestCount <= 0)
            {
                ModelState.AddModelError(nameof(GuestCount), "Kişi sayısı en az 1 olmalıdır.");
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
                    Value = userId.Value
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
                // SP içindeki RAISERROR'lar da buraya düşüyor
                ErrorMessage = "Rezervasyon oluşturulurken hata oluştu: " + ex.Message;
            }

            return Page();
        }
    }
}
