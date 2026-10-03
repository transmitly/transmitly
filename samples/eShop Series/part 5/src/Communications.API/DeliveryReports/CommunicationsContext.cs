using Microsoft.EntityFrameworkCore.Design;

namespace eShop.Communications.API.DeliveryReports;

/// <summary>
/// Communications' own database: what was sent, to whom, and what providers reported back.
/// </summary>
public sealed class CommunicationsContext(DbContextOptions<CommunicationsContext> options) : DbContext(options)
{
    public DbSet<CommunicationRecord> Communications => Set<CommunicationRecord>();

    public DbSet<DeliveryEventRecord> DeliveryEvents => Set<DeliveryEventRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CommunicationRecord>(communication =>
        {
            communication.ToTable("Communications");
            communication.Property(x => x.PipelineIntent).HasMaxLength(200);
            communication.Property(x => x.ChannelId).HasMaxLength(100);
            communication.Property(x => x.ChannelProviderId).HasMaxLength(200);
            communication.Property(x => x.ResourceId).HasMaxLength(200);
            communication.Property(x => x.RecipientId).HasMaxLength(200);
            communication.Property(x => x.Summary).HasMaxLength(500);
            communication.Property(x => x.Status).HasMaxLength(200);

            // Later reports are matched by the provider's message id.
            communication.HasIndex(x => new { x.ChannelId, x.ResourceId });
            // The inbox lists a recipient's communications, newest first.
            communication.HasIndex(x => new { x.RecipientId, x.CreatedAt });

            communication.HasMany(x => x.Events)
                .WithOne()
                .HasForeignKey(x => x.CommunicationRecordId);
        });

        modelBuilder.Entity<DeliveryEventRecord>(deliveryEvent =>
        {
            deliveryEvent.ToTable("DeliveryEvents");
            deliveryEvent.Property(x => x.EventName).HasMaxLength(200);
            deliveryEvent.Property(x => x.ChannelProviderId).HasMaxLength(200);
            deliveryEvent.Property(x => x.Status).HasMaxLength(200);
        });
    }
}

/// <summary>
/// Lets <c>dotnet ef</c> create migrations without starting the service and its dependencies.
/// </summary>
internal sealed class CommunicationsContextDesignFactory : IDesignTimeDbContextFactory<CommunicationsContext>
{
    public CommunicationsContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<CommunicationsContext>()
            .UseNpgsql("Host=localhost;Database=communicationsdb")
            .Options);
}
