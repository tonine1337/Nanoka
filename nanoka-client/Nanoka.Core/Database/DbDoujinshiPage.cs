using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Nanoka.Core.Database
{
    [Table("DoujinshiPage")]
    public class DbDoujinshiPage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Cid { get; set; }

        public int DoujinshiId { get; set; }
        public DbDoujinshi Doujinshi { get; set; }

        /// <summary>
        /// Zero-based index within the doujinshi.
        /// </summary>
        public int Index { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
            => builder.Entity<DbDoujinshiPage>(page =>
            {
                page.HasOne(p => p.Doujinshi)
                    .WithMany(d => d.Pages)
                    .HasForeignKey(p => p.DoujinshiId)
                    .IsRequired();
            });
    }
}