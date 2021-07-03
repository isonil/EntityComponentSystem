<p align="center">
  <img src="https://user-images.githubusercontent.com/8492408/124278535-ab1e1080-db46-11eb-8b5f-6781faf651b6.png">
</p>

# About

A simple and fast Entity-Component-System for C#.

Every entity is represented as an int.

The code is self-documenting. Start with creating an instance of Context.

# How to use

1. Create an instance of Context.
2. Add custom Systems, Entities, and Components by calling context.AddSystem(), context.AddEntity(), and context.AddComponent().
3. Call context.Update() every frame and/or use context.SendEvent() to send custom events between systems.

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

# Real world example

In this example there are 2 entities: player and enemy. Player has Position and Health components. The enemy has Position, Health, and AI components. Each turn the enemy looks for the closest entity to attack. The simulation ends when the player dies.

```C#
// components

public class Position : Component
{
    public float x, y, z;
}

public class AI : Component
{
}

public class Health : Component
{
    public float HP { get; set; } = 100;
}

// systems

public class PositionSystem : System<Position>
{
}

public class AISystem : System<AI>
{
    protected override void Update(AI component, int updateType)
    {
        base.Update(component, updateType);

        Console.WriteLine("Enemy with ID " + component.EntityID + " looks for someone to attack.");

        // get my position (methods like this only iterate over the elements of a cached list of components of this entity)
        var myPosition = Context.GetFirstComponentOfTypeOfEntity<Position>(component.EntityID);

        // find the first entity nearby
        foreach( Position enemyPosition in Context.GetComponentsOfType<Position>() )
        {
            if( enemyPosition == myPosition )
                continue; // it's me (note that we're comparing component references, not values)

            if( Math.Abs(myPosition.x - enemyPosition.x) <= 10f
                && Math.Abs(myPosition.y - enemyPosition.y) <= 10f
                && Math.Abs(myPosition.z - enemyPosition.z) <= 10f )
            {
                // attack
                var args = new AttackEventArgs { TargetEntityID = enemyPosition.EntityID, AttackStrength = 25 };
                SendEvent(0, component.EntityID, args);
                break;
            }
        }
    }
}

public class HealthSystem : System<Health>
{
    public override void ReceiveEvent(Event ev)
    {
        base.ReceiveEvent(ev);

        if( ev.Data is AttackEventArgs attack )
        {
            var victimHealth = GetComponentOfEntity(attack.TargetEntityID);
            victimHealth.HP -= attack.AttackStrength;

            Console.WriteLine("Player with ID " + attack.TargetEntityID + " attacked by enemy with ID " + ev.EntityID + "! HP left: " + victimHealth.HP);
        }
    }
}

// events

public class AttackEventArgs
{
    public int TargetEntityID { get; set; }
    public float AttackStrength { get; set; }
}

public class Program
{
    public static void Main(string[] args)
    {
        // create context
        var context = new Context();

        // add all systems
        context.AddSystem<PositionSystem>();
        context.AddSystem<HealthSystem>();
        context.AddSystem<AISystem>();

        // add player with Position and Health
        int player = context.AddEntity();
        var playerPosition = context.AddComponent<Position>(player);
        var playerHealth = context.AddComponent<Health>(player);
        playerPosition.x = 5;

        // add enemy with Position, Health, and AI
        int enemy = context.AddEntity();
        var enemyPosition = context.AddComponent<Position>(enemy);
        enemyPosition.x = 10;
        context.AddComponent<Health>(enemy);
        context.AddComponent<AI>(enemy);

        // do the simulation if the player is still alive
        while( playerHealth.HP > 0 )
        {
            context.Update();
        }

        Console.WriteLine("Player died.");
        Console.ReadLine();
    }
}
```

Output:

```
Enemy with ID 1 looks for someone to attack.
Player with ID 0 attacked by enemy with ID 1! HP left: 75
Enemy with ID 1 looks for someone to attack.
Player with ID 0 attacked by enemy with ID 1! HP left: 50
Enemy with ID 1 looks for someone to attack.
Player with ID 0 attacked by enemy with ID 1! HP left: 25
Enemy with ID 1 looks for someone to attack.
Player with ID 0 attacked by enemy with ID 1! HP left: 0
Player died.
```
