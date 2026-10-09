using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Infrastructure.Persistence.Migrations
{
    public partial class AddUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "Id", "CustomerId", "PasswordHash", "Role", "Username" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), null, "v1.600000.v/1H6x41FXYgiPv5Z5WXWQ==.abEgtNOuCopuMywr7rZs8javnZMEYJFhmLW7QGkiiV4=", "Admin", "admin" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "customer-001", "v1.600000.Wth+s2v7HS1WrnlrNZN8Rg==.zxUtdxcycvnbOfhqX9BGTeoav9Q4rX7DyrCC2w8wL0M=", "Customer", "customer" },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "customer-002", "v1.600000.nmEmoJon3mBxxOuPr8boFg==.6bAC2WWeOxYuuQcnBasJkWrYkq1yW+UrwWBpW0z0+Ko=", "Customer", "customer2" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_Username",
                table: "users",
                column: "Username",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
