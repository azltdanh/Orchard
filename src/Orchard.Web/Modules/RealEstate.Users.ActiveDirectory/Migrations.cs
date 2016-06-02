using System;
using Orchard.Data;
using Orchard.Data.Migration;
using RealEstate.Users.ActiveDirectory.Models;

namespace RealEstate.Users.ActiveDirectory
{
	public class Migrations : DataMigrationImpl
	{
        private readonly IRepository<SettingRecord> _repository;

        public Migrations(IRepository<SettingRecord> repository)
        {
            _repository = repository;
        }

		public int Create()
		{
			SchemaBuilder.CreateTable("DomainRecord", table => table
					.Column<int>("Id", column=>column.PrimaryKey().Identity())
					.Column<string>("Name", column => column.PrimaryKey().NotNull())
					.Column<string>("UserName")
					.Column<string>("Password")
				);

			return 1;
		}

        public int UpdateFrom1()
        {

            SchemaBuilder.CreateTable("SettingRecord", table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("DefaultDomain")
                );

            if (_repository == null)
                throw new InvalidOperationException("Couldn't find settings repository.");

            _repository.Create(new SettingRecord
            {
                DefaultDomain = null
            });

            return 2;
        }
	}
}