using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Nanoka.Core.Models;

namespace Nanoka.Core.Database
{
    [Table("DoujinshiMeta")]
    public class DbDoujinshiMeta
    {
        [Key]
        public int Id { get; set; }

        public DoujinshiMetaType Type { get; set; }

        [Required]
        public string Value { get; set; }

        public ICollection<DbDoujinshiMetaJoin> DoujinshiJoins { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
            => builder.Entity<DbDoujinshiMeta>(meta =>
            {
                // fast retrieval of meta objects
                meta.HasIndex(m => new { m.Type, m.Value }).IsUnique();
            });
    }
}