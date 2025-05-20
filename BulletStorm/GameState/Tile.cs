using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace BulletStorm.Map
{
    public class Tile
    {
        public TileType Type { get; set; }
        public Vector2 Position { get; set; }
        public Tile(TileType type, Vector2 position)
        {
            Type = type;
            Position = position;
        }
    }
}
