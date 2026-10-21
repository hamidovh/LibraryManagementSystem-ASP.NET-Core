using System;
using System.Collections.Generic;
using LibraryManagementSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data;

public partial class LMSdbContext : DbContext
{
    public LMSdbContext()
    {
    }

    public LMSdbContext(DbContextOptions<LMSdbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TblAuthor> TblAuthors { get; set; }

    public virtual DbSet<TblAuthorImage> TblAuthorImages { get; set; }

    public virtual DbSet<TblBook> TblBooks { get; set; }

    public virtual DbSet<TblBookImage> TblBookImages { get; set; }

    public virtual DbSet<TblBorrow> TblBorrows { get; set; }

    public virtual DbSet<TblCategory> TblCategories { get; set; }

    public virtual DbSet<TblCategoryImage> TblCategoryImages { get; set; }

    public virtual DbSet<TblFine> TblFines { get; set; }

    public virtual DbSet<TblRole> TblRoles { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    // #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.

    /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=LibraryManagementSystemDb;Integrated Security=True;TrustServerCertificate=True");*/

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("name=appconnection");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Latin1_General_100_CI_AS");

        modelBuilder.Entity<TblAuthor>(entity =>
        {
            entity.HasKey(e => e.AuthorId).HasName("PK_tblAuthors_AuthorID");

            entity.Property(e => e.AuthorCreatedDate).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.AuthorIsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<TblAuthorImage>(entity =>
        {
            entity.HasKey(e => e.AuthorImageId).HasName("PK_tblAuthorImages_AuthorImageID");

            entity.HasOne(d => d.Author).WithMany(p => p.TblAuthorImages).HasConstraintName("FK_tblAuthorImages_tblAuthors");
        });

        modelBuilder.Entity<TblBook>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("PK_tblBooks_BookID");

            entity.Property(e => e.BookCreatedDate).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.BookIsActive).HasDefaultValue(true);

            entity.HasMany(d => d.Authors).WithMany(p => p.Books)
                .UsingEntity<Dictionary<string, object>>(
                    "TblBookAuthor",
                    r => r.HasOne<TblAuthor>().WithMany()
                        .HasForeignKey("AuthorId")
                        .HasConstraintName("FK_tblBookAuthors_tblAuthors"),
                    l => l.HasOne<TblBook>().WithMany()
                        .HasForeignKey("BookId")
                        .HasConstraintName("FK_tblBookAuthors_tblBooks"),
                    j =>
                    {
                        j.HasKey("BookId", "AuthorId");
                        j.ToTable("tblBookAuthors");
                        j.HasIndex(new[] { "AuthorId", "BookId" }, "IX_tblBookAuthors_AuthorID_BookID");
                        j.IndexerProperty<int>("BookId").HasColumnName("BookID");
                        j.IndexerProperty<int>("AuthorId").HasColumnName("AuthorID");
                    });

            entity.HasMany(d => d.Categories).WithMany(p => p.Books)
                .UsingEntity<Dictionary<string, object>>(
                    "TblBookCategory",
                    r => r.HasOne<TblCategory>().WithMany()
                        .HasForeignKey("CategoryId")
                        .HasConstraintName("FK_tblBookCategories_tblCategories"),
                    l => l.HasOne<TblBook>().WithMany()
                        .HasForeignKey("BookId")
                        .HasConstraintName("FK_tblBookCategories_tblBooks"),
                    j =>
                    {
                        j.HasKey("BookId", "CategoryId");
                        j.ToTable("tblBookCategories");
                        j.HasIndex(new[] { "CategoryId", "BookId" }, "IX_tblBookCategories_CategoryID_BookID");
                        j.IndexerProperty<int>("BookId").HasColumnName("BookID");
                        j.IndexerProperty<int>("CategoryId").HasColumnName("CategoryID");
                    });
        });

        modelBuilder.Entity<TblBookImage>(entity =>
        {
            entity.HasKey(e => e.BookImageId).HasName("PK_tblBookImages_BookImageID");

            entity.HasOne(d => d.Book).WithMany(p => p.TblBookImages).HasConstraintName("FK_tblBookImages_tblBooks");
        });

        modelBuilder.Entity<TblBorrow>(entity =>
        {
            entity.HasKey(e => e.BorrowId).HasName("PK_tblBorrows_BorrowID");

            entity.HasIndex(e => e.BookId, "IX_tblBorrows_Active_BookID").HasFilter("([BorrowReturnDate] IS NULL)");

            entity.HasIndex(e => new { e.UserId, e.BookId }, "UX_tblBorrows_Active_User_Book")
                .IsUnique()
                .HasFilter("([BorrowReturnDate] IS NULL)");

            entity.Property(e => e.BorrowDate).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Book).WithMany(p => p.TblBorrows)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tblBorrows_tblBooks");

            entity.HasOne(d => d.User).WithMany(p => p.TblBorrows)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tblBorrows_tblUsers");
        });

        modelBuilder.Entity<TblCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK_tblCategories_CategoryID");

            entity.Property(e => e.CategoryCreatedDate).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.CategoryIsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<TblCategoryImage>(entity =>
        {
            entity.HasKey(e => e.CategoryImageId).HasName("PK_tblCategoryImages_CategoryImageID");

            entity.HasOne(d => d.Category).WithMany(p => p.TblCategoryImages).HasConstraintName("FK_tblCategoryImages_tblCategories");
        });

        modelBuilder.Entity<TblFine>(entity =>
        {
            entity.HasKey(e => e.FineId).HasName("PK_tblFines_FineID");

            entity.Property(e => e.FineDate).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Borrow).WithOne(p => p.TblFine)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tblFines_tblBorrows");
        });

        modelBuilder.Entity<TblRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK_tblRoles_RoleID");

            entity.Property(e => e.RoleCreatedDate).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.RoleIsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK_tblUsers_UserID");

            entity.Property(e => e.UserIsActive).HasDefaultValue(true);
            entity.Property(e => e.UserRegistrationDate).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Role).WithMany(p => p.TblUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tblUsers_tblRoles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
