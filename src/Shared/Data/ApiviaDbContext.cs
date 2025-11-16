using Apivia.Shared.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Apivia.Shared.Data;

/// <summary>
/// Main database context for Apivia
/// </summary>
public class ApiviaDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public ApiviaDbContext(DbContextOptions<ApiviaDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectTeam> ProjectTeams => Set<ProjectTeam>();
    public DbSet<ApiSpec> ApiSpecs => Set<ApiSpec>();
    public DbSet<DataDictionary> DataDictionaries => Set<DataDictionary>();
    public DbSet<DataEntity> DataEntities => Set<DataEntity>();
    public DbSet<DataAttribute> DataAttributes => Set<DataAttribute>();
    public DbSet<ApiSchemaElement> ApiSchemaElements => Set<ApiSchemaElement>();
    public DbSet<ImpactAnalysis> ImpactAnalyses => Set<ImpactAnalysis>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Proposal> Proposals => Set<Proposal>();
    public DbSet<MockServer> MockServers => Set<MockServer>();
    public DbSet<LintResult> LintResults => Set<LintResult>();
    public DbSet<Ruleset> Rulesets => Set<Ruleset>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Identity tables
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");

        // Configure User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.ModifiedAt).IsRequired();

            entity.HasMany(e => e.OwnedWorkspaces)
                .WithOne(w => w.Owner)
                .HasForeignKey(w => w.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.TeamMemberships)
                .WithOne(tm => tm.User)
                .HasForeignKey(tm => tm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.CreatedProjects)
                .WithOne(p => p.Creator)
                .HasForeignKey(p => p.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.CreatedApiSpecs)
                .WithOne(a => a.Creator)
                .HasForeignKey(a => a.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Comments)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Workspace
        modelBuilder.Entity<Workspace>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Name, e.OwnerId }).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.Owner)
                .WithMany(u => u.OwnedWorkspaces)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Projects)
                .WithOne(p => p.Workspace)
                .HasForeignKey(p => p.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Teams)
                .WithOne(t => t.Workspace)
                .HasForeignKey(t => t.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.DataDictionaries)
                .WithOne(d => d.Workspace)
                .HasForeignKey(d => d.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Team
        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.WorkspaceId, e.Name }).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();

            entity.HasMany(e => e.Members)
                .WithOne(tm => tm.Team)
                .HasForeignKey(tm => tm.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.ProjectTeams)
                .WithOne(pt => pt.Team)
                .HasForeignKey(pt => pt.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure TeamMember
        modelBuilder.Entity<TeamMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TeamId, e.UserId }).IsUnique();
            entity.Property(e => e.Role).IsRequired();
        });

        // Configure Project
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.WorkspaceId, e.Name }).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.Visibility).IsRequired();

            entity.HasMany(e => e.ApiSpecs)
                .WithOne(a => a.Project)
                .HasForeignKey(a => a.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.ProjectTeams)
                .WithOne(pt => pt.Project)
                .HasForeignKey(pt => pt.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Comments)
                .WithOne(c => c.Project)
                .HasForeignKey(c => c.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ProjectTeam
        modelBuilder.Entity<ProjectTeam>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ProjectId, e.TeamId }).IsUnique();
        });

        // Configure ApiSpec
        modelBuilder.Entity<ApiSpec>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ProjectId, e.Version }).IsUnique();
            entity.Property(e => e.Version).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Format).HasMaxLength(10).IsRequired();
            entity.Property(e => e.OpenApiVersion).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasMany(e => e.SchemaElements)
                .WithOne(s => s.ApiSpec)
                .HasForeignKey(s => s.ApiSpecId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.LintResults)
                .WithOne(l => l.ApiSpec)
                .HasForeignKey(l => l.ApiSpecId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.MockServers)
                .WithOne(m => m.ApiSpec)
                .HasForeignKey(m => m.ApiSpecId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure DataDictionary
        modelBuilder.Entity<DataDictionary>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.WorkspaceId, e.Name }).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();

            entity.HasMany(e => e.Entities)
                .WithOne(d => d.Dictionary)
                .HasForeignKey(d => d.DictionaryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure DataEntity
        modelBuilder.Entity<DataEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DictionaryId, e.Name }).IsUnique();
            entity.HasIndex(e => e.FunctionalDomain);
            entity.HasIndex(e => e.Sensitivity);
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Version).IsRequired();

            entity.HasMany(e => e.Attributes)
                .WithOne(a => a.DataEntity)
                .HasForeignKey(a => a.DataEntityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.LinkedSchemas)
                .WithOne(l => l.DataEntity)
                .HasForeignKey(l => l.DataEntityId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.ImpactAnalyses)
                .WithOne(i => i.DataEntity)
                .HasForeignKey(i => i.DataEntityId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure DataAttribute
        modelBuilder.Entity<DataAttribute>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DataEntityId, e.Name }).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.DataType).HasMaxLength(100).IsRequired();

            entity.HasMany(e => e.LinkedSchemas)
                .WithOne(l => l.DataAttribute)
                .HasForeignKey(l => l.DataAttributeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure ApiSchemaElement
        modelBuilder.Entity<ApiSchemaElement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ApiSpecId, e.SchemaPath }).IsUnique();
            entity.Property(e => e.SchemaPath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.LinkedAt).IsRequired();
        });

        // Configure ImpactAnalysis
        modelBuilder.Entity<ImpactAnalysis>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DataEntityId);
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.ChangeType).IsRequired();
            entity.Property(e => e.OverallRiskLevel).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // Configure Comment
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProjectId);
            entity.HasIndex(e => e.ApiSpecId);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(e => e.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Proposal
        modelBuilder.Entity<Proposal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProjectId);
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.Title).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Changes).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // Configure MockServer
        modelBuilder.Entity<MockServer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ApiSpecId);
            entity.Property(e => e.Port).IsRequired();
        });

        // Configure LintResult
        modelBuilder.Entity<LintResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ApiSpecId);
            entity.Property(e => e.IsValid).IsRequired();
            entity.Property(e => e.ExecutedAt).IsRequired();
        });

        // Configure AuditLog
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Action).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // Configure value conversions for enums (PostgreSQL compatibility)
        modelBuilder.Entity<TeamMember>()
            .Property(e => e.Role)
            .HasConversion<string>();

        modelBuilder.Entity<ProjectTeam>()
            .Property(e => e.Role)
            .HasConversion<string>();

        modelBuilder.Entity<Project>()
            .Property(e => e.Visibility)
            .HasConversion<string>();

        modelBuilder.Entity<ApiSpec>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<DataEntity>()
            .Property(e => e.Sensitivity)
            .HasConversion<string>();

        modelBuilder.Entity<DataEntity>()
            .Property(e => e.QualityLevel)
            .HasConversion<string>();

        modelBuilder.Entity<ImpactAnalysis>()
            .Property(e => e.ChangeType)
            .HasConversion<string>();

        modelBuilder.Entity<ImpactAnalysis>()
            .Property(e => e.OverallRiskLevel)
            .HasConversion<string>();

        modelBuilder.Entity<ImpactAnalysis>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Proposal>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<AuditLog>()
            .Property(e => e.Action)
            .HasConversion<string>();

        // Global query filters for soft delete
        modelBuilder.Entity<Workspace>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<Team>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<Project>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<DataEntity>().HasQueryFilter(e => e.IsActive);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.ModifiedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedAt = DateTime.UtcNow;
            }
        }
    }
}
