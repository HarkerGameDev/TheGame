using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Source.Collisions
{
    /// <summary>
    /// Simple AI that jumps every few seconds, as specified by GameData.AI_TIME
    /// </summary>
    public class AI : Player
    {
        private GameData.SimulatedControls controls;
        private float timer;

        public AI(Texture2D texture, Vector2 position, Character character, GameData.SimulatedControls controls)
            : base(texture, position, character)
        {
            this.controls = controls;
            timer = GameData.AI_WAIT;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            timer -= deltaTime;
            if (timer < 0)
            {
                controls.JumpHeld = false;
                timer = GameData.AI_WAIT;
            }
            else if (timer < GameData.AI_HOLD)
            {
                controls.JumpHeld = true;
            }
        }
    }
}
