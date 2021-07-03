namespace ECS
{

/// <summary>
/// Every entity consists of multiple Components.
/// Components usually contain raw data only.
/// Components are managed by their corresponding System.
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
    /// Every Component instance is owned by exactly one entity.
    /// </summary>
    public int EntityID
    {
        get { return entityID; }
        internal set { entityID = value; }
    }
}

}
