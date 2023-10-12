using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace domain.lfexentitys
{
    public partial class lfex_serviceContext : DbContext
    {
        public lfex_serviceContext(DbContextOptions<lfex_serviceContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Appeals> Appeals { get; set; }
        public virtual DbSet<AuthenticationInfos> AuthenticationInfos { get; set; }
        public virtual DbSet<BackstageUser> BackstageUser { get; set; }
        public virtual DbSet<CityCandyDividend> CityCandyDividend { get; set; }
        public virtual DbSet<CityCandyDividendRecord> CityCandyDividendRecord { get; set; }
        public virtual DbSet<CityCashDividend> CityCashDividend { get; set; }
        public virtual DbSet<CityCashDividendRecord> CityCashDividendRecord { get; set; }
        public virtual DbSet<CityEarnings> CityEarnings { get; set; }
        public virtual DbSet<CoinMoveRecord> CoinMoveRecord { get; set; }
        public virtual DbSet<CoinTrade> CoinTrade { get; set; }
        public virtual DbSet<CoinTradeExt> CoinTradeExt { get; set; }
        public virtual DbSet<CoinTradeLocation> CoinTradeLocation { get; set; }
        public virtual DbSet<CoinType> CoinType { get; set; }
        public virtual DbSet<ComInfoUpdateRecords> ComInfoUpdateRecords { get; set; }
        public virtual DbSet<FaceInitRecord> FaceInitRecord { get; set; }
        public virtual DbSet<GameCategories> GameCategories { get; set; }
        public virtual DbSet<GameInfoExt> GameInfoExt { get; set; }
        public virtual DbSet<GameInfos> GameInfos { get; set; }
        public virtual DbSet<GameLoginHistory> GameLoginHistory { get; set; }
        public virtual DbSet<GameSuppliers> GameSuppliers { get; set; }
        public virtual DbSet<GemRecords> GemRecords { get; set; }
        public virtual DbSet<GoldFlows> GoldFlows { get; set; }
        public virtual DbSet<LoginHistory> LoginHistory { get; set; }
        public virtual DbSet<Minnings> Minnings { get; set; }
        public virtual DbSet<NoticeInfos> NoticeInfos { get; set; }
        public virtual DbSet<OrderGames> OrderGames { get; set; }
        public virtual DbSet<PhoneAttribution> PhoneAttribution { get; set; }
        public virtual DbSet<Pictures> Pictures { get; set; }
        public virtual DbSet<Relations> Relations { get; set; }
        public virtual DbSet<RelationStats> RelationStats { get; set; }
        public virtual DbSet<SysBanner> SysBanner { get; set; }
        public virtual DbSet<SysClientVersions> SysClientVersions { get; set; }
        public virtual DbSet<SystemActions> SystemActions { get; set; }
        public virtual DbSet<SystemRolePermission> SystemRolePermission { get; set; }
        public virtual DbSet<SystemRoles> SystemRoles { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserAccountEquity> UserAccountEquity { get; set; }
        public virtual DbSet<UserAccountEquityRecord> UserAccountEquityRecord { get; set; }
        public virtual DbSet<UserAccountTicket> UserAccountTicket { get; set; }
        public virtual DbSet<UserAccountTicketRecord> UserAccountTicketRecord { get; set; }
        public virtual DbSet<UserAccountWallet> UserAccountWallet { get; set; }
        public virtual DbSet<UserAccountWalletRecord> UserAccountWalletRecord { get; set; }
        public virtual DbSet<UserBalance> UserBalance { get; set; }
        public virtual DbSet<UserBalanceFlow> UserBalanceFlow { get; set; }
        public virtual DbSet<UserCandyp> UserCandyp { get; set; }
        public virtual DbSet<UserExpand> UserExpand { get; set; }
        public virtual DbSet<UserExt> UserExt { get; set; }
        public virtual DbSet<UserGameBonusDetail> UserGameBonusDetail { get; set; }
        public virtual DbSet<UserLocations> UserLocations { get; set; }
        public virtual DbSet<UserVcodes> UserVcodes { get; set; }
        public virtual DbSet<UserWithdrawHistory> UserWithdrawHistory { get; set; }
        public virtual DbSet<YoyoActivity> YoyoActivity { get; set; }
        public virtual DbSet<YoyoActivityCoupon> YoyoActivityCoupon { get; set; }
        public virtual DbSet<YoyoActivityPrize> YoyoActivityPrize { get; set; }
        public virtual DbSet<YoyoActivityRaffleEcord> YoyoActivityRaffleEcord { get; set; }
        public virtual DbSet<YoyoActivityWinRecord> YoyoActivityWinRecord { get; set; }
        public virtual DbSet<YoyoAdClick> YoyoAdClick { get; set; }
        public virtual DbSet<YoyoAdMaster> YoyoAdMaster { get; set; }
        public virtual DbSet<YoyoBangAppeals> YoyoBangAppeals { get; set; }
        public virtual DbSet<YoyoBangCategory> YoyoBangCategory { get; set; }
        public virtual DbSet<YoyoBangRank> YoyoBangRank { get; set; }
        public virtual DbSet<YoyoBangRecord> YoyoBangRecord { get; set; }
        public virtual DbSet<YoyoBangStep> YoyoBangStep { get; set; }
        public virtual DbSet<YoyoBangTask> YoyoBangTask { get; set; }
        public virtual DbSet<YoyoBoxActivity> YoyoBoxActivity { get; set; }
        public virtual DbSet<YoyoBoxRecord> YoyoBoxRecord { get; set; }
        public virtual DbSet<YoyoBoxWiner> YoyoBoxWiner { get; set; }
        public virtual DbSet<YoyoCashDividend> YoyoCashDividend { get; set; }
        public virtual DbSet<YoyoCityMaster> YoyoCityMaster { get; set; }
        public virtual DbSet<YoyoEverydayDividend> YoyoEverydayDividend { get; set; }
        public virtual DbSet<YoyoLuckydrawDefaultuser> YoyoLuckydrawDefaultuser { get; set; }
        public virtual DbSet<YoyoLuckydrawPrize> YoyoLuckydrawPrize { get; set; }
        public virtual DbSet<YoyoLuckydrawRound> YoyoLuckydrawRound { get; set; }
        public virtual DbSet<YoyoLuckydrawUser> YoyoLuckydrawUser { get; set; }
        public virtual DbSet<YoyoMallOrder> YoyoMallOrder { get; set; }
        public virtual DbSet<YoyoMemberActive> YoyoMemberActive { get; set; }
        public virtual DbSet<YoyoMemberAddress> YoyoMemberAddress { get; set; }
        public virtual DbSet<YoyoMemberDailyTask> YoyoMemberDailyTask { get; set; }
        public virtual DbSet<YoyoMemberDevote> YoyoMemberDevote { get; set; }
        public virtual DbSet<YoyoMemberDuplicate> YoyoMemberDuplicate { get; set; }
        public virtual DbSet<YoyoMemberInviteRanking> YoyoMemberInviteRanking { get; set; }
        public virtual DbSet<YoyoMemberRelation> YoyoMemberRelation { get; set; }
        public virtual DbSet<YoyoMemberStarNow> YoyoMemberStarNow { get; set; }
        public virtual DbSet<YoyoPayRecord> YoyoPayRecord { get; set; }
        public virtual DbSet<YoyoRechargeOrder> YoyoRechargeOrder { get; set; }
        public virtual DbSet<YoyoShandwOrder> YoyoShandwOrder { get; set; }
        public virtual DbSet<YoyoSignRecord> YoyoSignRecord { get; set; }
        public virtual DbSet<YoyoSystemCopywriting> YoyoSystemCopywriting { get; set; }
        public virtual DbSet<YoyoSystemTask> YoyoSystemTask { get; set; }
        public virtual DbSet<YoyoTaskRecord> YoyoTaskRecord { get; set; }

        // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        // {
        //     if (!optionsBuilder.IsConfigured)
        //     {
        //         optionsBuilder.UseMySql(Configuration.GetConnectionString("yoyoServiceConStr"), myop =>
        //         {
        //             myop.ServerVersion(new Version(5, 7, 18), Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MySql)
        //                 .UnicodeCharSet(Pomelo.EntityFrameworkCore.MySql.Infrastructure.CharSet.Utf8mb4);
        //         });
        //     }
        // }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Appeals>(entity =>
            {
                entity.ToTable("appeals");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AppealResult)
                    .HasColumnName("appealResult")
                    .HasColumnType("tinyint(1)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasColumnType("text");

                entity.Property(e => e.OrderId)
                    .HasColumnName("orderId")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.PicUrl)
                    .HasColumnName("picUrl")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasColumnType("int(3)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");
            });

            modelBuilder.Entity<AuthenticationInfos>(entity =>
            {
                entity.ToTable("authentication_infos");

                entity.HasIndex(e => e.CertifyId)
                    .HasName("UK_certifyId")
                    .IsUnique();

                entity.HasIndex(e => e.IdNum)
                    .HasName("UNIQUE_IDNUM")
                    .IsUnique();

                entity.HasIndex(e => e.UserId)
                    .HasName("user_id")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AuthType)
                    .HasColumnName("authType")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.CertifyId)
                    .HasColumnName("certifyId")
                    .HasColumnType("varchar(64)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.FailReason)
                    .HasColumnName("failReason")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.IdNum)
                    .HasColumnName("idNum")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.Pic)
                    .HasColumnName("pic")
                    .HasColumnType("varchar(500)");

                entity.Property(e => e.Pic1)
                    .HasColumnName("pic1")
                    .HasColumnType("varchar(500)");

                entity.Property(e => e.Pic2)
                    .HasColumnName("pic2")
                    .HasColumnType("varchar(500)");

                entity.Property(e => e.TrueName)
                    .HasColumnName("trueName")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<BackstageUser>(entity =>
            {
                entity.ToTable("backstage_user");

                entity.HasIndex(e => e.RoleId)
                    .HasName("rId");

                entity.Property(e => e.Id)
                    .HasColumnType("varchar(36)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.AccountStatus)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.AccountType)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.FullName)
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Gender)
                    .HasColumnType("int(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.IdCard)
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.LastLoginIp)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.LastLoginTime).HasColumnType("datetime");

                entity.Property(e => e.LoginName)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Mobile)
                    .IsRequired()
                    .HasColumnType("varchar(20)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("'123456'");

                entity.Property(e => e.RoleId)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.SourceType)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UpdateTime).HasColumnType("datetime");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.BackstageUser)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("rId");
            });

            modelBuilder.Entity<CityCandyDividend>(entity =>
            {
                entity.ToTable("city_candy_dividend");

                entity.HasIndex(e => new { e.CityNo, e.StartDate })
                    .HasName("UNIQUE_CITY_DATE")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.Amount).HasColumnType("decimal(18,4)");

                entity.Property(e => e.CityNo)
                    .IsRequired()
                    .HasColumnType("varchar(12)");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.DividendType).HasColumnType("int(2)");

                entity.Property(e => e.EndDate).HasColumnType("date");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.Property(e => e.State).HasColumnType("int(11)");

                entity.Property(e => e.UpdateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");
            });

            modelBuilder.Entity<CityCandyDividendRecord>(entity =>
            {
                entity.ToTable("city_candy_dividend_record");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CityNo)
                    .IsRequired()
                    .HasColumnType("varchar(12)");

                entity.Property(e => e.CreateTime).HasColumnType("datetime");

                entity.Property(e => e.Incurred).HasColumnType("decimal(18,6)");

                entity.Property(e => e.ModifyDesc).HasColumnType("varchar(255)");

                entity.Property(e => e.ModifyType).HasColumnType("int(2)");

                entity.Property(e => e.PostChange).HasColumnType("decimal(18,6)");

                entity.Property(e => e.PreChange).HasColumnType("decimal(18,6)");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");
            });

            modelBuilder.Entity<CityCashDividend>(entity =>
            {
                entity.ToTable("city_cash_dividend");

                entity.HasIndex(e => new { e.CityNo, e.StartDate })
                    .HasName("UNIQUE_CITY_DATE")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.Amount).HasColumnType("decimal(18,4)");

                entity.Property(e => e.CityNo)
                    .IsRequired()
                    .HasColumnType("varchar(12)");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.DividendType).HasColumnType("int(2)");

                entity.Property(e => e.EndDate).HasColumnType("date");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.Property(e => e.State).HasColumnType("int(11)");

                entity.Property(e => e.UpdateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");
            });

            modelBuilder.Entity<CityCashDividendRecord>(entity =>
            {
                entity.ToTable("city_cash_dividend_record");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CityNo)
                    .IsRequired()
                    .HasColumnType("varchar(12)");

                entity.Property(e => e.CreateTime).HasColumnType("datetime");

                entity.Property(e => e.Incurred).HasColumnType("decimal(18,6)");

                entity.Property(e => e.ModifyDesc).HasColumnType("varchar(255)");

                entity.Property(e => e.ModifyType).HasColumnType("int(2)");

                entity.Property(e => e.PostChange).HasColumnType("decimal(18,6)");

                entity.Property(e => e.PreChange).HasColumnType("decimal(18,6)");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");
            });

            modelBuilder.Entity<CityEarnings>(entity =>
            {
                entity.HasKey(e => e.CityNo);

                entity.ToTable("city_earnings");

                entity.Property(e => e.CityNo).HasColumnType("varchar(12)");

                entity.Property(e => e.Candy)
                    .HasColumnType("decimal(16,4)")
                    .HasDefaultValueSql("'0.0000'");

                entity.Property(e => e.Cash)
                    .HasColumnType("decimal(16,4)")
                    .HasDefaultValueSql("'0.0000'");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.People).HasColumnType("int(11)");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");
            });

            modelBuilder.Entity<CoinMoveRecord>(entity =>
            {
                entity.ToTable("coin_move_record");

                // entity.HasIndex(e => new { e.UserId, e.RefId, e.CoinType, e.Status })
                //     .HasName("userId")
                //     .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasColumnName("address")
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Amount)
                    .HasColumnName("amount")
                    .HasColumnType("decimal(18,5)")
                    .HasDefaultValueSql("'0.00000'");

                entity.Property(e => e.CoinType)
                    .IsRequired()
                    .HasColumnName("coinType")
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.CreateTime)
                    .HasColumnName("createTime")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.RefId)
                    .IsRequired()
                    .HasColumnName("refId")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasColumnName("userId")
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("''");
            });

            modelBuilder.Entity<CoinTrade>(entity =>
            {
                entity.ToTable("coin_trade");

                entity.HasIndex(e => e.BuyerAlipay)
                    .HasName("FK_buyerAlipay");

                entity.HasIndex(e => e.BuyerUid)
                    .HasName("buyer_uid");

                entity.HasIndex(e => e.DealTime)
                    .HasName("deal_time");

                entity.HasIndex(e => e.PaidEndTime)
                    .HasName("NORMAL_PAIDENDTIME");

                entity.HasIndex(e => e.SellerUid)
                    .HasName("seller_uid");

                entity.HasIndex(e => e.Status)
                    .HasName("status");

                entity.HasIndex(e => e.TrendSide)
                    .HasName("trend_side");

                entity.HasIndex(e => new { e.TrendSide, e.Status })
                    .HasName("NORMAL");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Amount)
                    .HasColumnName("amount")
                    .HasColumnType("decimal(32,16)");

                entity.Property(e => e.AppealTime)
                    .HasColumnName("appealTime")
                    .HasColumnType("timestamp");

                entity.Property(e => e.BuyerAlipay)
                    .HasColumnName("buyerAlipay")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.BuyerBan)
                    .HasColumnName("buyerBan")
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.BuyerUid)
                    .HasColumnName("buyerUid")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CoinType)
                    .IsRequired()
                    .HasColumnName("coinType")
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("'USDT'");

                entity.Property(e => e.Ctime)
                    .HasColumnName("ctime")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.DealEndTime)
                    .HasColumnName("dealEndTime")
                    .HasColumnType("timestamp");

                entity.Property(e => e.DealTime)
                    .HasColumnName("dealTime")
                    .HasColumnType("timestamp");

                entity.Property(e => e.EntryOrderTime)
                    .HasColumnName("entryOrderTime")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Fee)
                    .HasColumnName("fee")
                    .HasColumnType("decimal(18,8)");

                entity.Property(e => e.FinishTime)
                    .HasColumnName("finishTime")
                    .HasColumnType("timestamp");

                entity.Property(e => e.MonthlyTradeCount)
                    .HasColumnName("monthlyTradeCount")
                    .HasColumnType("int(11)");

                entity.Property(e => e.PaidEndTime)
                    .HasColumnName("paidEndTime")
                    .HasColumnType("timestamp");

                entity.Property(e => e.PaidTime)
                    .HasColumnName("paidTime")
                    .HasColumnType("timestamp");

                entity.Property(e => e.PayCoinTime)
                    .HasColumnName("payCoinTime")
                    .HasColumnType("timestamp");

                entity.Property(e => e.PictureUrl)
                    .HasColumnName("pictureUrl")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Price)
                    .HasColumnName("price")
                    .HasColumnType("decimal(18,8)");

                entity.Property(e => e.SellerAlipay)
                    .HasColumnName("sellerAlipay")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.SellerBan)
                    .HasColumnName("sellerBan")
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.SellerUid)
                    .HasColumnName("sellerUid")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasColumnType("int(255)");

                entity.Property(e => e.TotalPrice)
                    .HasColumnName("totalPrice")
                    .HasColumnType("decimal(18,8)");

                entity.Property(e => e.TradeNumber)
                    .HasColumnName("tradeNumber")
                    .HasColumnType("varchar(36)");

                entity.Property(e => e.TrendSide)
                    .HasColumnName("trendSide")
                    .HasColumnType("varchar(10)");

                entity.Property(e => e.Utime)
                    .HasColumnName("utime")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'")
                    .ValueGeneratedOnAddOrUpdate();
            });

            modelBuilder.Entity<CoinTradeExt>(entity =>
            {
                entity.ToTable("coin_trade_ext");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Ctime)
                    .HasColumnName("ctime")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.SysMaxPrice)
                    .HasColumnName("sysMaxPrice")
                    .HasDefaultValueSql("'6'");

                entity.Property(e => e.SysMinPrice)
                    .HasColumnName("sysMinPrice")
                    .HasDefaultValueSql("'0.6'");

                entity.Property(e => e.TodayAmount)
                    .HasColumnName("todayAmount")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.TodayAvgPrice)
                    .HasColumnName("todayAvgPrice")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.TodayMaxPrice)
                    .HasColumnName("todayMaxPrice")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.TodayTradeAmount)
                    .HasColumnName("todayTradeAmount")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("'USDT'");

                entity.Property(e => e.UpRate)
                    .HasColumnName("upRate")
                    .HasDefaultValueSql("'0.01'");
            });

            modelBuilder.Entity<CoinTradeLocation>(entity =>
            {
                entity.HasKey(e => e.TradeId);

                entity.ToTable("coin_trade_location");

                entity.HasIndex(e => e.BuyerAreaCode)
                    .HasName("FK_Buyer_AreaCode");

                entity.HasIndex(e => e.BuyerCityCode)
                    .HasName("FK_Buyer_CityCode");

                entity.HasIndex(e => e.SellAreaCode)
                    .HasName("FK_Sell_AreaCode");

                entity.HasIndex(e => e.SellCityCode)
                    .HasName("FK_Sell_CityCode");

                entity.Property(e => e.TradeId).HasColumnType("varchar(36)");

                entity.Property(e => e.BuyerArea)
                    .IsRequired()
                    .HasColumnName("Buyer_Area")
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.BuyerAreaCode)
                    .IsRequired()
                    .HasColumnName("Buyer_AreaCode")
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.BuyerCity)
                    .IsRequired()
                    .HasColumnName("Buyer_City")
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.BuyerCityCode)
                    .IsRequired()
                    .HasColumnName("Buyer_CityCode")
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.BuyerLocationX)
                    .HasColumnName("Buyer_Location_X")
                    .HasColumnType("decimal(18,6)")
                    .HasDefaultValueSql("'0.000000'");

                entity.Property(e => e.BuyerLocationY)
                    .HasColumnName("Buyer_Location_Y")
                    .HasColumnType("decimal(18,6)")
                    .HasDefaultValueSql("'0.000000'");

                entity.Property(e => e.BuyerProvince)
                    .IsRequired()
                    .HasColumnName("Buyer_Province")
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.SellArea)
                    .IsRequired()
                    .HasColumnName("Sell_Area")
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.SellAreaCode)
                    .IsRequired()
                    .HasColumnName("Sell_AreaCode")
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.SellCity)
                    .IsRequired()
                    .HasColumnName("Sell_City")
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.SellCityCode)
                    .IsRequired()
                    .HasColumnName("Sell_CityCode")
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.SellLocationX)
                    .HasColumnName("Sell_Location_X")
                    .HasColumnType("decimal(18,6)")
                    .HasDefaultValueSql("'0.000000'");

                entity.Property(e => e.SellLocationY)
                    .HasColumnName("Sell_Location_Y")
                    .HasColumnType("decimal(18,6)")
                    .HasDefaultValueSql("'0.000000'");

                entity.Property(e => e.SellProvince)
                    .IsRequired()
                    .HasColumnName("Sell_Province")
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("''");
            });

            modelBuilder.Entity<CoinType>(entity =>
            {
                entity.ToTable("coin_type");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Count24)
                    .HasColumnName("count24")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.CountTotal)
                    .HasColumnName("countTotal")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'1000000'");

                entity.Property(e => e.CreateTime)
                    .HasColumnName("createTime")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Fee)
                    .HasColumnName("fee")
                    .HasColumnType("decimal(18,5)")
                    .HasDefaultValueSql("'0.00000'");

                entity.Property(e => e.LastPrice)
                    .HasColumnName("lastPrice")
                    .HasColumnType("decimal(18,5)")
                    .HasDefaultValueSql("'0.00000'");

                entity.Property(e => e.MinCanMove)
                    .HasColumnName("minCanMove")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'10'");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("'USDT'");

                entity.Property(e => e.NowPrice)
                    .HasColumnName("nowPrice")
                    .HasColumnType("decimal(18,5)")
                    .HasDefaultValueSql("'0.00000'");

                entity.Property(e => e.Remark)
                    .IsRequired()
                    .HasColumnName("remark")
                    .HasColumnType("text");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasColumnType("int(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasColumnType("int(2)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UpDown)
                    .HasColumnName("upDown")
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValueSql("'0.00'");

                entity.Property(e => e.UpdateTime)
                    .HasColumnName("updateTime")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");
            });

            modelBuilder.Entity<ComInfoUpdateRecords>(entity =>
            {
                entity.ToTable("com_info_update_records");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.NewValue)
                    .HasColumnName("newValue")
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.OldValue)
                    .HasColumnName("oldValue")
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Udate)
                    .HasColumnName("udate")
                    .HasColumnType("date");

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("int(11)");
            });

            modelBuilder.Entity<FaceInitRecord>(entity =>
            {
                entity.ToTable("face_init_record");

                entity.HasIndex(e => new { e.CertifyId, e.IdcardNum })
                    .HasName("UNIQUE_CID")
                    .IsUnique();

                entity.HasIndex(e => new { e.IdcardNum, e.TrueName })
                    .HasName("NORMAL_IDCARD");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.Alipay).HasColumnType("varchar(128)");

                entity.Property(e => e.CertifyId).HasColumnType("varchar(64)");

                entity.Property(e => e.CertifyUrl).HasColumnType("varchar(1024)");

                entity.Property(e => e.CreateTime).HasColumnType("datetime");

                entity.Property(e => e.IdcardNum)
                    .HasColumnName("IDCardNum")
                    .HasColumnType("varchar(64)");

                entity.Property(e => e.IsUsed)
                    .HasColumnType("int(2)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.TrueName).HasColumnType("varchar(32)");
            });

            modelBuilder.Entity<GameCategories>(entity =>
            {
                entity.ToTable("game_categories");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");
            });

            modelBuilder.Entity<GameInfoExt>(entity =>
            {
                entity.ToTable("game_info_ext");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AppKey)
                    .HasColumnName("appKey")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.AppSecret)
                    .HasColumnName("appSecret")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.CallbackUrl)
                    .HasColumnName("callbackUrl")
                    .HasColumnType("varchar(155)");

                entity.Property(e => e.CpUserId)
                    .HasColumnName("cpUserId")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.CreateTime)
                    .HasColumnName("createTime")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.GameId)
                    .HasColumnName("gameId")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.GamePlatform)
                    .HasColumnName("gamePlatform")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.IpWhiteList)
                    .HasColumnName("ipWhiteList")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.IsOpen)
                    .HasColumnName("isOpen")
                    .HasColumnType("int(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UpdateTime)
                    .HasColumnName("updateTime")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");
            });

            modelBuilder.Entity<GameInfos>(entity =>
            {
                entity.ToTable("game_infos");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.CompanyShareRatio)
                    .HasColumnName("companyShareRatio")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasColumnType("text");

                entity.Property(e => e.Discount)
                    .HasColumnName("discount")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.GH5url)
                    .HasColumnName("gH5Url")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.GPinyin)
                    .HasColumnName("gPinyin")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.GPlatform)
                    .HasColumnName("gPlatform")
                    .HasColumnType("int(11)");

                entity.Property(e => e.GSize)
                    .HasColumnName("gSize")
                    .HasColumnType("decimal(10,0)");

                entity.Property(e => e.GSort)
                    .HasColumnName("gSort")
                    .HasColumnType("decimal(10,0)");

                entity.Property(e => e.GTitle)
                    .HasColumnName("gTitle")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.GType)
                    .HasColumnName("gType")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.GVersion)
                    .HasColumnName("gVersion")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.GameCategoryId)
                    .HasColumnName("gameCategoryId")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.GameSupplierId)
                    .HasColumnName("gameSupplierId")
                    .HasColumnType("int(11)");

                entity.Property(e => e.GtProportionl)
                    .HasColumnName("gtProportionl")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.GtVip)
                    .HasColumnName("gtVIP")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.IsFirstPublish)
                    .HasColumnName("isFirstPublish")
                    .HasColumnType("tinyint(1)");

                entity.Property(e => e.IsShow)
                    .HasColumnName("isShow")
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.SdwId)
                    .HasColumnName("sdwId")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Synopsis)
                    .HasColumnName("synopsis")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.UseGem)
                    .HasColumnName("useGem")
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UseGemRate)
                    .HasColumnName("useGemRate")
                    .HasDefaultValueSql("'0'");
            });

            modelBuilder.Entity<GameLoginHistory>(entity =>
            {
                entity.ToTable("game_login_history");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.GameAppid)
                    .HasColumnName("gameAppid")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.RoleId)
                    .HasColumnName("roleId")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.RoleLevel)
                    .HasColumnName("roleLevel")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.RoleName)
                    .HasColumnName("roleName")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.ServerId)
                    .HasColumnName("serverId")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.ServerName)
                    .HasColumnName("serverName")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.Source)
                    .HasColumnName("source")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");
            });

            modelBuilder.Entity<GameSuppliers>(entity =>
            {
                entity.ToTable("game_suppliers");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.IsEnable)
                    .HasColumnName("isEnable")
                    .HasColumnType("tinyint(1)");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");
            });

            modelBuilder.Entity<GemRecords>(entity =>
            {
                entity.ToTable("gem_records");

                entity.HasIndex(e => e.CreatedAt)
                    .HasName("time");

                entity.HasIndex(e => e.GemSource)
                    .HasName("gemSource");

                entity.HasIndex(e => new { e.UserId, e.GemSource })
                    .HasName("user_id");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasColumnType("varchar(200)");

                entity.Property(e => e.GemMinningAt)
                    .HasColumnName("gemMinningAt")
                    .HasColumnType("datetime");

                entity.Property(e => e.GemSource)
                    .HasColumnName("gemSource")
                    .HasColumnType("int(3)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Num)
                    .HasColumnName("num")
                    .HasColumnType("decimal(18,8)")
                    .HasDefaultValueSql("'0.00000000'");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<GoldFlows>(entity =>
            {
                entity.ToTable("gold_flows");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Discribe)
                    .HasColumnName("discribe")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.IsRead)
                    .HasColumnName("isRead")
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Num)
                    .HasColumnName("num")
                    .HasColumnType("varchar(30)");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<LoginHistory>(entity =>
            {
                entity.ToTable("login_history");

                entity.HasIndex(e => e.Mobile)
                    .HasName("FK_mobile");

                entity.HasIndex(e => new { e.UniqueId, e.Mobile })
                    .HasName("NORMAL_UNIQUEID_MOBILE");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AppVersion)
                    .HasColumnName("appVersion")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.Ctime)
                    .HasColumnName("ctime")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.DeviceName)
                    .HasColumnName("deviceName")
                    .HasColumnType("varchar(200)");

                entity.Property(e => e.Mobile)
                    .HasColumnName("mobile")
                    .HasColumnType("varchar(11)");

                entity.Property(e => e.SystemName)
                    .HasColumnName("systemName")
                    .HasColumnType("varchar(200)");

                entity.Property(e => e.SystemVersion)
                    .HasColumnName("systemVersion")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.UnLockCount)
                    .HasColumnName("unLockCount")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UniqueId)
                    .HasColumnName("uniqueId")
                    .HasColumnType("varchar(200)");

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Utime)
                    .HasColumnName("utime")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");
            });

            modelBuilder.Entity<Minnings>(entity =>
            {
                entity.ToTable("minnings");

                entity.HasIndex(e => e.BeginTime)
                    .HasName("begin_time");

                entity.HasIndex(e => e.EndTime)
                    .HasName("end_time");

                entity.HasIndex(e => e.Source)
                    .HasName("source");

                entity.HasIndex(e => e.UserId)
                    .HasName("user_id");

                entity.HasIndex(e => new { e.UserId, e.Status })
                    .HasName("NORMAL_USERID_STATUS");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.BeginTime)
                    .HasColumnName("beginTime")
                    .HasColumnType("datetime");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.EndTime)
                    .HasColumnName("endTime")
                    .HasColumnType("datetime");

                entity.Property(e => e.MinningId)
                    .HasColumnName("minningId")
                    .HasColumnType("int(11)");

                entity.Property(e => e.MinningStatus)
                    .HasColumnName("minningStatus")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Source)
                    .HasColumnName("source")
                    .HasColumnType("int(3)");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.WorkingTime)
                    .HasColumnName("workingTime")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<NoticeInfos>(entity =>
            {
                entity.ToTable("notice_infos");

                entity.HasIndex(e => e.Type)
                    .HasName("FK_type");

                entity.HasIndex(e => e.UserId)
                    .HasName("FK_userId");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.CeratedAt)
                    .HasColumnName("ceratedAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Content)
                    .HasColumnName("content")
                    .HasColumnType("text");

                entity.Property(e => e.IsRead)
                    .HasColumnName("isRead")
                    .HasColumnType("int(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.RefId)
                    .HasColumnName("refId")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.Title)
                    .HasColumnName("title")
                    .HasColumnType("varchar(30)");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<OrderGames>(entity =>
            {
                entity.ToTable("order_games");

                entity.HasIndex(e => e.CreatedAt)
                    .HasName("FK_createdAt");

                entity.HasIndex(e => e.OrderId)
                    .HasName("UNIQUE_ORDERID")
                    .IsUnique();

                entity.HasIndex(e => e.UpdatedAt)
                    .HasName("FK_updatedAt");

                entity.HasIndex(e => new { e.OrderId, e.UserId })
                    .HasName("FK_orderId_userId");

                entity.HasIndex(e => new { e.GameAppid, e.UserId, e.Status })
                    .HasName("FK_gameAppid_userId_status");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Candy)
                    .HasColumnName("candy")
                    .HasColumnType("decimal(18,8)");

                entity.Property(e => e.CandyAmount)
                    .HasColumnName("candyAmount")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("datetime");

                entity.Property(e => e.GameAppid)
                    .HasColumnName("gameAppid")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.OrderAmount)
                    .HasColumnName("orderAmount")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.OrderId)
                    .HasColumnName("orderId")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.RealAmount)
                    .HasColumnName("realAmount")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasColumnType("int(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("datetime");

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Uuid)
                    .HasColumnName("uuid")
                    .HasColumnType("varchar(255)");
            });

            modelBuilder.Entity<PhoneAttribution>(entity =>
            {
                entity.ToTable("phone_attribution");

                entity.HasIndex(e => e.Phone)
                    .HasName("UNIQUE_PHONE")
                    .IsUnique();

                entity.HasIndex(e => e.Prefix)
                    .HasName("NORMAL_PREFIX");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.AreaCode)
                    .IsRequired()
                    .HasColumnType("varchar(8)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.ChinaCode)
                    .IsRequired()
                    .HasColumnType("varchar(8)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.City)
                    .IsRequired()
                    .HasColumnType("varchar(128)");

                entity.Property(e => e.Isp)
                    .IsRequired()
                    .HasColumnName("ISP")
                    .HasColumnType("varchar(16)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Phone)
                    .IsRequired()
                    .HasColumnType("varchar(8)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Postcode)
                    .IsRequired()
                    .HasColumnType("varchar(8)");

                entity.Property(e => e.Prefix)
                    .IsRequired()
                    .HasColumnType("varchar(8)");

                entity.Property(e => e.Province)
                    .IsRequired()
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("''");
            });

            modelBuilder.Entity<Pictures>(entity =>
            {
                entity.ToTable("pictures");

                entity.HasIndex(e => new { e.ImageableType, e.ImageableId })
                    .HasName("index_pictures_on_imageable_type_and_imageable_id");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("datetime");

                entity.Property(e => e.ImageableId)
                    .HasColumnName("imageableId")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ImageableType)
                    .HasColumnName("imageableType")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Size)
                    .HasColumnName("size")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("datetime");

                entity.Property(e => e.Url)
                    .HasColumnName("url")
                    .HasColumnType("varchar(255)");
            });

            modelBuilder.Entity<Relations>(entity =>
            {
                entity.ToTable("relations");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.InviterMobile)
                    .HasColumnName("inviterMobile")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.Mobile)
                    .HasColumnName("mobile")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");
            });

            modelBuilder.Entity<RelationStats>(entity =>
            {
                entity.ToTable("relation_stats");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.F10Id)
                    .HasColumnName("f10Id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.F1Id)
                    .HasColumnName("f1Id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.F2Id)
                    .HasColumnName("f2Id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.F3Id)
                    .HasColumnName("f3Id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.F4Id)
                    .HasColumnName("f4Id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.F5Id)
                    .HasColumnName("f5Id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.F6Id)
                    .HasColumnName("f6Id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.F7Id)
                    .HasColumnName("f7Id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.F8Id)
                    .HasColumnName("f8Id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.F9Id)
                    .HasColumnName("f9Id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("int(11)");
            });

            modelBuilder.Entity<SysBanner>(entity =>
            {
                entity.ToTable("sys_banner");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CityCode)
                    .HasColumnName("cityCode")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.ImageUrl)
                    .HasColumnName("imageUrl")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Params)
                    .HasColumnName("params")
                    .HasColumnType("text");

                entity.Property(e => e.Queue)
                    .HasColumnName("queue")
                    .HasColumnType("tinyint(3)");

                entity.Property(e => e.Source)
                    .HasColumnName("source")
                    .HasColumnType("tinyint(3)");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasColumnType("tinyint(3)")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.Title)
                    .HasColumnName("title")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasColumnType("tinyint(3)");
            });

            modelBuilder.Entity<SysClientVersions>(entity =>
            {
                entity.ToTable("sys_client_versions");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CurrentVersion)
                    .HasColumnName("currentVersion")
                    .HasColumnType("varchar(10)");

                entity.Property(e => e.DeviceSystem)
                    .HasColumnName("deviceSystem")
                    .HasColumnType("varchar(10)");

                entity.Property(e => e.DownloadUrl)
                    .HasColumnName("downloadUrl")
                    .HasColumnType("text");

                entity.Property(e => e.IsHotReload)
                    .HasColumnName("isHotReload")
                    .HasColumnType("tinyint(1)");

                entity.Property(e => e.IsNecessary)
                    .HasColumnName("isNecessary")
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.IsSilent)
                    .HasColumnName("isSilent")
                    .HasColumnType("tinyint(1)");

                entity.Property(e => e.Production)
                    .HasColumnName("production")
                    .HasColumnType("tinyint(1)");

                entity.Property(e => e.UpdateContent)
                    .HasColumnName("updateContent")
                    .HasColumnType("text");
            });

            modelBuilder.Entity<SystemActions>(entity =>
            {
                entity.HasKey(e => e.ActionId);

                entity.ToTable("system_actions");

                entity.Property(e => e.ActionId)
                    .HasColumnType("varchar(36)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.ActionDescription)
                    .IsRequired()
                    .HasColumnType("varchar(100)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.ActionName)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Icon).HasColumnType("varchar(100)");

                entity.Property(e => e.Orders).HasColumnType("int(11)");

                entity.Property(e => e.ParentAction).HasColumnType("varchar(255)");

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasDefaultValueSql("''");
            });

            modelBuilder.Entity<SystemRolePermission>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.ActionId });

                entity.ToTable("system_role_permission");

                entity.Property(e => e.RoleId)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.ActionId).HasColumnType("varchar(36)");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");
            });

            modelBuilder.Entity<SystemRoles>(entity =>
            {
                entity.ToTable("system_roles");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("''");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");

                entity.HasIndex(e => e.AuditState)
                    .HasName("audit_state");

                entity.HasIndex(e => e.Ctime)
                    .HasName("ctime");

                entity.HasIndex(e => e.Id)
                    .HasName("id_UNIQUE")
                    .IsUnique();

                entity.HasIndex(e => e.InviterMobile)
                    .HasName("FK_inviter_mobile");

                entity.HasIndex(e => e.Mobile)
                    .HasName("UK_mobile")
                    .IsUnique();

                entity.HasIndex(e => e.Rcode)
                    .HasName("rcode_2");

                entity.HasIndex(e => new { e.Alipay, e.AuditState })
                    .HasName("NORMAL_ALIPAY_AUDITSTATE");

                entity.HasIndex(e => new { e.InviterMobile, e.Mobile, e.Status })
                    .HasName("inviter_mobile");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Alipay)
                    .HasColumnName("alipay")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.AlipayUid)
                    .HasColumnName("alipayUid")
                    .HasColumnType("varchar(32)");

                entity.Property(e => e.AuditState)
                    .HasColumnName("auditState")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AvatarUrl)
                    .IsRequired()
                    .HasColumnName("avatarUrl")
                    .HasColumnType("varchar(225)")
                    .HasDefaultValueSql("'lfex/images/avatar/default/1.png'");

                entity.Property(e => e.CCount)
                    .HasColumnName("cCount")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.CandyNum)
                    .HasColumnName("candyNum")
                    .HasColumnType("decimal(18,8)")
                    .HasDefaultValueSql("'0.00000000'");

                entity.Property(e => e.CandyP)
                    .HasColumnName("candyP")
                    .HasColumnType("decimal(10,2)")
                    .HasDefaultValueSql("'0.00'");

                entity.Property(e => e.CnadyDoAt)
                    .HasColumnName("cnadyDoAt")
                    .HasColumnType("datetime");

                entity.Property(e => e.ContryCode)
                    .HasColumnName("contryCode")
                    .HasColumnType("varchar(8)");

                entity.Property(e => e.Ctime)
                    .HasColumnName("ctime")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.FreezeCandyNum)
                    .HasColumnName("freezeCandyNum")
                    .HasColumnType("decimal(18,8)")
                    .HasDefaultValueSql("'0.00000000'");

                entity.Property(e => e.Golds)
                    .HasColumnName("golds")
                    .HasColumnType("decimal(18,8)")
                    .HasDefaultValueSql("'0.00000000'");

                entity.Property(e => e.InviterMobile)
                    .HasColumnName("inviterMobile")
                    .HasColumnType("varchar(11)");

                entity.Property(e => e.Level)
                    .IsRequired()
                    .HasColumnName("level")
                    .HasColumnType("varchar(20)")
                    .HasDefaultValueSql("'lv0'");

                entity.Property(e => e.Mobile)
                    .HasColumnName("mobile")
                    .HasColumnType("varchar(11)");

                entity.Property(e => e.MonthlyTradeCount)
                    .HasColumnName("monthlyTradeCount")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.Password)
                    .HasColumnName("password")
                    .HasColumnType("varchar(80)");

                entity.Property(e => e.PasswordSalt)
                    .HasColumnName("passwordSalt")
                    .HasColumnType("varchar(30)");

                entity.Property(e => e.Rcode)
                    .HasColumnName("rcode")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.TodayAvaiableGolds)
                    .HasColumnName("todayAvaiableGolds")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'5'");

                entity.Property(e => e.TradePwd)
                    .HasColumnName("tradePwd")
                    .HasColumnType("varchar(80)");

                entity.Property(e => e.Utime)
                    .HasColumnName("utime")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Uuid)
                    .HasColumnName("uuid")
                    .HasColumnType("varchar(100)");
            });

            modelBuilder.Entity<UserAccountEquity>(entity =>
            {
                entity.HasKey(e => e.AccountId);

                entity.ToTable("user_account_equity");

                entity.HasIndex(e => e.UserId)
                    .HasName("FK_UserId")
                    .IsUnique();

                entity.Property(e => e.AccountId).HasColumnType("bigint(20)");

                entity.Property(e => e.Balance).HasColumnType("decimal(18,5)");

                entity.Property(e => e.Expenses).HasColumnType("decimal(18,5)");

                entity.Property(e => e.Frozen).HasColumnType("decimal(18,5)");

                entity.Property(e => e.ModifyTime).HasColumnType("datetime");

                entity.Property(e => e.Revenue).HasColumnType("decimal(18,5)");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<UserAccountEquityRecord>(entity =>
            {
                entity.HasKey(e => e.RecordId);

                entity.ToTable("user_account_equity_record");

                entity.HasIndex(e => e.AccountId)
                    .HasName("FK_AccountId");

                entity.HasIndex(e => e.ModifyTime)
                    .HasName("FK_ModifyTime");

                entity.HasIndex(e => e.ModifyType)
                    .HasName("FK_ModifyType");

                entity.HasIndex(e => new { e.AccountId, e.ModifyType })
                    .HasName("NORMAL_AID_MT");

                entity.HasIndex(e => new { e.ModifyType, e.AccountId })
                    .HasName("NORMAL_MT_AID");

                entity.Property(e => e.RecordId).HasColumnType("bigint(20)");

                entity.Property(e => e.AccountId).HasColumnType("bigint(20)");

                entity.Property(e => e.Incurred).HasColumnType("decimal(12,5)");

                entity.Property(e => e.ModifyDesc)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.ModifyTime).HasColumnType("datetime");

                entity.Property(e => e.ModifyType).HasColumnType("int(11)");

                entity.Property(e => e.PostChange).HasColumnType("decimal(12,5)");

                entity.Property(e => e.PreChange).HasColumnType("decimal(12,5)");
            });

            modelBuilder.Entity<UserAccountTicket>(entity =>
            {
                entity.HasKey(e => e.AccountId);

                entity.ToTable("user_account_ticket");

                entity.HasIndex(e => e.UserId)
                    .HasName("FK_UserId")
                    .IsUnique();

                entity.Property(e => e.AccountId).HasColumnType("bigint(20)");

                entity.Property(e => e.Balance).HasColumnType("decimal(18,5)");

                entity.Property(e => e.Expenses).HasColumnType("decimal(18,5)");

                entity.Property(e => e.Frozen).HasColumnType("decimal(18,5)");

                entity.Property(e => e.ModifyTime).HasColumnType("datetime");

                entity.Property(e => e.Revenue).HasColumnType("decimal(18,5)");

                entity.Property(e => e.State)
                    .HasColumnType("int(2)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<UserAccountTicketRecord>(entity =>
            {
                entity.HasKey(e => e.RecordId);

                entity.ToTable("user_account_ticket_record");

                entity.HasIndex(e => e.AccountId)
                    .HasName("FK_AccountId");

                entity.HasIndex(e => e.ModifyTime)
                    .HasName("FK_ModifyTime");

                entity.HasIndex(e => e.ModifyType)
                    .HasName("FK_ModifyType");

                entity.HasIndex(e => new { e.AccountId, e.ModifyType })
                    .HasName("NORMAL_AID_MT");

                entity.HasIndex(e => new { e.ModifyType, e.AccountId })
                    .HasName("NORMAL_MT_AID");

                entity.Property(e => e.RecordId).HasColumnType("bigint(20)");

                entity.Property(e => e.AccountId).HasColumnType("bigint(20)");

                entity.Property(e => e.Incurred).HasColumnType("decimal(12,5)");

                entity.Property(e => e.ModifyDesc)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.ModifyTime).HasColumnType("datetime");

                entity.Property(e => e.ModifyType).HasColumnType("int(11)");

                entity.Property(e => e.PostChange).HasColumnType("decimal(12,5)");

                entity.Property(e => e.PreChange).HasColumnType("decimal(12,5)");
            });

            modelBuilder.Entity<UserAccountWallet>(entity =>
            {
                entity.HasKey(e => e.AccountId);

                entity.ToTable("user_account_wallet");

                entity.HasIndex(e => e.UserId)
                    .HasName("FK_UserId");

                entity.HasIndex(e => new { e.CoinType, e.UserId })
                    .HasName("T_U_KEY")
                    .IsUnique();

                entity.Property(e => e.AccountId).HasColumnType("bigint(20)");

                entity.Property(e => e.Balance).HasColumnType("decimal(18,5)");

                entity.Property(e => e.CoinType)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("'USDT'");

                entity.Property(e => e.Expenses).HasColumnType("decimal(18,5)");

                entity.Property(e => e.Frozen).HasColumnType("decimal(18,5)");

                entity.Property(e => e.ModifyTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Revenue).HasColumnType("decimal(18,5)");

                entity.Property(e => e.Type)
                    .HasColumnType("int(3)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<UserAccountWalletRecord>(entity =>
            {
                entity.HasKey(e => e.RecordId);

                entity.ToTable("user_account_wallet_record");

                entity.HasIndex(e => e.AccountId)
                    .HasName("FK_AccountId");

                entity.HasIndex(e => e.ModifyTime)
                    .HasName("FK_ModifyTime");

                entity.Property(e => e.RecordId).HasColumnType("bigint(20)");

                entity.Property(e => e.AccountId).HasColumnType("bigint(20)");

                entity.Property(e => e.Incurred).HasColumnType("decimal(18,5)");

                entity.Property(e => e.ModifyDesc)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.ModifyTime).HasColumnType("datetime");


                entity.Property(e => e.PostChange).HasColumnType("decimal(18,5)");

                entity.Property(e => e.PreChange).HasColumnType("decimal(18,5)");
            });

            modelBuilder.Entity<UserBalance>(entity =>
            {
                entity.ToTable("user_balance");

                entity.HasIndex(e => e.BalanceNormal)
                    .HasName("index_user_balances_on_receiver_id");

                entity.HasIndex(e => e.UserId)
                    .HasName("index_user_balances_on_user_id_unique")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.BalanceLock)
                    .HasColumnName("balanceLock")
                    .HasColumnType("decimal(10,4)")
                    .HasDefaultValueSql("'0.0000'");

                entity.Property(e => e.BalanceNormal)
                    .HasColumnName("balanceNormal")
                    .HasColumnType("decimal(10,4)")
                    .HasDefaultValueSql("'0.0000'");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");
            });

            modelBuilder.Entity<UserBalanceFlow>(entity =>
            {
                entity.ToTable("user_balance_flow");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AmountChange)
                    .HasColumnName("amountChange")
                    .HasColumnType("decimal(10,4)")
                    .HasDefaultValueSql("'0.0000'");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.RefId)
                    .HasColumnName("refId")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Tag)
                    .HasColumnName("tag")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");
            });

            modelBuilder.Entity<UserCandyp>(entity =>
            {
                entity.ToTable("user_candyp");

                entity.HasIndex(e => new { e.UserId, e.Source })
                    .HasName("FK_userId_source");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CandyP)
                    .HasColumnName("candyP")
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValueSql("'0.00'");

                entity.Property(e => e.Content)
                    .HasColumnName("content")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("datetime");

                entity.Property(e => e.Source)
                    .HasColumnName("source")
                    .HasColumnType("smallint(6)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("datetime");

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("int(11)");
            });

            modelBuilder.Entity<UserExpand>(entity =>
            {
                entity.ToTable("user_expand");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CreateTime).HasColumnType("datetime");

                entity.Property(e => e.Mobile)
                    .IsRequired()
                    .HasColumnType("varchar(32)");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");

                entity.Property(e => e.Wechat)
                    .IsRequired()
                    .HasColumnType("varchar(32)");
            });

            modelBuilder.Entity<UserExt>(entity =>
            {
                entity.ToTable("user_ext");

                entity.HasIndex(e => e.UserId)
                    .HasName("user_unique")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AuthCount)
                    .HasColumnName("authCount")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.BigCandyH)
                    .HasColumnName("bigCandyH")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.CreateTime)
                    .HasColumnName("createTime")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.LittleCandyH)
                    .HasColumnName("littleCandyH")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.TeamCandyH)
                    .HasColumnName("teamCandyH")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.TeamCount)
                    .HasColumnName("teamCount")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.TeamStart)
                    .HasColumnName("teamStart")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UpdateTime)
                    .HasColumnName("updateTime")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("bigint(20)")
                    .HasDefaultValueSql("'0'");
            });

            modelBuilder.Entity<UserGameBonusDetail>(entity =>
            {
                entity.ToTable("user_game_bonus_detail");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.BonusAmount)
                    .HasColumnName("bonusAmount")
                    .HasColumnType("decimal(18,4)")
                    .HasDefaultValueSql("'0.0000'");

                entity.Property(e => e.Content)
                    .HasColumnName("content")
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.OrderId)
                    .HasColumnName("orderId")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");
            });

            modelBuilder.Entity<UserLocations>(entity =>
            {
                entity.ToTable("user_locations");

                entity.HasIndex(e => e.CityCode)
                    .HasName("FK_cityCode");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Area)
                    .HasColumnName("area")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.AreaCode)
                    .HasColumnName("areaCode")
                    .HasColumnType("varchar(11)");

                entity.Property(e => e.City)
                    .HasColumnName("city")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.CityCode)
                    .HasColumnName("cityCode")
                    .HasColumnType("varchar(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Latitude)
                    .HasColumnName("latitude")
                    .HasColumnType("decimal(10,6)")
                    .HasDefaultValueSql("'0.000000'");

                entity.Property(e => e.Longitude)
                    .HasColumnName("longitude")
                    .HasColumnType("decimal(10,6)")
                    .HasDefaultValueSql("'0.000000'");

                entity.Property(e => e.Province)
                    .HasColumnName("province")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.ProvinceCode)
                    .HasColumnName("provinceCode")
                    .HasColumnType("varchar(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<UserVcodes>(entity =>
            {
                entity.ToTable("user_vcodes");

                entity.HasIndex(e => e.Mobile)
                    .HasName("NORMAL_MOBILE");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("datetime");

                entity.Property(e => e.Mobile)
                    .HasColumnName("mobile")
                    .HasColumnType("varchar(30)");

                entity.Property(e => e.MsgId)
                    .HasColumnName("msgId")
                    .HasColumnType("varchar(64)");
            });

            modelBuilder.Entity<UserWithdrawHistory>(entity =>
            {
                entity.ToTable("user_withdraw_history");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Amount)
                    .HasColumnName("amount")
                    .HasColumnType("decimal(10,4)")
                    .HasDefaultValueSql("'0.0000'");

                entity.Property(e => e.Content)
                    .HasColumnName("content")
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.FailReason)
                    .HasColumnName("failReason")
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.OrderCode)
                    .HasColumnName("orderCode")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.WithdrawTo)
                    .HasColumnName("withdrawTo")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.WithdrawType)
                    .HasColumnName("withdrawType")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");
            });

            modelBuilder.Entity<YoyoActivity>(entity =>
            {
                entity.ToTable("yoyo_activity");

                entity.HasIndex(e => e.State)
                    .HasName("NORMAL_STATE");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.ActivityLimit)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.DailyLimit)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.EndLotteryTime).HasColumnType("time");

                entity.Property(e => e.EndTime).HasColumnType("datetime");

                entity.Property(e => e.Figure)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Remark)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.StartLotteryTime).HasColumnType("time");

                entity.Property(e => e.StartTime).HasColumnType("datetime");

                entity.Property(e => e.State)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.UseCandy)
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValueSql("'0.00'");

                entity.Property(e => e.UsePeel)
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValueSql("'0.00'");
            });

            modelBuilder.Entity<YoyoActivityCoupon>(entity =>
            {
                entity.ToTable("yoyo_activity_coupon");

                entity.HasIndex(e => e.UserId)
                    .HasName("UserId");

                entity.HasIndex(e => e.WinId)
                    .HasName("WinId");

                entity.HasIndex(e => new { e.UserId, e.CouponType })
                    .HasName("UserId_Type");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CouponType).HasColumnType("int(11)");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.EffectiveTime).HasColumnType("datetime");

                entity.Property(e => e.ExpireTime).HasColumnType("datetime");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");

                entity.Property(e => e.State).HasColumnType("int(2)");

                entity.Property(e => e.UseTime).HasColumnType("datetime");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");

                entity.Property(e => e.WinId).HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<YoyoActivityPrize>(entity =>
            {
                entity.ToTable("yoyo_activity_prize");

                entity.HasIndex(e => e.PrizeType)
                    .HasName("NORMAL_PRIZETYPE");

                entity.HasIndex(e => new { e.ActivityId, e.PrizeType })
                    .HasName("NORMAL_ACTIVITYID");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.ActivityId).HasColumnType("bigint(20)");

                entity.Property(e => e.AutoDeal)
                    .HasColumnType("int(2)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Bonus)
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValueSql("'0.00'");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.DailyWins)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Figure)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.PrizeDesc)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.PrizeTitle)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.PrizeType)
                    .HasColumnType("int(2)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Quantity)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Remark)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.State)
                    .HasColumnType("int(2)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.WinRatio)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");
            });

            modelBuilder.Entity<YoyoActivityRaffleEcord>(entity =>
            {
                entity.ToTable("yoyo_activity_raffle_ecord");

                entity.HasIndex(e => e.ActivityId)
                    .HasName("NORMAL_ACTIVITYID");

                entity.HasIndex(e => e.PrizeId)
                    .HasName("NORMAL_PRIZEID");

                entity.HasIndex(e => e.UserId)
                    .HasName("NORMAL_USERID");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.ActivityId)
                    .HasColumnType("bigint(20)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.PrizeId)
                    .HasColumnType("bigint(20)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Remark)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.UseCandy)
                    .HasColumnType("decimal(11,2)")
                    .HasDefaultValueSql("'0.00'");

                entity.Property(e => e.UsePeel)
                    .HasColumnType("decimal(11,2)")
                    .HasDefaultValueSql("'0.00'");

                entity.Property(e => e.UserId)
                    .HasColumnType("bigint(20)")
                    .HasDefaultValueSql("'0'");
            });

            modelBuilder.Entity<YoyoActivityWinRecord>(entity =>
            {
                entity.ToTable("yoyo_activity_win_record");

                entity.HasIndex(e => e.ActivityId)
                    .HasName("NORMAL_ACTIVITYID");

                entity.HasIndex(e => new { e.PrizeId, e.State })
                    .HasName("NORMAL_PRIZEID_STATE");

                entity.HasIndex(e => new { e.UserId, e.State })
                    .HasName("NORMAL_USERID_STATE");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.ActivityId)
                    .HasColumnType("bigint(20)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Contact)
                    .IsRequired()
                    .HasColumnType("varchar(12)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.MailingAddress)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Person)
                    .IsRequired()
                    .HasColumnType("varchar(64)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Postcode)
                    .IsRequired()
                    .HasColumnType("varchar(12)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.PrizeId)
                    .HasColumnType("bigint(20)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.ReceiveTime).HasColumnType("datetime");

                entity.Property(e => e.Remark)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.State)
                    .HasColumnType("int(2)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UserId)
                    .HasColumnType("bigint(20)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.WinningTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");
            });

            modelBuilder.Entity<YoyoAdClick>(entity =>
            {
                entity.ToTable("yoyo_ad_click");

                entity.HasIndex(e => new { e.ClickId, e.ClickDate })
                    .HasName("FK_ClickId_ClickDate")
                    .IsUnique();

                entity.HasIndex(e => new { e.UserId, e.ClickDate })
                    .HasName("FK_UserId,_ClickDate");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.AdId).HasColumnType("int(11)");

                entity.Property(e => e.CandyP).HasColumnType("decimal(18,2)");

                entity.Property(e => e.ClickDate).HasColumnType("date");

                entity.Property(e => e.ClickId)
                    .IsRequired()
                    .HasColumnType("varchar(64)");

                entity.Property(e => e.ClickTime).HasColumnType("datetime");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<YoyoAdMaster>(entity =>
            {
                entity.ToTable("yoyo_ad_master");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.Alt)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.CreateTime).HasColumnType("datetime");

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.ImgSrc)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Place)
                    .IsRequired()
                    .HasColumnType("varchar(32)");

                entity.Property(e => e.Remark)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Sort).HasColumnType("int(3)");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.Status).HasColumnType("int(2)");

                entity.Property(e => e.Type).HasColumnType("int(2)");

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasColumnType("varchar(255)");
            });

            modelBuilder.Entity<YoyoBangAppeals>(entity =>
            {
                entity.ToTable("yoyo_bang_appeals");

                entity.HasIndex(e => new { e.TaskId, e.RecordId })
                    .HasName("NORMAL_TASK_RECORD");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CreateTime).HasColumnType("datetime");

                entity.Property(e => e.Pictures)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Reason)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.RecordId).HasColumnType("bigint(20)");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");

                entity.Property(e => e.State).HasColumnType("int(2)");

                entity.Property(e => e.TaskId).HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<YoyoBangCategory>(entity =>
            {
                entity.ToTable("yoyo_bang_category");

                entity.HasIndex(e => e.Sort)
                    .HasName("NORMAL_PID");

                entity.HasIndex(e => new { e.Pid, e.Sort })
                    .HasName("NORMAL_PID_SORT");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.Desc)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Icon)
                    .IsRequired()
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.IsDel)
                    .HasColumnType("int(2)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.MinPrice)
                    .HasColumnType("decimal(10,2)")
                    .HasDefaultValueSql("'1.00'");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Pid)
                    .HasColumnType("bigint(20)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Sort)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");
            });

            modelBuilder.Entity<YoyoBangRank>(entity =>
            {
                entity.ToTable("yoyo_bang_rank");

                entity.HasIndex(e => new { e.TaskId, e.TaskType })
                    .HasName("NORMAL_TASK_TYPE");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CreateTime).HasColumnType("datetime");

                entity.Property(e => e.EffectiveTime).HasColumnType("datetime");

                entity.Property(e => e.ExpirationTime).HasColumnType("datetime");

                entity.Property(e => e.OfferPrice).HasColumnType("decimal(10,2)");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");

                entity.Property(e => e.TaskId).HasColumnType("bigint(20)");

                entity.Property(e => e.TaskType).HasColumnType("int(2)");
            });

            modelBuilder.Entity<YoyoBangRecord>(entity =>
            {
                entity.ToTable("yoyo_bang_record");

                entity.HasIndex(e => e.State)
                    .HasName("NORMAL_STATE");

                entity.HasIndex(e => new { e.TaskId, e.UserId, e.State })
                    .HasName("NORMAL_TASK_USERID");

                entity.HasIndex(e => new { e.UserId, e.TaskId, e.State })
                    .HasName("NORMAL_USER_TASK");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.AuditTime).HasColumnType("datetime");

                entity.Property(e => e.CutoffTime).HasColumnType("datetime");

                entity.Property(e => e.EntryTime).HasColumnType("datetime");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");

                entity.Property(e => e.State).HasColumnType("int(2)");

                entity.Property(e => e.SubmitTime).HasColumnType("datetime");

                entity.Property(e => e.TaskDetail).HasColumnType("varchar(4000)");

                entity.Property(e => e.TaskId)
                    .HasColumnType("bigint(20)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<YoyoBangStep>(entity =>
            {
                entity.ToTable("yoyo_bang_step");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.Content)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Describe)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");

                entity.Property(e => e.Sort).HasColumnType("int(2)");

                entity.Property(e => e.TaskId)
                    .HasColumnType("bigint(64)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Type)
                    .HasColumnType("int(2)")
                    .HasDefaultValueSql("'0'");
            });

            modelBuilder.Entity<YoyoBangTask>(entity =>
            {
                entity.ToTable("yoyo_bang_task");

                entity.HasIndex(e => e.IsRepeat)
                    .HasName("NORMAL_REPEAT");

                entity.HasIndex(e => e.RewardType)
                    .HasName("NORMAL_TYPE");

                entity.HasIndex(e => e.State)
                    .HasName("NORMAL_STATE");

                entity.HasIndex(e => new { e.Publisher, e.State })
                    .HasName("NORMAL_PUBLISHER_STATE");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.AuditHour)
                    .HasColumnType("int(2)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.CateId).HasColumnType("bigint(20)");

                entity.Property(e => e.Complete)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Desc)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.FeeRate)
                    .HasColumnType("decimal(6,4)")
                    .HasDefaultValueSql("'1.0000'");

                entity.Property(e => e.IsRepeat)
                    .HasColumnType("int(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Project)
                    .IsRequired()
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Publisher).HasColumnType("bigint(20)");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");

                entity.Property(e => e.RewardType).HasColumnType("int(2)");

                entity.Property(e => e.State)
                    .HasColumnType("int(2)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Step).HasColumnType("int(2)");

                entity.Property(e => e.SubmitHour)
                    .HasColumnType("int(2)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Total)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");

                entity.Property(e => e.UpdateTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<YoyoBoxActivity>(entity =>
            {
                entity.ToTable("yoyo_box_activity");

                entity.HasIndex(e => e.Period)
                    .HasName("UNIQUE_PERIOD")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.BuyTotal).HasColumnType("int(11)");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.EndTime).HasColumnType("datetime");

                entity.Property(e => e.Period)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.PrizePool)
                    .HasColumnType("decimal(18,4)")
                    .HasDefaultValueSql("'0.0000'");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");

                entity.Property(e => e.State).HasColumnType("int(2)");

                entity.Property(e => e.UnitPrice).HasColumnType("decimal(16,4)");
            });

            modelBuilder.Entity<YoyoBoxRecord>(entity =>
            {
                entity.ToTable("yoyo_box_record");

                entity.HasIndex(e => new { e.Period, e.UserId })
                    .HasName("NORMAL_PERIOD_USER");

                entity.HasIndex(e => new { e.UserId, e.Period })
                    .HasName("NORMAL_USER_PERIOD");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.BuyTotal).HasColumnType("int(11)");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Period).HasColumnType("int(11)");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");

                entity.Property(e => e.UnitPrice).HasColumnType("decimal(16,4)");

                entity.Property(e => e.UserId).HasColumnType("bigint(18)");
            });

            modelBuilder.Entity<YoyoBoxWiner>(entity =>
            {
                entity.ToTable("yoyo_box_winer");

                entity.HasIndex(e => e.Period)
                    .HasName("UNIQUE_PERIOD")
                    .IsUnique();

                entity.HasIndex(e => e.UserId)
                    .HasName("NORMAL_WINER");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.Award).HasColumnType("decimal(18,4)");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Dividend).HasColumnType("decimal(18,4)");

                entity.Property(e => e.Period).HasColumnType("int(11)");

                entity.Property(e => e.RecordId).HasColumnType("bigint(16)");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");

                entity.Property(e => e.SingleValue).HasColumnType("decimal(18,4)");

                entity.Property(e => e.UserId).HasColumnType("bigint(16)");
            });

            modelBuilder.Entity<YoyoCashDividend>(entity =>
            {
                entity.HasKey(e => e.CashDate);

                entity.ToTable("yoyo_cash_dividend");

                entity.Property(e => e.CashDate).HasColumnType("date");

                entity.Property(e => e.Cash).HasColumnType("decimal(16,2)");

                entity.Property(e => e.SingleCandy)
                    .HasColumnType("decimal(16,4)")
                    .HasDefaultValueSql("'0.0000'");
            });

            modelBuilder.Entity<YoyoCityMaster>(entity =>
            {
                entity.HasKey(e => e.CityId);

                entity.ToTable("yoyo_city_master");

                entity.HasIndex(e => e.CityCode)
                    .HasName("FK_CityCode")
                    .IsUnique();

                entity.Property(e => e.CityId).HasColumnType("int(11)");

                entity.Property(e => e.CityCode)
                    .IsRequired()
                    .HasColumnType("varchar(8)");

                entity.Property(e => e.CityName)
                    .IsRequired()
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.EndTime).HasColumnType("date");

                entity.Property(e => e.Mobile)
                    .IsRequired()
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.StartTime).HasColumnType("date");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");

                entity.Property(e => e.WeChat)
                    .IsRequired()
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("''");
            });

            modelBuilder.Entity<YoyoEverydayDividend>(entity =>
            {
                entity.HasKey(e => e.DividendDate);

                entity.ToTable("yoyo_everyday_dividend");

                entity.Property(e => e.DividendDate).HasColumnType("date");

                entity.Property(e => e.CandyFee).HasColumnType("decimal(18,6)");

                entity.Property(e => e.People1).HasColumnType("int(11)");

                entity.Property(e => e.People2).HasColumnType("int(11)");

                entity.Property(e => e.People3).HasColumnType("int(11)");

                entity.Property(e => e.People4).HasColumnType("int(11)");

                entity.Property(e => e.People5).HasColumnType("int(11)");

                entity.Property(e => e.Star1).HasColumnType("decimal(18,6)");

                entity.Property(e => e.Star2).HasColumnType("decimal(18,6)");

                entity.Property(e => e.Star3).HasColumnType("decimal(18,6)");

                entity.Property(e => e.Star4).HasColumnType("decimal(18,6)");

                entity.Property(e => e.Star5).HasColumnType("decimal(18,6)");
            });

            modelBuilder.Entity<YoyoLuckydrawDefaultuser>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.ToTable("yoyo_luckydraw_defaultuser");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<YoyoLuckydrawPrize>(entity =>
            {
                entity.ToTable("yoyo_luckydraw_prize");

                entity.HasIndex(e => e.CreatedTime);

                entity.HasIndex(e => e.Name);

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.Level).HasColumnType("int(11)");

                entity.Property(e => e.Mark).HasColumnType("varchar(1000)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.Pic).HasColumnType("varchar(500)");

                entity.Property(e => e.StatusDesc).HasColumnType("varchar(200)");
            });

            modelBuilder.Entity<YoyoLuckydrawRound>(entity =>
            {
                entity.ToTable("yoyo_luckydraw_round");

                entity.HasIndex(e => e.Level);

                entity.HasIndex(e => e.Status);

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.AutoNext).HasColumnType("bit(1)");

                entity.Property(e => e.CurrentRoundNumber).HasColumnType("int(11)");

                entity.Property(e => e.DelayHour).HasColumnType("int(11)");

                entity.Property(e => e.Level).HasColumnType("int(11)");

                entity.Property(e => e.MaxNumber).HasColumnType("int(11)");

                entity.Property(e => e.NeedRoundNumber).HasColumnType("int(11)");

                entity.Property(e => e.PrizeId).HasColumnType("int(11)");

                entity.Property(e => e.Status).HasColumnType("int(11)");

                entity.Property(e => e.WinnerType).HasColumnType("int(11)");
            });

            modelBuilder.Entity<YoyoLuckydrawUser>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoundId });

                entity.ToTable("yoyo_luckydraw_user");

                entity.HasIndex(e => e.CreatedTime);

                entity.HasIndex(e => e.RoundId);

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");

                entity.Property(e => e.RoundId).HasColumnType("int(11)");

                entity.Property(e => e.AddressId).HasColumnType("int(11)");

                entity.Property(e => e.CandyCount).HasColumnType("int(11)");

                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)");

                entity.Property(e => e.IsWin).HasColumnType("bit(1)");

                entity.Property(e => e.PrizeId).HasColumnType("int(11)");
            });

            modelBuilder.Entity<YoyoMallOrder>(entity =>
            {
                entity.ToTable("yoyo_mall_order");

                entity.HasIndex(e => e.CreateTime)
                    .HasName("FK_CREATETIME");

                entity.HasIndex(e => e.OrderStatus)
                    .HasName("FK_ORDERSTATUS");

                entity.HasIndex(e => e.UnionNo)
                    .HasName("UNIQUE_UNION_NO")
                    .IsUnique();

                entity.HasIndex(e => e.UnionType)
                    .HasName("FK_UNIONTYPE");

                entity.HasIndex(e => e.UserId)
                    .HasName("FK_USERID");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.Commission).HasColumnType("decimal(18,4)");

                entity.Property(e => e.CreateTime).HasColumnType("datetime");

                entity.Property(e => e.GoodsId).HasColumnType("bigint(20)");

                entity.Property(e => e.GoodsImage)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.GoodsName)
                    .IsRequired()
                    .HasColumnType("varchar(1024)");

                entity.Property(e => e.GoodsPrice).HasColumnType("decimal(18,2)");

                entity.Property(e => e.GoodsQuantity).HasColumnType("int(11)");

                entity.Property(e => e.ModifyTime).HasColumnType("datetime");

                entity.Property(e => e.OrderAmount).HasColumnType("decimal(18,2)");

                entity.Property(e => e.OrderStatus).HasColumnType("int(11)");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");

                entity.Property(e => e.UnionCustom)
                    .IsRequired()
                    .HasColumnType("varchar(64)");

                entity.Property(e => e.UnionNo)
                    .IsRequired()
                    .HasColumnType("varchar(64)");

                entity.Property(e => e.UnionPid)
                    .IsRequired()
                    .HasColumnType("varchar(64)");

                entity.Property(e => e.UnionType).HasColumnType("int(11)");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<YoyoMemberActive>(entity =>
            {
                entity.ToTable("yoyo_member_active");

                entity.HasIndex(e => e.ActiveTime)
                    .HasName("NORMAL_ACTIVE");

                entity.HasIndex(e => e.UserId)
                    .HasName("UNIQUE_USERID")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.ActiveTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.JpushId)
                    .IsRequired()
                    .HasColumnName("JPushId")
                    .HasColumnType("varchar(64)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Remark)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<YoyoMemberAddress>(entity =>
            {
                entity.ToTable("yoyo_member_address");

                entity.HasIndex(e => e.UserId)
                    .HasName("FK_UserId");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Area)
                    .IsRequired()
                    .HasColumnType("varchar(32)");

                entity.Property(e => e.City)
                    .IsRequired()
                    .HasColumnType("varchar(32)");

                entity.Property(e => e.IsDefault).HasColumnType("int(11)");

                entity.Property(e => e.IsDel).HasColumnType("int(11)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(64)");

                entity.Property(e => e.Phone)
                    .IsRequired()
                    .HasColumnType("varchar(11)");

                entity.Property(e => e.PostCode)
                    .IsRequired()
                    .HasColumnType("varchar(8)");

                entity.Property(e => e.Province)
                    .IsRequired()
                    .HasColumnType("varchar(32)");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<YoyoMemberDailyTask>(entity =>
            {
                entity.ToTable("yoyo_member_daily_task");

                entity.HasIndex(e => new { e.UserId, e.TaskId })
                    .HasName("UK_UserTask")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.Carry).HasColumnType("int(11)");

                entity.Property(e => e.CompleteDate).HasColumnType("date");

                entity.Property(e => e.Completed).HasColumnType("int(11)");

                entity.Property(e => e.TaskId).HasColumnType("int(11)");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<YoyoMemberDevote>(entity =>
            {
                entity.ToTable("yoyo_member_devote");

                entity.HasIndex(e => new { e.UserId, e.DevoteDate })
                    .HasName("UK_UserId_DevoteDate")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.Devote).HasColumnType("decimal(18,2)");

                entity.Property(e => e.DevoteDate).HasColumnType("date");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<YoyoMemberDuplicate>(entity =>
            {
                entity.ToTable("yoyo_member_duplicate");

                entity.HasIndex(e => new { e.Date, e.UserId })
                    .HasName("UK_Date_UserId")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.Date).HasColumnType("date");

                entity.Property(e => e.Duplicate).HasColumnType("decimal(18,5)");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<YoyoMemberInviteRanking>(entity =>
            {
                entity.ToTable("yoyo_member_invite_ranking");

                entity.HasIndex(e => e.InviteTotal)
                    .HasName("FK_InviteTotal");

                entity.HasIndex(e => new { e.Phase, e.UserId })
                    .HasName("FK_Phase_UserId")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.InviteDate).HasColumnType("date");

                entity.Property(e => e.InviteToday).HasColumnType("int(11)");

                entity.Property(e => e.InviteTotal).HasColumnType("int(11)");

                entity.Property(e => e.Phase).HasColumnType("int(11)");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<YoyoMemberRelation>(entity =>
            {
                entity.HasKey(e => e.MemberId);

                entity.ToTable("yoyo_member_relation");

                entity.HasIndex(e => e.ParentId)
                    .HasName("FK_ParentId");

                entity.Property(e => e.MemberId).HasColumnType("bigint(20)");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.ParentId).HasColumnType("bigint(20)");

                entity.Property(e => e.RelationLevel).HasColumnType("int(11)");

                entity.Property(e => e.Topology).HasColumnType("text");
            });

            modelBuilder.Entity<YoyoMemberStarNow>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.ToTable("yoyo_member_star_now");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");

                entity.Property(e => e.BigCandyH)
                    .HasColumnName("bigCandyH")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.LittleCandyH)
                    .HasColumnName("littleCandyH")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.TeamCandyH)
                    .HasColumnName("teamCandyH")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.TeamStart)
                    .HasColumnName("teamStart")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");
            });

            modelBuilder.Entity<YoyoPayRecord>(entity =>
            {
                entity.HasKey(e => e.PayId);

                entity.ToTable("yoyo_pay_record");

                entity.HasIndex(e => e.UserId)
                    .HasName("FK_UserId");

                entity.Property(e => e.PayId).HasColumnType("bigint(20)");

                entity.Property(e => e.ActionType).HasColumnType("int(8)");

                entity.Property(e => e.Amount).HasColumnType("decimal(10,2)");

                entity.Property(e => e.Channel).HasColumnType("int(8)");

                entity.Property(e => e.ChannelUid)
                    .IsRequired()
                    .HasColumnName("ChannelUID")
                    .HasColumnType("varchar(64)");

                entity.Property(e => e.CreateTime).HasColumnType("datetime");

                entity.Property(e => e.Currency).HasColumnType("int(8)");

                entity.Property(e => e.Custom)
                    .IsRequired()
                    .HasColumnType("varchar(128)");

                entity.Property(e => e.Fee).HasColumnType("decimal(10,2)");

                entity.Property(e => e.ModifyTime).HasColumnType("datetime");

                entity.Property(e => e.PayStatus).HasColumnType("int(11)");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<YoyoRechargeOrder>(entity =>
            {
                entity.ToTable("yoyo_recharge_order");

                entity.HasIndex(e => e.ChannelNo)
                    .HasName("ChannelNo");

                entity.HasIndex(e => e.OrderNo)
                    .HasName("OrderNo");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.Account)
                    .IsRequired()
                    .HasColumnType("varchar(64)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.BuyNum)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.ChannelNo)
                    .IsRequired()
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.FaceValue)
                    .IsRequired()
                    .HasColumnType("varchar(64)");

                entity.Property(e => e.OrderNo)
                    .IsRequired()
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.OrderType)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.PayCandy)
                    .HasColumnType("decimal(16,4)")
                    .HasDefaultValueSql("'0.0000'");

                entity.Property(e => e.PayPeel).HasColumnType("decimal(16,4)");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(16,4)")
                    .HasDefaultValueSql("'0.0000'");

                entity.Property(e => e.ProductId)
                    .IsRequired()
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.PurchasePrice)
                    .HasColumnType("decimal(10,2)")
                    .HasDefaultValueSql("'0.00'");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");

                entity.Property(e => e.State).HasColumnType("int(11)");

                entity.Property(e => e.UpdateTime).HasColumnType("datetime");

                entity.Property(e => e.UserId)
                    .HasColumnType("bigint(20)")
                    .HasDefaultValueSql("'0'");
            });

            modelBuilder.Entity<YoyoShandwOrder>(entity =>
            {
                entity.ToTable("yoyo_shandw_order");

                entity.HasIndex(e => e.ChannelOrderNo)
                    .HasName("UNIQUE_ORDER_NO")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.Amount).HasColumnType("decimal(18,4)");

                entity.Property(e => e.ChannelNo)
                    .IsRequired()
                    .HasColumnType("varchar(36)");

                entity.Property(e => e.ChannelOrderNo)
                    .IsRequired()
                    .HasColumnType("varchar(64)");

                entity.Property(e => e.ChannelUid)
                    .IsRequired()
                    .HasColumnType("varchar(36)");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.GameAppId)
                    .IsRequired()
                    .HasColumnType("varchar(64)");

                entity.Property(e => e.PayMoney).HasColumnType("decimal(18,4)");

                entity.Property(e => e.PayTime).HasColumnType("datetime");

                entity.Property(e => e.Product)
                    .IsRequired()
                    .HasColumnType("varchar(36)");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");

                entity.Property(e => e.State).HasColumnType("int(11)");

                entity.Property(e => e.UserId).HasColumnType("bigint(16)");
            });

            modelBuilder.Entity<YoyoSignRecord>(entity =>
            {
                entity.HasKey(e => e.Sign);

                entity.ToTable("yoyo_sign_record");

                entity.Property(e => e.Sign).HasColumnType("varchar(32)");

                entity.Property(e => e.ActionName)
                    .IsRequired()
                    .HasColumnType("varchar(64)");

                entity.Property(e => e.ClientTime).HasColumnType("datetime");

                entity.Property(e => e.ControllerName)
                    .IsRequired()
                    .HasColumnType("varchar(32)");

                entity.Property(e => e.ServerTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<YoyoSystemCopywriting>(entity =>
            {
                entity.ToTable("yoyo_system_copywriting");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.Key)
                    .HasColumnName("key")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Text)
                    .IsRequired()
                    .HasColumnName("text")
                    .HasColumnType("text");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnName("title")
                    .HasColumnType("varchar(512)");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnName("type")
                    .HasColumnType("varchar(32)");
            });

            modelBuilder.Entity<YoyoSystemTask>(entity =>
            {
                entity.ToTable("yoyo_system_task");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.Aims).HasColumnType("int(11)");

                entity.Property(e => e.Devote).HasColumnType("decimal(18,2)");

                entity.Property(e => e.Reward).HasColumnType("decimal(18,2)");

                entity.Property(e => e.Sort).HasColumnType("int(11)");

                entity.Property(e => e.Status).HasColumnType("int(11)");

                entity.Property(e => e.TaskDesc)
                    .IsRequired()
                    .HasColumnType("varchar(1024)");

                entity.Property(e => e.TaskTitle)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.TaskType).HasColumnType("int(11)");

                entity.Property(e => e.Unit)
                    .IsRequired()
                    .HasColumnType("varchar(8)");
            });

            modelBuilder.Entity<YoyoTaskRecord>(entity =>
            {
                entity.ToTable("yoyo_task_record");

                entity.HasIndex(e => new { e.UserId, e.CreateDate, e.MId })
                    .HasName("USER_DATE")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CreateDate).HasColumnType("date");

                entity.Property(e => e.EndTime).HasColumnType("datetime");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");

                entity.Property(e => e.Schedule).HasColumnType("decimal(11,2)");

                entity.Property(e => e.Source).HasColumnType("int(3)");

                entity.Property(e => e.StartTime).HasColumnType("datetime");

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");
            });
        }
    }
}
