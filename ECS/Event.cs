using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS
{

public struct Event
{
    private int kind;
    private System sender;
    private int entityID;
    private object data;

    public int Kind { get { return kind; } }
    public System Sender { get { return sender; } }
    public int EntityID { get { return entityID; } }
    public object Data { get { return data; } }

    public Event(int kind, System sender, int entityID, object data)
    {
        this.kind = kind;
        this.sender = sender;
        this.entityID = entityID;
        this.data = data;
    }
}

}
