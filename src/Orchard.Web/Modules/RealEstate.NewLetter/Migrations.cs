using System;
using System.Collections.Generic;
using System.Data;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace RealEstate.NewLetter {
    public class Migrations : DataMigrationImpl {

        public int Create() {
			// Creating table CustomerEmailExceptionPartRecord
			SchemaBuilder.CreateTable("CustomerEmailExceptionPartRecord", table => table
				.ContentPartRecord()
				.Column("EmailException", DbType.String)
				.Column("CodeRandom", DbType.String)
				.Column("StatusActive", DbType.Boolean)
			);
            ContentDefinitionManager.AlterTypeDefinition("CustomerEmailException", cfg => cfg.WithPart("CustomerEmailExceptionPart"));

            return 1;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.CreateTable("ContactInboxPartRecord", table => table
                           .ContentPartRecord()
                           .Column("FullName", DbType.String)
                           .Column("Email", DbType.String)
                           .Column("Phone", DbType.String)
                           .Column("Title", DbType.String)
                           .Column("Content", DbType.String, c => c.WithType(DbType.String).Unlimited())
                           .Column("Link", DbType.String)
                           .Column("IsRead", DbType.Boolean, c=> c.WithDefault(false))
                           .Column("DateCreated", DbType.DateTime)
                       );
            ContentDefinitionManager.AlterTypeDefinition("ContactInbox", cfg => cfg.WithPart("ContactInboxPart"));

            return 2;
        }

        public int UpdateFrom2()
        {
            SchemaBuilder.CreateTable("MessageInboxPartRecord", table => table
                           .ContentPartRecord()
                           .Column("ParentId", DbType.Int32)
                           .Column("PreviousId", DbType.Int32)
                           .Column("UserSend_id", DbType.Int32)
                           .Column("UserReceived_id", DbType.Int32)
                           .Column("Title", DbType.String)
                           .Column("Content", DbType.String, c => c.WithType(DbType.String).Unlimited())
                           .Column("DateSend", DbType.DateTime)
                           .Column("IsRead", DbType.Boolean, c => c.WithDefault(false))
                           .Column("IsAdmin", DbType.Boolean, c => c.WithDefault(false))
                           .Column("ReadByStaff", DbType.Boolean, c => c.WithDefault(false))
                           .Column("IsUserDelete", DbType.Boolean, c=>c.WithDefault(false))
                       );
            ContentDefinitionManager.AlterTypeDefinition("MessageInbox", cfg => cfg.WithPart("MessageInboxPart"));

            return 3;
        }

        public int UpdateFrom3()
        {
            SchemaBuilder.AlterTable("ContactInboxPartRecord", table => table
              .AddColumn("HostName", DbType.String)
            );
            SchemaBuilder.AlterTable("MessageInboxPartRecord", table => table
             .AddColumn("HostName", DbType.String)
           );
            return 4;
        }
    }
}