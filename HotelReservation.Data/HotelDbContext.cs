using HotelReservation.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace HotelReservation.Data
{
    public class HotelDbContext : DbContext
    {
        public HotelDbContext(DbContextOptions<HotelDbContext> options)
            : base(options)
        {
        }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<HotelReview> HotelReviews { get; set; }
        public DbSet<AvailableRoomResult> AvailableRoomResults { get; set; }
        public DbSet<HotelSummary> HotelSummaries { get; set; }
        public DbSet<HotelStatsResult> HotelStatsResults { get; set; }
        public DbSet<CustomerReservationResult> CustomerReservationResults { get; set; }
        public DbSet<HotelImage> HotelImages { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<HotelAmenity> HotelAmenities { get; set; }
        public DbSet<RoomAmenity> RoomAmenities { get; set; }
        public DbSet<ManagerApplication> ManagerApplications { get; set; }
        public DbSet<ReviewReply> ReviewReplies { get; set; }
        public DbSet<Tour> Tours { get; set; }
        public DbSet<TourBooking> TourBookings { get; set; }
        public DbSet<TourImage> TourImages { get; set; }
        public DbSet<TourReview> TourReviews { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // AppUser
            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.Property(u => u.FullName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.HasIndex(u => u.Email)
                      .IsUnique(); // aynı mailden 2 tane olmasın
            });

            // Hotel
            modelBuilder.Entity<Hotel>(entity =>
            {
                entity.Property(h => h.Name)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(h => h.City)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(h => h.Address)
                      .IsRequired()
                      .HasMaxLength(250);

                entity.HasOne(h => h.Manager)
                      .WithMany(u => u.ManagedHotels)
                      .HasForeignKey(h => h.ManagerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(h => h.Images)
                      .WithOne(i => i.Hotel)
                      .HasForeignKey(i => i.HotelId)
                      .OnDelete(DeleteBehavior.Cascade);

                // 🔽 Hotels tablosundaki trigger'ı da bildir
                entity.ToTable(tb =>
                {
                    tb.HasTrigger("trg_Hotels_StatusHistory");
                });
            });

            // Room
            modelBuilder.Entity<Room>(entity =>
            {
                entity.Property(r => r.RoomNumber)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.Property(r => r.PricePerNight)
                      .HasColumnType("decimal(18,2)");

                entity.HasOne(r => r.Hotel)
                      .WithMany(h => h.Rooms)
                      .HasForeignKey(r => r.HotelId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.RoomType)
                      .WithMany(rt => rt.Rooms)
                      .HasForeignKey(r => r.RoomTypeId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Reservation
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.Property(r => r.TotalPrice)
                      .HasColumnType("decimal(18,2)");

                entity.HasOne(r => r.Customer)
                      .WithMany(u => u.Reservations)
                      .HasForeignKey(r => r.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Room)
                      .WithMany(room => room.Reservations)
                      .HasForeignKey(r => r.RoomId)
                      .OnDelete(DeleteBehavior.Restrict);

                //EF Core'a bu tabloda trigger olduğunu söyle
                entity.ToTable(tb =>
                {
                    tb.HasTrigger("trg_Reservations_Insert_Audit");
                    tb.HasTrigger("trg_Reservations_Update_Audit");
                    tb.HasTrigger("trg_Reservations_Delete_Audit");
                });
            });

            // HotelReview
            modelBuilder.Entity<HotelReview>(entity =>
            {
                entity.Property(hr => hr.Rating)
                      .IsRequired();

                entity.Property(hr => hr.Comment)
                      .HasMaxLength(1000);

                entity.HasOne(hr => hr.Hotel)
                      .WithMany(h => h.Reviews)
                      .HasForeignKey(hr => hr.HotelId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(hr => hr.Customer)
                      .WithMany(u => u.Reviews)
                      .HasForeignKey(hr => hr.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(hr => hr.Reservation)
                      .WithMany()
                      .HasForeignKey(hr => hr.ReservationId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ReviewReply
            modelBuilder.Entity<ReviewReply>(entity =>
            {
                entity.Property(rr => rr.ReplyText)
                      .IsRequired()
                      .HasMaxLength(1000);

                entity.HasOne(rr => rr.Review)
                      .WithMany(r => r.Replies)
                      .HasForeignKey(rr => rr.ReviewId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rr => rr.Manager)
                      .WithMany()
                      .HasForeignKey(rr => rr.ManagerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Keyless entity: Stored Procedure sonucu
            modelBuilder.Entity<AvailableRoomResult>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); // Gerçek bir view'e map etmiyor, sadece query için kullanıyoruz
            });

            // vw_HotelSummary view'i için keyless entity
            modelBuilder.Entity<HotelSummary>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_HotelSummary"); // view adı
            });

            // sp_GetHotelStats sonucu için keyless entity
            modelBuilder.Entity<HotelStatsResult>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); // gerçek bir view'e bağlı değil, sadece FromSql için
            });

            // sp_GetCustomerReservations sonucu için keyless entity
            modelBuilder.Entity<CustomerReservationResult>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });

            modelBuilder.Entity<HotelImage>(entity =>
            {
                entity.Property(i => i.ImagePath)
                      .IsRequired()
                      .HasMaxLength(500);

                entity.Property(i => i.UploadedAt)
                      .HasDefaultValueSql("GETUTCDATE()");
            });

            // RoomType
            modelBuilder.Entity<RoomType>(entity =>
            {
                entity.Property(rt => rt.Name)
                      .IsRequired()
                      .HasMaxLength(100);
            });

            // HotelAmenity
            modelBuilder.Entity<HotelAmenity>(entity =>
            {
                entity.Property(ha => ha.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.HasOne(ha => ha.Hotel)
                      .WithMany(h => h.Amenities)
                      .HasForeignKey(ha => ha.HotelId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // RoomAmenity
            modelBuilder.Entity<RoomAmenity>(entity =>
            {
                entity.Property(ra => ra.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.HasOne(ra => ra.Room)
                      .WithMany(r => r.Amenities)
                      .HasForeignKey(ra => ra.RoomId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ManagerApplication
            modelBuilder.Entity<ManagerApplication>(entity =>
            {
                entity.HasOne(ma => ma.User)
                      .WithMany()
                      .HasForeignKey(ma => ma.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ma => ma.ApprovedByUser)
                      .WithMany()
                      .HasForeignKey(ma => ma.ApprovedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(ma => ma.RejectionReason)
                      .HasMaxLength(500);

                entity.Property(ma => ma.Notes)
                      .HasMaxLength(1000);
            });

            // Tour
            modelBuilder.Entity<Tour>(entity =>
            {
                entity.Property(t => t.Name)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(t => t.City)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(t => t.Price)
                      .HasColumnType("decimal(18,2)");

                entity.HasOne(t => t.Manager)
                      .WithMany(u => u.ManagedTours)
                      .HasForeignKey(t => t.ManagerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(t => t.Images)
                      .WithOne(i => i.Tour)
                      .HasForeignKey(i => i.TourId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // TourBooking
            modelBuilder.Entity<TourBooking>(entity =>
            {
                entity.Property(tb => tb.TotalPrice)
                      .HasColumnType("decimal(18,2)");

                entity.HasOne(tb => tb.Customer)
                      .WithMany(u => u.TourBookings)
                      .HasForeignKey(tb => tb.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(tb => tb.Tour)
                      .WithMany(t => t.Bookings)
                      .HasForeignKey(tb => tb.TourId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // TourImage
            modelBuilder.Entity<TourImage>(entity =>
            {
                entity.Property(i => i.ImagePath)
                      .IsRequired()
                      .HasMaxLength(500);

                entity.Property(i => i.UploadedAt)
                      .HasDefaultValueSql("GETUTCDATE()");
            });

            // TourReview
            modelBuilder.Entity<TourReview>(entity =>
            {
                entity.Property(tr => tr.Rating)
                      .IsRequired();

                entity.Property(tr => tr.Comment)
                      .HasMaxLength(1000);

                entity.HasOne(tr => tr.Tour)
                      .WithMany(t => t.Reviews)
                      .HasForeignKey(tr => tr.TourId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(tr => tr.Customer)
                      .WithMany(u => u.TourReviews)
                      .HasForeignKey(tr => tr.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(tr => tr.TourBooking)
                      .WithMany()
                      .HasForeignKey(tr => tr.TourBookingId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

        }
    }
}
