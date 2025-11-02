using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    public partial class UpdateStudentIdAutoIncrement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing tables if they exist
            migrationBuilder.Sql("IF OBJECT_ID(N'dbo.Students', N'U') IS NOT NULL DROP TABLE dbo.Students");

            // Create new table with auto-incrementing ID
            migrationBuilder.Sql(@"
                CREATE TABLE [dbo].[Students](
                    [Id] [int] IDENTITY(1,1) NOT NULL,
                    [Lastname] [nvarchar](25) NOT NULL,
                    [Firstname] [nvarchar](25) NOT NULL,
                    [Course] [nvarchar](max) NULL,
                    [Email] [nvarchar](max) NULL,
                    [DateCreated] [datetime2](7) NOT NULL,
                    CONSTRAINT [PK_Students] PRIMARY KEY CLUSTERED ([Id] ASC)
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