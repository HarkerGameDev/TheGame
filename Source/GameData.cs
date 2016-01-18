using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Source.Collisions;

namespace Source
{
    internal static class GameData
    {
#if DEBUG
        public const int numPlayers = 1;
#else
        public const int numPlayers = 2;            // number of players
#endif

        public const int NEW_SEED_MINS = 10;     // minutes until a new level seed will be generated
        public const float LOAD_NEW = 70f;     // the next level will be loaded when the player is this far from the current end
        public const int MAX_FLOORS = 50;    // maximum number of floors at any given time
        public const int MAX_WALLS = 90;    // maximum number of walls
        public const int MAX_OBSTACLES = 40;    // maximum obstacles

        public const int MIN_LEVEL_WIDTH = 20;  // width of levels
        public const int MAX_LEVEL_WIDTH = 50;
        public const float MIN_FLOOR_HOLE = 9f; // size of a hole inside a building
        public const float MAX_FLOOR_HOLE = 12f - MIN_FLOOR_HOLE;
        public const float MIN_FLOOR_DIST = 15; // width of a floor until a hole is reached
        public const float MAX_FLOOR_DIST = 60 - MIN_FLOOR_DIST;
        public const int MIN_LEVEL_STEP = 5;    // size of each floor in building
        public const int MAX_LEVEL_STEP = 9;
        public const int MIN_NUM_FLOORS = 5;    // number of floors per building
        public const int MAX_NUM_FLOORS = 11;
        public const int MIN_WALL_DIST = 10;    // distance between walls inside rooms
        public const int MAX_WALL_DIST = 90;
        public const int MIN_OBSTACLE_DIST = 10;    // distance between obstacles inside rooms
        public const int MAX_OBSTACLE_DIST = 50;
        public const int LEVEL_DIST_MIN = 5;    // the space between different buildings
        public const int LEVEL_DIST_MAX = 12;

        public const float FLOOR_HOLE = 4.4f;   // size in m of hole to make when slamming
        public const int WINDOW_HEALTH = 1;     // windows are on the side of buildings
        public const int WALL_HEALTH = 3;       // walls are inside the buildings themselves
        public const int STAIR_HEALTH = 2;  // hits until a stair is broken
        public const float MIN_FLOOR_WIDTH = 2f;  // floors cannot be smaller than this (by random generation or slamming)
        public const double STAIR_CHANCE = 0.6; // chance a hole will have a stair to it (from 0 to 1)
        public const float STAIR_WIDTH = MIN_FLOOR_DIST;   // horizontal distance of a stair
        public const float MIN_STAIR_DIST = 6.3f;    // minimum size of a hole for a stair to be created

        public const float PLAYER_START = 2f;   // starting x position of player
        public const int MIN_SPAWN = MAX_LEVEL_STEP * 2 + 1;  // minimum spawning position (vertically)
        public const int MAX_SPAWN = MIN_LEVEL_STEP * MIN_NUM_FLOORS + 10;  // maximum spawning position (vertically)
        public const float SPAWN_PROTECT = 5f;  // stuff this far apart from the player will be destroyed when the player is spawned

        public const float ZOOM_STEP = 1.5f;       // scale by which zoom is changed with + and -
        public const float PIXEL_METER = 24f;      // pixels per meter for normal game
        public const float PIXEL_METER_EDIT = 8f;  // pixels per meter when in edit mode for level
        public const int VIEW_WIDTH = 1280;        // width of unscaled screen in pixels
        public const int VIEW_HEIGHT = 720;        // height of unscaled screen in pixels

        public const float SHADOW_SCALE = PIXEL_METER / 3.4f; // what light calculations will be normalized to for (smaller scale = larger light)
        public static Color DARK_COLOR = new Color(new Vector3(0.1f));  // color mask for non-lit areas
        public static Color LIGHT_COLOR = Color.Wheat;

#if DEBUG
        public const float DEAD_START = 0;
        public const float DEAD_END = 0;
#else
        public const float DEAD_START = RUN_VELOCITY - 2f;   // m/s -- the speed of dead wave at start of game
        public const float DEAD_END = RUN_VELOCITY + 0.0f; // m/s -- the speed at which the dead 'wave' on the left moves by the end of the game
#endif

