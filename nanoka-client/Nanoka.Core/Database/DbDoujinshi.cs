using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Nanoka.Core.Database
{
    [Table("Doujinshi")]
    public class DbDoujinshi
    {
        [Key]
        public int Id { get; set; }

        public int ChunkId { get; set; }
        public DbIndexChunk Chunk { get; set; }

        public int GroupId { get; set; }
        public DbDoujinshiGroup Group { get; set; }

        public DateTime UpdateTime { get; set; }

        public string LocalizedName { get; set; }
        public string EnglishName { get; set; }
        public string RomanizedName { get; set; }

        public ICollection<DbDoujinshiMetaJoin> MetaJoins { get; set; }
        public ICollection<DbDoujinshiPage> Pages { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
            => builder.Entity<DbDoujinshi>(doujinshi =>
            {
                doujinshi.HasOne(d => d.Chunk)
                         .WithMany(i => i.Doujinshi)
                         .HasForeignKey(d => d.ChunkId)
                         .IsRequired();

                doujinshi.HasOne(d => d.Group)
                         .WithMany(g => g.Items)
                         .HasForeignKey(d => d.GroupId)
                         .IsRequired();

                DbDoujinshiMetaJoin.OnModelCreating(builder);
            });
    }

    /// <summary>
    /// Join table: <see cref="DbDoujinshi"/> - <see cref="DbDoujinshiMeta"/>
    /// </summary>
    [Table("DoujinshiMeta_Join")]
    public class DbDoujinshiMetaJoin
    {
        public int DoujinshiId { get; set; }
        public DbDoujinshi Doujinshi { get; set; }

        public int MetaId { get; set; }
        public DbDoujinshiMeta Meta { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
            => builder.Entity<DbDoujinshiMetaJoin>(join =>
            {
                join.HasKey(j => new { j.DoujinshiId, j.MetaId });

                join.HasOne(j => j.Doujinshi)
                    .WithMany(d => d.MetaJoins)
                    .HasForeignKey(j => j.DoujinshiId)
                    .IsRequired();

                join.HasOne(j => j.Meta)
                    .WithMany(m => m.DoujinshiJoins)
                    .HasForeignKey(j => j.MetaId)
                    .IsRequired();
            });
    }
}