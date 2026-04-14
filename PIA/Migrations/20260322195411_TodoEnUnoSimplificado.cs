using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PIA.Migrations
{
    /// <inheritdoc />
    public partial class TodoEnUnoSimplificado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemsCarrito_VariantesProducto_VarianteProductoId",
                table: "ItemsCarrito");

            migrationBuilder.DropTable(
                name: "VariantesProducto");

            migrationBuilder.RenameColumn(
                name: "VarianteProductoId",
                table: "ItemsCarrito",
                newName: "ProductoId");

            migrationBuilder.RenameIndex(
                name: "IX_ItemsCarrito_VarianteProductoId",
                table: "ItemsCarrito",
                newName: "IX_ItemsCarrito_ProductoId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_ItemsCarrito_Productos_ProductoId",
                table: "ItemsCarrito",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemsCarrito_Productos_ProductoId",
                table: "ItemsCarrito");

            migrationBuilder.DropColumn(
                name: "Sabor",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Stock",
                table: "Productos");

            migrationBuilder.RenameColumn(
                name: "ProductoId",
                table: "ItemsCarrito",
                newName: "VarianteProductoId");

            migrationBuilder.RenameIndex(
                name: "IX_ItemsCarrito_ProductoId",
                table: "ItemsCarrito",
                newName: "IX_ItemsCarrito_VarianteProductoId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_ItemsCarrito_VariantesProducto_VarianteProductoId",
                table: "ItemsCarrito",
                column: "VarianteProductoId",
                principalTable: "VariantesProducto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
