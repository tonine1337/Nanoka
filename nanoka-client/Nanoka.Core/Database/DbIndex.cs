using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Nanoka.Core.Database
{
    [Table("Index")]
    public class DbIndex
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Endpoint { get; set; }

        public ICollection<DbIndexChunk> Chunks { get; set; }

        public static void OnModelCreating(ModelBuilder builder) => builder.Entity<DbIndex>(index => { });
    }
}