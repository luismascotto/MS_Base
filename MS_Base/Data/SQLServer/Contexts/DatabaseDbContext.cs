using Microsoft.EntityFrameworkCore;

namespace MS_Base.Data.SQLServer.Contexts
{
    public class DatabaseDbContext : DbContext
    {
        public string ConnectionString { get; }

        public DatabaseDbContext()
        {
            ConnectionString = SQLDbManager.GetConnectionString();
        }

        public DatabaseDbContext(string connString)
        {
            ConnectionString = connString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }

}
