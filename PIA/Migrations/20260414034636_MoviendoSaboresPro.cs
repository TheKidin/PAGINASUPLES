using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PIA.Migrations
{
    /// <inheritdoc />
    public partial class MoviendoSaboresPro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sabor",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Stock",
                table: "Productos");

            migrationBuilder.AddColumn<string>(
                name: "ImagenUrl",
                table: "Productos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VariantesProducto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Sabor = table.Column<string>(type: "TEXT", nullable: false),
                    Stock = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VariantesProducto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VariantesProducto_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VariantesProducto_ProductoId",
                table: "VariantesProducto",
                column: "ProductoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VariantesProducto");

            migrationBuilder.DropColumn(
                name: "ImagenUrl",
                table: "Productos");

            migrationBuilder.AddColumn<string>(
                name: "Sabor",
                table: "Productos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "Productos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
