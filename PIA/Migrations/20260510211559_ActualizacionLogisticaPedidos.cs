using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PIA.Migrations
{
    /// <inheritdoc />
    public partial class ActualizacionLogisticaPedidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Fecha",
                table: "Pedidos",
                newName: "FechaEntregaEstimada");

            migrationBuilder.RenameColumn(
                name: "Estado",
                table: "Pedidos",
                newName: "FechaCompra");

            migrationBuilder.AddColumn<string>(
                name: "Ciudad",
                table: "Pedidos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "Pedidos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Estatus",
                table: "Pedidos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ciudad",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "Estatus",
                table: "Pedidos");

            migrationBuilder.RenameColumn(
                name: "FechaEntregaEstimada",
                table: "Pedidos",
                newName: "Fecha");

            migrationBuilder.RenameColumn(
                name: "FechaCompra",
                table: "Pedidos",
                newName: "Estado");
        }
    }
}
