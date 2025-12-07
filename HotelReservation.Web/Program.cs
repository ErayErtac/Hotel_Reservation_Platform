using HotelReservation.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//builder.Services.AddRazorPages();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Manager", "ManagerOnly");
    // Customer klasörü için klasör seviyesinde authorization yapmıyoruz, sayfa seviyesinde yapıyoruz
    
    // Customer sayfaları - sayfa bazlı authorization
    options.Conventions.AuthorizePage("/Customer/MyReservations", "CustomerOrManager");
    options.Conventions.AuthorizePage("/Customer/CancelReservation", "CustomerOrManager");
    options.Conventions.AuthorizePage("/Customer/Profile", "CustomerOrManager");
    options.Conventions.AuthorizePage("/Customer/BecomeManager", "CustomerOnly"); // Sadece Customer başvurabilir
    options.Conventions.AuthorizePage("/Customer/CreateReview", "CustomerOrManager");
    
    // Rezervasyon sayfaları
    options.Conventions.AuthorizePage("/Reservations/Create", "CustomerOrManager");
    options.Conventions.AuthorizePage("/Reservations/Confirmation", "CustomerOrManager");

    // Giriş ve erişim reddedildi sayfaları herkese açık
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/Register");
    options.Conventions.AllowAnonymousToPage("/Account/AccessDenied");
});

// DbContext kaydı
builder.Services.AddDbContext<HotelDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("HotelConnection");
    options.UseSqlServer(connectionString);
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";       // giriş sayfası
        options.LogoutPath = "/Account/Logout";     // çıkış sayfası
        options.AccessDeniedPath = "/Account/AccessDenied"; // (istersek yaparız)
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("ManagerOnly", policy =>
        policy.RequireRole("HotelManager"));

    options.AddPolicy("CustomerOnly", policy =>
        policy.RequireRole("Customer"));
    
    options.AddPolicy("CustomerOrManager", policy =>
        policy.RequireRole("Customer", "HotelManager"));
});


var app = builder.Build();

// SEED ÇAĞRISI (scope açarak)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
    DbInitializer.Seed(dbContext);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
