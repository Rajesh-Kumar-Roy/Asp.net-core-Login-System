﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AuthorizationTestProject.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthorizationTestProject.DBContext
{
    public class ATestDBContext:IdentityDbContext<ApplicationUser>
    {
        public ATestDBContext(DbContextOptions<ATestDBContext> options):base(options)
        {
            
        }

        public DbSet<Student> Students { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            foreach (var foreignKey in builder.Model.GetEntityTypes().SelectMany(c=>c.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}
