using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace Source
{
    public struct Character
    {
        public readonly Color Color;
        public readonly AbilityOne Ability1;
        public readonly AbilityTwo Ability2;
        public readonly AbilityThree Ability3;

        public enum AbilityOne
        {
            GravityPull
        }

        public enum AbilityTwo
        {
            GravityPush
        }

        public enum AbilityThree
        {
            Singularity
        }

        public Character(Color color, AbilityOne ability1, AbilityTwo ability2, AbilityThree ability3)
        {
            Color = color;
            Ability1 = ability1;
            Ability2 = ability2;
            Ability3 = ability3;
        }
    }
}
