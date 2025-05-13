using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace assignmentAPI.Models;

public partial class AssignmentDbContext : DbContext
{
    public AssignmentDbContext()
    {
    }

    public AssignmentDbContext(DbContextOptions<AssignmentDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<User> Users { get; set; }

 //   protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
   //     => optionsBuilder.UseSqlServer("Server=lab7L95SR\\SQLEXPRESS;Database=AssignmentDb;Trusted_Connection=True;TrustServerCertificate=True;");

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Item>(entity =>
    {
        entity.HasKey(e => e.ItemId).HasName("PK__Items__727E83EBC3C405E3");

        entity.Property(e => e.ItemId).HasColumnName("ItemID");
        entity.Property(e => e.ItemCategory).HasMaxLength(100);
        entity.Property(e => e.ItemName).HasMaxLength(255);
        entity.Property(e => e.ProductionDate).HasMaxLength(50);
        entity.Property(e => e.UserId)
            .HasMaxLength(255)
            .HasColumnName("UserID");

        entity.HasOne(d => d.User).WithMany(p => p.Items)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK__Items__UserID__398D8EEE");
    });

    modelBuilder.Entity<User>(entity =>
    {
        entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC7874A7DA");

        entity.Property(e => e.UserId)
            .HasMaxLength(255)
            .HasColumnName("UserID");

        entity.Property(e => e.FirstName).HasMaxLength(100);
        entity.Property(e => e.LastName).HasMaxLength(100);
        entity.Property(e => e.UserRole).HasMaxLength(50);
 
    });

    OnModelCreatingPartial(modelBuilder);
}

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
