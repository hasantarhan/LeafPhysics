using System;
using UnityEngine;

namespace _Game.Code.Utils
{
    public class AutoRotation : MonoBehaviour
    {
        private VelocityUtil velocityUtil;
        [SerializeField] private float speed=0.5f;
        private void Awake()
        {
            velocityUtil = new VelocityUtil(transform);
        }

        private void Update()
        {
            velocityUtil.Update();
            var motion = velocityUtil.Motion;
            if (motion != Vector3.zero)
            {
                var rotation = Quaternion.LookRotation(motion,Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, speed);
            }
        }
    }
}