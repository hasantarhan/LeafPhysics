using System;
using UnityEngine;

namespace _Game.Code
{
    [RequireComponent(typeof(Animator))]
    public class AutoAnimator : MonoBehaviour
    {
        private Animator animator;
        private VelocityUtil velocityUtil;
        private float maxSpeed;
        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
            velocityUtil = new VelocityUtil(transform);
        }

        private void Update()
        {
            velocityUtil.Update();
            var speed = velocityUtil.speed;
            if (speed>maxSpeed)
            {
                maxSpeed = speed;
            }
            speed = speed.Remap(0, maxSpeed, 0, 1);
            animator.SetFloat("Speed", speed);
        }
    }
}