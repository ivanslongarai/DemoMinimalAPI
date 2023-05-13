using DemoMinimalAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DemoMinimalAPI.Data
{
    public class MinimalContextDb : DbContext
    {
        public MinimalContextDb(DbContextOptions<MinimalContextDb> options) : base(options)
        {

        }

        public DbSet<Supplier> Suppliers { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Supplier>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<Supplier>()
                .Property(s => s.Name)
                .IsRequired()
                .HasColumnType("varchar(200)");

            modelBuilder.Entity<Supplier>()
                .Property(s => s.Document)
                .IsRequired()
                .HasColumnType("varchar(14)");

            modelBuilder.Entity<Supplier>()
                .ToTable("Suppliers");

            base.OnModelCreating(modelBuilder);
        }

    }
}
