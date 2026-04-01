using Microsoft.EntityFrameworkCore;
using SLT.Core.Entities;

namespace SLT.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<LearningEntry> LearningEntries => Set<LearningEntry>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Flashcard> Flashcards => Set<Flashcard>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<CollectionEntry> CollectionEntries => Set<CollectionEntry>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<TeamSpace> TeamSpaces => Set<TeamSpace>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<TeamEntry> TeamEntries => Set<TeamEntry>();
    public DbSet<EntryComment> EntryComments => Set<EntryComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).IsRequired().HasMaxLength(255);
            e.Property(u => u.FullName).IsRequired().HasMaxLength(100);
        });

        // LearningEntry
        modelBuilder.Entity<LearningEntry>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.Url).IsRequired().HasMaxLength(2048);
            e.Property(l => l.Title).IsRequired().HasMaxLength(500);
            e.Property(l => l.Author).HasMaxLength(200);
            e.HasOne(l => l.User)
             .WithMany(u => u.LearningEntries)
             .HasForeignKey(l => l.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            // Many-to-many with Tags
            e.HasMany(l => l.Tags)
             .WithMany(t => t.LearningEntries)
             .UsingEntity(j => j.ToTable("LearningEntryTags"));
        });

        // Tag
        modelBuilder.Entity<Tag>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Name).IsRequired().HasMaxLength(50);
            e.HasOne(t => t.User)
             .WithMany()
             .HasForeignKey(t => t.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        

        modelBuilder.Entity<Flashcard>(e =>
        {
            e.HasKey(f => f.Id);

            e.Property(f => f.Question).IsRequired().HasMaxLength(1000);

            e.Property(f => f.Answer).IsRequired().HasMaxLength(2000);

            e.HasOne(f => f.LearningEntry)

            .WithMany()

            .HasForeignKey(f => f.LearningEntryId)

            .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(f => f.User)

            .WithMany()

            .HasForeignKey(f => f.UserId)

            .OnDelete(DeleteBehavior.Restrict);

        });

        modelBuilder.Entity<Collection>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(100);
            e.Property(c => c.Description).HasMaxLength(500);
            e.Property(c => c.Emoji).HasMaxLength(10);
            e.HasIndex(c => c.ShareSlug).IsUnique().HasFilter("\"ShareSlug\" IS NOT NULL");
            e.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CollectionEntry>(e =>
        {
            e.HasKey(ce => ce.Id);
            e.HasOne(ce => ce.Collection)
            .WithMany(c => c.CollectionEntries)
            .HasForeignKey(ce => ce.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ce => ce.LearningEntry)
            .WithMany()
            .HasForeignKey(ce => ce.LearningEntryId)
            .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(ce => new { ce.CollectionId, ce.LearningEntryId }).IsUnique();
        });

        modelBuilder.Entity<Quote>(e =>
        {
            e.HasKey(q => q.Id);
            e.Property(q => q.Text).IsRequired().HasMaxLength(2000);
            e.Property(q => q.Note).HasMaxLength(500);
            e.Property(q => q.Color).HasMaxLength(20);
            e.HasOne(q => q.LearningEntry)
            .WithMany()
            .HasForeignKey(q => q.LearningEntryId)
            .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(q => q.User)
            .WithMany()
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TeamSpace>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Name).IsRequired().HasMaxLength(100);
            e.Property(t => t.InviteCode).IsRequired().HasMaxLength(20);
            e.HasIndex(t => t.InviteCode).IsUnique();
            e.HasOne(t => t.Owner).WithMany().HasForeignKey(t => t.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TeamMember>(e =>
        {
            e.HasKey(m => m.Id);
            e.HasIndex(m => new { m.TeamSpaceId, m.UserId }).IsUnique();
            e.HasOne(m => m.TeamSpace).WithMany(t => t.Members)
            .HasForeignKey(m => m.TeamSpaceId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.User).WithMany()
            .HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TeamEntry>(e =>
        {
            e.HasKey(te => te.Id);
            e.HasOne(te => te.TeamSpace).WithMany(t => t.SharedEntries)
            .HasForeignKey(te => te.TeamSpaceId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(te => te.LearningEntry).WithMany()
            .HasForeignKey(te => te.LearningEntryId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(te => te.SharedByUser).WithMany()
            .HasForeignKey(te => te.SharedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EntryComment>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Text).IsRequired().HasMaxLength(1000);
            e.HasOne(c => c.TeamEntry).WithMany(te => te.Comments)
            .HasForeignKey(c => c.TeamEntryId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(c => c.User).WithMany()
            .HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}