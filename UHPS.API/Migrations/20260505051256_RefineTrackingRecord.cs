using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UHPS.API.Migrations
{
    /// <inheritdoc />
    public partial class RefineTrackingRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrackingRecords_Addresses_DestinationAddressId",
                table: "TrackingRecords");

            migrationBuilder.DropIndex(
                name: "IX_TrackingRecords_DestinationAddressId",
                table: "TrackingRecords");

            migrationBuilder.DropColumn(
                name: "DestinationAddressId",
                table: "TrackingRecords");

            migrationBuilder.DropColumn(
                name: "TimeIn",
                table: "TrackingRecords");

            migrationBuilder.DropColumn(
                name: "TimeOut",
                table: "TrackingRecords");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "TrackingRecords",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TrackingRecords",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "TrackingRecords");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TrackingRecords");

            migrationBuilder.AddColumn<int>(
                name: "DestinationAddressId",
                table: "TrackingRecords",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeIn",
                table: "TrackingRecords",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeOut",
                table: "TrackingRecords",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrackingRecords_DestinationAddressId",
                table: "TrackingRecords",
                column: "DestinationAddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrackingRecords_Addresses_DestinationAddressId",
                table: "TrackingRecords",
                column: "DestinationAddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
