using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ITHelper.Models;

namespace ITHelper.Data
{
    public class ITHelperContext : DbContext
    {
        public ITHelperContext (DbContextOptions<ITHelperContext> options)
            : base(options)
        {
        }

        public DbSet<ITHelper.Models.ITTicket> ITTickets { get; set; }

        public DbSet<ITHelper.Models.BuildingTicket> BuildingTickets { get; set; }

        public DbSet<ITHelper.Models.Update> Updates { get; set; }

        public DbSet<ITHelper.Models.SystemParameter> SystemParameters { get; set; }
    }
}
