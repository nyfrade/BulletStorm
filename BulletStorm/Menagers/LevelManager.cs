using System.Collections.Generic;
using BulletStorm.Entities;

namespace BulletStorm.Managers
{
    public class LevelManager
    {
        public int CurrentLevel { get; set; } = 1;
        public int EnemiesKilled { get; set; } = 0;
        public int Phase3Kills { get; set; } = 0;
        public bool BossSpawned { get; set; } = false;
        public List<Enemy> Enemies { get; } = new();

        // Add this to track active coins in the level
        public List<Coin> Coins { get; } = new();

        // Optionally, track total coins collected in this level
        public int CoinsCollected { get; set; } = 0;

        // You can add methods to reset coins, add coins, etc. as needed
    }
}
