using LolTeamTracker.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LolTeamTracker.Api.Data
{
    /// <summary>
    /// EF Core 的資料庫工作階段。
    /// 它同時是 Repository（每個 DbSet 是一個集合入口）
    /// 與 Unit of Work（SaveChanges 把整批變更當一筆交易送出）。
    /// </summary>
    public class AppDbContext : DbContext
    {
        // 建構子由 DI 注入設定（連線字串、供應商等），不要在這裡自己 new 連線
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Player> Players { get; set; } = null!;
        public DbSet<Match> Matches { get; set; } = null!;
        public DbSet<MatchPlayer> MatchPlayers { get; set; } = null!;
        public DbSet<QueueDefinition> QueueDefinitions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Puuid).HasMaxLength(78)
                      .IsRequired();

                // 防止同一個玩家改名後被重複加入（GameName + TagLine 擋不住改名情境）
                entity.HasIndex(e => e.Puuid).IsUnique();

                entity.Property(e => e.GameName).HasMaxLength(30)
                      .IsRequired();

                entity.Property(e => e.TagLine).HasMaxLength(10)
                      .IsRequired();

                entity.Property(e => e.CreatedAt)
                      .IsRequired();

                entity.Property(e => e.UpdatedAt)
                      .IsRequired();

                // Riot ID 查詢與避免重複
                entity.HasIndex(e => new { e.GameName, e.TagLine })
                      .IsUnique();
            });

            modelBuilder.Entity<Match>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(30);
                entity.Property(e => e.QueueId).IsRequired();
                entity.Property(e => e.GameDate).IsRequired();
                entity.Property(e => e.GameDuration);
                entity.Property(e => e.CreatedAt)
                      .IsRequired();
                entity.Property(e => e.UpdatedAt)
                      .IsRequired();

                // 與 QueueDefinition 的外鍵關係
                entity.HasOne(m => m.Queue)
                      .WithMany(q => q.Matches)
                      .HasForeignKey(m => m.QueueId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<MatchPlayer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MatchId).HasMaxLength(30);
                entity.Property(e => e.PlayerId).IsRequired();

                entity.Property(e => e.ChampionId).IsRequired();
                entity.Property(e => e.TeamPosition).HasMaxLength(10);
                entity.Property(e => e.Kills).IsRequired();
                entity.Property(e => e.Deaths).IsRequired();
                entity.Property(e => e.Assists).IsRequired();
                entity.Property(e => e.Win).IsRequired();
                entity.Property(e => e.LaneCS).IsRequired();
                entity.Property(e => e.JungleCS).IsRequired();
                entity.Property(e => e.Gold).IsRequired();
                entity.Property(e => e.GameDate).IsRequired();

                entity.Property(e => e.CreatedAt)
                        .IsRequired();

                entity.Property(e => e.UpdatedAt)
                      .IsRequired();

                // 與 Match 的外鍵關係
                entity.HasOne(mp => mp.Match)
                      .WithMany(mp => mp.Participants)
                      .HasForeignKey(mp => mp.MatchId)
                      .OnDelete(DeleteBehavior.Restrict);

                // 與 Player 的外鍵關係
                entity.HasOne(mp => mp.Player)
                      .WithMany()
                      .HasForeignKey(mp => mp.PlayerId)
                      .OnDelete(DeleteBehavior.Restrict);

                // 避免重複 :  MatchId + PlayerId （同一場比賽同一個玩家只能有一筆紀錄）
                entity.HasIndex(e => new { e.MatchId, e.PlayerId })
                      .IsUnique();

                // 效能：服務「查某人最近 N 場」—— 反正規化 GameDate 的全部理由。
                // INCLUDE(MatchId) 讓這個索引「涵蓋」整個查詢，不必回主表撈欄位。
                // 2026-08-22 實測（25 萬列，撈某玩家 26,373 筆）：
                //   無索引            3,798 次邏輯讀取
                //   有索引但要回表    1,051 次（執行計畫顯示 95% 成本在 Key Lookup）
                //   涵蓋索引            151 次（執行計畫只剩一個 Index Seek）
                entity.HasIndex(e => new { e.PlayerId, e.GameDate }) 
                      .IncludeProperties(e => e.MatchId);
            });

            modelBuilder.Entity<QueueDefinition>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever(); // 自訂義Id : 如 420 , 440

                entity.Property(e => e.Name)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.Description)
                      .HasMaxLength(255);

                entity.Property(e => e.UpdatedAt)
                      .IsRequired();
            });
        }
    }
}
