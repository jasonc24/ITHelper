using Microsoft.EntityFrameworkCore;

namespace ITHelper.Data
{
    public class ITHelperContext : DbContext
    {
        public ITHelperContext (DbContextOptions<ITHelperContext> options)
            : base(options)
        {
        }

        public DbSet<Models.ITTicket> ITTickets { get; set; }

        public DbSet<Models.BuildingTicket> BuildingTickets { get; set; }

        public DbSet<Models.Update> Updates { get; set; }

        public DbSet<Models.SystemParameter> SystemParameters { get; set; }
    }
}
