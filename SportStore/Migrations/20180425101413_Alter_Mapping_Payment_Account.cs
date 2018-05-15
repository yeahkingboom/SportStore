using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SportStore.Migrations
{
    public partial class Alter_Mapping_Payment_Account : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AccountID",
                table: "Payment",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payment_AccountID",
                table: "Payment",
                column: "AccountID");

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_Accounts_AccountID",
                table: "Payment",
                column: "AccountID",
                principalTable: "Accounts",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payment_Accounts_AccountID",
                table: "Payment");

            migrationBuilder.DropIndex(
                name: "IX_Payment_AccountID",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "AccountID",
                table: "Payment");
        }
    }
}