        public const float DEAD_MAX = 40f; // m -- maximum distance between player and death if player is doing well
        public const int DEAD_WIDTH = 600;
        public const int DEAD_HEIGHT = 2000;
        public const float WIN_TIME = 60f;  // s -- survive for this long to win
        public const float MAX_SPEED_SCALE = 0.9f; // game is this much faster by the end of win
        public const int WIN_SCORE = 10;    // player gets 1 point for every WIN_SCORE seconds they survive
        public const int LOSE_SCORE = 5;    // score to lose when hit by purple
        public const int DEATH_LOSS = 2;    // score to lose when the purple only catches 1 player

#if DEBUG
        public const float BOOST_SPEED = -1; // m/s -- horizontal velocity when boosting
        public const float BOOST_REGEN = 0.1f; // boost will be refilled in this time (from 0)
#else
        public const float BOOST_SPEED = 26f; // m/s -- horizontal velocity when boosting
        public const float BOOST_REGEN = 8.7f; // boost will be refilled in this time (from 0)
#endif

        public const float GRAVITY = 36f;   // m/s^2 -- gravity for players
        public const float GRAVITY_PART = 15f; // m/s^2 -- gravity for particles
        public const float MIN_VELOCITY = 1f;  // m/s -- what can be considered target velocity
        public const float RUN_VELOCITY = 22f; // m/s -- maximum horizontal velocity for player
        public const float MAX_ACCEL = 30f;   // m/s^2 -- the impulse which is applied when player starts moving after standing still
        public const float JUMP_SPEED = 18f; // m/s -- the initial upwards velocity when player jumps
        public const float OBSTACLE_JUMP = 25f; // m/s -- initial upwards velocity after vaulting off of an obstacle succesfully
        public const float JUMP_SLOW = 0.85f;   // -- x velocity is scaled by this when jumping
        public const float WINDOW_SLOW = 0.2f;    // -- player speed is reduced to this ratio when a window is hit
        public const float WALL_SLOW = 0.2f;    // -- player speed is reduced to this ratio when a wall is hit while flying
        public const float SLAM_SPEED = 17f; // m/s -- the speed at which the player goes down when slamming
        public const float MIN_WOBBLE = 0f;  //     -- the minimum ratio between max velocity and (max - current velocity) for wobbling
        public const float MAX_WOBBLE = 0f;    //     -- the maximum ratio for wobbling; we don't want wobble amplifier 40x
        public const float RESPAWN_DIST = 10;
        public const float CLIMB_SPEED = 8f;     // m/s -- speed of climbing onto a ledge

        public const float BOOST_LENGTH = 6f;  // how long a player can boost for
        public const float SHOOT_COST = 0.3f; // boost bar cost of a shot

        //public const string SONG = "afln_s_gdc-1.wav";    // the song to play, over, and over, and over again. NEVER STOP THE PARTY!
        //public const float VOLUME = 0.0f;                // volume for song

        public const float CAMERA_SCALE_X = 4f;         // how fast the camera moves
        public const float CAMERA_SCALE_Y = 20f;        // vertical speed of camera
        public const float MAX_CAMERA_SPEED_X = 1f;    // maximum x speed of camera
        public const float MAX_CAMERA_SPEED_Y = 3f;    // maximum y speed of camera
        public const float SCREEN_LEFT = 0.2f;         // defines how far left the player can be on wobble-screen
        public const float SCREEN_RIGHT = 0.35f;       // defines the right limit of the player on wobble-screen
        public const float SCREEN_SPACE = 0.45f;        // camera will begin zooming out when the player are SCREEN_SPACE % of the screen apart from each other
        public const float SCREEN_TOP = 0.3f;          // defines the distance from the top or bottom of the screen for the player in wobble-screen
        public const float DEAD_DIST = 240f;            // players this distance or more behind the average x will move to the maximum player (in pixels)
        public const double DEAD_TIME = 3;             // respawn time when a player gets behind the cutoff
        public const double PHASE_TIME = 1;            // the point at which the player will be visible again after dying to get the player prepared
        public const float ACTION_TIME = 0.2f;          // leway for how much time during which "Action" button applies after being hit

