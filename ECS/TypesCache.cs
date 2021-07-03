using System;

namespace ECS
{

internal static class TypesCache<T>
{
    internal static readonly Type Type = typeof(T);
}

}
