using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS
{
 
/// <summary>
/// A container for all entities, Components, and Systems which should
/// interact with each other. In most cases you want to have only one instance
/// of this class.
/// </summary>
[Serializable]
public sealed class Context
{
    // main
    private readonly HashSet<int> entities = new HashSet<int>();
    private readonly HashSet<System> systems = new HashSet<System>();
    private readonly HashSet<Component> components = new HashSet<Component>();
    private int nextEntityID;

    // cached components
    [NonSerialized] private readonly Dictionary<int, HashSet<Component>> cachedComponentsByEntity
        = new Dictionary<int, HashSet<Component>>();

    [NonSerialized] private readonly Dictionary<Type, HashSet<Component>> cachedComponentsOfType
        = new Dictionary<Type, HashSet<Component>>();

    // systems - cached for fast iteration
    [NonSerialized] private readonly List<System> systems_fastIt = new List<System>();
    [NonSerialized] private bool systems_fastIt_dirty;

    // components - cached for fast iteration
    [NonSerialized] private readonly Dictionary<Type, List<Component>> cachedComponentsOfType_fastIt
        = new Dictionary<Type, List<Component>>();

    [NonSerialized] private bool cachedComponentsOfType_fastIt_dirty;

    /// <summary>
    /// Returns all entities associated with this Context.
    /// </summary>
    public IEnumerable<int> Entities { get { return entities; } }

    /// <summary>
    /// Returns all Systems associated with this Context.
    /// </summary>
    public IEnumerable<System> Systems { get { return systems; } }

    /// <summary>
    /// Returns all Components of all entities associated with this Context.
    /// </summary>
    public IEnumerable<Component> Components { get { return components; } }

    /// <summary>
    /// Calls update on every System.
    /// </summary>
    public void Update()
    {
        Update(0);
    }

    /// <summary>
    /// Calls update on every system.
    /// </summary>
    /// <param name="updateType">Extra parameter passed to all Systems.</param>
    public void Update(int updateType)
    {
        EnsureSystemsFastItNotDirty();

        for( int i = 0, count = systems_fastIt.Count; i < count; i++ )
        {
            systems_fastIt[i].Update(updateType);
        }
    }

    internal void RecacheCachedComponentsForSystemIfNeeded(System system)
    {
        EnsureCachedComponentsOfTypeFastItNotDirty();

        if( system.ReassignCachedComponents )
        {
            List<Component> cachedComponents;

            if( !cachedComponentsOfType_fastIt.TryGetValue(system.ComponentType, out cachedComponents) )
                cachedComponents = null;

            system.SetCachedComponents(cachedComponents);
            system.ReassignCachedComponents = false;
        }
    }

    //////////////////
    /// Add system ///
    //////////////////

    /// <summary>
    /// Creates a new System and adds it to this Context.
    /// </summary>
    /// <typeparam name="T">Type of the System to add.</typeparam>
    /// <returns>Created System instance.</returns>
    public T AddSystem<T>()
        where T : System, new()
    {
        var newSystem = new T();
        AddSystem(newSystem);
        return newSystem;
    }

    /// <summary>
    /// Creates a new System and adds it to this Context.
    /// </summary>
    /// <param name="type">Type of the System to add.</param>
    /// <returns>Created System instance.</returns>
    public System AddSystem(Type type)
    {
        if( type == null )
            throw new ArgumentNullException("type");

        var newSystem = (System)Activator.CreateInstance(type);
        AddSystem(newSystem);
        return newSystem;
    }

    /////////////////////
    /// Query systems ///
    /////////////////////

    /// <summary>
    /// Returns whether this Context contains the given System.
    /// </summary>
    /// <param name="system">System instance to check.</param>
    /// <returns>True if this Context contains this System.</returns>
    public bool ContainsSystem(System system)
    {
        if( system == null )
            return false;

        return systems.Contains(system);
    }

    /// <summary>
    /// Returns the first System instace of the given type.
    /// </summary>
    /// <typeparam name="T">Type of the System to check.</typeparam>
    /// <returns>Instance of the first System of the given type.</returns>
    public T GetFirstSystemOfType<T>()
        where T : System
    {
        EnsureSystemsFastItNotDirty();

        for( int i = 0, count = systems_fastIt.Count; i < count; i++ )
        {
            var sAsT = systems_fastIt[i] as T;

            if( sAsT != null )
                return sAsT;
        }

        return null;
    }

    /// <summary>
    /// Returns the first System instace of the given type.
    /// </summary>
    /// <param name="type">Type of the System to check.</param>
    /// <returns>Instance of the first System of the given type.</returns>
    public System GetFirstSystemOfType(Type type)
    {
        if( type == null )
            throw new ArgumentNullException("type");

        Type prevType = null;

        EnsureSystemsFastItNotDirty();

        for( int i = 0, count = systems_fastIt.Count; i < count; i++ )
        {
            var s = systems_fastIt[i];

            var systemType = s.GetType();

            if( systemType == prevType )
                continue;

            if( type.IsAssignableFrom(systemType) )
                return s;

            prevType = systemType;
        }

        return null;
    }

    /// <summary>
    /// Returns the first System which is responsible for handling Components
    /// of the given Component type.
    /// </summary>
    /// <typeparam name="T">Component type to check.</typeparam>
    /// <returns>The first System instance which handles Components of the given type.</returns>
    public System GetFirstSystemWithComponentType<T>()
        where T : Component
    {
        EnsureSystemsFastItNotDirty();

        for( int i = 0, count = systems_fastIt.Count; i < count; i++ )
        {
            var s = systems_fastIt[i];

            if( s.ComponentType is T )
                return s;
        }

        return null;
    }

    /// <summary>
    /// Returns the first System which is responsible for handling Components
    /// of the given Component type.
    /// </summary>
    /// <param name="type">Component type to check.</param>
    /// <returns>The first System instance which handles Components of the given type.</returns>
    public System GetFirstSystemWithComponentType(Type type)
    {
        if( type == null )
            throw new ArgumentNullException("type");

        Type prevComponentType = null;

        EnsureSystemsFastItNotDirty();

        for( int i = 0, count = systems_fastIt.Count; i < count; i++ )
        {
            var s = systems_fastIt[i];

            var componentType = s.ComponentType;

            if( componentType == prevComponentType )
                continue;

            if( type.IsAssignableFrom(componentType) )
                return s;

            prevComponentType = componentType;
        }

        return null;
    }

    /////////////////////
    /// Remove system ///
    /////////////////////

    /// <summary>
    /// Removes the given System from this Context.
    /// </summary>
    /// <param name="system">System to remove.</param>
    /// <returns>True if removed successfully.</returns>
    public bool RemoveSystem(System system)
    {
        if( system == null )
            return false;

        if( systems.Remove(system) )
        {
            systems_fastIt_dirty = true;

            system.Context = null;
            system.SetCachedComponents(null);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes all Systems of the given type.
    /// </summary>
    /// <typeparam name="T">System type to check.</typeparam>
    /// <returns>The number of Systems removed.</returns>
    public int RemoveSystemsOfType<T>()
        where T : System
    {
        EnsureSystemsFastItNotDirty();

        int countRemoved = 0;

        for( int i = 0, count = systems_fastIt.Count; i < count; i++ )
        {
            var s = systems_fastIt[i];

            if( s is T )
            {
                s.Context = null;
                s.SetCachedComponents(null);

                systems.Remove(s);
                systems_fastIt_dirty = true;
                countRemoved++;
            }
        }

        return countRemoved;
    }

    public int RemoveSystemsWithComponentType<T>()
        where T : Component
    {
        EnsureSystemsFastItNotDirty();

        int countRemoved = 0;

        for( int i = 0, count = systems_fastIt.Count; i < count; i++ )
        {
            var s = systems_fastIt[i];

            if( s.ComponentType is T )
            {
                s.Context = null;
                s.SetCachedComponents(null);

                systems.Remove(s);
                systems_fastIt_dirty = true;
                countRemoved++;
            }
        }

        return countRemoved;
    }

    public int RemoveSystemsOfType(Type type)
    {
        if( type == null )
            throw new ArgumentNullException("type");

        Type prevSystemType = null;
        bool prevSystemTypeResult = false;
        int countRemoved = 0;

        EnsureSystemsFastItNotDirty();

        for( int i = 0, count = systems_fastIt.Count; i < count; i++ )
        {
            var s = systems_fastIt[i];

            var systemType = s.GetType();

            if( systemType != prevSystemType )
            {
                prevSystemType = systemType;
                prevSystemTypeResult = type.IsAssignableFrom(systemType);
            }

            if( !prevSystemTypeResult )
                continue;

            s.Context = null;
            s.SetCachedComponents(null);

            systems.Remove(s);
            systems_fastIt_dirty = true;
            countRemoved++;
        }

        return countRemoved;
    }

    public int RemoveSystemsWithComponentType(Type type)
    {
        if( type == null )
            throw new ArgumentNullException("type");

        Type prevComponentType = null;
        bool prevComponentTypeResult = false;
        int countRemoved = 0;

        EnsureSystemsFastItNotDirty();

        for( int i = 0, count = systems_fastIt.Count; i < count; i++ )
        {
            var s = systems_fastIt[i];

            var componentType = s.ComponentType;

            if( componentType != prevComponentType )
            {
                prevComponentType = componentType;
                prevComponentTypeResult = type.IsAssignableFrom(componentType);
            }

            if( !prevComponentTypeResult )
                continue;

            s.Context = null;
            s.SetCachedComponents(null);

            systems.Remove(s);
            systems_fastIt_dirty = true;
            countRemoved++;
        }

        return countRemoved;
    }

    //////////////////
    /// Add entity ///
    //////////////////

    public int AddEntity()
    {
        int entityID = nextEntityID++;
        entities.Add(entityID);
        return entityID;
    }

    //////////////////////
    /// Query entities ///
    //////////////////////

    public bool ContainsEntity(int entityID)
    {
        return entities.Contains(entityID);
    }

    /////////////////////
    /// Remove entity ///
    /////////////////////

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

    /////////////////////
    /// Add component ///
    /////////////////////

    public T AddComponent<T>(int entityID)
        where T : Component, new()
    {
        if( !ContainsEntity(entityID) )
            throw new InvalidOperationException("Entity with ID " + entityID + " does not exist.");

        var newComponent = new T();
        AddComponent(newComponent, entityID);
        return newComponent;
    }

    public Component AddComponent(int entityID, Type type)
    {
        if( !ContainsEntity(entityID) )
            throw new InvalidOperationException("Entity with ID " + entityID + " does not exist.");

        if( type == null )
            throw new ArgumentNullException("type");

        var newComponent = (Component)Activator.CreateInstance(type);
        AddComponent(newComponent, entityID);
        return newComponent;
    }

    ////////////////////////
    /// Query components ///
    ////////////////////////

    public bool ContainsComponent(Component component)
    {
        if( component == null )
            return false;

        return components.Contains(component);
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
        if( type == null )
            throw new ArgumentNullException("type");

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
            foreach( var c in components )
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
        if( type == null )
            throw new ArgumentNullException("type");

        HashSet<Component> components = null;

        if( cachedComponentsByEntity.TryGetValue(entityID, out components) )
        {
            Type prevComponentType = null;

            foreach( var c in components )
            {
                var componentType = c.GetType();

                if( componentType == prevComponentType )
                    continue;

                if( type.IsAssignableFrom(componentType) )
                    return c;

                prevComponentType = componentType;
            }
        }

        return null;
    }

    ////////////////////////
    /// Remove component ///
    ////////////////////////

    public bool RemoveComponent(Component component)
    {
        if( component == null )
            return false;

        if( components.Remove(component) )
        {
            component.Context = null;
            component.EntityID = -1;

            cachedComponentsOfType[component.GetType()].Remove(component);
            cachedComponentsByEntity[component.EntityID].Remove(component);

            cachedComponentsOfType_fastIt_dirty = true;

            return true;
        }

        return false;
    }

    public int RemoveComponentsOfType<T>()
        where T : Component
    {
        return RemoveComponentsOfType(TypesCache<T>.Type);
    }

    public int RemoveComponentsOfType(Type type)
    {
        if( type == null )
            throw new ArgumentNullException("type");

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

                c.Context = null;
                c.EntityID = -1;

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

                c.Context = null;
                c.EntityID = -1;

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
        if( type == null )
            throw new ArgumentNullException("type");

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

                c.Context = null;
                c.EntityID = -1;

                byType.Remove(c);
                components.Remove(c);
                cachedComponentsOfType_fastIt_dirty = true;
            }

            toRemove.Clear();

            return count;
        }

        return 0;
    }

    //////////////
    /// Events ///
    //////////////

    public void SendEvent(Event ev)
    {
        EnsureSystemsFastItNotDirty();

        for( int i = 0, count = systems_fastIt.Count; i < count; i++ )
        {
            systems_fastIt[i].ReceiveEvent(ev);
        }
    }

    ///////////////
    /// Private ///
    ///////////////

    private void EnsureSystemsFastItNotDirty()
    {
        if( systems_fastIt_dirty )
        {
            systems_fastIt.Clear();
            systems_fastIt.AddRange(systems);
            systems_fastIt_dirty = false;
        }
    }

    private void EnsureCachedComponentsOfTypeFastItNotDirty()
    {
        if( cachedComponentsOfType_fastIt_dirty )
        {
            // clear cached lists
            foreach( var kvp in cachedComponentsOfType_fastIt )
            {
                kvp.Value.Clear();
            }

            // fill cached lists
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

            // notify systems
            for( int i = 0, count = systems_fastIt.Count; i < count; i++ )
            {
                var s = systems_fastIt[i];

                s.SetCachedComponents(null);
                s.ReassignCachedComponents = true;
            }

            cachedComponentsOfType_fastIt_dirty = false;
        }
    }

    private void AddSystem(System newSystem)
    {
        if( newSystem == null )
            throw new ArgumentNullException("newSystem");

        try
        {
            newSystem.Context = this;
            systems.Add(newSystem);
            systems_fastIt_dirty = true;
        }
        catch
        {
            newSystem.Context = null;
            throw;
        }
    }

    private void AddComponent(Component newComponent, int entityID)
    {
        if( newComponent == null )
            throw new ArgumentNullException("newComponent");

        try
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
        catch
        {
            newComponent.Context = null;
            newComponent.EntityID = -1;

            components.Remove(newComponent);
            
            HashSet<Component> entityComponents;

            if( cachedComponentsByEntity.TryGetValue(entityID, out entityComponents) )
                entityComponents.Remove(newComponent);

            HashSet<Component> typeComponents;

            if( cachedComponentsOfType.TryGetValue(newComponent.GetType(), out typeComponents) )
                typeComponents.Remove(newComponent);

            throw;
        }
    }
}

}
