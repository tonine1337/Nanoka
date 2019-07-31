using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Nanoka.Core.Database
{
    [Table("IndexChunk")]
    public class DbIndexChunk
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Cid { get; set; }

        public int IndexId { get; set; }
        public DbIndex Index { get; set; }

        public ICollection<DbDoujinshi> Doujinshi { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
            => builder.Entity<DbIndexChunk>(doujinshi =>
            {
                doujinshi.HasOne(d => d.Index)
                         .WithMany(i => i.Chunks)
                         .HasForeignKey(d => d.IndexId)
                         .IsRequired();
            });
    }
}