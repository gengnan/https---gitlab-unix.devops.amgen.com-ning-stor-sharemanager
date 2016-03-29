using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OnboardingApi.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;



namespace OnboardingApi.DAL
{
    public class ShareContext : DbContext
    {
        public ShareContext() : base("ApiContextConnection") { }

        
        public DbSet<CifsShare> CifsShares { get; set; }
        public DbSet<Ou> Ous { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}