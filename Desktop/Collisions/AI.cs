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
        public GameData.SimulatedControls Controls;
        private float timer;

        public AI(Texture2D texture, Vector2 position, Character character, LinkedListNode<Vector2> checkpoint, GameData.SimulatedControls controls, Direction direction)
            : base(texture, position, character, checkpoint)
        {
            this.Controls = controls;
            timer = GameData.AI_WAIT;
            if (direction == Direction.Right)
                controls.Right = true;
            else if (direction == Direction.Left)
                controls.Left = true;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            timer -= deltaTime;
            if (timer < 0)
            {
                Controls.JumpHeld = false;
                timer = GameData.AI_WAIT;
            }
            else if (timer < GameData.AI_HOLD)
            {
                Controls.JumpHeld = true;
            }
        }
    }
}