        public const float PARTICLE_WIDTH = .125f;  // width of a particle (as a square)
        public const float PARTICLE_LIFETIME = 1.4f;  // how long a particle lasts for (in s)
        public const float PARTICLE_LIFETIME_TEXT = 0.3f; // lifetime of a text particle
        public const float PARTICLE_MAX_SPIN = 10f; // maximum angular velocity (in rad/s)
        public const float PARTICLE_X = 4f;         // maximum x velocity of a particle when randomly generating in either direction
        public const float PARTICLE_Y = 5f;         // maximum y velocity of a particle in either direction
        public const int NUM_PART_WALL = 10;        // number of particles to spawn when a wall is exploded
        public const int NUM_PART_FLOOR = 5;        // number of particles to spawn when slamming and a hole is made
        public const int NUM_PART_OBSTACLE = 3;     // number of particles to spawn when an obstacle is hit or destroyed
        public const float BOOST_PART_TIME = 1 / 30f;   // time until a new particle will be spawned when boosting

        public const float PROJ_WIDTH = 1f;     // width of a projectile in m
        public const float PROJ_HEIGHT = 0.25f; // height of a projectile in m
        public const float PROJ_SPEED = 60f;    // speed of a projectile
        public const float PROJ_LIVE = 1.0f;     // lifetime of a projectile in s

        public const float DROP_LIVE = 1f;      // lifetime of a drop
        public const float DROP_SPEED_X = 8f;   // initial x speed of drop
        public const float DROP_SPEED_Y = -14f;  // initial y speed of drop
        public const float DROP_FRICTION = 0.6f; // ratio of speed lost per second
        public const float STUN_TIME = 0.5f;    // time of stun
        public const float OBSTACLE_STUN = 0.5f;    // time of stun after vaulting off obstacle
        public const float OBSTACLE_HIT_STUN = 0.4f;    // time of stun after hitting an obstacle
        public const float STUN_RADIUS = 3.3f;    // radius within which a player will be stuned from an explosion (in m)
        public const float GRAVITY_FORCE = 150f;  // G (in physics) in essence
        public const float BOMB_FORCE = 200f;        // force in m/s when a bomb explodes
        public const float MAX_FORCE = 40f;    // maximum (m/s^2)^2 for a gravity force

        public const float BUTTON_WIDTH = 0.4f;  // width of a button in proportion of screen
        public const float BUTTON_HEIGHT = 0.12f;    // height of button in proportion

        public const float BACK1_MOVE = 0.4f;   // speed of layer
        public const float BACK1_CENTER = 0.6f; // y center on screen
        public const float BACK1_SIZE = 0.5f;   // size of layer
        public static Color BACK1_COLOR = Color.White; // color mask for layer

        public const float BACK2_MOVE = 0.2f;   // speed of layer
        public const float BACK2_CENTER = 0.55f; // y center on screen
        public const float BACK2_SIZE = 0.25f;   // size of layer
        public static Color BACK2_COLOR = Color.Gray; // color mask for layer

        public const float BACK3_MOVE = 0.08f;   // speed of layer
        public const float BACK3_CENTER = 0.5f; // y center on screen
        public const float BACK3_SIZE = 0.1f;   // size of layer
        public static Color BACK3_COLOR = Color.Black; // color mask for layer

        public static string Version
        {
            get
            {
                Version ver = Assembly.GetEntryAssembly().GetName().Version;
                return String.Format("{0}.{1}.{2}", ver.Major, ver.Minor, ver.Build);
            }
        }

        public static int GetSeed
        {
            get
            {
                return (DateTime.UtcNow - new DateTime(2015, 1, 1)).Minutes / GameData.NEW_SEED_MINS;
            }
        }

