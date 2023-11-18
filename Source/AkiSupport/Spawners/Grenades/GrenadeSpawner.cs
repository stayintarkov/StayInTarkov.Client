using Comfort.Common;
using EFT;
using UnityEngine;

namespace SIT.Tarkov.Core.Spawners.Grenades
{
    public class GrenadeSpawner : MonoBehaviour
    {
        public static float rate = 0.5f;

        public static float range = 20f;

        public static float delay = 5f;

        private object bullet;

        private Player player;

        public virtual int Count { get; set; } = 1;

        public virtual string TemplateId { get; set; } = "5d70e500a4b9364de70d38ce";

        private void Start()
        {
            this.bullet = ShotFactory.GetBullet(TemplateId);
            this.player = Singleton<GameWorld>.Instance.AllAlivePlayersList.Find((p) => p.IsYourPlayer);
            ShotFactory.Init(this.player);
            this.InvokeRepeating("Tick", delay, rate);
        }

        private void Tick()
        {
            Vector3 position = this.transform.position;
            position.x += Random.Range(0f - range, range);
            position.z += Random.Range(0f - range, range);
            position.y += 300f;
            ShotFactory.MakeShot(this.bullet, position, Vector3.down, 1f);
            if (--this.Count <= 0)
            {
                Destroy(this);
            }
        }
    }
}
