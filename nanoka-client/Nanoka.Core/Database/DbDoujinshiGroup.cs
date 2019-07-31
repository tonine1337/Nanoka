using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Nanoka.Core.Database
{
    [Table("DoujinshiGroup")]
    public class DbDoujinshiGroup
    {
        [Key]
        public int Id { get; set; }

        public string LocalizedName { get; set; }
        public string EnglishName { get; set; }
        public string RomanizedName { get; set; }

        public string Artist { get; set; }
        public string Group { get; set; }
        public string Parody { get; set; }
        public string Character { get; set; }
        public string Category { get; set; }
        public string Language { get; set; }
        public string Tag { get; set; }
        public string Convention { get; set; }

        public ICollection<DbDoujinshi> Items { get; set; }

        public static void OnModelCreating(ModelBuilder builder) => builder.Entity<DbDoujinshiGroup>(doujinshi => { });
    }
}