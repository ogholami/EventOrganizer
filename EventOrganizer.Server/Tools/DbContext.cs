using Microsoft.EntityFrameworkCore;
using EventOrganizer.Server.Models;
using System.Collections.Generic;

namespace EventOrganizer.Server.Tools
{
    namespace EventOrganizer.Server.Data
    {
        public class ApplicationDbContext : DbContext
        {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options) { }

            public DbSet<User> Users { get; set; }

            // Add other entities here
        }
    }
}
