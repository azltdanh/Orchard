
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;

namespace RealEstate.MiniForum.FrontEnd
{
    public class Migration : DataMigrationImpl
    {
        public int Create()
        {
            ContentDefinitionManager.AlterTypeDefinition("ForumIsMarketWidget", type => type
                // Attach the "ForumIsMarketWidgetPart"
              .WithPart("ForumIsMarketWidgetPart")
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
            ContentDefinitionManager.AlterTypeDefinition("ForumIsHeighLightWidget", type => type
                // Attach the "ForumIsHeighLightWidgetPart"
               .WithPart("ForumIsHeighLightWidgetPart")
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
            ContentDefinitionManager.AlterTypeDefinition("ForumIsNewestWidget", type => type
                // Attach the "ForumIsNewestWidgetPart"
               .WithPart("ForumIsNewestWidgetPart")
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
            ContentDefinitionManager.AlterTypeDefinition("ForumByThreadWidget", type => type
               .WithPart("ForumByThreadWidgetPart")
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
            ContentDefinitionManager.AlterTypeDefinition("ForumFormFilterWidget", type => type
               .WithPart("ForumFormFilterWidgetPart")
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
            ContentDefinitionManager.AlterTypeDefinition("ForumPostOfUserWidget", type => type
               .WithPart("ForumPostOfUserWidgetPart")
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
            ContentDefinitionManager.AlterTypeDefinition("ForumIsLawHousingWidget", type => type
               .WithPart("ForumIsLawHousingWidgetPart")
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
            ContentDefinitionManager.AlterTypeDefinition("UserProfilePictureWidget", type => type
               .WithPart("UserProfilePictureWidgetPart")
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
            ContentDefinitionManager.AlterTypeDefinition("UserProfilePictureOrtherWidget", type => type
               .WithPart("UserProfilePictureOrtherWidgetPart")
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
            ContentDefinitionManager.AlterTypeDefinition("UserProfileSideMenuWidget", type => type
               .WithPart("UserProfileSideMenuWidgetPart")
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
            ContentDefinitionManager.AlterTypeDefinition("UserProfileSideMenuOrtherWidget", type => type
               .WithPart("UserProfileSideMenuOrtherWidgetPart")
                // In order to turn this content type into a widget, it needs the WidgetPart
               .WithPart("WidgetPart")
                // It also needs a setting called "Stereotype" to be set to "Widget"
               .WithSetting("Stereotype", "Widget")
               .WithPart("CommonPart")
               );
            return 11;
        }
        public int UpdateFrom11()
        {
            ContentDefinitionManager.AlterTypeDefinition("UserProfileAgencyWidget", type => type
               .WithPart("UserProfileAgencyWidgetPart")
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
            ContentDefinitionManager.AlterTypeDefinition("AgencyRealEstateDetailWidget", type => type
               .WithPart("AgencyRealEstateDetailWidgetPart")
                // In order to turn this content type into a widget, it needs the WidgetPart
               .WithPart("WidgetPart")
                // It also needs a setting called "Stereotype" to be set to "Widget"
               .WithSetting("Stereotype", "Widget")
               .WithPart("CommonPart")
               );
            return 13;
        }
        public int UpdateFrom13()
        {
            ContentDefinitionManager.AlterTypeDefinition("ForumMenuOfUserWidget", type => type
               .WithPart("ForumMenuOfUserWidgetPart")
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
            ContentDefinitionManager.AlterTypeDefinition("ForumIsNewsMainWidget", type => type
                // Attach the "ForumIsNewsMainWidget"
               .WithPart("ForumIsNewsMainWidgetPart")
                // In order to turn this content type into a widget, it needs the WidgetPart
               .WithPart("WidgetPart")
                // It also needs a setting called "Stereotype" to be set to "Widget"
               .WithSetting("Stereotype", "Widget")
               .WithPart("CommonPart")
               );
            return 15;
        }
        public int UpdateFrom15()
        {
            ContentDefinitionManager.AlterTypeDefinition("ForumIsPostInThreadWidget", type => type
                // Attach the "ForumIsPostInThreadWidget"
               .WithPart("ForumIsPostInThreadWidgetPart")
                // In order to turn this content type into a widget, it needs the WidgetPart
               .WithPart("WidgetPart")
                // It also needs a setting called "Stereotype" to be set to "Widget"
               .WithSetting("Stereotype", "Widget")
               .WithPart("CommonPart")
               );
            return 16;
        }
        public int UpdateFrom16()
        {
            ContentDefinitionManager.AlterTypeDefinition("MediaVideoWidget", type => type
               .WithPart("MediaVideoWidgetPart")
                // In order to turn this content type into a widget, it needs the WidgetPart
               .WithPart("WidgetPart")
                // It also needs a setting called "Stereotype" to be set to "Widget"
               .WithSetting("Stereotype", "Widget")
               .WithPart("CommonPart")
               );
            return 17;
        }
        public int UpdateFrom17()
        {
            ContentDefinitionManager.AlterTypeDefinition("ForumIsProjectWidget", type => type
               .WithPart("ForumIsProjectWidgetPart")
                // In order to turn this content type into a widget, it needs the WidgetPart
               .WithPart("WidgetPart")
                // It also needs a setting called "Stereotype" to be set to "Widget"
               .WithSetting("Stereotype", "Widget")
               .WithPart("CommonPart")
               );
            return 18;
        }
    }
}