using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApplication1.Migrations
{
    public partial class FixAutoIncrementId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Reset the identity column
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'dbo.Students') AND name = 'Id')
                BEGIN
                    DECLARE @MaxId int
                    SELECT @MaxId = ISNULL(MAX(Id), 0) FROM Students
                    DBCC CHECKIDENT ('Students', RESEED, @MaxId)
                END
                ELSE
                BEGIN
                    -- Drop primary key
                    ALTER TABLE Students DROP CONSTRAINT PK_Students
                    
                    -- Drop the Id column and recreate it as identity
                    ALTER TABLE Students DROP COLUMN Id
                    
                    ALTER TABLE Students ADD Id INT IDENTITY(1,1) NOT NULL
                    
                    -- Add primary key back
                    ALTER TABLE Students ADD CONSTRAINT PK_Students PRIMARY KEY (Id)
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No down migration needed
        }
    }
}