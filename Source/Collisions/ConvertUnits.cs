using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Source.Collisions
{
    public static class ConvertUnits
    {
        private static float Ratio;

        public static void SetDisplayUnitToSimUnitRatio(float displayToSimRatio) {
            Ratio = displayToSimRatio;
        }

        public static Vector2 ToDisplayUnits(Vector2 vector)
        {
            return vector * Ratio;
        }

        public static Vector2 ToSimUnits(Vector2 vector)
        {
            return vector / Ratio;
        }

        public static float ToDisplayUnits(float num)
        {
            return num * Ratio;
        }

        public static float ToSimUnits(float num)
        {
            return num / Ratio;
        }
    }
}
