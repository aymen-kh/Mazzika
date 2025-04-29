using Microsoft.EntityFrameworkCore;
using Mazzika.Models;

namespace Mazzika.Data
{
    public class MusicDbContext : DbContext
    {
        public DbSet<TopTrack> TopTracks { get; set; }

        public MusicDbContext(DbContextOptions<MusicDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TopTrack>(entity =>
            {
                entity.HasIndex(e => e.VideoId).IsUnique();
                entity.HasIndex(e => e.PlayCount);
                entity.HasIndex(e => e.LastPlayed);

                entity.Property(e => e.LastPlayed)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
}