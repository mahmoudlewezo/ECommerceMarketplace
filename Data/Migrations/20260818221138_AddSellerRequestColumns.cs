using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceMarketplace.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerRequestColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequestDate",
                table: "SellerRequests",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "SellerRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "SellerRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedById",
                table: "SellerRequests",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SellerRequests_ReviewedById",
                table: "SellerRequests",
                column: "ReviewedById");

            migrationBuilder.AddForeignKey(
                name: "FK_SellerRequests_AspNetUsers_ReviewedById",
                table: "SellerRequests",
                column: "ReviewedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SellerRequests_AspNetUsers_ReviewedById",
                table: "SellerRequests");

            migrationBuilder.DropIndex(
                name: "IX_SellerRequests_ReviewedById",
                table: "SellerRequests");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "SellerRequests");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "SellerRequests");

            migrationBuilder.DropColumn(
                name: "ReviewedById",
                table: "SellerRequests");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "SellerRequests",
                newName: "RequestDate");
        }
    }
}
