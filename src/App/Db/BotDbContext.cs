namespace TelegramBot.App;

#region << Using >>

using CRUD.DAL.EntityFramework;
using CRUD.Logging.EntityFramework;
using Microsoft.EntityFrameworkCore;

#endregion

public class BotDbContext : DbContext, IEfDbContext
{
    #region Constructors

    public BotDbContext(DbContextOptions<BotDbContext> options)
            : base(options)
    {
        Database.EnsureCreated();
        Database.SetCommandTimeout(TimeSpan.FromSeconds(20));
    }

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LogMapping).Assembly);
    }
}