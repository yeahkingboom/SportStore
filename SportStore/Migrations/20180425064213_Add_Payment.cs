using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SportStore.Migrations
{
    public partial class Add_Payment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayMent",
                table: "Order");

            migrationBuilder.AddColumn<long>(
                name: "PayMentID",
                table: "Order",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PayWay = table.Column<int>(nullable: false),
                    Total = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Order_PayMentID",
                table: "Order",
                column: "PayMentID");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Payment_PayMentID",
                table: "Order",
                column: "PayMentID",
                principalTable: "Payment",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Payment_PayMentID",
                table: "Order");

            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropIndex(
                name: "IX_Order_PayMentID",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "PayMentID",
                table: "Order");

            migrationBuilder.AddColumn<string>(
                name: "PayMent",
                table: "Order",
                nullable: true);
        }
    }
}
