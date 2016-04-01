using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Source.Collisions
{
    public class Drop : Polygon
    {
        public float LiveTime;
        public Types Type;
        public Player Player;

        public enum Types
        {
            Singularity, Bomb, Trap
        }

        public Drop(Player player, Texture2D texture, Vector2 position, float radius, Types type, Color color)
            : base(texture, position, new Vector2(radius * 2))
        {
            Color = color;
            LiveTime = GameData.DROP_LIVE;
            Velocity = player.Velocity * GameData.DROP_SCALE + new Vector2(GameData.DROP_SPEED_X, GameData.DROP_SPEED_Y);
            this.Type = type;
            this.Player = player;
        }
    }
}
