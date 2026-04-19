using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mozart.Data.Entities;
using Mozart.Metadata;
using Mozart.Options;

namespace Mozart.Data.Contexts;

public sealed class MainDbContext(
    DbContextOptions<MainDbContext> contextOptions,
    IOptions<AuthOptions> authOptions
) : DbContext(contextOptions)
{
    public DbSet<User> Users { get; init; }

    public DbSet<UserRanking> UserRankings { get; init; }

    public DbSet<UserRankingExtended> UserRankingsExtended { get; init; }

    public DbSet<Member> Members { get; init; }

    public DbSet<AuthSession> Sessions { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureMember(modelBuilder);
        ConfigureWallet(modelBuilder);
        ConfigureLoadout(modelBuilder);
        ConfigureRanking(modelBuilder);
        ConfigureAcquiredMusicList(modelBuilder);
        ConfigureAttributiveItem(modelBuilder);
        ConfigureGiftItem(modelBuilder);
        ConfigureGiftMusic(modelBuilder);
        ConfigureUserMessage(modelBuilder);
        ConfigureUserScore(modelBuilder);
        ConfigurePenalty(modelBuilder);
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

            entity.HasOne<Member>("Member")
                .WithOne()
                .HasForeignKey<Member>(m => m.Username)
                .HasPrincipalKey<User>(u => u.Username)
                .IsRequired();

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

            entity.HasOne<UserRankingExtended>("UserRankingExtended")
                .WithOne()
                .HasForeignKey<UserRankingExtended>(r => r.UserId)
                .IsRequired(false);

            entity.HasMany<AcquiredMusic>(u => u.AcquiredMusicList)
                .WithOne()
                .HasForeignKey(a => a.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.MusicScoreRecords)
                .WithOne()
                .HasForeignKey(r => r.UserId)
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

            entity.HasMany<UserMessage>("UserMessages")
                .WithOne()
                .HasForeignKey(m => m.ReceiverId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Penalty>("Penalty")
                .WithOne()
                .HasForeignKey<Penalty>(p => p.UserId)
                .IsRequired(false);

            entity.Navigation("Member")
                .AutoInclude();

            entity.Navigation("Wallet")
                .AutoInclude();

            entity.Navigation("Loadout")
                .AutoInclude();

            entity.Navigation("UserRanking")
                .AutoInclude();

            entity.Navigation("UserRankingExtended")
                .AutoInclude();

            entity.Navigation(e => e.AcquiredMusicList)
                .AutoInclude();

            entity.Navigation(e => e.MusicScoreRecords)
                .AutoInclude();

            entity.Navigation("AttributiveItems")
                .AutoInclude();

            entity.Navigation("GiftItems")
                .AutoInclude();

            entity.Navigation("GiftMusics")
                .AutoInclude();

            entity.Navigation("UserMessages")
                .AutoInclude();

            entity.Navigation("Penalty")
                .AutoInclude();

            entity.HasIndex(e => e.Username)
                .IsUnique();

            entity.HasIndex(e => e.Nickname)
                .IsUnique();
        });
    }

    private void ConfigureMember(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Member>(entity =>
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

            entity.Property(e => e.Vip)
                .HasColumnName("vip")
                .HasDefaultValue((short)0);

            entity.Property(e => e.VipDate)
                .HasColumnName("vipdate");

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

            entity.Property(e => e.BagExpansionCount)
                .HasColumnName("BAG_EXT_COUNT")
                .HasDefaultValue(0);
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

        modelBuilder.Entity<UserRankingExtended>(entity =>
        {
            entity.ToTable("t_o2jam_user_ranking");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("Seq")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.UserId)
                .HasColumnName("User_Index_ID");

            entity.Property(e => e.Username)
                .HasColumnName("User_ID")
                .HasMaxLength(40);

            entity.Property(e => e.Nickname)
                .HasColumnName("User_NickName")
                .HasMaxLength(40);

            entity.Property(e => e.Sex)
                .HasColumnName("Sex");

            entity.Property(e => e.Level)
                .HasColumnName("Level");

            entity.Property(e => e.Battle)
                .HasColumnName("Battle");

            entity.Property(e => e.Win)
                .HasColumnName("Win");

            entity.Property(e => e.Draw)
                .HasColumnName("Draw");

            entity.Property(e => e.Lose)
                .HasColumnName("Lose");

            entity.Property(e => e.Experience)
                .HasColumnName("Experience");

            entity.Property(e => e.WriteDate)
                .HasColumnName("WriteDate")
                .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Original is GetDate(), but this work across different RDBMS

            entity.Property(e => e.Ranking)
                .HasColumnName("Ranking")
                .HasDefaultValue(0);

            entity.Property(e => e.ChangeType)
                .HasColumnName("ChangeType");

            entity.Property(e => e.ChangeRanking)
                .HasColumnName("ChangeRanking");

            entity.HasIndex(e => e.UserId)
                .IsUnique();
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

    private void ConfigureUserMessage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserMessage>(entity =>
        {
            entity.ToTable("t_o2jam_message");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("Seq")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.SenderUsername)
                .HasColumnName("SenderID")
                .HasMaxLength(50);

            entity.Property(e => e.SenderId)
                .HasColumnName("SenderIndexID");

            entity.Property(e => e.SenderNickname)
                .HasColumnName("SenderNickName")
                .HasMaxLength(50);

            entity.Property(e => e.ReceiverUsername)
                .HasColumnName("ReceiverID")
                .HasMaxLength(50);

            entity.Property(e => e.ReceiverId)
                .HasColumnName("ReceiverIndexID");

            entity.Property(e => e.ReceiverNickname)
                .HasColumnName("ReceiverNickName")
                .HasMaxLength(50);

            entity.Property(e => e.Title)
                .HasMaxLength(40);

            entity.Property(e => e.Content)
                .HasMaxLength(400);

            entity.Property(e => e.WriteDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Original is GetDate(), but this work across different RDBMS

            entity.Property(e => e.IsRead)
                .HasColumnName("ReadFlag")
                .HasColumnType("char(1)")
                .HasConversion(
                    v => v ? '1' : '0',
                    v => v == '1'
                )
                .HasDefaultValue(false);

            entity.Property(e => e.GiftType)
                .HasColumnName("TypeFlag")
                .HasColumnType("char(1)")
                .HasConversion(
                    v => (char)('0' + (byte)v),
                    v => (GiftType)(byte)(v - '0')
                );

            entity.HasIndex(e => e.ReceiverId);

            entity.HasQueryFilter(x => !x.IsRead);
        });
    }

    private void ConfigurePenalty(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Penalty>(entity =>
        {
            entity.ToTable("t_o2jam_penalty");

            entity.HasKey(e => e.UserId);

            entity.Property(e => e.UserId)
                .HasColumnName("USER_INDEX_ID")
                .ValueGeneratedNever();

            entity.Property(e => e.Level)
                .HasColumnName("LEVEL");

            entity.Property(e => e.Count)
                .HasColumnName("COUNT");
        });
    }

    private void ConfigureUserScore(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MusicScoreRecord>(entity =>
        {
            entity.ToTable("t_o2jam_user_music_ranking");

            entity.HasKey(e => new { e.UserId, e.MusicId, e.Difficulty });

            entity.Property(e => e.UserId)
                .HasColumnName("USER_INDEX_ID")
                .ValueGeneratedNever();

            entity.Property(e => e.MusicId)
                .HasColumnName("MUSIC_INDEX_ID")
                .ValueGeneratedNever();

            entity.Property(e => e.Difficulty)
                .HasColumnName("DIFFICULTY")
                .ValueGeneratedNever();

            entity.Property(e => e.Score)
                .HasColumnName("USER_SCORE");

            entity.Property(e => e.ClearType)
                .HasColumnName("FLAG");
        });
    }
}
