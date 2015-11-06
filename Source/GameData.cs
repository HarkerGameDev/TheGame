using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Source.Collisions;

namespace Source
{
    internal static class GameData
    {
        public const int LEVEL = 1;            // if this is greater than -1, levels will not be procedurally generated (useful for editing)
        public const int numPlayers = 1;            // number of players

        public static bool[] useController = { false, false, false };    // true means the player at the index will be using a controller.
        public static Controls[] keyboardControls = {            // defines the keyboard controls which will be used
                                                       new Controls(Keys.Left, Keys.Right, Keys.Up, Keys.Down, Keys.RightShift),
                                                       new Controls(Keys.A, Keys.D, Keys.W, Keys.S, Keys.LeftShift),
                                                       new Controls(Keys.J, Keys.L, Keys.I, Keys.K, Keys.O)};

        // LEVEL_FILE should point to "level*.lvl" in the root project directory
        public const String LEVELS_DIR = "../../../../";
        public const String LEVELS_DIR2 = "../../../../../../";
        public const float LOAD_NEW = 100f;     // the next level will be loaded when the player is this far from the current end

        // Farseer user data - basically, just use this as if it were an enum
        public const int PLAYER = 0;
        public const int FLOOR = 1;
        public const int MAX_LEVELS_LOADED = 6;    // how many levels to keep loaded at a given time
        public const float PIXEL_METER = 32f;      // pixels per meter for normal game
        public const float PIXEL_METER_EDIT = 8f;  // pixels per meter when in edit mode for level
        public const int VIEW_WIDTH = 1280;        // width of unscaled screen in pixels
        public const int VIEW_HEIGHT = 720;        // height of unscaled screen in pixels
        public static Vector2 PLAYER_POSITION = new Vector2(2, -20f);   // starting position of player
        public static Color[] playerColors = { Color.Red, Color.Yellow, Color.Purple };     // colors of each player

        public const float MIN_VELOCITY = 1f;  // m/s -- what can be considered 0 horizontal velocity
        public const float MAX_VELOCITY = 30f; // m/s -- maximum horizontal velocity for player
        public const float MAX_IMPULSE = 40f;   // m/s^2 -- the impulse which is applied when player starts moving after standing still
        public const double IMPULSE_POW = 0.5; //     -- the player's horizontal input impulse is taken to the following power for extra smoothing
        public const float JUMP_IMPULSE = 14f; // m/s -- the initial upwards velocity when player jumps
        public const float SLOWDOWN = 45f;      // m/s^2 -- impulse applied in opposite direction of travel to simulate friction
        public const float AIR_RESIST = 0.75f; //     -- air resistance on a scale of 0 to 1, where 1 is as if player is on ground
        public const double JUMP_WAIT = 0.5;   // s   -- how long the player needs to wait before jumping again
        public const float PUSH_VEL = 1f;      // m/s -- the player is given a little push going down platforms under this velocity
        public const float PUSH_POW = 10f;     // m/s -- the impulse applied to the player to get down a platform
        public const float MIN_WOBBLE = 0f;  //     -- the minimum ratio between max velocity and (max - current velocity) for wobbling
        public const float MAX_WOBBLE = 0f;    //     -- the maximum ratio for wobbling; we don't want wobble amplifier 40x
        public const int LEVEL_DIST = 10;   // the space between levels

        public const string SONG = "Chiptune dash";    // the song to play, over, and over, and over again. NEVER STOP THE PARTY!
        public const float VOLUME = 0f;                // volume for song

        public const float CAMERA_SCALE = 20f;         // how fast the camera moves
        public const float MAX_CAMERA_SPEED_X = 5f;    // maximum x speed of camera
        public const float MAX_CAMERA_SPEED_Y = 3f;    // maximum y speed of camera
        public const float SCREEN_LEFT = 0.2f;         // defines how far left the player can be on wobble-screen
        public const float SCREEN_RIGHT = 0.35f;       // defines the right limit of the player on wobble-screen
        public const float SCREEN_TOP = 0.3f;          // defines the distance from the top or bottom of the screen for the player in wobble-screen
        public const float DEAD_DIST = 10f;            // players this distance or more behind the average x will move to the maximum player
        public const double DEAD_TIME = 3;             // respawn time when a player gets behind the cutoff
        public const double PHASE_TIME = 1;            // the point at which the player will be visible again after dying to get the player prepared
        
        public struct Controls {
            public Keys left, right, up, down, shoot;

            public Controls(Keys left, Keys right, Keys up, Keys down, Keys shoot) {
                this.left = left;
                this.right = right;
                this.up = up;
                this.down = down;
                this.shoot = shoot;
            }
        }

        /// <summary>
        /// Stores data for each level in memory
        /// </summary>
        public class FloorData
        {
            private List<Data>[] data;
            private World world;
            private Texture2D texture;
            private List<Floor> floors;
            private int[] max;
            private Random rand;
            private int[] levelLengths = new int[MAX_LEVELS_LOADED];

            private struct Data
            {
                public float Width;
                public Vector2 Center;
                public float Rotation;
                public bool Solid;

                public Data(float width, Vector2 center, float rotation, bool solid)
                {
                    this.Width = width;
                    this.Center = center;
                    this.Rotation = rotation;
                    this.Solid = solid;
                }
            }

            /// <summary>
            /// Creates a new floordata object to store level data
            /// </summary>
            /// <param name="world">The current world</param>
            /// <param name="texture">The texture for the floor</param>
            /// <param name="floors">The list where floors are stored</param>
            /// <param name="totalLevels">Total number of levels</param>
            public FloorData(World world, Texture2D texture, List<Floor> floors, int totalLevels)
            {
                this.world = world;
                this.texture = texture;
                this.floors = floors;
                data = new List<Data>[totalLevels];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = new List<Data>();
                }
                max = new int[totalLevels];
                rand = new Random();
            }

            /// <summary>
            /// Adds a new floor to the specified level
            /// </summary>
            /// <param name="size">Size of the floor</param>
            /// <param name="center">Center of the floor</param>
            /// <param name="rotation">Rotation of the floor</param>
            /// <param name="level">The level in which this floor is located</param>
            public void AddFloor(float width, Vector2 center, float rotation, bool solid, int level)
            {
                data[level].Add(new Data(width, center, rotation, solid));
                float end = center.X + (float)Math.Cos(rotation) * width / 2f;
                if (end > max[level]) max[level] = (int)Math.Floor(end);
            }

            /// <summary>
            /// Loads a level into the world and floors list
            /// </summary>
            /// <param name="levelEnd">The current end of the game</param>
            /// <returns>The amount by which levelEnd should be incremented</returns>
            public int LoadLevel(int levelEnd)
            {
                for (int j = 0; j < levelLengths[0]; j++)
                {
                    floors.RemoveAt(0);
                }
                for (int k = 0; k < levelLengths.Length - 1; k++)
                {
                    levelLengths[k] = levelLengths[k + 1];
                }
                int i;
                if (LEVEL < 0)
                    i = rand.Next(data.Length);
                else
                    i = LEVEL;
                int length = 0;
                foreach (Data floor in data[i])
                {
                    Floor item = new Floor(texture, new Vector2(floor.Center.X + levelEnd, floor.Center.Y), new Vector2(floor.Width, Floor.FLOOR_HEIGHT), floor.Rotation);
                    if (floor.Solid)
                        item.ToggleSolid();
                    floors.Add(item);
                    length++;
                }
                levelLengths[levelLengths.Length - 1] = length;
                return max[i];
            }
        }
    }
}
