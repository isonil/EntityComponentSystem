using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS.Test
{

public static class Test
{
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
}

}
