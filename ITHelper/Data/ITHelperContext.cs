using Microsoft.EntityFrameworkCore;
using ITHelper.Models;

namespace ITHelper.Data
{
    public class ITHelperContext : DbContext
    {
        public ITHelperContext (DbContextOptions<ITHelperContext> options) : base(options)
        { }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Location> Locations { get; set; }

        public DbSet<Ticket> Tickets { get; set; }

        public DbSet<AVTicket> AVTickets { get; set; }

        public DbSet<Update> Updates { get; set; }

        public DbSet<SystemParameter> SystemParameters { get; set; }        
    }
}
