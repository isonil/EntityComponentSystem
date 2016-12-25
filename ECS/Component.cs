using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS
{

public abstract class Component
{
    private Context context;
    private int entityID = -1;

    public Context Context
    {
        get { return context; }
        internal set { context = value; }
    }

    public int EntityID
    {
        get { return entityID; }
        internal set { entityID = value; }
    }
}

}
