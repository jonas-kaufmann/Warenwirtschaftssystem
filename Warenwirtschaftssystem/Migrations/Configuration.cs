namespace Warenwirtschaftssystem.Migrations
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<Warenwirtschaftssystem.Model.Db.DbModel>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        public Configuration(string connectionString, string provider)
        {
            AutomaticMigrationsEnabled = true;
            TargetDatabase = new DbConnectionInfo(connectionString, provider);
        }
    }
}
