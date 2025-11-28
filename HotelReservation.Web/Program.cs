using HotelReservation.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// DbContext kaydý
builder.Services.AddDbContext<HotelDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("HotelConnection");
    options.UseSqlServer(connectionString);
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

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
