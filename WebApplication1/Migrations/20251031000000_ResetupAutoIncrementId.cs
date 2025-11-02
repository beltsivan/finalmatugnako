using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApplication1.Migrations
{
    public partial class ResetupAutoIncrementId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing data and recreate the table with proper identity
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS Students;
                
                CREATE TABLE Students (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Lastname NVARCHAR(25) NOT NULL,
                    Firstname NVARCHAR(25) NOT NULL,
                    Course NVARCHAR(MAX) NULL,
                    Email NVARCHAR(MAX) NULL,
                    DateCreated DATETIME2 NOT NULL
                );
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Students");
        }
    }
}