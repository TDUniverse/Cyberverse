using Cyberverse.Server.Types;

namespace Cyberverse.Server.Services;

/// <summary>
/// Responsible for keeping track of (owning) entities and sending packets related to them.
/// It does _not_ track the visibility or automatically spawn/despawn entities.
/// That happens via the EntityTracker, that could also have custom logic on which entities to even automatically spawn.
/// Also houses the networkedEntityId counter.
/// </summary>
public class EntityService
{
    // TODO: Make this private and provide API to get by Id and iterate over so that we can enforce using CreateEntity?
    public readonly Dictionary<ulong, Entity> SpawnedEntities = new();
    private ulong _entityIdCounter = 0u;
    
    public Entity CreateEntity(ulong recordId)
    {
        var entity = new Entity(_entityIdCounter++, recordId);
        SpawnedEntities.Add(entity.NetworkedEntityId, entity);
        return entity;
    }
    
    [Obsolete("Try to use recordIds as ulongs directly, where possible")]
    public Entity CreateEntity(string recordId)
    {
        return CreateEntity(RecordIdUtils.ToRecordId(recordId));
    }

    /// <summary>
    /// Remove an entity from the list of spawned entities (kill it)
    /// Note: this does _not_ send any packets. Use the entity tracker for that
    /// </summary>
    /// <param name="entityId">the id of the entity</param>
    /// <returns>true if the element is successfully found and removed; otherwise, false</returns>
    public bool RemoveEntity(ulong entityId)
    {
        return SpawnedEntities.Remove(entityId);
    }
}