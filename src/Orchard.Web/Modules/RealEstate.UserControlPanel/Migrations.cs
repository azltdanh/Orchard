using System;
using System.Collections.Generic;
using System.Data;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using RealEstate.UserControlPanel.Models;

namespace RealEstate.UserControlPanel
{
    public class Migrations : DataMigrationImpl {

        public int Create() {
            ContentDefinitionManager.AlterTypeDefinition("UserControlNavigationWidget", type => type
                // Attach the "ShoppingCartWidgetPart"
                .WithPart("UserControlNavigationWidgetPart")
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
            ContentDefinitionManager.AlterTypeDefinition("UserControlPanelWidget", type => type
                // Attach the "ShoppingCartWidgetPart"
                .WithPart("UserControlPanelWidgetPart")
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
            ContentDefinitionManager.AlterTypeDefinition("UserControlWishListWidget", type => type
                // Attach the "ShoppingCartWidgetPart"
                .WithPart("UserControlWishListWidgetPart")
                // In order to turn this content type into a widget, it needs the WidgetPart
                .WithPart("WidgetPart")
                // It also needs a setting called "Stereotype" to be set to "Widget"
                .WithSetting("Stereotype", "Widget")
                .WithPart("CommonPart")
                );
            return 3;
        }

        public int UpdateFrom3()
        {
            ContentDefinitionManager.AlterTypeDefinition("UserControlMailBoxWidget", type => type
                // Attach the "ShoppingCartWidgetPart"
                .WithPart("UserControlMailBoxWidgetPart")
                // In order to turn this content type into a widget, it needs the WidgetPart
                .WithPart("WidgetPart")
                // It also needs a setting called "Stereotype" to be set to "Widget"
                .WithSetting("Stereotype", "Widget")
                .WithPart("CommonPart")
                );
            return 4;
        }

        public int UpdateFrom4()
        {
            /*SchemaBuilder.CreateTable("UserProfileInfomationPartRecord", table => table
                .ContentPartRecord()
                .Column("Address", DbType.String)
                .Column("Phone", DbType.String)
                .Column("CompanyName", DbType.String)
                .Column("DateOfBirth", DbType.DateTime)
                .Column("Website", DbType.String)
            );
            ContentDefinitionManager.AlterTypeDefinition("User",
                cfg => cfg
                    .WithPart(typeof(UserProfileInfomationPart).Name)
                );*/
            return 5;
        }
    }
}