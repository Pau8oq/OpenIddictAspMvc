namespace Server.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initmigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OpenIddictApplications",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ClientId = c.String(maxLength: 100),
                        ClientSecret = c.String(),
                        ConcurrencyToken = c.String(maxLength: 50),
                        ConsentType = c.String(maxLength: 50),
                        DisplayName = c.String(),
                        DisplayNames = c.String(),
                        Permissions = c.String(),
                        PostLogoutRedirectUris = c.String(),
                        Properties = c.String(),
                        RedirectUris = c.String(),
                        Requirements = c.String(),
                        Type = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.ClientId, unique: true);
            
            CreateTable(
                "dbo.OpenIddictAuthorizations",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ConcurrencyToken = c.String(maxLength: 50),
                        CreationDate = c.DateTime(),
                        Properties = c.String(),
                        Scopes = c.String(),
                        Status = c.String(maxLength: 50),
                        Subject = c.String(maxLength: 400),
                        Type = c.String(maxLength: 50),
                        ApplicationId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.OpenIddictApplications", t => t.ApplicationId)
                .Index(t => t.ApplicationId);
            
            CreateTable(
                "dbo.OpenIddictTokens",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ConcurrencyToken = c.String(maxLength: 50),
                        CreationDate = c.DateTime(),
                        ExpirationDate = c.DateTime(),
                        Payload = c.String(),
                        Properties = c.String(),
                        RedemptionDate = c.DateTime(),
                        ReferenceId = c.String(maxLength: 100),
                        Status = c.String(maxLength: 50),
                        Subject = c.String(maxLength: 400),
                        Type = c.String(maxLength: 50),
                        AuthorizationId = c.String(maxLength: 128),
                        ApplicationId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.OpenIddictAuthorizations", t => t.AuthorizationId, cascadeDelete: true)
                .ForeignKey("dbo.OpenIddictApplications", t => t.ApplicationId)
                .Index(t => t.ReferenceId)
                .Index(t => t.AuthorizationId)
                .Index(t => t.ApplicationId);
            
            CreateTable(
                "dbo.OpenIddictScopes",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ConcurrencyToken = c.String(maxLength: 50),
                        Description = c.String(),
                        Descriptions = c.String(),
                        DisplayName = c.String(),
                        DisplayNames = c.String(),
                        Name = c.String(maxLength: 200),
                        Properties = c.String(),
                        Resources = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OpenIddictTokens", "ApplicationId", "dbo.OpenIddictApplications");
            DropForeignKey("dbo.OpenIddictAuthorizations", "ApplicationId", "dbo.OpenIddictApplications");
            DropForeignKey("dbo.OpenIddictTokens", "AuthorizationId", "dbo.OpenIddictAuthorizations");
            DropIndex("dbo.OpenIddictScopes", new[] { "Name" });
            DropIndex("dbo.OpenIddictTokens", new[] { "ApplicationId" });
            DropIndex("dbo.OpenIddictTokens", new[] { "AuthorizationId" });
            DropIndex("dbo.OpenIddictTokens", new[] { "ReferenceId" });
            DropIndex("dbo.OpenIddictAuthorizations", new[] { "ApplicationId" });
            DropIndex("dbo.OpenIddictApplications", new[] { "ClientId" });
            DropTable("dbo.OpenIddictScopes");
            DropTable("dbo.OpenIddictTokens");
            DropTable("dbo.OpenIddictAuthorizations");
            DropTable("dbo.OpenIddictApplications");
        }
    }
}
