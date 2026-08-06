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
        }
    }
}
