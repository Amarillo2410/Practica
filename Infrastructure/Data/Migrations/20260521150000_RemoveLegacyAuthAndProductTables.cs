using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    [DbContext(typeof(Infrastructure.Context.AppDbContext))]
    [Migration("20260521150000_RemoveLegacyAuthAndProductTables")]
    public partial class RemoveLegacyAuthAndProductTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS public.users_rols;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS public.rols;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS public.refreshtokens;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS public.users_members;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS public.products;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally left empty for prototype cleanup migration.
        }
    }
}
