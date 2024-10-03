using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace SKHelpers.Plugins.EntityFrameworkCore;

public class ModelPlugin(DbContext dbContext)
{
    [KernelFunction]
    [Description("Gets the entity types in the model.")]
    [return: Description("An array of entity types in the model.")]
    public string[] GetEntityTypes()
        => dbContext.Model.GetEntityTypes().Select(e => e.Name).ToArray();

    [KernelFunction]
    [Description("Gets the properties of the specified entity type.")]
    [return: Description("An array of properties of the specified entity type.")]
    public string[] GetProperties(
        [Description("The entity type.")]
        string entityType)
        => dbContext.Model.FindEntityType(entityType)
            ?.GetProperties().Select(p => p.Name).ToArray()
            ?? throw new ArgumentException($"Entity type '{entityType}' not found.");

    [KernelFunction]
    [Description("Gets the navigation properties of the specified entity type.")]
    [return: Description("An array of navigation properties of the specified entity type.")]
    public string[] GetNavigations(
        [Description("The entity type.")]
        string entityType)
        => dbContext.Model.FindEntityType(entityType)
            ?.GetNavigations().Select(n => n.Name).ToArray()
            ?? throw new ArgumentException($"Entity type '{entityType}' not found.");

    [KernelFunction]
    [Description("Gets the primary key properties of the specified entity type.")]
    [return: Description("An array of primary key properties of the specified entity type.")]
    public string[] GetPrimaryKeys(
        [Description("The entity type.")]
        string entityType)
        => dbContext.Model.FindEntityType(entityType)
            ?.FindPrimaryKey()?.Properties.Select(p => p.Name).ToArray()
            ?? throw new ArgumentException($"Entity type '{entityType}' not found.");

}
