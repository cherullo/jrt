using JRT.Data;

using UnityEngine;

namespace JRT
{
    public class SpawnInUnitSphere : MonoBehaviour
    {
        public GameObject Prefab;

        public int Count;

        // Start is called before the first frame update
        void Start()
        {
            RNG random = new RNG();
            random.State = 999;

            for (int i = 0; i < Count; i++)
            {
                GameObject go = Instantiate(Prefab);
                go.transform.SetParent(transform, false);
                go.transform.localPosition = random.UnitSphere;
                go.SetActive(true);
            }
        }
    }
}
