using UnityEngine;

namespace JRT
{
    public class FrameLimiter : MonoBehaviour
    {
        [SerializeField]
        private int _targetFrameRate = 60;

        private void Awake()
        {
            Application.targetFrameRate = _targetFrameRate;
        }
    }
}
