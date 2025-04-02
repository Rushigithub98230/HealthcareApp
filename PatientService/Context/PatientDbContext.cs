using Microsoft.EntityFrameworkCore;
using PatientService.Models;

namespace PatientService.Context
{
    public class PatientDbContext : DbContext
    {
        public PatientDbContext(DbContextOptions<PatientDbContext> options) : base(options) { }

        public DbSet<Patient> Patients => Set<Patient>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patient>(entity =>
            {
               
                entity.HasKey(p => p.Id)
                      .HasName("PK_Patients");

                
                entity.Property(p => p.Id)
                      .ValueGeneratedOnAdd();

                
                entity.HasIndex(p => p.Email)
                      .IsUnique()
                      .HasDatabaseName("IX_Patients_Email");

                entity.HasIndex(p => p.Username)
                      .IsUnique()
                      .HasDatabaseName("IX_Patients_Username");
            });
        }
    }
}
