using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace DAL
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<User>()
                .HasIndex(e => e.Email)
                .IsUnique();

            modelBuilder.Entity<Avatar>().ToTable(nameof(Avatars));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(b => b.MigrationsAssembly("API"));

        public DbSet<User> Users => Set<User>();
        public DbSet<Subscribe> Subs => Set<Subscribe>();
        public DbSet<UserSession> UserSessions => Set<UserSession>();

        public DbSet<Post> Posts => Set<Post>();
        public DbSet<PostLike> PostLikes => Set<PostLike>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<CommentLike> CommentLikes => Set<CommentLike>();

        public DbSet<Attach> Attaches => Set<Attach>();
        public DbSet<Avatar> Avatars => Set<Avatar>();
        public DbSet<PostContent> PostContents => Set<PostContent>();
    }
}
