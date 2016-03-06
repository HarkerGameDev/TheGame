using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Source;
using EmptyKeys.UserInterface.Controls;

namespace Source.Collisions
{
    public static class ConvertUnits
    {
        private static float Ratio;
        private static float ResolutionScale = 1;
        private static float MouseScale = 1;

        public static void SetDisplayUnitToSimUnitRatio(float displayToSimRatio) {
            Ratio = displayToSimRatio;
        }

        public static float GetRatio()
        {
            return Ratio;
        }

        public static float GetResolutionScale()
        {
            return ResolutionScale;
        }

        public static Vector2 ToDisplayUnits(Vector2 vector)
        {
            return vector * Ratio * ResolutionScale;
        }

        public static Vector2 ToSimUnits(Vector2 vector)
        {
            return vector / Ratio / ResolutionScale;
        }

        public static float ToDisplayUnits(float num)
        {
            return num * Ratio * ResolutionScale;
        }

        public static float ToSimUnits(float num)
        {
            return num / Ratio / ResolutionScale;
        }

        public static void SetResolutionScale(float scale)
        {
            ResolutionScale = scale;
            Console.WriteLine("Resolution Scale: " + scale);
        }

        public static void SetMouseScale(bool fullscreen, float scale)
        {
            MouseScale = fullscreen ? scale : 1;
            Console.WriteLine("Mouse Scale: " + scale);
        }

        public static Vector2 GetMousePos(MouseState mouse)
        {
            return mouse.Position.ToVector2() / MouseScale;
        }
    }
}