        public enum ControlKey
        {
            Special1, Special2, Special3, Boost, Jump, Slam, Action
        }

        // TODO intuitive controls
        public interface Controls
        {
            bool Special1 { get; }   // toggle
            bool Special2 { get; }  // toggle
            bool Special3 { get; }  // toggle
            bool Boost { get; }     // hold
            bool Jump { get; }      // hold
            bool Slam { get; }      // hold
            bool Action { get; }     // toggle

            string ToString();
        }

        public class SimulatedControls : Controls
        {
            public bool Special1 { get; set; }
            public bool Special2 { get; set; }
            public bool Special3 { get; set; }
            public bool Boost { get; set; }
            public bool Jump { get; set; }
            public bool Slam { get; set; }
            public bool Action { get; set; }

            public SimulatedControls(Game1 game)
            {
                Special1 = false;
                Special2 = false;
                Special3 = false;
                Boost = false;
                Jump = false;
                Slam = false;
                Action = false;
            }
        }

        public struct KeyboardControls : Controls
        {
            public bool Special1 { get { return game.ToggleKey(special1); } }
            public bool Special2 { get { return game.ToggleKey(special2); } }
            public bool Special3 { get { return game.ToggleKey(special3); } }
            public bool Boost { get { return Keyboard.GetState().IsKeyDown(boost); } }
            public bool Jump { get { return Keyboard.GetState().IsKeyDown(jump); } }
            public bool Slam { get { return Keyboard.GetState().IsKeyDown(slam); } }
            public bool Action { get { return game.ToggleKey(action); } }

            private Game1 game;
            private Keys special1, special2, special3, boost, jump, slam, action;

            public KeyboardControls(Game1 game, Keys special1, Keys special2, Keys special3, Keys boost, Keys jump, Keys slam, Keys action)
            {
                this.game = game;
                this.special1 = special1;
                this.special2 = special2;
                this.special3 = special3;
                this.boost = boost;
                this.jump = jump;
                this.slam = slam;
                this.action = action;
            }

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Boost = " + boost)
                    .AppendLine("Jump = " + jump)
                    .AppendLine("Slam = " + slam)
                    .AppendLine("Action = " + action)
                    .AppendLine("Special1 = " + special1)
                    .AppendLine("Special2 = " + special2)
                    .AppendLine("Special3 = " + special3);

                return builder.ToString();
            }
        }

        public struct GamePadControls : Controls
        {
            public bool Special1 { get { return game.ToggleButton(playerIndex, special1); } }
            public bool Special2 { get { return game.ToggleButton(playerIndex, special2); } }
            public bool Special3 { get { return game.ToggleButton(playerIndex, special3); } }
            public bool Boost { get { return GamePad.GetState(playerIndex, GamePadDeadZone.Circular).IsButtonDown(boost); } }
            public bool Jump { get { return GamePad.GetState(playerIndex, GamePadDeadZone.Circular).IsButtonDown(jump); } }
            public bool Slam { get { return GamePad.GetState(playerIndex, GamePadDeadZone.Circular).IsButtonDown(slam); } }
            public bool Action { get { return game.ToggleButton(playerIndex, action); } }

            private Game1 game;
            private PlayerIndex playerIndex;
            private Buttons special1, special2, special3, boost, jump, slam, action;

            public GamePadControls(Game1 game, PlayerIndex playerIndex, Buttons special1, Buttons special2, Buttons special3, Buttons boost, Buttons jump, Buttons slam, Buttons action)
            {
                this.game = game;
                this.playerIndex = playerIndex;
                this.special1 = special1;
                this.special2 = special2;
                this.special3 = special3;
                this.boost = boost;
                this.jump = jump;
                this.slam = slam;
                this.action = action;
            }

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Boost = " + boost)
                    .AppendLine("Jump = " + jump)
                    .AppendLine("Slam = " + slam)
                    .AppendLine("Action = " + action)
                    .AppendLine("Special1 = " + special1)
                    .AppendLine("Special2 = " + special2)
                    .AppendLine("Special3 = " + special3);
                return builder.ToString();
            }
        }
    }
}
