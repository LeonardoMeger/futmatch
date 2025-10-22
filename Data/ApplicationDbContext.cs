using FutMatchApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace FutMatchApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Court> Courts { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();

                entity.HasOne(e => e.SelectedTeam)
                    .WithMany()
                    .HasForeignKey(e => e.SelectedTeamId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Teams)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Court>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PrecoPorHora).HasColumnType("decimal(10,2)");
                entity.Property(e => e.GoogleRating).HasColumnType("decimal(3,2)");
                entity.Property(e => e.Latitude).HasColumnType("decimal(10,8)");
                entity.Property(e => e.Longitude).HasColumnType("decimal(11,8)");

                entity.HasIndex(e => e.GooglePlaceId);
            });

            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Reservations)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Team)
                    .WithMany(t => t.Reservations)
                    .HasForeignKey(e => e.TeamId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Court)
                    .WithMany(c => c.Reservations)
                    .HasForeignKey(e => e.CourtId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.OpponentTeam)
                    .WithMany()
                    .HasForeignKey(e => e.OpponentTeamId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.OpponentUser)
                    .WithMany()
                    .HasForeignKey(e => e.OpponentUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Reservation)
                .WithMany()
                .HasForeignKey(n => n.ReservationId)
                .OnDelete(DeleteBehavior.SetNull);

            base.OnModelCreating(modelBuilder);

            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Court>().HasData(
                new Court { Id = 1, Nome = "Quadra Central", Descricao = "Quadra principal com grama sintética", Localizacao = "Centro Esportivo - Quadra 1", PrecoPorHora = 80.00m },
                new Court { Id = 2, Nome = "Quadra Norte", Descricao = "Quadra com piso de concreto", Localizacao = "Centro Esportivo - Quadra 2", PrecoPorHora = 60.00m },
                new Court { Id = 3, Nome = "Quadra Sul", Descricao = "Quadra coberta", Localizacao = "Centro Esportivo - Quadra 3", PrecoPorHora = 100.00m }
            );
        }
    }
}
