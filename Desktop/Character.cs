﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace Source
{
    public struct Character
    {
        public static Character[] playerCharacters = {
            new Character(Color.CadetBlue, "Cadet", AbilityOne.Jetpack, AbilityTwo.Rocket, AbilityThree.Temp),
            new Character(Color.YellowGreen, "Acrobat", AbilityOne.Jump, AbilityTwo.Boomerang, AbilityThree.Temp),
            new Character(Color.Purple, "Psychic", AbilityOne.Platform, AbilityTwo.Clone, AbilityThree.Temp),
            //new Character(Color.GhostWhite, "Planeswalker", AbilityOne.Blink, AbilityTwo.Timewarp, AbilityThree.Temp),
            new Character(Color.ForestGreen, "Hunter", AbilityOne.Grapple, AbilityTwo.Hook, AbilityThree.Trap)

            //new Character(Color.Yellow, "Alchemist", AbilityOne.Transmute, AbilityTwo.Explosive, AbilityThree.Singularity),
            //new Character(Color.WhiteSmoke, "Chronowalker", AbilityOne.Slowdown, AbilityTwo.Chronoshift, AbilityThree.LostTime),
            //new Character(Color.Purple, "Psychic", AbilityOne.Destroy, AbilityTwo.Move, AbilityThree.Swap),
            //new Character(Color.Peru, "Spectre", AbilityOne.PhaseShift, AbilityTwo.Hallucination, AbilityThree.Foresight),
            //new Character(Color.Green, "Graviton", AbilityOne.ReverseGravity, AbilityTwo.Attract, AbilityThree.Repel),
            //new Character(Color.RosyBrown, "Hunter", AbilityOne.Trap, AbilityTwo.Hookshot, AbilityThree.Nothing)
        };

        public readonly Color Color;
        public readonly string Name;
        public readonly AbilityOne Ability1;
        public readonly AbilityTwo Ability2;
        public readonly AbilityThree Ability3;

        public enum AbilityOne
        {
            Platform, Grapple, Blink, Jetpack, Jump
            //Transmute, Slowdown, Destroy, PhaseShift, ReverseGravity, Trap
        }

        public enum AbilityTwo
        {
            Clone, Hook, Timewarp, Rocket, Boomerang
            //Explosive, Chronoshift, Move, Hallucination, Attract, Hookshot
        }

        public enum AbilityThree
        {
            Invert, Trap, Temp
            //Singularity, LostTime, Swap, Foresight, Repel, Nothing
        }

        public Character(Color color, string name, AbilityOne ability1, AbilityTwo ability2, AbilityThree ability3)
        {
            Color = color;
            Name = name;
            Ability1 = ability1;
            Ability2 = ability2;
            Ability3 = ability3;
        }
    }
}
