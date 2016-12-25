using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS
{

internal static class SystemTypesCache<T>
    where T : System
{
    internal static readonly Predicate<System> IsT = IsTImpl;

    private static bool IsTImpl(System system)
    {
        return system is T;
    }
}

}
