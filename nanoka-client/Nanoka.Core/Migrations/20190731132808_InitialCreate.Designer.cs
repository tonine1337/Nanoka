﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Nanoka.Core.Database;

namespace Nanoka.Core.Migrations
{
    [DbContext(typeof(NanokaDbContext))]
    [Migration("20190731132808_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity("Nanoka.Core.Database.DbDoujinshi", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ChunkId");

                    b.Property<string>("EnglishName");

                    b.Property<int>("GroupId");

                    b.Property<string>("LocalizedName");

                    b.Property<string>("RomanizedName");

                    b.Property<DateTime>("UpdateTime");

                    b.HasKey("Id");

                    b.HasIndex("ChunkId");

                    b.HasIndex("GroupId");

                    b.ToTable("Doujinshi");
                });

            modelBuilder.Entity("Nanoka.Core.Database.DbDoujinshiGroup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Artist");

                    b.Property<string>("Category");

                    b.Property<string>("Character");

                    b.Property<string>("Convention");

                    b.Property<string>("EnglishName");

                    b.Property<string>("Group");

                    b.Property<string>("Language");

                    b.Property<string>("LocalizedName");

                    b.Property<string>("Parody");

                    b.Property<string>("RomanizedName");

                    b.Property<string>("Tag");

                    b.HasKey("Id");

                    b.ToTable("DoujinshiGroup");
                });

            modelBuilder.Entity("Nanoka.Core.Database.DbDoujinshiMeta", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Type");

                    b.Property<string>("Value")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("Type", "Value")
                        .IsUnique();

                    b.ToTable("DoujinshiMeta");
                });

            modelBuilder.Entity("Nanoka.Core.Database.DbDoujinshiMetaJoin", b =>
                {
                    b.Property<int>("DoujinshiId");

                    b.Property<int>("MetaId");

                    b.HasKey("DoujinshiId", "MetaId");

                    b.HasIndex("MetaId");

                    b.ToTable("DoujinshiMeta_Join");
                });

            modelBuilder.Entity("Nanoka.Core.Database.DbDoujinshiPage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Cid")
                        .IsRequired();

                    b.Property<int>("DoujinshiId");

                    b.Property<int>("Index");

                    b.HasKey("Id");

                    b.HasIndex("DoujinshiId");

                    b.ToTable("DoujinshiPage");
                });

            modelBuilder.Entity("Nanoka.Core.Database.DbIndex", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Endpoint");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Index");
                });

            modelBuilder.Entity("Nanoka.Core.Database.DbIndexChunk", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Cid")
                        .IsRequired();

                    b.Property<int>("IndexId");

                    b.HasKey("Id");

                    b.HasIndex("IndexId");

                    b.ToTable("IndexChunk");
                });

            modelBuilder.Entity("Nanoka.Core.Database.DbDoujinshi", b =>
                {
                    b.HasOne("Nanoka.Core.Database.DbIndexChunk", "Chunk")
                        .WithMany("Doujinshi")
                        .HasForeignKey("ChunkId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Nanoka.Core.Database.DbDoujinshiGroup", "Group")
                        .WithMany("Items")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Nanoka.Core.Database.DbDoujinshiMetaJoin", b =>
                {
                    b.HasOne("Nanoka.Core.Database.DbDoujinshi", "Doujinshi")
                        .WithMany("MetaJoins")
                        .HasForeignKey("DoujinshiId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Nanoka.Core.Database.DbDoujinshiMeta", "Meta")
                        .WithMany("DoujinshiJoins")
                        .HasForeignKey("MetaId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Nanoka.Core.Database.DbDoujinshiPage", b =>
                {
                    b.HasOne("Nanoka.Core.Database.DbDoujinshi", "Doujinshi")
                        .WithMany("Pages")
                        .HasForeignKey("DoujinshiId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Nanoka.Core.Database.DbIndexChunk", b =>
                {
                    b.HasOne("Nanoka.Core.Database.DbIndex", "Index")
                        .WithMany("Chunks")
                        .HasForeignKey("IndexId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
