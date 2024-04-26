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

        public unsafe static UnsafeList<T> ToUnsafeList<T>(this T[] array) where T : unmanaged
        {
            int length = array.Length;
            var ret = new UnsafeList<T>(length, AllocatorManager.Persistent);
            ret.Resize(length);

            void* arrayPtr = UnsafeUtility.PinGCArrayAndGetDataAddress(array, out ulong gchandle);
            UnsafeUtility.MemCpy(ret.Ptr, arrayPtr, UnsafeUtility.SizeOf<T>() * length);
            UnsafeUtility.ReleaseGCObject(gchandle);

            return ret;
        }
    }
}
