<p align="center">
  <img src="https://user-images.githubusercontent.com/8492408/124278535-ab1e1080-db46-11eb-8b5f-6781faf651b6.png">
</p>

# About

A simple and fast Entity-Component-System for C#.

Every entity is represented as an int.

The code is self-documenting. Start with creating an instance of Context.

# How to use

1. Create an instance of Context.
2. Add custom Systems and Components.
3. Call Context.Update() every frame or use Context.SendEvent() to send custom events.

# Example

This example is in the Test directory.

```C#

public class Comp1 : Component
{
}

public class Comp2 : Component
{
}

public class System1 : System<Comp1>
{
    protected override void Update(Comp1 component, int updateType)
    {
        Console.WriteLine("Updating Comp1 of " + component.EntityID);

        Console.WriteLine("System1 sends event. EntityID = " + component.EntityID);
        SendEvent(0, component.EntityID, null);
    }
}

public class System2 : System<Comp2>
{
    protected override void Update(Comp2 component, int updateType)
    {
        Console.WriteLine("Updating Comp2 of " + component.EntityID);
    }

    public override void ReceiveEvent(Event ev)
    {
        base.ReceiveEvent(ev);

        Console.WriteLine("System2 received event. Kind = " + ev.Kind + ", EntityID = " + ev.EntityID);
    }
}

public static void Run()
{
    var context = new Context();
    context.AddSystem<System1>();
    context.AddSystem<System2>();
    int entity1 = context.AddEntity();
    int entity2 = context.AddEntity();
    int entity3 = context.AddEntity();
    context.AddComponent<Comp1>(entity1);
    context.AddComponent<Comp2>(entity2);
    context.AddComponent<Comp1>(entity3);
    context.AddComponent<Comp2>(entity3);

    for( int i = 0; i < 5; ++i )
    {
        if( i != 0 )
            Console.WriteLine();

        Console.WriteLine("Iteration " + i);

        context.Update();
    }
}
```
