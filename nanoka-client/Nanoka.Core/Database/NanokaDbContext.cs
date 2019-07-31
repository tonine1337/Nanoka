using Microsoft.EntityFrameworkCore;

namespace Nanoka.Core.Database
{
    public class NanokaDbContext : DbContext
    {
        public DbSet<DbIndex> Indexes { get; set; }
        public DbSet<DbIndexChunk> IndexChunks { get; set; }

        public DbSet<DbDoujinshi> Doujinshi { get; set; }
        public DbSet<DbDoujinshiGroup> DoujinshiGroups { get; set; }
        public DbSet<DbDoujinshiMeta> DoujinshiMetas { get; set; }
        public DbSet<DbDoujinshiPage> DoujinshiPages { get; set; }

        public NanokaDbContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            DbIndex.OnModelCreating(modelBuilder);
            DbIndexChunk.OnModelCreating(modelBuilder);

            DbDoujinshi.OnModelCreating(modelBuilder);
            DbDoujinshiGroup.OnModelCreating(modelBuilder);
            DbDoujinshiMeta.OnModelCreating(modelBuilder);
            DbDoujinshiPage.OnModelCreating(modelBuilder);
        }
    }
}