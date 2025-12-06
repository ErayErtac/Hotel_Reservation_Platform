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
    options.Conventions.AuthorizeFolder("/Customer", "CustomerOnly");

    // Giriþ sayfasý herkese açýk
    options.Conventions.AllowAnonymousToPage("/Account/Login");
});

// DbContext kaydý
builder.Services.AddDbContext<HotelDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("HotelConnection");
    options.UseSqlServer(connectionString);
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";       // giriþ sayfasý
        options.LogoutPath = "/Account/Logout";     // çýkýþ sayfasý
        options.AccessDeniedPath = "/Account/AccessDenied"; // (istersek yaparýz)
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("ManagerOnly", policy =>
        policy.RequireRole("HotelManager"));

    options.AddPolicy("CustomerOnly", policy =>
        policy.RequireRole("Customer"));
});


var app = builder.Build();

// SEED ÇAÐRISI (scope açarak)
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
