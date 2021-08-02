using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace TelegramCryptoChatBot.Models
{
    public partial class bscBotContext : DbContext
    {
        public bscBotContext()
        {
        }

        public bscBotContext(DbContextOptions<bscBotContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Choosencontract> Choosencontracts { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=bscBot;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Choosencontract>(entity =>
            {
                entity.ToTable("choosencontracts");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Contract)
                    .IsRequired()
                    .HasMaxLength(60)
                    .HasColumnName("contract");

                entity.Property(e => e.Ticker)
                    .HasMaxLength(10)
                    .HasColumnName("ticker");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("userID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Choosencontracts)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_choosencontracts_users");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.Property(e => e.UserId)
                    .HasMaxLength(50)
                    .HasColumnName("userID");

                entity.Property(e => e.CurrentState)
                    .IsRequired()
                    .HasMaxLength(5)
                    .HasColumnName("currentState");

                entity.Property(e => e.Username)
                    .HasMaxLength(50)
                    .HasColumnName("username");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
