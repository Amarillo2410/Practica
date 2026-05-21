using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    [DbContext(typeof(Infrastructure.Context.AppDbContext))]
    [Migration("20260521143000_RemoveLegacyRolesTables")]
    public partial class RemoveLegacyRolesTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS public.users_rols;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS public.rols;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally left empty for prototype cleanup migration.
        }
    }
}
