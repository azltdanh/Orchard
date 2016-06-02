using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

using RealEstate.Models;
using RealEstate.Services;
using Orchard;
using Orchard.Services;
using Orchard.ContentManagement;
using Orchard.Security;
using System.Transactions;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.UI.Notify;
using System.Web;
using System.Net;
using System.IO;

namespace RealEstate.Estimation {
    public class Migrations : DataMigrationImpl
    {

        public Migrations(IOrchardServices services)
        {
            Services = services;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public int Create()
        {
            // Creating table EstimateRecord
            SchemaBuilder.CreateTable("EstimateRecord", table => table
                .Column("Id", DbType.Int32, column => column.PrimaryKey().Identity())
                .Column("StartTime", DbType.DateTime)
                .Column("EndTime", DbType.DateTime)
                .Column("TotalItems", DbType.Int32)
                .Column("SucessItems", DbType.Int32)
                .Column("Msg", DbType.String)
            );

            return 1;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.AlterTable("EstimateRecord", table => table.AddColumn("ErrorMsg", DbType.String));

            return 2;
        }

        public int UpdateFrom2()
        {
            SchemaBuilder.AlterTable("EstimateRecord", table => table.AddColumn("StartIndex", DbType.Int32));
            SchemaBuilder.AlterTable("EstimateRecord", table => table.AddColumn("EndIndex", DbType.Int32));

            return 3;
        }
    }
}
