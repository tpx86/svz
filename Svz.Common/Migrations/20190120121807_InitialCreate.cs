using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Svz.Common.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "Book",
                table => new
                {
                    BookId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Title = table.Column<string>(maxLength: 500, nullable: false),
                    Isbn = table.Column<string>(maxLength: 30, nullable: true),
                    Ean = table.Column<string>(maxLength: 30, nullable: false),
                    Category = table.Column<string>(maxLength: 255, nullable: true),
                    Publisher = table.Column<string>(maxLength: 255, nullable: true),
                    Authors = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    IsInStock = table.Column<bool>(nullable: false),
                    Description = table.Column<string>(maxLength: 3000, nullable: false),
                    PagesCount = table.Column<short>(nullable: false),
                    YearPublished = table.Column<short>(nullable: false),
                    FormatWidthCm = table.Column<decimal>(nullable: false),
                    FormatHeightCm = table.Column<decimal>(nullable: false),
                    Cover = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Book", x => x.BookId); });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "Book");
        }
    }
}