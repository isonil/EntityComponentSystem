using System;
using System.Collections.Generic;

namespace ECS
{

public abstract class System
{
    private Context context;
    private bool reassignCachedComponents;
    private List<Component> cachedComponents;

    public Context Context
    {
        get { return context; }
        internal set { context = value; }
    }

    internal bool ReassignCachedComponents
    {
        get { return reassignCachedComponents; }
        set { reassignCachedComponents = value; }
    }

    protected List<Component> CachedComponents
    {
        get
        {
            context.RecacheCachedComponentsForSystemIfNeeded(this);

            return cachedComponents;
        }
    }

    public abstract Type ComponentType { get; }

    // for some reason C# doesn't allow properties with protected get and internal set
    // so this must be a separate method
    internal void SetCachedComponents(List<Component> cachedComponents)
    {
        this.cachedComponents = cachedComponents;
    }

    protected Component GetComponentOfEntity(int entityID)
    {
        return context.GetFirstComponentOfTypeOfEntity(ComponentType, entityID);
    }

    protected void SendEvent(Event ev)
    {
        context.SendEvent(ev);
    }

    protected void SendEvent(int entityID, object data)
    {
        context.SendEvent(new Event(this, entityID, data));
    }

    public virtual void ReceiveEvent(Event ev)
    {
    }

    public abstract void Update(object updateData);
}
    
public abstract class System<T> : System
    where T : Component
{
    public override Type ComponentType { get { return TypesCache<T>.Type; } }

    protected new T GetComponentOfEntity(int entityID)
    {
        return Context.GetFirstComponentOfTypeOfEntity<T>(entityID);
    }

    public override void Update(object updateData)
    {
        var cachedComponents = CachedComponents;

        if( cachedComponents != null )
        {
            for( int i = 0, count = cachedComponents.Count; i < count; i++ )
            {
                Update((T)cachedComponents[i], updateData);
            }
        }
    }

    protected virtual void Update(T component, object updateData)
    {
    }
}

}
