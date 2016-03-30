using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Source.Collisions
{
    public enum Tile : byte
    {
        Empty, Filled
    }

    public static class Collisions
    {
        public static Vector2 Collide(Tile tile, float tileX, float tileY, AABB box)
        {
            //tileX *= GameData.TILE_SIZE;
            //tileY *= GameData.TILE_SIZE;
            switch (tile)
            {
                case Tile.Empty:
                    return Vector2.Zero;
                case Tile.Filled:
                    float x1 = box.Right - tileX;
                    float x2 = (tileX + 1) - box.Left;
                    float y1 = box.Bottom - tileY;
                    float y2 = (tileY + 1) - box.Top;
                    if (x1 < 0 || x2 < 0 || y1 < 0 || y2 < 0)
                        return Vector2.Zero;
                    if (x1 < x2)
                    {
                        if (x1 < y1 && x1 < y2)
                            return new Vector2(x1, 0);
                        else if (y1 < y2)
                            return new Vector2(0, y1);
                        else
                            return new Vector2(0, y2);
                    }
                    else
                    {
                        if (x2 < y1 && x2 < y2)
                            return new Vector2(x2, 0);
                        else if (y1 < y2)
                            return new Vector2(0, y1);
                        else
                            return new Vector2(0, y2);
                    }
                default:
                    throw new Exception("Tile collisions for " + tile + " not being calculated!");
            }
        }
    }
}
