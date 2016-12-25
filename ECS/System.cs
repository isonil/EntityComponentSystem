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

    public abstract void Update();

    public virtual void Init()
    {
    }
}
    
public abstract class System<T> : System
    where T : Component
{
    public override Type ComponentType { get { return TypesCache<T>.Type; } }

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
