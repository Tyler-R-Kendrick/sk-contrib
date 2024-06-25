using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SKHelpers.Plugins.EntityFrameworkCore;

public delegate object Deserialize(string entityType, string serializedEntity);

public class CRUDPlugin(DbContext dbContext, Deserialize deserialize)
{
    [KernelFunction]
    [Description("Reads the specified entity from the database.")]
    [return: Description("The entity that was read.")]
    public object Read(
        [Description("The entity type.")]
        string entityType,
        [Description("The primary key of the entity to read.")]
        string primaryKey)
    {
        var entityTypeModel = dbContext.Model.FindEntityType(entityType)
            ?? throw new ArgumentException($"Entity type '{entityType}' not found.");
        return dbContext.Find(entityTypeModel.ClrType, primaryKey)
            ?? throw new ArgumentException($"Entity '{entityType}' with primary key '{primaryKey}' not found.");
    }

    [KernelFunction]
    [Description("Creates and inserts a new entity into the database.")]
    [return: Description("The entity entry that was added.")]
    public EntityEntry Create(
        [Description("The entity type.")]
        string entityType,
        [Description("The serialized entity to insert.")]
        string serializedEntity)
    {
        _ = dbContext.Model.FindEntityType(entityType)
            ?? throw new ArgumentException($"Entity type '{entityType}' not found.");
        var entity = deserialize(entityType, serializedEntity);
        return dbContext.Add(entity);
    }

    [KernelFunction]
    [Description("Deletes the specified entity from the database.")]
    [return: Description("The entity entry that was removed.")]
    public EntityEntry Delete(
        [Description("The entity type.")]
        string entityType,
        [Description("The primary key of the entity to delete.")]
        string primaryKey)
    {
        var entityTypeModel = dbContext.Model.FindEntityType(entityType)
            ?? throw new ArgumentException($"Entity type '{entityType}' not found.");
        var entity = dbContext.Find(entityTypeModel.ClrType, primaryKey)
            ?? throw new ArgumentException($"Entity '{entityType}' with primary key '{primaryKey}' not found.");
        return dbContext.Remove(entity);
    }

    [KernelFunction]
    [Description("Updates the specified entity in the database.")]
    [return: Description("The entity entry that was updated.")]
    public EntityEntry Update(
        [Description("The entity type.")]
        string entityType,
        [Description("The serialized entity to update.")]
        string serializedEntity)
    {
        var entity = deserialize(entityType, serializedEntity);
        return dbContext.Update(entity);
    }

    [KernelFunction]
    [Description("Saves changes to the underlying database.")]
    [return: Description("The number of state entries written to the database.")]
    public int SaveChanges(
        [Description("Indicates whether all changes should be accepted on success.")]
        bool acceptAllChangesOnSuccess = true)
        => dbContext.SaveChanges(acceptAllChangesOnSuccess);
}
