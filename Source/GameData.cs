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
        // TODO stuff some of the user-configurable stuff into a main menu screen (like controls and # of players)
        //public const int LEVEL = -1;            // if this is greater than -1, levels will not be procedurally generated (useful for editing)
        public const int numPlayers = 2;            // number of players

        public static bool[] useController = { false, false, false };    // true means the player at the index will be using a controller.
        public static Controls[] keyboardControls = {            // defines the keyboard controls which will be used
                                                       new Controls(Keys.Left, Keys.Right, Keys.Up, Keys.Down, Keys.RightShift),
                                                       new Controls(Keys.A, Keys.D, Keys.W, Keys.S, Keys.LeftShift),
                                                       new Controls(Keys.J, Keys.L, Keys.I, Keys.K, Keys.O)};

        public const float LOAD_NEW = 100f;     // the next level will be loaded when the player is this far from the current end

        public const int MIN_LEVEL_WIDTH = 20;
        public const int MAX_LEVEL_WIDTH = 50;
        public const int MIN_LEVEL_STEP = 4;
        public const int MAX_LEVEL_STEP = 6;
        public const int MIN_NUM_FLOORS = 4;
        public const int MAX_NUM_FLOORS = 11;

        public const int MAX_LEVELS_LOADED = 4;    // how many levels to keep loaded at a given time
        public const float PIXEL_METER = 32f;      // pixels per meter for normal game
        public const float PIXEL_METER_EDIT = 8f;  // pixels per meter when in edit mode for level
        public const int VIEW_WIDTH = 1280;        // width of unscaled screen in pixels
        public const int VIEW_HEIGHT = 720;        // height of unscaled screen in pixels
        public static Vector2 PLAYER_POSITION = new Vector2(2, -20f);   // starting position of player
        public static Color[] playerColors = { Color.Red, Color.Yellow, Color.Purple };     // colors of each player

        public const float DEAD_SPEED = 20f; // m/s -- the speed at which the dead 'wave' on the left moves
        public const float DEAD_MAX = 30f; // m -- maximum distance between player and death if player is doing well
        public const int DEAD_WIDTH = 200;
        public const int DEAD_HEIGHT = 2000;

        //public const float MIN_VELOCITY = 1f;  // m/s -- what can be considered 0 horizontal velocity
        public const float SLOW_SPEED = 13f; // m/s -- speed player is going at when slowing down
        public const float RUN_VELOCITY = 20f; // m/s -- maximum horizontal velocity for player
        public const float BOOST_SPEED = 27f; // m/s -- horizontal velocity when boosting
        public const float MAX_ACCEL = 40f;   // m/s^2 -- the impulse which is applied when player starts moving after standing still
        public const float JUMP_SPEED = 18f; // m/s -- the initial upwards velocity when player jumps
        public const float SLAM_SPEED = 17f; // m/s -- the speed at which the player goes down when slamming
        public const float SLOWDOWN = 45f;      // m/s^2 -- impulse applied in opposite direction of travel to simulate friction
        public const float AIR_RESIST = 0.75f; //     -- air resistance on a scale of 0 to 1, where 1 is as if player is on ground
        //public const double JUMP_WAIT = 0.5;   // s   -- how long the player needs to wait before jumping again
        //public const float PUSH_VEL = 1f;      // m/s -- the player is given a little push going down platforms under this velocity
        //public const float PUSH_POW = 10f;     // m/s -- the impulse applied to the player to get down a platform
        public const float MIN_WOBBLE = 0f;  //     -- the minimum ratio between max velocity and (max - current velocity) for wobbling
        public const float MAX_WOBBLE = 0f;    //     -- the maximum ratio for wobbling; we don't want wobble amplifier 40x
        public const int LEVEL_DIST_MIN = 3;   // the min space between levels
        public const int LEVEL_DIST_MAX = 15;   // max space between levels
        public const float RESPAWN_DIST = 10;

        public const string SONG = "Chiptune dash";    // the song to play, over, and over, and over again. NEVER STOP THE PARTY!
        public const float VOLUME = 0f;                // volume for song

        public const float CAMERA_SCALE = 20f;         // how fast the camera moves
        public const float MAX_CAMERA_SPEED_X = 5f;    // maximum x speed of camera
        public const float MAX_CAMERA_SPEED_Y = 3f;    // maximum y speed of camera
        public const float SCREEN_LEFT = 0.2f;         // defines how far left the player can be on wobble-screen
        public const float SCREEN_RIGHT = 0.35f;       // defines the right limit of the player on wobble-screen
        public const float SCREEN_SPACE = 0.65f;        // camera will begin zooming out when the player are SCREEN_SPACE % of the screen apart from each other
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
    }
}
