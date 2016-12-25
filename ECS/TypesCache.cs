using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS
{

internal static class TypesCache<T>
{
    internal static readonly Type Type = typeof(T);
}

}
