using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace SKHelpers.Plugins.EntityFrameworkCore;

public class DatabasePlugin(DbContext dbContext)
{
    [KernelFunction]
    [Description("Determines whether the database can be connected.")]
    [return: Description("True if the database can be connected; otherwise, false.")]
    public bool CanConnect() => dbContext.Database.CanConnect();

    [KernelFunction]
    [Description("Gets or Sets auto save for the database.")]
    [return: Description("The current auto save setting.")]
    public bool AutoSavepointsEnabled(
        [Description("The new auto save setting.")]
        bool? enabled = null)
    {
        if (enabled.HasValue)
        {
            dbContext.Database.AutoSavepointsEnabled = enabled.Value;
        }
        return dbContext.Database.AutoSavepointsEnabled;
    }

    [KernelFunction]
    [Description("Gets or Sets auto transactions for the database.")]
    [return: Description("The current auto transactions setting.")]
    public AutoTransactionBehavior AutoTransactionsBehavior(
        [Description("The new auto transactions behavior.")]
        AutoTransactionBehavior? behavior = null)
    {
        if (behavior.HasValue)
        {
            dbContext.Database.AutoTransactionBehavior = behavior.Value;
        }
        return dbContext.Database.AutoTransactionBehavior;
    }
}