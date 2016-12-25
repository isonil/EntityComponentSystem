using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* todo: reset Context and other fields when removing component or system
 */

namespace ECS
{
   
public sealed partial class Context
{
    private HashSet<int> entities = new HashSet<int>();
    private HashSet<System> systems = new HashSet<System>();
    private HashSet<Component> components = new HashSet<Component>();

    private Dictionary<int, HashSet<Component>> cachedComponentsByEntity = new Dictionary<int, HashSet<Component>>();
    private Dictionary<Type, HashSet<Component>> cachedComponentsOfType = new Dictionary<Type, HashSet<Component>>();

    private List<System> systems_fastIt = new List<System>();
    private bool systems_fastIt_dirty;

    private Dictionary<Type, List<Component>> cachedComponentsOfType_fastIt = new Dictionary<Type, List<Component>>();
    private bool cachedComponentsOfType_fastIt_dirty;

    private int nextEntityID;

    public IEnumerable<int> Entities { get { return entities; } }
    public IEnumerable<System> Systems { get { return systems; } }
    public IEnumerable<Component> Components { get { return components; } }

    public void Update()
    {
        if( systems_fastIt_dirty )
        {
            systems_fastIt_dirty = false;
            systems_fastIt.Clear();
            systems_fastIt.AddRange(systems);
        }

        if( cachedComponentsOfType_fastIt_dirty )
        {
            cachedComponentsOfType_fastIt_dirty = false;

            foreach( var kvp in cachedComponentsOfType_fastIt )
            {
                kvp.Value.Clear();
            }

            foreach( var kvp in cachedComponentsOfType )
            {
                List<Component> cachedComponents;

                if( !cachedComponentsOfType_fastIt.TryGetValue(kvp.Key, out cachedComponents) )
                {
                    cachedComponents = new List<Component>();
                    cachedComponentsOfType_fastIt.Add(kvp.Key, cachedComponents);
                }

                cachedComponents.AddRange(kvp.Value);
            }

            for( int i = 0, count = systems_fastIt.Count; i < count; ++i )
            {
                var system = systems_fastIt[i];

                List<Component> cachedComponents;

                if( !cachedComponentsOfType_fastIt.TryGetValue(system.ComponentType, out cachedComponents) )
                    cachedComponents = null;

                system.SetCachedComponents(cachedComponents);
            }
        }

        for( int i = 0, count = systems_fastIt.Count; i < count; ++i )
        {
            systems_fastIt[i].Update();
        }
    }

    public T AddSystem<T>()
        where T : System, new()
    {
        var newSystem = new T();
        AddSystem(newSystem);
        return newSystem;
    }

    public System AddSystem(Type type)
    {
        var newSystem = (System)Activator.CreateInstance(type);
        AddSystem(newSystem);
        return newSystem;
    }

    public bool RemoveSystem(System system)
    {
        if( systems.Remove(system) )
        {
            systems_fastIt_dirty = true;
            return true;
        }

        return false;
    }

    public int RemoveSystemsOfType<T>()
        where T : System
    {
        int removed = systems.RemoveWhere(SystemTypesCache<T>.IsT);

        if( removed != 0 )
            systems_fastIt_dirty = true;

        return removed;
    }

    public int RemoveSystemsOfType(Type type)
    {
        int removed = systems.RemoveWhere(x => x.GetType() == type);

        if( removed != 0 )
            systems_fastIt_dirty = true;

        return removed;
    }

    public T GetFirstSystemOfType<T>()
        where T : System
    {
        foreach( var s in systems )
        {
            var sAsT = s as T;

            if( sAsT != null )
                return sAsT;
        }

        return null;
    }

    public System GetFirstSystemOfType(Type type)
    {
        Type prevType = null;

        foreach( var s in systems )
        {
            var systemType = s.GetType();

            if( systemType == prevType )
                continue;

            if( type.IsAssignableFrom(systemType) )
                return s;

            prevType = systemType;
        }

        return null;
    }

    public System<T> GetFirstSystemWithComponentType<T>()
        where T : Component
    {
        foreach( var s in systems )
        {
            var sAsT = s as System<T>;

            if( sAsT != null )
                return sAsT;
        }

        return null;
    }

    public System GetFirstSystemWithComponentType(Type type)
    {
        var genericType = typeof(System<>).MakeGenericType(type);
        Type prevType = null;

        foreach( var s in systems )
        {
            var systemType = s.GetType();

            if( systemType == prevType )
                continue;

            if( genericType.IsAssignableFrom(systemType) )
                return s;

            prevType = systemType;
        }

        return null;
    }

    public int AddEntity()
    {
        int entityID = nextEntityID++;
        entities.Add(entityID);
        return entityID;
    }

    public bool RemoveEntity(int entityID)
    {
        if( entities.Remove(entityID) )
        {
            RemoveComponentsFromEntity(entityID);
            cachedComponentsByEntity.Remove(entityID);

            return true;
        }

        return false;
    }

    public IEnumerable<Component> GetComponentsOfType<T>()
        where T : Component
    {
        HashSet<Component> components = null;

        if( cachedComponentsOfType.TryGetValue(TypesCache<T>.Type, out components) )
            return components;

        return null;
    }

