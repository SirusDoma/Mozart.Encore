using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mozart.Metadata;
using Mozart.Data.Entities;
using Mozart.Options;

namespace Mozart.Data.Contexts;

public sealed class UserDbContext(DbContextOptions<UserDbContext> contextOptions,
    IOptions<AuthOptions> authOptions) : DbContext(contextOptions)
{
    public DbSet<User> Users { get; init; }

    public DbSet<Credential> Credentials { get; init; }

    public DbSet<AuthSession> Sessions { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureAuth(modelBuilder);
        ConfigureInventory(modelBuilder);
        ConfigureWallet(modelBuilder);
    }

    private void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("t_o2jam_charinfo");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("USER_INDEX_ID")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Username)
                .HasColumnName("USER_ID")
                .HasMaxLength(50)
                .HasConversion(
                    str => str.Trim(),
                    str => str.Trim()
                );

            entity.Property(e => e.Nickname)
                .HasColumnName("USER_NICKNAME")
                .HasMaxLength(50)
                .HasConversion(
                    str => str.Trim(),
                    str => str.Trim()
                );;

            entity.Property(e => e.Gender)
                .HasColumnName("Sex")
                .HasConversion(
                    gender    => gender == Gender.Male || gender == Gender.Any,
                    flag => flag ? Gender.Male : Gender.Female
                );

            entity.Property(e => e.IsAdministrator)
                .HasColumnName("AdminLevel")
                .HasConversion(
                    admin => admin ? 1 : 0,
                    flag    => flag != 0
                );

            entity.HasOne<Inventory>("Items")
                .WithOne()
                .HasForeignKey<Inventory>(i => i.UserId)
                .IsRequired(false);

            entity.HasOne<Wallet>("Wallet")
                .WithOne()
                .HasForeignKey<Wallet>(c => c.UserId)
                .IsRequired(false);

            entity.Navigation("Items")
                .AutoInclude();

            entity.Navigation("Wallet")
                .AutoInclude();

            entity.Ignore(e => e.Gem);
            entity.Ignore(e => e.Point);
            entity.Ignore(e => e.Inventory);
            entity.Ignore(e => e.Equipments);

            entity.HasIndex(e => new { e.Username, e.Nickname })
                .IsUnique();
        });
    }

    private void ConfigureAuth(ModelBuilder modelBuilder)
    {
        if (authOptions.Value.Mode == AuthMode.Default)
        {
            modelBuilder.Entity<Credential>(entity =>
            {
                entity.ToTable("t_o2jam_credentials");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Username)
                    .HasMaxLength(12);

                entity.HasIndex(e => e.Username)
                    .IsUnique();
            });
        }
        else
        {
            modelBuilder.Entity<Credential>(entity =>
            {
                entity.ToTable("member");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Username)
                    .HasColumnName("userid")
                    .HasMaxLength(12);

                entity.Property(e => e.Password)
                    .HasColumnName("passwd")
                    .HasMaxLength(20)
                    .HasConversion(
                        pwd => Encoding.UTF8.GetString(pwd),
                        str => Encoding.UTF8.GetBytes(str)
                    );

                entity.Ignore(e => e.UserId);

                entity.HasIndex(e => e.Username)
                    .IsUnique();
            });
        }

        modelBuilder.Entity<AuthSession>(entity =>
        {
            entity.ToTable("t_o2jam_login");

            entity.Property(e => e.UserId)
                .HasColumnName("USER_INDEX_ID")
                .ValueGeneratedNever();

            entity.HasKey("UserId");

            entity.Property(e => e.GatewayId)
                .HasColumnName("GATEWAY_ID")
                .HasMaxLength(20);

            entity.Property(e => e.ServerId)
                .HasColumnName("MAIN_CH");

            entity.Property(e => e.ChannelId)
                .HasColumnName("SUB_CH");

            entity.Property(e => e.Address)
                .HasColumnName("ADDR_IP")
                .HasConversion(
                    ip        => ip.ToString(),
                    str => IPAddress.Parse(str)
                );

            entity.Property(e => e.Username)
                .HasColumnName("USER_ID")
                .HasMaxLength(20);

            entity.Property(e => e.Token)
                .HasColumnName("TUSER_ID")
                .HasMaxLength(50);

            entity.Property(e => e.LoginTime)
                .HasColumnName("LOGIN_TIME");
        });
    }

    private void ConfigureInventory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.ToTable("t_o2jam_item");

            entity.HasKey(e => e.UserId);

            entity.Property(e => e.UserId)
                .HasColumnName("USER_INDEX_ID")
                .ValueGeneratedNever();
        });
    }

    private void ConfigureWallet(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.ToTable("t_o2jam_charcash");

            entity.HasKey(e => e.UserId);

            entity.Property(e => e.UserId)
                .HasColumnName("USER_INDEX_ID")
                .ValueGeneratedNever();
        });
    }
}