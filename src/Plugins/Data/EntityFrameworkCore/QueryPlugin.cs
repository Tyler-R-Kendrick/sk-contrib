using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace SKHelpers.Plugins.EntityFrameworkCore;

public class QueryPlugin(DbContext dbContext, Func<string, IQueryable?> getQueryable)
{
    [KernelFunction]
    [Description("Queries the DbSet for entities of the specified type.")]
    [return: Description("An IQueryable of entities of the specified type.")]
    public IQueryable Get(
        [Description("The type of entity for a queryable set.")]
        string entityType)
    {
        var entityTypeModel = dbContext.Model.FindEntityType(entityType)
            ?? throw new ArgumentException($"Entity type '{entityType}' not found.");
        var clrType = entityTypeModel.ClrType;
        var set = typeof(DbContext).GetMethod(nameof(DbContext.Set), Type.EmptyTypes)
            ?.MakeGenericMethod(clrType)
            ?.Invoke(dbContext, null)
            ?? throw new InvalidOperationException($"Failed to get DbSet for entity type '{entityType}'.");
        return (IQueryable)set;
    }

    [KernelFunction]
    [Description("Filters the DbSet for entities of the specified type.")]
    [return: Description("A filtered IQueryable of entities of the specified type.")]
    public IQueryable Filter(
        [Description("The type of entity for a queryable set.")]
        string entityType,
        [Description("A predicate string to filter the entities.")]
        string predicate)
        => (getQueryable(entityType) ?? Get(entityType)).Where(predicate);

    [KernelFunction]
    [Description("Maps the DbSet for entities of the specified type.")]
    [return: Description("A projected IQueryable of entities of the specified type.")]
    public IQueryable Map(
        [Description("The type of entity for a queryable set.")]
        string entityType,
        [Description("A projection string to map the entities.")]
        string projection)
        => (getQueryable(entityType) ?? Get(entityType)).Select(projection);

    [KernelFunction]
    [Description("Orders the DbSet for entities of the specified type.")]
    [return: Description("An ordered IQueryable of entities of the specified type.")]
    public IQueryable OrderBy(
        [Description("The type of entity for a queryable set.")]
        string entityType,
        [Description("An order string to sort the entities.")]
        string order)
        => (getQueryable(entityType) ?? Get(entityType)).OrderBy(order);

    [KernelFunction]
    [Description("Reduces the DbSet for entities of the specified type.")]
    [return: Description("A reduced IQueryable of entities of the specified type.")]
    public object Reduce(
        [Description("The type of entity for a queryable set.")]
        string entityType,
        [Description("A reduction function to reduce the entities.")]
        string function,
        [Description("A reduction member to reduce the entities.")]
        string member)
        => (getQueryable(entityType) ?? Get(entityType)).Aggregate(function, member);

}
