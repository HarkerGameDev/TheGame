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
        public float Timer;
        public Player Parent;

        public AI(Texture2D texture, Vector2 position, Character character, LinkedListNode<Vector2> checkpoint, GameData.SimulatedControls controls, bool facingRight)
            : base(texture, position, character, checkpoint)
        {
            Parent = null;
            this.Controls = controls;
            Timer = 0;
            if (facingRight)
                Controls.Right = true;
            else
                Controls.Left = true;
        }

        public AI(Player parent, GameData.SimulatedControls controls)
            : base(parent.texture, parent.Position, parent.CurrentCharacter, parent.Node)
        {
            Parent = parent;
            Progress = parent.Progress;
            Checkpoints = parent.Checkpoints;
            Velocity = parent.Velocity;

            Controls = controls;
            Timer = 0;
            if (parent.FacingRight)
                controls.Right = true;
            else
                controls.Left = true;

            Color.A /= 2;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            Timer -= deltaTime;
            if (Timer < 0)
            {
                Controls.JumpHeld = false;
                //Timer = GameData.AI_WAIT;
            }
            else
            {
                Controls.JumpHeld = true;
            }
        }
    }
}
