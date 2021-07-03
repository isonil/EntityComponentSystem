using System;
using ECS;

namespace Test
{

public class System1 : System<Comp1>
{
    protected override void Update(Comp1 component, int updateType)
    {
        Console.WriteLine("Updating Comp1 of " + component.EntityID);

        Console.WriteLine("System1 sends event. EntityID = " + component.EntityID);
        SendEvent(component.EntityID, null);
    }
}

}
