using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletStorm.Entities
{
    public enum WeaponType
    {
        Sword,
        Gun
    }

    public enum WeaponRarity
    {
        SS, // Super rare
        S,
        A,
        B
    }

    public class Weapon
    {
        public string Name;
        public WeaponType Type;
        public WeaponRarity Rarity;
        public Vector2 Offset;
        public float Angle;
        public float OrbitRadius = 40f;
        public Texture2D Texture;

        // Stats
        public float FireRate = 1.0f;      // Shots per second
        public int Damage = 1;
        public float ProjectileSpeed = 400f;
        public float CritChance = 0.0f;    // 0.0 - 1.0
        public float EffectDuration = 0f;  // For status effects
        public string EffectDescription = "";
        public float FireTimer = 0f;

        // Optional: Custom effect delegate (implement in your projectile/enemy logic)
        public Action<Enemy> OnHitEffect;

        public Weapon(string name, WeaponType type, Texture2D texture, float angle)
        {
            Name = name;
            Type = type;
            Texture = texture;
            Angle = angle;
        }

        public void Update(Vector2 playerPosition, float delta)
        {
            Angle += delta;
            Offset = new Vector2(
                (float)Math.Cos(Angle) * OrbitRadius,
                (float)Math.Sin(Angle) * OrbitRadius
            );
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 playerPosition)
        {
            spriteBatch.Draw(Texture, playerPosition + Offset, Color.White);
        }

        // Factory method to create all weapons (textures must be loaded in Game1 and passed in)
        public static List<Weapon> CreateAllWeapons(Texture2D[] swordTextures, Texture2D[] gunTextures)
        {
            var weapons = new List<Weapon>();

            // --- BASIC STARTER WEAPONS ---
            weapons.Add(new Weapon("Blaze Edge", WeaponType.Sword, swordTextures[0], 0f)
            {
                Rarity = WeaponRarity.B,
                OrbitRadius = 40f,
                FireRate = 1.0f,
                Damage = 2,
                ProjectileSpeed = 400f,
                CritChance = 0.05f,
                EffectDuration = 0f,
                EffectDescription = ""
            });

            weapons.Add(new Weapon("Frostbite Saber", WeaponType.Sword, swordTextures[1], MathF.PI * 2 / 6)
            {
                Rarity = WeaponRarity.B,
                OrbitRadius = 45f,
                FireRate = 1.0f,
                Damage = 2,
                ProjectileSpeed = 400f,
                CritChance = 0.05f,
                EffectDuration = 0f,
                EffectDescription = ""
            });

            weapons.Add(new Weapon("Pulse Blaster", WeaponType.Gun, gunTextures[0], MathF.PI * 2 / 3)
            {
                Rarity = WeaponRarity.B,
                OrbitRadius = 80f,
                FireRate = 1.5f,
                Damage = 1,
                ProjectileSpeed = 500f,
                CritChance = 0.05f,
                EffectDuration = 0f,
                EffectDescription = ""
            });

            // --- ADVANCED SWORDS ---
            weapons.Add(new Weapon("Thunderbrand", WeaponType.Sword, swordTextures[2], MathF.PI * 4 / 6)
            {
                Rarity = WeaponRarity.A,
                OrbitRadius = 50f,
                FireRate = 1.5f,
                Damage = 2,
                ProjectileSpeed = 430f,
                CritChance = 0.15f,
                EffectDuration = 0.5f,
                EffectDescription = "Chance to stun for 0.5s"
            });

            weapons.Add(new Weapon("Venomspike", WeaponType.Sword, swordTextures[3], MathF.PI * 6 / 6)
            {
                Rarity = WeaponRarity.S,
                OrbitRadius = 55f,
                FireRate = 1.1f,
                Damage = 3,
                ProjectileSpeed = 410f,
                CritChance = 0.08f,
                EffectDuration = 2.5f,
                EffectDescription = "Poisons enemies for 2.5s"
            });

            weapons.Add(new Weapon("Celestial Katana", WeaponType.Sword, swordTextures[4], MathF.PI * 8 / 7)
            {
                Rarity = WeaponRarity.S,
                OrbitRadius = 60f,
                FireRate = 1.3f,
                Damage = 4,
                ProjectileSpeed = 440f,
                CritChance = 0.18f,
                EffectDuration = 0f,
                EffectDescription = "High crit chance"
            });

            weapons.Add(new Weapon("Shadow Reaver", WeaponType.Sword, swordTextures[5], MathF.PI * 10 / 7)
            {
                Rarity = WeaponRarity.SS,
                OrbitRadius = 65f,
                FireRate = 0.9f,
                Damage = 5,
                ProjectileSpeed = 400f,
                CritChance = 0.20f,
                EffectDuration = 1.0f,
                EffectDescription = "Steals a small amount of health"
            });

            // --- ADVANCED GUNS ---
            weapons.Add(new Weapon("Void Cannon", WeaponType.Gun, gunTextures[1], MathF.PI * 4 / 3)
            {
                Rarity = WeaponRarity.S,
                OrbitRadius = 90f,
                FireRate = 1.8f,
                Damage = 4,
                ProjectileSpeed = 550f,
                CritChance = 0.12f,
                EffectDuration = 1.5f,
                EffectDescription = "Creates a small explosion on hit"
            });

            weapons.Add(new Weapon("Star Piercer", WeaponType.Gun, gunTextures[2], MathF.PI * 6 / 3)
            {
                Rarity = WeaponRarity.SS,
                OrbitRadius = 100f,
                FireRate = 3.0f,
                Damage = 1,
                ProjectileSpeed = 700f,
                CritChance = 0.25f,
                EffectDuration = 0f,
                EffectDescription = "High crit chance, pierces through enemies"
            });

            return weapons;
        }

    }
}

