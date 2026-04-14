using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PIA.Migrations
{
    /// <inheritdoc />
    public partial class CreandoCarritoCompras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemsCarrito_Productos_ProductoId",
                table: "ItemsCarrito");

            migrationBuilder.RenameColumn(
                name: "ProductoId",
                table: "ItemsCarrito",
                newName: "VarianteProductoId");

            migrationBuilder.RenameIndex(
                name: "IX_ItemsCarrito_ProductoId",
                table: "ItemsCarrito",
                newName: "IX_ItemsCarrito_VarianteProductoId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemsCarrito_VariantesProducto_VarianteProductoId",
                table: "ItemsCarrito",
                column: "VarianteProductoId",
                principalTable: "VariantesProducto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemsCarrito_VariantesProducto_VarianteProductoId",
                table: "ItemsCarrito");

            migrationBuilder.RenameColumn(
                name: "VarianteProductoId",
                table: "ItemsCarrito",
                newName: "ProductoId");

            migrationBuilder.RenameIndex(
                name: "IX_ItemsCarrito_VarianteProductoId",
                table: "ItemsCarrito",
                newName: "IX_ItemsCarrito_ProductoId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemsCarrito_Productos_ProductoId",
                table: "ItemsCarrito",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
