using System;
using UnityEngine;

namespace _Game.Code
{
    public class Movement : MonoBehaviour
    {
        private Joystick joystick;
        public float speed=4;
        private void Awake()
        {
            joystick = FindObjectOfType<Joystick>();
        }

        private void Update()
        {
            var h = joystick.Horizontal*speed;
            var v = joystick.Vertical*speed;
            
            transform.Translate(h*Time.deltaTime,0,v*Time.deltaTime);
        }
    }
}