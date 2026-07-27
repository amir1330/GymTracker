using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GymTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardCharts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DashboardCharts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Metric = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExerciseId = table.Column<int>(type: "integer", nullable: true),
                    Period = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ChartType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardCharts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DashboardCharts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DashboardCharts_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DashboardCharts_ExerciseId",
                table: "DashboardCharts",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_DashboardCharts_UserId",
                table: "DashboardCharts",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DashboardCharts");
        }
    }
}
