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
        private static float MouseScale = 1;

        public static void SetDisplayUnitToSimUnitRatio(float displayToSimRatio) {
            Ratio = displayToSimRatio;
        }

        public static float GetRatio()
        {
            return Ratio;
        }

        public static float GetMouseScale()
        {
            return MouseScale;
        }

        public static Vector2 ToDisplayUnits(Vector2 vector)
        {
            return vector * Ratio * MouseScale;
        }

        public static Vector2 ToSimUnits(Vector2 vector)
        {
            return vector / Ratio / MouseScale;
        }

        public static float ToDisplayUnits(float num)
        {
            return num * Ratio * MouseScale;
        }

        public static float ToSimUnits(float num)
        {
            return num / Ratio / MouseScale;
        }

        public static void SetMouseScale(float scale)
        {
            MouseScale = scale;
            Console.WriteLine("Mouse Scale: " + scale);
        }

        public static Vector2 GetMousePos(MouseState mouse)
        {
            return mouse.Position.ToVector2() / MouseScale;
        }
    }
}
