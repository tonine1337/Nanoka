using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Nanoka.Core.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "DoujinshiGroup",
                table => new
                {
                    Id = table.Column<int>()
                              .Annotation("Sqlite:Autoincrement", true),
                    LocalizedName = table.Column<string>(nullable: true),
                    EnglishName   = table.Column<string>(nullable: true),
                    RomanizedName = table.Column<string>(nullable: true),
                    Artist        = table.Column<string>(nullable: true),
                    Group         = table.Column<string>(nullable: true),
                    Parody        = table.Column<string>(nullable: true),
                    Character     = table.Column<string>(nullable: true),
                    Category      = table.Column<string>(nullable: true),
                    Language      = table.Column<string>(nullable: true),
                    Tag           = table.Column<string>(nullable: true),
                    Convention    = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_DoujinshiGroup", x => x.Id); });

            migrationBuilder.CreateTable(
                "DoujinshiMeta",
                table => new
                {
                    Id = table.Column<int>()
                              .Annotation("Sqlite:Autoincrement", true),
                    Type  = table.Column<int>(),
                    Value = table.Column<string>()
                },
                constraints: table => { table.PrimaryKey("PK_DoujinshiMeta", x => x.Id); });

            migrationBuilder.CreateTable(
                "Index",
                table => new
                {
                    Id = table.Column<int>()
                              .Annotation("Sqlite:Autoincrement", true),
                    Name     = table.Column<string>(nullable: true),
                    Endpoint = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Index", x => x.Id); });

            migrationBuilder.CreateTable(
                "IndexChunk",
                table => new
                {
                    Id = table.Column<int>()
                              .Annotation("Sqlite:Autoincrement", true),
                    Cid     = table.Column<string>(),
                    IndexId = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndexChunk", x => x.Id);
                    table.ForeignKey(
                        "FK_IndexChunk_Index_IndexId",
                        x => x.IndexId,
                        "Index",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "Doujinshi",
                table => new
                {
                    Id = table.Column<int>()
                              .Annotation("Sqlite:Autoincrement", true),
                    ChunkId       = table.Column<int>(),
                    GroupId       = table.Column<int>(),
                    UpdateTime    = table.Column<DateTime>(),
                    LocalizedName = table.Column<string>(nullable: true),
                    EnglishName   = table.Column<string>(nullable: true),
                    RomanizedName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doujinshi", x => x.Id);
                    table.ForeignKey(
                        "FK_Doujinshi_IndexChunk_ChunkId",
                        x => x.ChunkId,
                        "IndexChunk",
                        "Id",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        "FK_Doujinshi_DoujinshiGroup_GroupId",
                        x => x.GroupId,
                        "DoujinshiGroup",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "DoujinshiMeta_Join",
                table => new
                {
                    DoujinshiId = table.Column<int>(),
                    MetaId      = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoujinshiMeta_Join", x => new { x.DoujinshiId, x.MetaId });
                    table.ForeignKey(
                        "FK_DoujinshiMeta_Join_Doujinshi_DoujinshiId",
                        x => x.DoujinshiId,
                        "Doujinshi",
                        "Id",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        "FK_DoujinshiMeta_Join_DoujinshiMeta_MetaId",
                        x => x.MetaId,
                        "DoujinshiMeta",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "DoujinshiPage",
                table => new
                {
                    Id = table.Column<int>()
                              .Annotation("Sqlite:Autoincrement", true),
                    Cid         = table.Column<string>(),
                    DoujinshiId = table.Column<int>(),
                    Index       = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoujinshiPage", x => x.Id);
                    table.ForeignKey(
                        "FK_DoujinshiPage_Doujinshi_DoujinshiId",
                        x => x.DoujinshiId,
                        "Doujinshi",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                "IX_Doujinshi_ChunkId",
                "Doujinshi",
                "ChunkId");

            migrationBuilder.CreateIndex(
                "IX_Doujinshi_GroupId",
                "Doujinshi",
                "GroupId");

            migrationBuilder.CreateIndex(
                "IX_DoujinshiMeta_Type_Value",
                "DoujinshiMeta",
                new[] { "Type", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                "IX_DoujinshiMeta_Join_MetaId",
                "DoujinshiMeta_Join",
                "MetaId");

            migrationBuilder.CreateIndex(
                "IX_DoujinshiPage_DoujinshiId",
                "DoujinshiPage",
                "DoujinshiId");

            migrationBuilder.CreateIndex(
                "IX_IndexChunk_IndexId",
                "IndexChunk",
                "IndexId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoujinshiMeta_Join");

            migrationBuilder.DropTable(
                name: "DoujinshiPage");

            migrationBuilder.DropTable(
                name: "DoujinshiMeta");

            migrationBuilder.DropTable(
                name: "Doujinshi");

            migrationBuilder.DropTable(
                name: "IndexChunk");

            migrationBuilder.DropTable(
                name: "DoujinshiGroup");

            migrationBuilder.DropTable(
                name: "Index");
        }
    }
}