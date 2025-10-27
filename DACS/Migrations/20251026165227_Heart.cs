using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DACS.Migrations
{
    /// <inheritdoc />
    public partial class Heart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SanPhamYeuThichs",
                columns: table => new
                {
                    M_YeuThich = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    M_SanPham = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NgayThem = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPhamYeuThichs", x => x.M_YeuThich);
                    table.ForeignKey(
                        name: "FK_SanPhamYeuThichs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SanPhamYeuThichs_SanPhams_M_SanPham",
                        column: x => x.M_SanPham,
                        principalTable: "SanPhams",
                        principalColumn: "M_SanPham",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SanPhamYeuThichs_M_SanPham",
                table: "SanPhamYeuThichs",
                column: "M_SanPham");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhamYeuThichs_UserId",
                table: "SanPhamYeuThichs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SanPhamYeuThichs");
        }
    }
}
