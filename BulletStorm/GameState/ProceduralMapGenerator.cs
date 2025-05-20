using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace BulletStorm.Map
{
    public class ProceduralMapGenerator
    {
        public int Width { get; }
        public int Height { get; }
        public Tile[,] Tiles { get; private set; }
        private Random random = new();

        public ProceduralMapGenerator(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new Tile[width, height];
        }

        public void Generate()
        {
            // Exemplo: preencher tudo com grama e adicionar árvores/bushes aleatórios
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var pos = new Vector2(x * 32, y * 32); // 32 = tileSize
                    TileType type = TileType.Grass;

                    // Bordas com árvores
                    if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                        type = TileType.Tree;
                    // Aleatoriamente espalhar bushes e rochas
                    else if (random.NextDouble() < 0.07)
                        type = TileType.Bush;
                    else if (random.NextDouble() < 0.03)
                        type = TileType.Rock;

                    Tiles[x, y] = new Tile(type, pos);
                }
            }
            // Adicione lógica para casa, caminhos, etc, conforme desejado
        }
    }
}