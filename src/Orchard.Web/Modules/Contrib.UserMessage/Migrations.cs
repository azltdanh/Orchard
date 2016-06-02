using System;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace Contrib.UserMessage {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("UserMessagePartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("SentFrom", c => c.WithLength(128))
                    .Column<string>("SentTo", c => c.WithLength(128))
                    .Column<string>("Title", c => c.WithLength(256))
                    .Column<string>("Message", c => c.WithLength(500))
                    .Column<DateTime>("SentDateTime", c => c.WithDefault(DateTime.UtcNow))
                    .Column<bool>("SenderRemoved", c => c.WithDefault(false))
                    .Column<bool>("ReceiverRemoved", c => c.WithDefault(false))
                    .Column<bool>("ReceiverRead", c => c.WithDefault(false))
                    .Column<int>("PreviousMessage_Id")
                );

            ContentDefinitionManager.AlterTypeDefinition("UserMessage",
                cfg => cfg
                    .WithPart("UserMessagePart")
                );

            return 1;
        }
    }
}