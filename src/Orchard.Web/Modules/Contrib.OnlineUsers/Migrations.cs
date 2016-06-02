using System;
using System.Collections.Generic;
using System.Data;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Contrib.OnlineUsers.Models;

namespace Contrib.OnlineUsers {
    public class Migrations : DataMigrationImpl {

        public int Create() {

            SchemaBuilder.CreateTable("MembershipPartRecord", table => table
                            .ContentPartRecord()
                            .Column("LastActive", DbType.DateTime)
                        );

            ContentDefinitionManager.AlterTypeDefinition("User",
                cfg => cfg
                    .WithPart(typeof(MembershipPart).Name)
                );

            ContentDefinitionManager.AlterTypeDefinition("OnlineUsersWidget", type => type

                // Attach the "OnlineUsersPart"
                .WithPart(typeof(OnlineUsersPart).Name)

                // In order to turn this content type into a widget, it needs the WidgetPart
                .WithPart("WidgetPart")

                // It also needs a setting called "Stereotype" to be set to "Widget"
                .WithSetting("Stereotype", "Widget")

                .WithPart("CommonPart")
                );

            return 1;
        }
        public int UpdateFrom1()
        {
            ContentDefinitionManager.AlterTypeDefinition("ListOnlineUsersWidget", type => type

                // Attach the "ListOnlineUsersPart"
                .WithPart(typeof(ListOnlineUsersPart).Name)

                // In order to turn this content type into a widget, it needs the WidgetPart
                .WithPart("WidgetPart")

                // It also needs a setting called "Stereotype" to be set to "Widget"
                .WithSetting("Stereotype", "Widget")

                .WithPart("CommonPart")
                );
            return 2;
        }

        public int UpdateFrom2()
        {
            SchemaBuilder.CreateTable("UserUpdateProfileRecord", table => table
               .ContentPartRecord()
               .Column("Avatar", DbType.String)
               .Column("FirstName", DbType.String)
               .Column("LastName", DbType.String)
               .Column("DisplayName", DbType.String)
               .Column("Gender", DbType.String)
               .Column("DateOfBirth", DbType.DateTime)
               .Column("Address", DbType.String)
               .Column("Phone", DbType.String)
               .Column("Email", DbType.String)
               .Column("Job", DbType.String)
               .Column("Level", DbType.String)
               .Column("Website", DbType.String)
               .Column("Note", DbType.String)
               .Column("Signature", DbType.String)
               .Column("IsAgency", DbType.Boolean)
               .Column("IsSignature", DbType.Boolean)
               .Column("PublishPhone", DbType.String)
               .Column("PublishAddress", DbType.String)
               .Column("PublishJob", DbType.String)
               .Column("PublishLevel", DbType.String)
           );
            ContentDefinitionManager.AlterTypeDefinition("User",
                cfg => cfg
                    .WithPart(typeof(UserUpdateProfilePart).Name)
                );
            return 3;
        }
        public int UpdateFrom3()
        {
            SchemaBuilder.AlterTable("UserUpdateProfileRecord", table => table
                .AddColumn<string>("AreaAgency")
             );
            return 4;
        }
        public int UpdateFrom4()
        {
            SchemaBuilder.AlterTable("UserUpdateProfileRecord", table => {
                    table.DropColumn("IsAgency"); 
                    table.DropColumn("AreaAgency"); 
            });
            return 5;
        }
         public int UpdateFrom5()
         {
             SchemaBuilder.AlterTable("UserUpdateProfileRecord", table => {
                    table.AddColumn("AreaAgencies", DbType.String);
                    table.AddColumn("EndDateAgencing", DbType.DateTime);
             });
             return 6;
         }
     
         public int UpdateFrom6()
         {
             SchemaBuilder.AlterTable("UserUpdateProfileRecord", table =>
             {
                 table.AddColumn("NickName", DbType.String);
             });
             return 7;
         }
         public int UpdateFrom7()
         {
             SchemaBuilder.AlterTable("UserUpdateProfileRecord", table =>
             {
                 table.AddColumn("IsSelling", DbType.Boolean);
                 table.AddColumn("IsLeasing", DbType.Boolean);
             });
             return 8;
         }
    }
}