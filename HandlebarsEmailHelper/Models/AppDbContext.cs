using Microsoft.EntityFrameworkCore;

namespace HandlebarsEmailHelper.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
    public DbSet<Partial> Partials => Set<Partial>();
    public DbSet<EmailAttachment> EmailAttachments => Set<EmailAttachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Subject).IsRequired();
            entity.Property(e => e.HtmlBody).IsRequired();
        });

        modelBuilder.Entity<Partial>(entity =>
        {
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.HtmlContent).IsRequired();
        });

        modelBuilder.Entity<EmailAttachment>(entity =>
        {
            entity.Property(e => e.FileName).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.ContentType).IsRequired();
            
            // Configure relationship with EmailTemplate
            entity.HasOne<EmailTemplate>()
                  .WithMany()
                  .HasForeignKey(e => e.EmailTemplateId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}


