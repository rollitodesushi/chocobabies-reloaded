using ChocobabiesReloaded.Models;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Net.Sockets;

namespace ChocobabiesReloaded.Data
{
    public class RifaDbContext : IdentityDbContext<User, IdentityRole<int>, int>, IDataProtectionKeyContext

    {
        public RifaDbContext(DbContextOptions<RifaDbContext> options)
            : base(options)
        {
        }

        public DbSet<Rifa> rifas { get; set; }
        public DbSet<Tiquete> tiquetes { get; set; }
        public DbSet<Participante> participantes { get; set; }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DataProtectionKey>().ToTable("DataProtectionKeys");

            // Configure Participant-User relationship as optional
            modelBuilder.Entity<Participante>()
                .HasOne(p => p.user)
                .WithMany() // No navigation property in User
                .HasForeignKey(p => p.userId)
                .IsRequired(false); // UserId is optional

            // Ensure TicketNumber is unique per Raffle
            modelBuilder.Entity<Tiquete>()
                .HasIndex(t => new { t.rifaID, t.numeroTiquete })
                .IsUnique();
        }

    }
}
