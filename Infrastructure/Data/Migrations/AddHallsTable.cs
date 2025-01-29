using Microsoft.EntityFrameworkCore.Migrations;

namespace Infrastructure.Data.Migrations
{
    public partial class AddHallsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Створюємо таблицю Halls
            migrationBuilder.CreateTable(
                name: "Halls",
                columns: table => new
                {
                    HallId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    RowsCount = table.Column<int>(type: "int", nullable: false),
                    SeatsPerRow = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Halls", x => x.HallId);
                });

            // Додаємо початкові дані
            migrationBuilder.InsertData(
                table: "Halls",
                columns: new[] { "Name", "Capacity", "RowsCount", "SeatsPerRow", "IsActive", "Description" },
                values: new object[] { "Hall 1", 100, 10, 10, true, "Main hall" });

            // Додаємо колонку HallId до таблиці Sessions, але поки що дозволяємо null
            migrationBuilder.AddColumn<int>(
                name: "HallId",
                table: "Sessions",
                type: "int",
                nullable: true);  // Тимчасово дозволяємо null

            // Оновлюємо існуючі сеанси, встановлюючи HallId = 1 (перший зал)
            migrationBuilder.Sql("UPDATE Sessions SET HallId = 1");

            // Тепер робимо HallId not null
            migrationBuilder.AlterColumn<int>(
                name: "HallId",
                table: "Sessions",
                type: "int",
                nullable: false,
                defaultValue: 1);

            // Додаємо зовнішній ключ
            migrationBuilder.CreateIndex(
                name: "IX_Sessions_HallId",
                table: "Sessions",
                column: "HallId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Halls_HallId",
                table: "Sessions",
                column: "HallId",
                principalTable: "Halls",
                principalColumn: "HallId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Halls_HallId",
                table: "Sessions");

            migrationBuilder.DropTable(
                name: "Halls");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_HallId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "HallId",
                table: "Sessions");
        }
    }
} 