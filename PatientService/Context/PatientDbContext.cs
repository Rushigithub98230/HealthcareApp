using Microsoft.EntityFrameworkCore;
using PatientService.Models;

namespace PatientService.Context
{
    public class PatientDbContext : DbContext
    {
        public PatientDbContext(DbContextOptions<PatientDbContext> options) : base(options) { }

        public DbSet<Patient> Patients => Set<Patient>();
    }
}
