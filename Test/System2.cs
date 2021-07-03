using System;
using ECS;

namespace Test
{

public class System2 : System<Comp2>
{
    protected override void Update(Comp2 component, int updateType)
    {
        Console.WriteLine("Updating Comp2 of " + component.EntityID);
    }

    public override void ReceiveEvent(Event ev)
    {
        base.ReceiveEvent(ev);

        Console.WriteLine("System2 received event. EntityID = " + ev.EntityID);
    }
}

}
