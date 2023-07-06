using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Фоновый шарик
    /// </summary>
    public class Ballon
    {
        public float Speed { get; set; }
        /// <summary>
        /// Направление движения
        /// </summary>
        public Vector2 Direction { get; set; }
        public GameObject Instance { get; set; }
        public Ballon() { }
        public Ballon(float speed, Vector2 direction, GameObject instance)
        {
            Speed = speed;
            Direction = direction;
            Instance = instance;
        }
    }
}
