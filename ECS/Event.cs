using System;

namespace ECS
{

[Serializable]
public struct Event : IEquatable<Event>
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

    public static bool operator ==(Event lhs, Event rhs)
    {
        return lhs.kind == rhs.kind
            && lhs.sender == rhs.sender
            && lhs.entityID == rhs.entityID
            && lhs.data == rhs.data;
    }

    public static bool operator !=(Event lhs, Event rhs)
    {
        return !(lhs == rhs);
    }

    public override bool Equals(object obj)
    {
        if( !(obj is Event) )
            return false;

        return Equals((Event)obj);
    }

    public bool Equals(Event other)
    {
        return kind.Equals(other.kind)
            && System.Equals(sender, other.sender)
            && entityID.Equals(other.entityID)
            && object.Equals(data, other.data);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;

            hash = hash * 23 + kind;

            if( sender != null )
                hash = hash * 23 + sender.GetHashCode();

            hash = hash * 23 + entityID;

            if( data != null )
                hash = hash * 23 + data.GetHashCode();

            return hash;
        }
    }
}

}
