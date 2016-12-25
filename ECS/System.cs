using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS
{

public abstract class System
{
    private Context context;
    private List<Component> cachedComponents;

    public Context Context
    {
        get { return context; }
        internal set { context = value; }
    }

    protected List<Component> CachedComponents { get { return cachedComponents; } }

    public abstract Type ComponentType { get; }

    // for some reason C# doesn't allow properties with protected get and internal set
    // so this must be a separate method
    internal void SetCachedComponents(List<Component> cachedComponents)
    {
        this.cachedComponents = cachedComponents;
    }

    protected Component GetComponentOfEntity(int entityID)
    {
        var components = context.GetComponentsOfEntity(entityID);

        if( components == null )
            return null;

        var componentType = ComponentType;

        foreach( var c in components )
        {
            if( componentType == c.GetType() )
                return c;
        }

        return null;
    }

    protected void SendEvent(Event ev)
    {
        context.SendEvent(ev);
    }

    protected void SendEvent(int kind, int entityID, object data)
    {
        context.SendEvent(new Event(kind, this, entityID, data));
    }

    public virtual void ReceiveEvent(Event ev)
    {
    }

    public abstract void Update();
}
    
public abstract class System<T> : System
    where T : Component
{
    public override Type ComponentType { get { return TypesCache<T>.Type; } }

    protected new T GetComponentOfEntity(int entityID)
    {
        var components = Context.GetComponentsOfEntity(entityID);

        if( components == null )
            return null;

        foreach( var c in components )
        {
            var cAsT = c as T;

            if( cAsT != null )
                return cAsT;
        }

        return null;
    }

    public override void Update()
    {
        var cachedComponents = CachedComponents;

        if( cachedComponents != null )
        {
            for( int i = 0, count = cachedComponents.Count; i < count; ++i )
            {
                Update((T)cachedComponents[i]);
            }
        }
    }

    protected virtual void Update(T component)
    {
    }
}

}
