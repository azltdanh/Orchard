using System.Data;
using Mello.ImageGallery.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Mello.ImageGallery {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("ImageGalleryRecord", table => table
                    .ContentPartRecord()
                    .Column("ImageGalleryName", DbType.String)
                    .Column("SelectedPlugin", DbType.Byte)
                );

	        ContentDefinitionManager.AlterPartDefinition(
		        typeof (ImageGalleryPart).Name, cfg => cfg.Attachable()
		                                                  .WithDescription(
			                                                  "Adds image gallery"));


            SchemaBuilder.CreateTable("ImageGallerySettingsRecord", table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("ImageGalleryName", column => column.WithLength(255))
                    .Column<int>("ThumbnailWidth")
                    .Column<int>("ThumbnailHeight")
                );


            SchemaBuilder.CreateTable("ImageGalleryImageSettingsRecord", table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name", column => column.WithLength(255))
                    .Column<string>("Caption", column => column.WithLength(255))
                    .Column<int>("ImageGallerySettingsRecord_Id")
                    .Column<string>("Title")
                    .Column("Position", DbType.Int32)
                );

            return 1;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("ImageGallerySettingsRecord", table => table
                .AddColumn<bool>("KeepAspectRatio")
            );

            ContentDefinitionManager.AlterTypeDefinition("ImageGalleryWidget", cfg => cfg
                .WithPart("ImageGalleryPart")
                .WithPart("WidgetPart")
                .WithPart("CommonPart")

                .WithSetting("Stereotype", "Widget"));

            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("ImageGalleryRecord", table => table
                .AddColumn<bool>("DisplayImageGallery")
            );

            return 3;
        }
        public int UpdateFrom3()
        {
            SchemaBuilder.AlterTable("ImageGalleryImageSettingsRecord", table => table
                .AddColumn<string>("href")
            );
            return 4;
        }
        public int UpdateFrom4()
        {
            SchemaBuilder.AlterTable("ImageGalleryImageSettingsRecord", table => table
               .AddColumn("DateBegin", DbType.DateTime)
            );
            SchemaBuilder.AlterTable("ImageGalleryImageSettingsRecord", table => table
               .AddColumn("DateEnd", DbType.DateTime)
            );
            return 5;
        }
        public int UpdateFrom5()
        {
            SchemaBuilder.AlterTable("ImageGalleryImageSettingsRecord", table => table
               .AddColumn("Enable", DbType.Boolean, c => c.WithDefault(true))
            );

            return 6;
        }
        public int UpdateFrom6()
        {
            SchemaBuilder.AlterTable("ImageGalleryImageSettingsRecord", table => table
               .AddColumn("Blank", DbType.Boolean, c => c.WithDefault(true))
            );

            return 7;
        }
        public int UpdateFrom7()
        {
            SchemaBuilder.AlterTable("ImageGallerySettingsRecord", table => table.AddColumn<bool>("ExpandToFill"));
            return 8;
        }
        public int UpdateFrom8()
        {
            SchemaBuilder.AlterTable("ImageGalleryImageSettingsRecord", table => table
               .AddColumn("IsPublish", DbType.Boolean, c => c.WithDefault(false))
            );
            return 9;
        }
    }
}