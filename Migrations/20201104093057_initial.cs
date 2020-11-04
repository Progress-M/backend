using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace registry.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "product_category",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_category", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(nullable: true),
                    password = table.Column<string>(nullable: true),
                    name = table.Column<string>(nullable: true),
                    surname = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "company",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(nullable: true),
                    representative = table.Column<string>(nullable: true),
                    email = table.Column<string>(nullable: true),
                    inn = table.Column<string>(nullable: true),
                    password = table.Column<string>(nullable: true),
                    address = table.Column<string>(nullable: true),
                    time_of_work = table.Column<string>(nullable: true),
                    product_сategory_id = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company", x => x.id);
                    table.ForeignKey(
                        name: "fk_company_product_category_product_сategory_id",
                        column: x => x.product_сategory_id,
                        principalTable: "product_category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "offer",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    text = table.Column<string>(nullable: true),
                    time_start = table.Column<DateTime>(nullable: false),
                    time_end = table.Column<DateTime>(nullable: false),
                    company_id = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offer", x => x.id);
                    table.ForeignKey(
                        name: "fk_offer_company_company_id",
                        column: x => x.company_id,
                        principalTable: "company",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "offer_user",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    offer_id = table.Column<int>(nullable: true),
                    user_id = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offer_user", x => x.id);
                    table.ForeignKey(
                        name: "fk_offer_user_offer_offer_id",
                        column: x => x.offer_id,
                        principalTable: "offer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_offer_user_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_company_email",
                table: "company",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_company_inn",
                table: "company",
                column: "inn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_company_product_сategory_id",
                table: "company",
                column: "product_сategory_id");

            migrationBuilder.CreateIndex(
                name: "ix_offer_company_id",
                table: "offer",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_offer_user_offer_id",
                table: "offer_user",
                column: "offer_id");

            migrationBuilder.CreateIndex(
                name: "ix_offer_user_user_id",
                table: "offer_user",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "offer_user");

            migrationBuilder.DropTable(
                name: "offer");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "company");

            migrationBuilder.DropTable(
                name: "product_category");
        }
    }
}
