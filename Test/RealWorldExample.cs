using System;
using ECS;

namespace Test
{
    public static class RealWorldExample
    {
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

        public static void Run()
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
}
