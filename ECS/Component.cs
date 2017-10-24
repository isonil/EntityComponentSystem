using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS
{

/// <summary>
/// Every entiy consists of Components which add additional functionality.
/// Components can be responsible for example for making the entity interact
/// with the physical world, or responding to specific events in any way.
/// </summary>
public abstract class Component
{
    private Context context;
    private int entityID = -1;

    /// <summary>
    /// Returns the Context associated with this Component.
    /// </summary>
    public Context Context
    {
        get { return context; }
        internal set { context = value; }
    }

    /// <summary>
    /// Returns the ID of the entity which owns this component.
    /// Every Component instance can be assigned to only one entity,
    /// this happens automatically when you add a Component via Context.
    /// </summary>
    public int EntityID
    {
        get { return entityID; }
        internal set { entityID = value; }
    }
}

}
