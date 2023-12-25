namespace Warenwirtschaftssystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsSupplierProportionFixed : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Articles", "IsSupplierProportionFixed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Articles", "IsSupplierProportionFixed");
        }
    }
}
