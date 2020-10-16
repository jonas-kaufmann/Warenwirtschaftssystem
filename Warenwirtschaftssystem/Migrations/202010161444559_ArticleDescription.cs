namespace Warenwirtschaftssystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ArticleDescription : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Articles", "Description", c => c.String());
            AlterColumn("dbo.Reservations", "From", c => c.DateTime());
            AlterColumn("dbo.Reservations", "Until", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Reservations", "Until", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Reservations", "From", c => c.DateTime(nullable: false));
            DropColumn("dbo.Articles", "Description");
        }
    }
}
