using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Assertions;

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

            int itemsInEachChunk = itemCount / parts;
            int remaining = itemCount % parts;

            int chunked = 0;
            for (int i = 0; i < parts; i++)
            {
                int inChunk = itemsInEachChunk + (i < remaining ? 1 : 0);

                yield return collection.Skip(chunked).Take(inChunk).ToList();
                chunked += inChunk;
            }

            Debug.Assert(chunked == itemCount, "Split did not take all items in collection.");
        }
    }
}
