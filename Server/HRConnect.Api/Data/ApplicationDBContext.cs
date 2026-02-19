namespace HRConnect.Api.Data
{
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;
  public class ApplicationDBContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
  {
    public DbSet<User> Users { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<PasswordResetPin> PasswordResetPins { get; set; }
    public DbSet<PasswordHistory> PasswordHistories { get; set; }


    // Model Configurations ----------------------------------------------------
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      modelBuilder.Entity<Employee>()
        .HasOne(e => e.Position)
        .WithMany(p => p.Employees)
        .HasForeignKey(e => e.PositionId);


      // Convert enums to stirng in the database

      modelBuilder.Entity<Employee>()
        .Property(e => e.Title)
        .HasConversion<string>();
      
      modelBuilder.Entity<Employee>()
        .Property(e => e.Gender)
        .HasConversion<string>();

      modelBuilder.Entity<Employee>()
        .Property(e => e.Branch)
        .HasConversion<string>();

      modelBuilder.Entity<Employee>()
        .Property(e => e.EmploymentStatus)
        .HasConversion<string>();


      modelBuilder.Entity<Position>().HasData(
        new Position { PositionId = 1, PositionTitle = "Chief Executive Officer"},
        new Position { PositionId = 2, PositionTitle = "Associate Software Engineer"},
        new Position { PositionId = 3, PositionTitle = "Trainee Software Developer"}
      );

    }

  }
}