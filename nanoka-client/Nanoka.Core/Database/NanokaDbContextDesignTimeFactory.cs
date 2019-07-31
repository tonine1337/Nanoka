using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nanoka.Core.Database
{
    public class NanokaDbContextDesignTimeFactory : IDesignTimeDbContextFactory<NanokaDbContext>
    {
        public NanokaDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder().UseSqlite("Data Source=temp.db");

            return new NanokaDbContext(builder.Options);
        }
    }
}
