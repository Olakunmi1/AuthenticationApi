using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Authenticate.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }


    }
}
