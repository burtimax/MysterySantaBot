using BotFramework.Db;
using BotFramework.Db.Entity;
using BotFramework.Options;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MysterySantaBot.Database.Entities;

namespace MysterySantaBot.Database;

public class SantaBotDbContext : DbContext
{
    private static string schema = "app";
    private readonly string _connection; 
    
    public DbSet<UserForm> UsersForm { get; set; }
    public DbSet<UserChoice> UserChoices { get; set; }
    public DbSet<ShowHistory> ShowHistory { get; set; }
    public DbSet<LetterMark> LetterMarks { get; set; }
    public DbSet<ModeratorLetterQueue> ModeratorLetterQueue { get; set; }

    public SantaBotDbContext(IOptions<BotConfiguration> options) : this(options.Value.DbConnection)
    {
    }
    
    public SantaBotDbContext(string connection)
    {
        _connection = connection;
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserForm>().ToTable("user_form", schema);
        modelBuilder.Entity<UserChoice>().ToTable("user_choice", schema);
        modelBuilder.Entity<ShowHistory>().ToTable("show_history", schema);
        modelBuilder.Entity<LetterMark>().ToTable("letter_mark", schema);
        modelBuilder.Entity<ModeratorLetterQueue>().ToTable("moderator_letter_queue", schema);
        BotDbContextConfiguration.SetBaseConfiguration(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connection);
        base.OnConfiguring(optionsBuilder);
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var e in
                 ChangeTracker.Entries<BaseBotEntity<long>>())
        {
            switch (e.State)
            {
                case EntityState.Added:
                    e.Entity.CreatedAt = DateTimeOffset.Now;
                    break;
                case EntityState.Modified:
                    e.Entity.UpdatedAt = DateTimeOffset.Now;
                    break;
                case EntityState.Deleted:
                    e.Entity.DeletedAt = DateTimeOffset.Now;
                    e.State = EntityState.Modified;
                    break;
            }
        }

        return base.SaveChangesAsync(ct);
    }
}