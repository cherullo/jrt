using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace JRT.Utils
{
    public static class IEnumerableExtensions
    {
        public static UnsafeList<T> ToUnsafeList<T>(this IEnumerable<T> items) where T : unmanaged
        {
            UnsafeList<T> ret = new UnsafeList<T>(items.Count(), AllocatorManager.Persistent);

            foreach (T item in items) 
                ret.AddNoResize(item);

            return ret;
        }
    }
}
