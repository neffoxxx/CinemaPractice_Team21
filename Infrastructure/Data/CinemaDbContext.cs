using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    using Infrastructure.Entities;
    using Microsoft.EntityFrameworkCore;

    public class CinemaDbContext : DbContext
    {
        public CinemaDbContext(DbContextOptions<CinemaDbContext> options) : base(options) { }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<MovieActor> MovieActors { get; set; }
        public DbSet<MovieGenre> MovieGenres { get; set; }
        public DbSet<Hall> Halls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Застосувати всі конфігурації з Assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CinemaDbContext).Assembly);

            modelBuilder.Entity<MovieActor>()
                .HasKey(ma => new { ma.MovieId, ma.ActorId });

            modelBuilder.Entity<MovieGenre>()
                .HasKey(mg => new { mg.MovieId, mg.GenreId });
                

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Session)
                .WithMany(s => s.Tickets)
                .HasForeignKey(t => t.SessionId);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tickets)
                .HasForeignKey(t => t.UserId);

            modelBuilder.Entity<Hall>(entity =>
            {
                entity.HasKey(e => e.HallId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
                
                // Зв'язок один-до-багатьох з Sessions
                entity.HasMany(h => h.Sessions)
                      .WithOne(s => s.Hall)
                      .HasForeignKey(s => s.HallId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Конфігурація зв'язку між Session та Hall
            modelBuilder.Entity<Session>()
                .HasOne(s => s.Movie)
                .WithMany(m => m.Sessions)
                .HasForeignKey(s => s.MovieId);

            modelBuilder.Entity<Session>()
                .HasOne(s => s.Hall)
                .WithMany(h => h.Sessions)
                .HasForeignKey(s => s.HallId);
        }
    }

}