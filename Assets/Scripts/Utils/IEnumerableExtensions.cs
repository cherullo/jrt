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

        public static IEnumerable<List<T>> Split<T>(this IEnumerable<T> list, int parts)
        {
            var collection = list is List<T> c
                ? c
                : list.ToList();

            var itemCount = collection.Count;

            int itemsInEachChunk;
            int chunks;
            if (itemCount <= parts)
            {
                itemsInEachChunk = 1;
                chunks = itemCount;
            }
            else
            {
                itemsInEachChunk = itemCount / parts;

                chunks = itemCount % parts == 0
                   ? parts
                   : parts - 1;
            }

            var itemsToChunk = chunks * itemsInEachChunk;

            for (int i = 0; i < chunks; i++)
            {
                yield return collection.Skip(i * itemsInEachChunk).Take(itemsInEachChunk).ToList();
            }
            //foreach (var chunk in collection.Take(itemsToChunk).Chunk(itemsInEachChunk))
            //{
            //    yield return chunk;
            //}

            if (itemsToChunk < itemCount)
            {
                yield return collection.Skip(itemsToChunk).ToList();
            }
        }
    }
}
