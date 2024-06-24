using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Bank.net.Models;

public partial class BankDBContext : DbContext
{
    public BankDBContext()
    {
    }

    public BankDBContext(DbContextOptions<BankDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Loan> Loans { get; set; }

    public virtual DbSet<Token> Tokens { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:BankDB");
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__loans__3213E83F10E4CFC7");

            entity.ToTable("loans");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.Length).HasColumnName("length");
            entity.Property(e => e.MonthlyRate)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("monthly_rate");
            entity.Property(e => e.PaymentLeft)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("payment_left");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.Type)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Loans)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_loans_user");
        });

        modelBuilder.Entity<Token>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tmp_ms_x__3213E83FEFA3E0D1");

            entity.ToTable("token");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Expired).HasColumnName("expired");
            entity.Property(e => e.Revoked).HasColumnName("revoked");
            entity.Property(e => e.Token1)
                .IsUnicode(false)
                .HasColumnName("token");
            entity.Property(e => e.TokenType)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("token_type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Tokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_token_user");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__transact__3213E83F8A68EB8C");

            entity.ToTable("transactions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(6)
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.Recipient)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("recipient");
            entity.Property(e => e.Type)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_transactions_user");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tmp_ms_x__3213E83F03726888");

            entity.ToTable("_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountNumber)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("account_number");
            entity.Property(e => e.Balance)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("balance");
            entity.Property(e => e.Dob).HasColumnName("dob");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Firstname)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("firstname");
            entity.Property(e => e.HasLoan).HasColumnName("has_loan");
            entity.Property(e => e.Lastname)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("lastname");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Pesel)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("pesel");
            entity.Property(e => e.Role)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("role");
        });
        modelBuilder.HasSequence("_user_seq").IncrementsBy(50);
        modelBuilder.HasSequence("token_seq").IncrementsBy(50);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
