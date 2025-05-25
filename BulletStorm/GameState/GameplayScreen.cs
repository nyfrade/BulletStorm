using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Teste;

public class GameplayScreen
{
    private Texture2D _tileset;
    private Texture2D _tileset2;
    private readonly int _tileWidth = 16;
    private readonly int _tileHeight = 16;

    // Tiles sources
    private readonly Rectangle grassSource = new Rectangle(80, 48, 16, 16);
    private readonly Rectangle purpleflowerSource = new Rectangle(83, 12, 9, 18);
    private readonly Rectangle blueflowerSource = new Rectangle(99, 12, 9, 18);
    private readonly Rectangle redflowerSource = new Rectangle(115, 12, 9, 18);
    private readonly Rectangle yellowflowerSource = new Rectangle(131, 12, 9, 18);
    private readonly Rectangle detailsgrassSource = new Rectangle(18, 34, 12, 11);

    private Vector2[] purpleflowerPositions;
    private Vector2[] blueflowerPositions;
    private Vector2[] redflowerPositions;
    private Vector2[] yellowflowerPositions;
    private Vector2[] detailsgrassPositions;

    private Random random = new();

    private GraphicsDevice _graphicsDevice;

    public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content, GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;

        _tileset = content.Load<Texture2D>("TileCraftGroundSetVersion2");
        _tileset2 = content.Load<Texture2D>("objects_terrain");

        purpleflowerPositions = GenerateRandomPositions(10, purpleflowerSource.Width, purpleflowerSource.Height);
        blueflowerPositions = GenerateRandomPositions(10, blueflowerSource.Width, blueflowerSource.Height);
        redflowerPositions = GenerateRandomPositions(10, redflowerSource.Width, redflowerSource.Height);
        yellowflowerPositions = GenerateRandomPositions(10, yellowflowerSource.Width, yellowflowerSource.Height);
        detailsgrassPositions = GenerateRandomPositions(20, detailsgrassSource.Width, detailsgrassSource.Height);
    }

    internal void Draw(SpriteBatch spriteBatch)
    {
        Draw(spriteBatch, _graphicsDevice);
    }

    public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        int tilesHorizontais = (int)Math.Ceiling(graphicsDevice.Viewport.Width / (float)_tileWidth);
        int tilesVerticais = (int)Math.Ceiling(graphicsDevice.Viewport.Height / (float)_tileHeight);

        for (int y = 0; y < tilesVerticais; y++)
        {
            for (int x = 0; x < tilesHorizontais; x++)
            {
                Vector2 position = new Vector2(x * _tileWidth, y * _tileHeight);
                spriteBatch.Draw(_tileset, position, grassSource, Color.White);
            }
        }

        foreach (var pos in purpleflowerPositions)
            spriteBatch.Draw(_tileset2, new Rectangle((int)pos.X, (int)pos.Y, purpleflowerSource.Width, purpleflowerSource.Height), purpleflowerSource, Color.White);
        foreach (var pos in blueflowerPositions)
            spriteBatch.Draw(_tileset2, new Rectangle((int)pos.X, (int)pos.Y, blueflowerSource.Width, blueflowerSource.Height), blueflowerSource, Color.White);
        foreach (var pos in redflowerPositions)
            spriteBatch.Draw(_tileset2, new Rectangle((int)pos.X, (int)pos.Y, redflowerSource.Width, redflowerSource.Height), redflowerSource, Color.White);
        foreach (var pos in yellowflowerPositions)
            spriteBatch.Draw(_tileset2, new Rectangle((int)pos.X, (int)pos.Y, yellowflowerSource.Width, yellowflowerSource.Height), yellowflowerSource, Color.White);
        foreach (var pos in detailsgrassPositions)
            spriteBatch.Draw(_tileset2, new Rectangle((int)pos.X, (int)pos.Y, detailsgrassSource.Width, detailsgrassSource.Height), detailsgrassSource, Color.White);
    }

    private Vector2[] GenerateRandomPositions(int count, int width, int height)
    {
        var positions = new List<Rectangle>();
        int maxX = 800 - width;
        int maxY = 600 - height;
        int tentativasMax = 1000;

        for (int i = 0; i < count; i++)
        {
            int tentativas = 0;
            bool encontrou = false;
            while (tentativas < tentativasMax && !encontrou)
            {
                int x = random.Next(0, maxX);
                int y = random.Next(0, maxY);
                var novoRect = new Rectangle(x, y, width, height);

                bool sobrepoe = false;
                foreach (var rect in positions)
                {
                    if (rect.Intersects(novoRect))
                    {
                        sobrepoe = true;
                        break;
                    }
                }

                if (!sobrepoe)
                {
                    positions.Add(novoRect);
                    encontrou = true;
                }
                tentativas++;
            }
            if (!encontrou)
                break;
        }

        return positions.Select(r => new Vector2(r.X, r.Y)).ToArray();
    }
}

