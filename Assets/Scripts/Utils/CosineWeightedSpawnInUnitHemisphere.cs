using JRT.Data;
using Unity.Mathematics;
using UnityEngine;

namespace JRT
{
    public class CosineWeightedSpawnInUnitHemisphere : MonoBehaviour
    {
        public GameObject Prefab;

        public int Count;

        // Start is called before the first frame update
        void Start()
        {
            RNG random = new RNG();
            random.State = 999;
            Hemisphere hem = new Hemisphere(0, transform.up);
            Data.Material mat = new Data.Material();

            for (int i = 0; i < Count; i++)
            {
                GameObject go = Instantiate(Prefab);
                go.transform.SetParent(transform, false);

                float3 hemPosition;
                mat.GetHemisphereSample(ref random, out hemPosition, out _);

                //if (hemPosition.z < 0)
                //    hemPosition.z *= -1.0f;

                go.transform.localPosition = hem.ToGlobal(hemPosition);
                go.SetActive(true);
            }
        }
    }
}
