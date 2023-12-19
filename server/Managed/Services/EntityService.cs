using CyberM.Server.Types;

namespace CyberM.Server.Services;

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
    
    public Entity CreateEntity(string recordId)
    {
        var entity = new Entity(_entityIdCounter++, recordId);
        SpawnedEntities.Add(entity.NetworkedEntityId, entity);
        return entity;
    }
}