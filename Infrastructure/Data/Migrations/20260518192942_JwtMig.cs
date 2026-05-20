using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class JwtMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "products",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "rols",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    rolName = table.Column<string>(type: "varchar", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    createdAt = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "CURRENT_DATE"),
                    updatedAt = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "CURRENT_DATE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rols", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users_members",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "varchar", maxLength: 255, nullable: false),
                    createdAt = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "CURRENT_DATE"),
                    updatedAt = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "CURRENT_DATE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_members", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "refreshtokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: true),
                    Expires = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Revoked = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refreshtokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refreshtokens_users_members_UserId",
                        column: x => x.UserId,
                        principalTable: "users_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users_rols",
                columns: table => new
                {
                    UserMemberId = table.Column<int>(type: "integer", nullable: false),
                    RolId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_rols", x => new { x.UserMemberId, x.RolId });
                    table.ForeignKey(
                        name: "FK_users_rols_rols_RolId",
                        column: x => x.RolId,
                        principalTable: "rols",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_users_rols_users_members_UserMemberId",
                        column: x => x.UserMemberId,
                        principalTable: "users_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_refreshtokens_UserId",
                table: "refreshtokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_users_members_email",
                table: "users_members",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_rols_RolId",
                table: "users_rols",
                column: "RolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "refreshtokens");

            migrationBuilder.DropTable(
                name: "users_rols");

            migrationBuilder.DropTable(
                name: "rols");

            migrationBuilder.DropTable(
                name: "users_members");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "products");
        }
    }
}
