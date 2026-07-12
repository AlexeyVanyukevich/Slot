using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UBP.Booking.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "availability_slots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    booked_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_availability_slots", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    slot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    slot_start_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    slot_end_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    customer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    customer_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    customer_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    customer_notes = table.Column<string>(type: "text", nullable: true),
                    external_reference = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancellation_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bookings", x => x.id);
                    table.ForeignKey(
                        name: "fk_bookings_availability_slots_slot_id",
                        column: x => x.slot_id,
                        principalTable: "availability_slots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "booking_status_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    to_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    changed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    changed_by_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_booking_status_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_booking_status_history_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_availability_slots_resource_id_start_at",
                table: "availability_slots",
                columns: new[] { "resource_id", "start_at" });

            migrationBuilder.CreateIndex(
                name: "ix_availability_slots_tenant_id",
                table: "availability_slots",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_booking_status_history_booking_id",
                table: "booking_status_history",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "ix_booking_status_history_changed_at",
                table: "booking_status_history",
                column: "changed_at");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_customer_email",
                table: "bookings",
                column: "customer_email");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_external_reference",
                table: "bookings",
                column: "external_reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_bookings_resource_id",
                table: "bookings",
                column: "resource_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_slot_id",
                table: "bookings",
                column: "slot_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_status",
                table: "bookings",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_tenant_id",
                table: "bookings",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "booking_status_history");

            migrationBuilder.DropTable(
                name: "bookings");

            migrationBuilder.DropTable(
                name: "availability_slots");
        }
    }
}
