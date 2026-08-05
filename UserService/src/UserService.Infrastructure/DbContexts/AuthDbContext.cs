using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserService.Domain;
using UserService.Domain.User;

namespace UserService.Infrastructure.DbContexts;

public class AuthDbContext(IConfiguration configuration) : IdentityDbContext<User, Role, Guid>
{
    public DbSet<RefreshSession> RefreshSessions => Set<RefreshSession>();
    public DbSet<ProcessedEvent>  ProcessedEvents => Set<ProcessedEvent>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Permission> Permissions => Set<Permission>();
    
    public DbSet<AdminAccount> AdminAccounts => Set<AdminAccount>();
    public DbSet<ParticipantAccount> ParticipantAccounts => Set<ParticipantAccount>();
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("Database"));
        optionsBuilder.UseSnakeCaseNamingConvention();
        optionsBuilder.UseLoggerFactory(CreateLoggerFactory());
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<Role>().ToTable("roles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("user_roles");
        
        modelBuilder.Entity<User>()
            .HasMany(u => u.Roles)
            .WithMany()
            .UsingEntity<IdentityUserRole<Guid>>();
        
        modelBuilder.Entity<AdminAccount>().ToTable("admin_accounts");
        modelBuilder.Entity<AdminAccount>().HasOne(u => u.User)
            .WithOne(a => a.AdminAccount)
            .HasForeignKey<AdminAccount>(i => i.UserId);
        
        modelBuilder.Entity<ParticipantAccount>().ToTable("participant_accounts");
        modelBuilder.Entity<ParticipantAccount>().HasOne(u => u.User)
            .WithOne(p => p.ParticipantAccount)
            .HasForeignKey<ParticipantAccount>(i => i.UserId);
        
        modelBuilder.Entity<Permission>().ToTable("permissions");
        modelBuilder.Entity<Permission>().HasIndex(c => c.Code).IsUnique();
        modelBuilder.Entity<Permission>().Property(d => d.Description).HasMaxLength(200);
        
        modelBuilder.Entity<RolePermission>().ToTable("role_permissions");
        modelBuilder.Entity<RolePermission>()
            .HasKey(rp => new { rp.RoleId, rp.PermissionId });

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId);

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany()
            .HasForeignKey(rp => rp.PermissionId);

        modelBuilder.Entity<User>().ComplexProperty(u => u.UserAccess, ub =>
        {
            ub.Property(t => t.TokenBalance).IsRequired();
            ub.Property(se => se.SubscriptionExpireAt).IsRequired(false);
            ub.Ignore(s => s.IsSubscribed);
        });
    }
    
    private ILoggerFactory CreateLoggerFactory() =>
        LoggerFactory.Create(b => b.AddConsole());
}