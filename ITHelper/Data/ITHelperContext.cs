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

        public DbSet<ITHelper.Models.Ticket> Tickets { get; set; }

        public DbSet<ITHelper.Models.Update> Updates { get; set; }
    }
}