    public IEnumerable<Component> GetComponentsOfType(Type type)
    {
        HashSet<Component> components = null;

        if( cachedComponentsOfType.TryGetValue(type, out components) )
            return components;

        return null;
    }

    public IEnumerable<Component> GetComponentsOfEntity(int entityID)
    {
        HashSet<Component> components = null;

        if( cachedComponentsByEntity.TryGetValue(entityID, out components) )
            return components;

        return null;
    }

    public T GetFirstComponentOfTypeOfEntity<T>(int entityID)
        where T : Component
    {
        HashSet<Component> components = null;

        if( cachedComponentsByEntity.TryGetValue(entityID, out components) )
        {
            foreach( var c in cachedComponentsByEntity )
            {
                var cAsT = c as T;

                if( cAsT != null )
                    return cAsT;
            }
        }

        return null;
    }

    public Component GetFirstComponentOfTypeOfEntity(Type type, int entityID)
    {
        HashSet<Component> components = null;

        if( cachedComponentsByEntity.TryGetValue(entityID, out components) )
        {
            foreach( var c in components )
            {
                if( type.IsAssignableFrom(c.GetType()) )
                    return c;
            }
        }

        return null;
    }

    public T AddComponent<T>(int entityID)
        where T : Component, new()
    {
        var newComponent = new T();
        AddComponent(newComponent, entityID);
        return newComponent;
    }

    public Component AddComponent(int entityID, Type type)
    {
        var newComponent = (Component)Activator.CreateInstance(type);
        AddComponent(newComponent, entityID);
        return newComponent;
    }

    public int RemoveComponentsOfType<T>()
        where T : Component
    {
        return RemoveComponentsOfType(TypesCache<T>.Type);
    }

    public int RemoveComponentsOfType(Type type)
    {
        HashSet<Component> typeComponents;

        if( cachedComponentsOfType.TryGetValue(type, out typeComponents) && typeComponents.Any() )
        {
            int count = typeComponents.Count;

            HashSet<Component> byEntity = null;
            int byEntityForEntityID = -1;

            foreach( var c in typeComponents )
            {
                int entityID = c.EntityID;

                if( entityID != byEntityForEntityID )
                {
                    byEntity = cachedComponentsByEntity[entityID];
                    byEntityForEntityID = entityID;
                }

                byEntity.Remove(c);
                components.Remove(c);
            }

            typeComponents.Clear();
            cachedComponentsOfType_fastIt_dirty = true;

            return count;
        }

        return 0;
    }

    public int RemoveComponentsFromEntity(int entityID)
    {
        HashSet<Component> toRemove;

        if( cachedComponentsByEntity.TryGetValue(entityID, out toRemove) && toRemove.Any() )
        {
            int count = toRemove.Count;

            HashSet<Component> byType = null;
            Type byTypeForType = null;

            foreach( var c in toRemove )
            {
                var type = c.GetType();

                if( type != byTypeForType )
                {
                    byType = cachedComponentsOfType[type];
                    byTypeForType = type;
                }

                byType.Remove(c);
                components.Remove(c);
            }

            toRemove.Clear();
            cachedComponentsOfType_fastIt_dirty = true;

            return count;
        }

        return 0;
    }

    public int RemoveComponentsOfTypeFromEntity<T>(int entityID)
        where T : Component
    {
        return RemoveComponentsOfTypeFromEntity(entityID, TypesCache<T>.Type);
    }

    public int RemoveComponentsOfTypeFromEntity(int entityID, Type type)
    {
        HashSet<Component> toRemove;

        if( cachedComponentsByEntity.TryGetValue(entityID, out toRemove) && toRemove.Any() )
        {
            int count = toRemove.Count;

            HashSet<Component> byType = null;

            foreach( var c in toRemove )
            {
                if( c.GetType() != type )
                    continue;

                if( byType == null )
                    byType = cachedComponentsOfType[type];

                byType.Remove(c);
                components.Remove(c);
                cachedComponentsOfType_fastIt_dirty = true;
            }

            toRemove.Clear();

            return count;
        }

        return 0;
    }

    private void AddSystem(System newSystem)
    {
        newSystem.Context = this;
        systems.Add(newSystem);
        systems_fastIt_dirty = true;
    }

    private void AddComponent(Component newComponent, int entityID)
    {
        newComponent.Context = this;
        newComponent.EntityID = entityID;
        components.Add(newComponent);

        HashSet<Component> entityComponents;

        if( !cachedComponentsByEntity.TryGetValue(entityID, out entityComponents) )
        {
            entityComponents = new HashSet<Component>();
            cachedComponentsByEntity.Add(entityID, entityComponents);
        }

        entityComponents.Add(newComponent);

        HashSet<Component> typeComponents;
        var componentType = newComponent.GetType();

        if( !cachedComponentsOfType.TryGetValue(componentType, out typeComponents) )
        {
            typeComponents = new HashSet<Component>();
            cachedComponentsOfType.Add(componentType, typeComponents);
        }

        typeComponents.Add(newComponent);

        cachedComponentsOfType_fastIt_dirty = true;
    }
}

}
