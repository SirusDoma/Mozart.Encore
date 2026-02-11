using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mozart.Metadata;
using Mozart.Data.Entities;
using Mozart.Options;

namespace Mozart.Data.Contexts;

public sealed class MainDbContext(
    DbContextOptions<MainDbContext> contextOptions,
    IOptions<AuthOptions> authOptions
) : DbContext(contextOptions)
{
    public DbSet<User> Users { get; init; }

    public DbSet<UserRanking> UserRankings { get; init; }

    public DbSet<Credential> Credentials { get; init; }

    public DbSet<AuthSession> Sessions { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureAuth(modelBuilder);
        ConfigureWallet(modelBuilder);
        ConfigureLoadout(modelBuilder);
        ConfigureRanking(modelBuilder);
        ConfigureAcquiredMusicList(modelBuilder);
        ConfigureAttributiveItem(modelBuilder);
        ConfigureGiftItem(modelBuilder);
        ConfigureGiftMusic(modelBuilder);
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
                );

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

            entity.HasOne<Wallet>("Wallet")
                .WithOne()
                .HasForeignKey<Wallet>(c => c.UserId)
                .IsRequired(false);

            entity.HasOne<Loadout>("Loadout")
                .WithOne()
                .HasForeignKey<Loadout>(b => b.UserId)
                .IsRequired(false);

            entity.HasOne<UserRanking>("UserRanking")
                .WithOne()
                .HasForeignKey<UserRanking>(r => r.UserId)
                .IsRequired(false);

            entity.HasMany<AcquiredMusic>(u => u.AcquiredMusicList)
                .WithOne()
                .HasForeignKey(a => a.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany<AttributiveItem>("AttributiveItems")
                .WithOne()
                .HasForeignKey(a => a.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany<GiftItem>("GiftItems")
                .WithOne()
                .HasForeignKey(i => i.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany<GiftMusic>("GiftMusics")
                .WithOne()
                .HasForeignKey(m => m.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Navigation("Wallet")
                .AutoInclude();

            entity.Navigation("Loadout")
                .AutoInclude();

            entity.Navigation("UserRanking")
                .AutoInclude();

            entity.Navigation(e => e.AcquiredMusicList)
                .AutoInclude();

            entity.Navigation("AttributiveItems")
                .AutoInclude();

            entity.Navigation("GiftItems")
                .AutoInclude();

            entity.Navigation("GiftMusics")
                .AutoInclude();

            entity.HasIndex(e => e.Username)
                .IsUnique();

            entity.HasIndex(e => e.Nickname)
                .IsUnique();
        });
    }

    private void ConfigureAuth(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Credential>(entity =>
        {
            entity.ToTable("member");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            if (authOptions.Value.Mode == AuthMode.Default)
            {
                entity.Property(e => e.Username)
                    .HasColumnName("userid")
                    .HasMaxLength(50);

                entity.Property(e => e.Password)
                    .HasColumnName("passwd");
            }
            else
            {
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
            }

            entity.Property<DateTime>("registdate")
                .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Original is GetDate(), but this work across different RDBMS

            entity.HasIndex(e => e.Username)
                .IsUnique();
        });

        modelBuilder.Entity<AuthSession>(entity =>
        {
            entity.ToTable("t_o2jam_login");

            entity.Property(e => e.UserId)
                .HasColumnName("USER_INDEX_ID")
                .ValueGeneratedNever();

            entity.HasKey(e => e.UserId);

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

    private void ConfigureLoadout(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Loadout>(entity =>
        {
            entity.ToTable("t_o2jam_item");

            entity.HasKey(e => e.UserId);

            entity.Property(e => e.UserId)
                .HasColumnName("USER_INDEX_ID")
                .ValueGeneratedNever();
        });
    }

    private void ConfigureRanking(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRanking>(entity =>
        {
            entity.ToTable("t_o2jam_dumpranking");

            entity.HasKey(e => e.Ranking);

            entity.Property(e => e.UserId)
                .HasColumnName("User_Index_ID");

            entity.Property(e => e.Ranking)
                .HasColumnName("Ranking")
                .ValueGeneratedOnAdd();

            entity.HasIndex(e => e.UserId);
        });
    }

    private void ConfigureAttributiveItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AttributiveItem>(entity =>
        {
            entity.ToTable("t_o2jam_char_attr_item");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("INDEX_ID")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.UserId)
                .HasColumnName("USER_INDEX_ID");

            entity.Property(e => e.ItemId)
                .HasColumnName("ITEM_INDEX_ID");

            entity.Property(e => e.Count)
                .HasColumnName("USED_COUNT");

            entity.Property(e => e.AcquiredAt)
                .HasColumnName("REG_DATE")
                .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Original is GetDate(), but this work across different RDBMS

            entity.Property(e => e.PreviousCount)
                .HasColumnName("OLD_USED_COUNT")
                .HasDefaultValue(0);

            entity.HasIndex(e => e.UserId);
        });
    }

    private void ConfigureAcquiredMusicList(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcquiredMusic>(entity =>
        {
            entity.ToTable("t_o2jam_musiclist");

            entity.HasKey(e => e.UserId);

            entity.Property(e => e.UserId)
                .HasColumnName("USER_INDEX_ID")
                .ValueGeneratedNever();

            entity.Property(e => e.MusicId)
                .HasColumnName("MUSIC_ID");
        });
    }

    private void ConfigureGiftItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GiftItem>(entity =>
        {
            entity.ToTable("t_o2jam_gift_item");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("Seq")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.UserId)
                .HasColumnName("User_Index_ID");

            entity.Property(e => e.ItemId)
                .HasColumnName("ItemID");

            entity.Property(e => e.SenderId)
                .HasColumnName("Sender_Index_ID");

            entity.Property(e => e.SenderNickname)
                .HasColumnName("SenderNickname")
                .HasMaxLength(50);

            entity.Property(e => e.SendDate)
                .HasColumnName("SendDate")
                .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Original is GetDate(), but this work across different RDBMS

            entity.HasIndex(e => e.UserId);
        });
    }

    private void ConfigureGiftMusic(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GiftMusic>(entity =>
        {
            entity.ToTable("t_o2jam_gift_music");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("Seq")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.UserId)
                .HasColumnName("User_Index_ID");

            entity.Property(e => e.MusicId)
                .HasColumnName("Music_ID");

            entity.Property(e => e.SenderId)
                .HasColumnName("Sender_Index_ID");

            entity.Property(e => e.SenderNickname)
                .HasColumnName("SenderNickname")
                .HasMaxLength(50);

            entity.Property(e => e.SendDate)
                .HasColumnName("SendDate")
                .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Original is GetDate(), but this work across different RDBMS

            entity.HasIndex(e => e.UserId);
        });
    }
}
