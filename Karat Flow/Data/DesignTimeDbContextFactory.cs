using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Karat_Flow.Data;

namespace Karat_Flow.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<KaratFlowDbContext>
    {
        public KaratFlowDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<KaratFlowDbContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=KaratFlowDB;Trusted_Connection=True;");

            return new KaratFlowDbContext(optionsBuilder.Options);
        }
    }
}
