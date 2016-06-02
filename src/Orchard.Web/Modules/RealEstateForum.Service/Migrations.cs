using System;
using System.Collections.Generic;
using System.Data;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.ContentManagement;
using Orchard;
using Orchard.Localization;
using Orchard.Security;
using RealEstateForum.Service.Models;

namespace RealEstateForum.Service
{
    public class Migrations : DataMigrationImpl
    {

        private readonly IMembershipService _membershipService;

        public Migrations(IOrchardServices services, IMembershipService membershipService)
        {
            Services = services;
            _membershipService = membershipService;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public int Create()
        {

            // 1. Creating table PublishStatusPartRecord
            SchemaBuilder.CreateTable("PublishStatusPartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String)
            );
            ContentDefinitionManager.AlterTypeDefinition("PublishStatus", cfg => cfg.WithPart("PublishStatusPart"));

            // 2. Creating table ForumPostStatusPartRecord
            SchemaBuilder.CreateTable("ForumPostStatusPartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("CssClass", DbType.String)
                .Column("SeqOrder", DbType.Int32)
                .Column("IsEnabled", DbType.Boolean)
            );
            ContentDefinitionManager.AlterTypeDefinition("ForumPostStatus", cfg => cfg.WithPart("ForumPostStatusPart"));

            // 3. Creating table ForumThreadPartRecord
            SchemaBuilder.CreateTable("ForumThreadPartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("Description", DbType.String)
                .Column("SeqOrder", DbType.Int32)
                .Column("ParentThreadId", DbType.Int32)
                .Column("DefaultImage", DbType.String)
                .Column("IsOpen", DbType.Boolean)
                .Column("DateCreated", DbType.DateTime)
            );
            ContentDefinitionManager.AlterTypeDefinition("ForumThread", cfg => cfg.WithPart("ForumThreadPart"));

            // 4. Creating table ForumPostPartRecord
            SchemaBuilder.CreateTable("ForumPostPartRecord", table => table
                .ContentPartRecord()
                .Column("Thread_id", DbType.Int32)
                .Column("Blog_id", DbType.Int32)
                .Column("Description", DbType.String, c => c.WithType(DbType.String).Unlimited())
                .Column("Title", DbType.String)
                .Column("Content", DbType.String, c => c.WithType(DbType.String).Unlimited())
                .Column("UserPost_id", DbType.Int32)
                .Column("CssImage", DbType.String)
                .Column("IsPinned", DbType.Boolean, c => c.WithDefault(false))
                .Column("TimeExpiredPinned", DbType.DateTime)
                .Column("IsProject", DbType.Boolean)
                .Column("IsMarket", DbType.Boolean)
                .Column("IsHeighLight", DbType.Boolean)
                .Column("PublishStatus_id", DbType.Int32)
                .Column("DateCreated", DbType.DateTime)
                .Column("DateUpdated", DbType.DateTime)
                .Column("Views", DbType.Int64)
                .Column("StatusPost_id", DbType.Int32)
                .Column("IsShareBlog", DbType.Boolean)
                .Column("BlogDateCreated", DbType.DateTime)
                .Column("SeqOrder", DbType.Int32)
            );
            ContentDefinitionManager.AlterTypeDefinition("ForumPost", cfg => cfg.WithPart("ForumPostPart"));

            // 5. Creating table CommentForumPartRecord
            SchemaBuilder.CreateTable("CommentForumPartRecord", table => table
                .ContentPartRecord()
                .Column("ForumPost_id", DbType.Int32)
                .Column("ParentCommentId", DbType.Int32)
                .Column("Content", DbType.String)
                .Column("DateCreated", DbType.DateTime)
                .Column("SortOrder", DbType.Int32)
            );
            ContentDefinitionManager.AlterTypeDefinition("CommentForum", cfg => cfg.WithPart("CommentForumPart"));

            // 6. Creating table CommentLikePartRecord
            SchemaBuilder.CreateTable("CommentLikePartRecord", table => table
                .ContentPartRecord()
                .Column("Comment_id", DbType.Int32)
                .Column("User_id", DbType.Int32)
            );
            ContentDefinitionManager.AlterTypeDefinition("CommentLike", cfg => cfg.WithPart("CommentLikePart"));

            // 7. Creating table FilterRulesPartRecord
            SchemaBuilder.CreateTable("FilterRulesPartRecord", table => table
                .ContentPartRecord()
                .Column("FromWord", DbType.String)
                .Column("ToWord", DbType.String)
            );
            ContentDefinitionManager.AlterTypeDefinition("FilterRules", cfg => cfg.WithPart("FilterRulesPart"));

            return 1;
        }
        public int UpdateFrom1()
        {
            // PublishStatus
            IEnumerable<PublishStatusPartRecord> _publishStatus =
            new List<PublishStatusPartRecord> {
                new PublishStatusPartRecord {Name = "Cộng đồng"},
                new PublishStatusPartRecord {Name = "Bạn bè"},
                new PublishStatusPartRecord {Name = "Chỉ mình tôi"},
            };
            foreach (var item in _publishStatus)
            {
                var model = Services.ContentManager.New<PublishStatusPart>("PublishStatus");
                model.Name = item.Name;
                Services.ContentManager.Create(model);
            }

            return 2;
        }
        public int UpdateFrom2()
        {
            // ForumPostStatus
            IEnumerable<ForumPostStatusPartRecord> _poststatus =
            new List<ForumPostStatusPartRecord> {
                new ForumPostStatusPartRecord {Name = "Bài viết bình thường", ShortName = "binh-thuong", CssClass= "st-none", SeqOrder = 1, IsEnabled = true},
                new ForumPostStatusPartRecord {Name = "Bài viết rác", ShortName = "bai-rac", CssClass= "st-bin", SeqOrder = 2, IsEnabled = true},
                new ForumPostStatusPartRecord {Name = "Đang soạn", ShortName = "dang-soan", CssClass= "st-posting", SeqOrder = 3 , IsEnabled = true},
            };
            foreach (var item in _poststatus)
            {
                var model = Services.ContentManager.New<ForumPostStatusPart>("ForumPostStatus");
                model.Name = item.Name;
                model.ShortName = item.ShortName;
                model.CssClass = item.CssClass;
                model.SeqOrder = item.SeqOrder;
                model.IsEnabled = item.IsEnabled;
                Services.ContentManager.Create(model);
            }

            return 3;
        }
        public int UpdateFrom3()
        {
            SchemaBuilder.AlterTable("CommentForumPartRecord", table => table
               .AddColumn("UserComment_id", DbType.Int32)
            );

            return 4;
        }

        public int UpdateFrom4()
        {
            // 4. Add column in table ForumThreadPartRecord
            SchemaBuilder.AlterTable("ForumThreadPartRecord", table => table
                .AddColumn("HostName", DbType.String)
              );
            // 4. Add column in table ForumPostPartRecord
            SchemaBuilder.AlterTable("ForumPostPartRecord", table => table
              .AddColumn("HostName", DbType.String)
            );

            // 4. Creating table HostNamePartRecord
            SchemaBuilder.CreateTable("HostNamePartRecord", table => table
                           .ContentPartRecord()
                           .Column("Name", DbType.String)
                           .Column("ShortName", DbType.String)
                           .Column("CssClass", DbType.String)
                           .Column("SeqOrder", DbType.Int32)
                           .Column("IsEnabled", DbType.Boolean)
                       );
            ContentDefinitionManager.AlterTypeDefinition("HostName", cfg => cfg.WithPart("HostNamePart"));
            
            return 5;
        }
        public int UpdateFrom5()
        {
            // alter table CommentForumPartRecord
            SchemaBuilder.AlterTable("CommentForumPartRecord", table => table.AlterColumn("Content", column => column.WithLength(5000).WithType(DbType.String)));
            return 6;
        }
        public int UpdateFrom6()
        {
            // Creating table SupportOnlineConfigPartRecord
            SchemaBuilder.CreateTable("SupportOnlineConfigPartRecord", table => table
                .ContentPartRecord()
                .Column("NumberPhone", DbType.String)
                .Column("YahooNick", DbType.String)
                .Column("SkypeNick", DbType.String)
            );
            ContentDefinitionManager.AlterTypeDefinition("SupportOnlineConfig", cfg => cfg.WithPart("SupportOnlineConfigPart"));

            return 7;
        }
        public int UpdateFrom7()
        {
            // Creating table ForumFriendPartRecord
            SchemaBuilder.CreateTable("ForumFriendPartRecord", table => table
                .ContentPartRecord()
                .Column("UserRequest_id", DbType.Int32)
                .Column("UserReceived_id", DbType.Int32)
                .Column("Status", DbType.Boolean, c => c.WithDefault(false))
                .Column("DateRequest", DbType.String)
            );
            ContentDefinitionManager.AlterTypeDefinition("ForumFriend", cfg => cfg.WithPart("ForumFriendPart"));

            return 8;
        }
        public int UpdateFrom8()
        {
            // Creating table ForumFriendPartRecord
            SchemaBuilder.CreateTable("AdsPriceConfigPartRecord", table => table
                .ContentPartRecord()
                .Column("Price", DbType.String)
                .Column("CssClass", DbType.String)
            );
            ContentDefinitionManager.AlterTypeDefinition("AdsPriceConfig", cfg => cfg.WithPart("AdsPriceConfigPart"));

            return 9;
        }
        public int UpdateFrom9()
        {
            SchemaBuilder.AlterTable("ForumPostPartRecord", table => table
               .AddColumn("BlogPostId", DbType.Int32)
            );

            return 10;
        }

        public int UpdateFrom10()
        {
            SchemaBuilder.CreateTable("UnitInvestPartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String)
                .Column("Avatar", DbType.String)
                .Column("Website", DbType.String)
                .Column("Content", DbType.String)
                .Column("SeqOrder", DbType.Int32)
                .Column("IsEnabled", DbType.Boolean)
            );
            ContentDefinitionManager.AlterTypeDefinition("UnitInvest", cfg => cfg.WithPart("UnitInvestPart"));

            return 11;
        }

        public int UpdateFrom11()
        {
            SchemaBuilder.AlterTable("UnitInvestPartRecord", table => table
               .AddColumn("GroupId", DbType.Int32)
            );

            return 12;
        }

        public int UpdateFrom12()
        {
            // ForumPostStatus
            IEnumerable<ForumPostStatusPartRecord> _poststatus =
            new List<ForumPostStatusPartRecord> {
                new ForumPostStatusPartRecord {Name = "Bài viết chờ duyệt", ShortName = "cho-duyet", CssClass= "st-peding", SeqOrder = 4, IsEnabled = true}
            };
            foreach (var item in _poststatus)
            {
                var model = Services.ContentManager.New<ForumPostStatusPart>("ForumPostStatus");
                model.Name = item.Name;
                model.ShortName = item.ShortName;
                model.CssClass = item.CssClass;
                model.SeqOrder = item.SeqOrder;
                model.IsEnabled = item.IsEnabled;
                Services.ContentManager.Create(model);
            }

            return 13;
        }
        public int UpdateFrom13()
        {
            // ForumPostStatus
            var model = Services.ContentManager.Get<ForumPostStatusPart>(1026356);//Online: => 1026356 ; local: 986469
            if(model != null)
            {
                model.CssClass = "st-pending";
            }

            return 14;
        }
    }
}