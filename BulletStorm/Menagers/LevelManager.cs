using System.Collections.Generic;
using BulletStorm.Entities;

namespace BulletStorm.Managers
{
    public class LevelManager
    {
        public int CurrentLevel { get; set; }
        public int EnemiesKilled { get; set; }
        public int Phase3Kills { get; set; }
        public bool BossSpawned { get; set; }
        public List<Enemy> Enemies { get; } = new();
    }
}


