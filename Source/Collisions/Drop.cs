using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Source.Collisions
{
    public class Drop : Body
    {
        public float LiveTime;
        public Type type;
        public Player Player;

        public enum Type
        {
            Singularity, Bomb
        }

        public Drop(Player player, Texture2D texture, Vector2 position, float radius, Type type)
            : base(texture, position, new Vector2(radius * 2))
        {
            LiveTime = GameData.DROP_LIVE;
            this.type = type;
        }
    }
}
