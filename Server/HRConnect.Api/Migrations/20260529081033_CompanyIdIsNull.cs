using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRConnect.Api.Migrations
{
  /// <inheritdoc />
  public partial class CompanyIdIsNull : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      /// <summary>            
      /// This is just to brings EFCore up to date with the changes in migrations
      /// to align them. Employee.CompanyId foreign key has been changed to 
      /// nullible
      /// <summary>
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
  }
}