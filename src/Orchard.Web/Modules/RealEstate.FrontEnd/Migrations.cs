using System.Data;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace RealEstate.FrontEnd {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            ContentDefinitionManager.AlterTypeDefinition("FilterWidget", type => type

                // Attach the "ShoppingCartWidgetPart"
                .WithPart("FilterWidgetPart")

                // In order to turn this content type into a widget, it needs the WidgetPart
                .WithPart("WidgetPart")

                // It also needs a setting called "Stereotype" to be set to "Widget"
                .WithSetting("Stereotype", "Widget")
                );
            return 1;
        }

        #region 1- 10

        public int UpdateFrom1()
        {
            // Update the ShoppingCartWidget so that it has a CommonPart attached, which is required for widgets (it's generally a good idea to have this part attached)
            ContentDefinitionManager.AlterTypeDefinition("FilterWidget", type => type
                .WithPart("CommonPart")
            );

            return 2;
        }

        public int UpdateFrom2()
        {
            ContentDefinitionManager.AlterTypeDefinition("OnlineSupportWidget", type => type

                // Attach the "ShoppingCartWidgetPart"
                .WithPart("OnlineSupportWidgetPart")

                // In order to turn this content type into a widget, it needs the WidgetPart
                .WithPart("WidgetPart")

                // It also needs a setting called "Stereotype" to be set to "Widget"
                .WithSetting("Stereotype", "Widget")

                .WithPart("CommonPart")
                );
            return 3;
        }

        public int UpdateFrom3() {
            // Creating table UserPropertyRecord
            SchemaBuilder.CreateTable("UserPropertyRecord", table => table
                .Column("Id", DbType.Int32, column => column.PrimaryKey().Identity())
                .Column("UserPartRecord_Id",DbType.Int32)
                .Column("PropertyPartRecord_Id", DbType.Int32)
            );
            return 4;
        }

        public int UpdateFrom4()
        {
            ContentDefinitionManager.AlterTypeDefinition("PropertyTagsWidget", type => type

                // Attach the "ShoppingCartWidgetPart"
                .WithPart("PropertyTagsWidgetPart")

                // In order to turn this content type into a widget, it needs the WidgetPart
                .WithPart("WidgetPart")

                // It also needs a setting called "Stereotype" to be set to "Widget"
                .WithSetting("Stereotype", "Widget")

                .WithPart("CommonPart")
                );
            return 5;
        }

        public int UpdateFrom5()
        {
            ContentDefinitionManager.AlterTypeDefinition("FooterLinkWidget", type => type

                // Attach the "ShoppingCartWidgetPart"
                .WithPart("FooterLinkWidgetPart")

                // In order to turn this content type into a widget, it needs the WidgetPart
                .WithPart("WidgetPart")

                // It also needs a setting called "Stereotype" to be set to "Widget"
                .WithSetting("Stereotype", "Widget")

                .WithPart("CommonPart")
                );
            return 6;
        }

        public int UpdateFrom6()
        {
            ContentDefinitionManager.AlterTypeDefinition("AsideFirstLinkWidget", type => type

                // Attach the "ShoppingCartWidgetPart"
                .WithPart("AsideFirstLinkWidgetPart")

                // In order to turn this content type into a widget, it needs the WidgetPart
                .WithPart("WidgetPart")

                // It also needs a setting called "Stereotype" to be set to "Widget"
                .WithSetting("Stereotype", "Widget")

                .WithPart("CommonPart")
                );
            return 7;
        }

        public int UpdateFrom7()
        {
            ContentDefinitionManager.AlterTypeDefinition("BreadcrumbWidget", type => type

                // Attach the "ShoppingCartWidgetPart"
                .WithPart("BreadcrumbWidgetPart")

                // In order to turn this content type into a widget, it needs the WidgetPart
                .WithPart("WidgetPart")

                // It also needs a setting called "Stereotype" to be set to "Widget"
                .WithSetting("Stereotype", "Widget")

                .WithPart("CommonPart")
                );
            return 8;
        }

        public int UpdateFrom8()
        {
            ContentDefinitionManager.AlterTypeDefinition("PropertyGoodDealWidget", type => type

                // Attach the "ShoppingCartWidgetPart"
                .WithPart("PropertyGoodDealWidgetPart")

                // In order to turn this content type into a widget, it needs the WidgetPart
                .WithPart("WidgetPart")

                // It also needs a setting called "Stereotype" to be set to "Widget"
                .WithSetting("Stereotype", "Widget")

                .WithPart("CommonPart")
                );
            return 9;
        }

        public int UpdateFrom9()
        {
            ContentDefinitionManager.AlterTypeDefinition("PropertyVipWidget", type => type

                // Attach the "ShoppingCartWidgetPart"
                .WithPart("PropertyVipWidgetPart")

                // In order to turn this content type into a widget, it needs the WidgetPart
                .WithPart("WidgetPart")

                // It also needs a setting called "Stereotype" to be set to "Widget"
                .WithSetting("Stereotype", "Widget")

                .WithPart("CommonPart")
                );
            return 10;
        }

        public int UpdateFrom10()
        {
            ContentDefinitionManager.AlterTypeDefinition("PropertyNewWidget", type => type

                // Attach the "ShoppingCartWidgetPart"
                .WithPart("PropertyNewWidgetPart")

                // In order to turn this content type into a widget, it needs the WidgetPart
                .WithPart("WidgetPart")

                // It also needs a setting called "Stereotype" to be set to "Widget"
                .WithSetting("Stereotype", "Widget")

                .WithPart("CommonPart")
                );
            return 11;
        }

        #endregion

        #region 11 - 20

        public int UpdateFrom11()
        {
            ContentDefinitionManager.AlterTypeDefinition("PropertyHighlightsWidget", type => type

                // Attach the "ShoppingCartWidgetPart"
                .WithPart("PropertyHighlightsWidgetPart")

                // In order to turn this content type into a widget, it needs the WidgetPart
                .WithPart("WidgetPart")

                // It also needs a setting called "Stereotype" to be set to "Widget"
                .WithSetting("Stereotype", "Widget")

                .WithPart("CommonPart")
                );
            return 12;
        }

        public int UpdateFrom12()
        {
            // Creating table FrontEndSettingPartRecord
            SchemaBuilder.CreateTable("FrontEndSettingPartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String, column => column.WithLength(500))
                .Column("Value", DbType.String, column => column.WithLength(500))
                .Column("Description", DbType.String, column => column.WithLength(500))
                .Column("SeqOrder", DbType.Int32)
            );
            ContentDefinitionManager.AlterTypeDefinition("FrontEndSetting", cfg => cfg.WithPart("FrontEndSettingPart"));
            return 13;
        }

        public int UpdateFrom13()
        {
            ContentDefinitionManager.AlterTypeDefinition("EstimatePropertyWidget", type => type

                // Attach the "ShoppingCartWidgetPart"
                .WithPart("EstimatePropertyWidgetPart")

                // In order to turn this content type into a widget, it needs the WidgetPart
                .WithPart("WidgetPart")

                // It also needs a setting called "Stereotype" to be set to "Widget"
                .WithSetting("Stereotype", "Widget")

                .WithPart("CommonPart")
                );
            return 14;
        }

        public int UpdateFrom14()
        {
            SchemaBuilder.CreateTable("AliasesMetaPartRecord", table => table
                .ContentPartRecord()
                .Column("Alias", DbType.String, column => column.WithLength(500))
                .Column("Route", DbType.String, column => column.WithLength(500))
                .Column("Title", DbType.String, column => column.WithLength(500))
                .Column("Keywords", DbType.String, column => column.WithLength(500))
                .Column("Description", DbType.String, column => column.WithLength(500))
                .Column("SeqOrder", DbType.Int32)
            );
            ContentDefinitionManager.AlterTypeDefinition("AliasesMeta", cfg => cfg.WithPart("AliasesMetaPart"));
            return 15;
        }

        public int UpdateFrom15()
        {
            SchemaBuilder.AlterTable("AliasesMetaPartRecord", table => {
                table.DropColumn("Alias");
                table.DropColumn("Route");
            });
            return 16;
        }

        public int UpdateFrom16()
        {
            SchemaBuilder.AlterTable("AliasesMetaPartRecord", table => table.AddColumn("Alias_Id", DbType.Int32));
            return 17;
        }

        public int UpdateFrom17()
        {
            ContentDefinitionManager.AlterTypeDefinition("AliasesMetaWidget", type => type

                // Attach the "ShoppingCartWidgetPart"
                .WithPart("AliasesMetaPart")

                // In order to turn this content type into a widget, it needs the WidgetPart
                .WithPart("WidgetPart")

                // It also needs a setting called "Stereotype" to be set to "Widget"
                .WithSetting("Stereotype", "Widget")

                .WithPart("CommonPart")
                );
            return 18;
        }

        public int UpdateFrom18()
        {
            ContentDefinitionManager.AlterTypeDefinition("MapPlanningWidget", type => type

                // Attach the "ShoppingCartWidgetPart"
                .WithPart("MapPlanningWidgetPart")

                // In order to turn this content type into a widget, it needs the WidgetPart
                .WithPart("WidgetPart")

                // It also needs a setting called "Stereotype" to be set to "Widget"
                .WithSetting("Stereotype", "Widget")

                .WithPart("CommonPart")
                );
            return 19;
        }

        public int UpdateFrom19()
        {
            ContentDefinitionManager.AlterTypeDefinition("FilterPropertyWidgetSidebar", type => type
               .WithPart("FilterProperyWidgetSidebarPart")
                // In order to turn this content type into a widget, it needs the WidgetPart
               .WithPart("WidgetPart")
                // It also needs a setting called "Stereotype" to be set to "Widget"
               .WithSetting("Stereotype", "Widget")
               .WithPart("CommonPart")
               );
            return 20;
        }

        public int UpdateFrom20()
        {
            ContentDefinitionManager.AlterTypeDefinition("FilterHeader", type => type
               .WithPart("FilterHeaderPart")
                // In order to turn this content type into a widget, it needs the WidgetPart
               .WithPart("WidgetPart")
                // It also needs a setting called "Stereotype" to be set to "Widget"
               .WithSetting("Stereotype", "Widget")
               .WithPart("CommonPart")
               );
            return 21;
        }

        #endregion

        #region 21 - 30

        public int UpdateFrom21()
        {
            ContentDefinitionManager.AlterTypeDefinition("PropertyListHighlightWidget", type => type

                // Attach the "ShoppingCartWidgetPart"
                .WithPart("PropertyListHighlightWidgetPart")

                // In order to turn this content type into a widget, it needs the WidgetPart
                .WithPart("WidgetPart")

                // It also needs a setting called "Stereotype" to be set to "Widget"
                .WithSetting("Stereotype", "Widget")

                .WithPart("CommonPart")
                );
            return 22;
        }

        public int UpdateFrom22()
        {
            ContentDefinitionManager.AlterTypeDefinition("FilerApartmentWidget", type => type

                // Attach the "FilterApartmentWidgetPart"
                .WithPart("FilterApartmentWidgetPart")

                // In order to turn this content type into a widget, it needs the WidgetPart
                .WithPart("WidgetPart")

                // It also needs a setting called "Stereotype" to be set to "Widget"
                .WithSetting("Stereotype", "Widget")

                .WithPart("CommonPart")
                );
            return 23;
        }

        public int UpdateFrom23()
        {
            ContentDefinitionManager.AlterTypeDefinition("PropertyLeasingNewWidget", type => type

                // Attach the "PropertyLeasingNewWidgetPart"
                .WithPart("PropertyLeasingNewWidgetPart")

                // In order to turn this content type into a widget, it needs the WidgetPart
                .WithPart("WidgetPart")

                // It also needs a setting called "Stereotype" to be set to "Widget"
                .WithSetting("Stereotype", "Widget")

                .WithPart("CommonPart")
                );
            return 24;
        }
        public int UpdateFrom24()
        {
            ContentDefinitionManager.AlterTypeDefinition("ApartmentHighlightsWidget", type => type

                // Attach the "PropertyLeasingNewWidgetPart"
                .WithPart("ApartmentHighlightsWidgetPart")

                // In order to turn this content type into a widget, it needs the WidgetPart
                .WithPart("WidgetPart")

                // It also needs a setting called "Stereotype" to be set to "Widget"
                .WithSetting("Stereotype", "Widget")

                .WithPart("CommonPart")
                );
            return 25;
        }

        public int UpdateFrom25()
        {
            //DropTable UserPropertyRecord
            SchemaBuilder.DropTable("UserPropertyRecord");

            return 26;
        }

        public int UpdateFrom26()
        {
            SchemaBuilder.AlterTable("AliasesMetaPartRecord", table => table.AddColumn("DomainGroupId", DbType.Int32));

            return 27;
        }

        public int UpdateFrom27()
        {
            ContentDefinitionManager.AlterTypeDefinition("PropertyNewIsAuctionWidget", type => type

                // Attach the "ShoppingCartWidgetPart"
                .WithPart("PropertyNewIsAuctionWidgetPart")

                // In order to turn this content type into a widget, it needs the WidgetPart
                .WithPart("WidgetPart")

                // It also needs a setting called "Stereotype" to be set to "Widget"
                .WithSetting("Stereotype", "Widget")

                .WithPart("CommonPart")
                );
            return 28;
        }
        public int UpdateFrom28()
        {
            ContentDefinitionManager.AlterTypeDefinition("PropertyGoodDealIsAuctionWidget", type => type

                // Attach the "ShoppingCartWidgetPart"
                .WithPart("PropertyGoodDealIsAuctionWidgetPart")

                // In order to turn this content type into a widget, it needs the WidgetPart
                .WithPart("WidgetPart")

                // It also needs a setting called "Stereotype" to be set to "Widget"
                .WithSetting("Stereotype", "Widget")

                .WithPart("CommonPart")
                );
            return 29;
        }
        public int UpdateFrom29()
        {
            ContentDefinitionManager.AlterTypeDefinition("PropertyListCompactWidget", type => type

                // Attach the "PropertyLeasingNewWidgetPart"
                .WithPart("PropertyListCompactWidgetPart")

                // In order to turn this content type into a widget, it needs the WidgetPart
                .WithPart("WidgetPart")

                // It also needs a setting called "Stereotype" to be set to "Widget"
                .WithSetting("Stereotype", "Widget")

                .WithPart("CommonPart")
                );
            return 30;
        }
        #endregion
    }
}