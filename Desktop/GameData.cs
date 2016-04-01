using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Source
{
    public static class GameData
    {
        public static int MAX_PLAYERS = 4;          // maximum amount of players at one time
        public static int[] PLAYERS;                // number of players
        public static int LEVEL_FILE = 1;           // default level
#if DEBUG
        public static int DEFAULT_PLAYERS = 1;      // default number of players
#else
        public static int DEFAULT_PLAYERS = 2;      // default number of players
#endif
        public static Vector2 PLAYER_START = new Vector2(1f, -10f);

        public static float PLAYER_WIDTH = 0.6f;    // width of player in m
        public static float PLAYER_HEIGHT = 1.8f;   // height of player in m
        public static Game1.CameraType CAMERA_TYPE = Game1.CameraType.Path;
        public static float NODE_SIZE = 2f;         // length of square side when drawing node in m

        // Settings for user-defined values
        public static int WINDOW_WIDTH = 1280;       // default width of window
        public static int WINDOW_HEIGHT = 720;       // default height of window
        public static bool FULLSCREEN = false;       // whether the game is fullscreen
        public static bool BORDERLESS = false;      // whether the game is in windowed borderless mode
        public static bool VSYNC = true;            // if the vertical retrace should be syncrhonized
        public static float VOLUME = 0.8f;                // volume for songs
        public static bool MUTED = true;            // whether the game music is muted

        public const float SCROLL_STEP = 90f;      // scale for zooming using the scroll wheel
        public const float ZOOM_STEP = 1.5f;       // scale by which zoom is changed with + and -
        public const float PIXEL_METER = 24f;      // pixels per meter for normal game
        public const float PIXEL_METER_EDIT = PIXEL_METER;  // pixels per meter when in edit mode for level
        public const int VIEW_WIDTH = 1920;        // width of unscaled screen in pixels
        public const int VIEW_HEIGHT = 1080;        // height of unscaled screen in pixels

        public const float SHADOW_SCALE = PIXEL_METER / 3.4f; // what light calculations will be normalized to (smaller scale = larger light)
        public static Color DARK_COLOR = new Color(new Vector3(0.1f));  // color mask for non-lit areas
        public static Color LIGHT_COLOR = Color.Wheat;

        public const float JUMP_HOLD_PROP = 0.7f;   // keep this proportion of your current Y velocity when holding a jump
        public const float JUMP_SPEED = 8f; // m/s -- the upwards velocity when player jumps (and holds it)
        //public const float JUMP_ACCEL = GRAVITY + 3f;   // m/s^2 -- acceleration when holding jump
        public const float JUMP_TIME = 0.55f;  // s -- can hold jump for this long and still have upwards velocity
        public const float WALL_JUMP_Y = 6f;   // m/s -- vertical jump off a wall
        public const float WALL_JUMP_X = 7.6f;   // m/s -- horizontal jump off a wall
        public const float WALL_STICK_VEL = 3.4f;     // m/s -- downwards velocity when sticking to a wall
        public const float WALL_STICK_ACCEL = 90f;   // m/s^2 -- how quickly player will accelerate to wall-sticking velocity (constant w/ respect to distance)
        //public const float WALL_JUMP_LEWAY = 0.3f;    // s -- time after which player can no longer wall jump after leaving a wall
        //public const float WALL_STICK_SCALE = 0.5f; // -- scale of vertical velocity when beginning to wall slide
        //public const float WALL_SLIDE_SCALE = 0.2f; // -- gravity scale when sliding on the wall

        public const float GRAVITY = 21f;   // m/s^2 -- gravity for players
        public const float GRAVITY_PART = 15f; // m/s^2 -- gravity for particles

        public const float MIN_VELOCITY = 1f;  // m/s -- what can be considered target velocity
        public const float MAX_VELOCITY = 20f; // m/s -- maximum horizontal velocity for player
        public const float LAND_ACCEL = 20f;   // m/s^2 -- acceleration applied when reaching TargetVelocity
        public const float AIR_ACCEL = LAND_ACCEL - 1f;   // m/s^2 -- acceleration while in air
        public const float LAND_DRAG = 3.1f;     // m/s^2 -- decelleration to 0 when on the ground
        public const float AIR_DRAG = 0.55f;       // m/s^2 -- decelleration to 0 when in air
        public const float OBSTACLE_JUMP = 25f; // m/s -- initial upwards velocity after vaulting off of an obstacle succesfully
        //public const float CLIMB_SPEED = 8f;     // m/s -- speed of climbing onto a ledge

        public const float PROJ_WIDTH = 1f;     // width of a projectile in m
        public const float PROJ_HEIGHT = 0.25f; // height of a projectile in m
        public const float PROJ_SPEED = 45f;    // speed of a projectile
        public const float PROJ_LIVE = 20.0f;     // lifetime of a projectile in s

        public const float DROP_SCALE = 0.4f;   // velocity scale of host when dropping a drop
        public const float DROP_LIVE = 3f;      // lifetime of a drop
        public const float DROP_SPEED_X = 0f;   // initial x speed of drop
        public const float DROP_SPEED_Y = -7f;  // initial y speed of drop
        public const float DROP_FRICTION = 4.7f; // ratio of speed lost per second for drop

        public const float GRAVITY_FORCE = 112f;  // G (in physics) in essence
        public const float BOMB_FORCE = 150f;        // force in m/s when a bomb explodes

        public const float JUMP_SLOW = 1.0f;   // -- x velocity is scaled by this when jumping
        public const float OBSTACLE_SLOW = 0.2f;    // -- player speed is reduced to this ratio when an obstacle is hit incorrectly
        //public const float WINDOW_SLOW = 0.2f;    // -- player speed is reduced to this ratio when a window is hit
        //public const float WALL_SLOW = 0.2f;    // -- player speed is reduced to this ratio when a wall is hit while flying

        public static float CAMERA_SPEED;           // speed of camera catchup (scales as the square root of distance)
        public const float FAST_CAMERA_SPEED = 70f;     // speed of camera when holding RightShift
        public const float SLOW_CAMERA_SPEED = 7f; // speed of camera when NOT holding RightShift
        //public const float CAMERA_SCALE_X = 4f;         // horizontal speed of camera
        //public const float CAMERA_SCALE_Y = 20f;        // vertical speed of camera
        //public const float MAX_CAMERA_SPEED_X = 1f;    // maximum x speed of camera
        //public const float MAX_CAMERA_SPEED_Y = 3f;    // maximum y speed of camera
        //public const float SCREEN_CATCHUP = 0.1f;       // proportion of distance camera will catch up every tick
        public const float SCREEN_LEFT = 0.3f;         // defines how far left the player can be on wobble-screen
        public const float SCREEN_RIGHT = 0.7f;       // defines the right limit of the player on wobble-screen
        public const float SCREEN_SPACE = 0.25f;        // camera will begin zooming out when the player are SCREEN_SPACE % of the screen apart from each other
        public const float SCREEN_TOP = 0.3f;          // defines the distance from the top or bottom of the screen for the player in wobble-screen
        public const float SPLIT_HEIGHT = 5f;        // heigh in pixels of separator for split-screen
        public const float DEAD_DIST = 240f;            // players this distance or more behind the average x will move to the maximum player (in pixels)
        public const double DEAD_TIME = 3;             // respawn time when a player gets behind the cutoff
        public const double PHASE_TIME = 1;            // the point at which the player will be visible again after dying to get the player prepared

        public const float PARTICLE_WIDTH = .125f;  // width of a particle (as a square)
        public const float PARTICLE_LIFETIME = 0.6f;  // how long a particle lasts for (in s)
        public const float PARTICLE_LIFETIME_TEXT = 0.3f; // lifetime of a text particle
        public const float PARTICLE_MAX_SPIN = 10f; // maximum angular velocity (in rad/s)
        public const float PARTICLE_X = 2f;         // maximum x velocity of a particle when randomly generating in either direction
        public const float PARTICLE_Y = 5f;         // maximum y velocity of a particle in either direction
        //public const int NUM_PART_WALL = 10;        // number of particles to spawn when a wall is exploded
        //public const int NUM_PART_FLOOR = 5;        // number of particles to spawn when slamming and a hole is made
        public const int NUM_PART_OBSTACLE = 3;     // number of particles to spawn when an obstacle is hit or destroyed

        public const float STUN_TIME = 0.9f;    // time of stun
        public const float OBSTACLE_STUN = 0.5f;    // time of stun after vaulting off obstacle
        public const float OBSTACLE_HIT_STUN = 0.4f;    // time of stun after hitting an obstacle
        public const float STUN_RADIUS = 3.3f;    // radius within which a player will be stuned from an explosion (in m)

        // Character constants
#if DEBUG
        public const int CHARACTER = 1;
#endif

        public const float PLATFORM_DIST = 1.5f;    // distance from player to y center of platform
        public const float PLATFORM_WIDTH = 10f;    // width of platform
        public const float PLATFORM_HEIGHT = 1f;    // height of platform
        public const float PLATFORM_COOLDOWN = 4f;  // cooldown for platform ability
        public const float PLATFORM_LIFE = 2.2f;      // how long the platform lasts before despawning
        public const float INVERT_TIME = 10f;        // time during which controls will be inverted
        public const float INVERT_COOLDOWN = 22f;   // cooldown for casting invert

        public const float GRAPPLE_HEIGHT = 3f;     // pixel height of grapple rope
        public const float MAX_GRAPPLE = 12f;       // maximum grapple distance (in m)
        public const float GRAPPLE_ANGLE = -1f;     // direction height of grapple assuming horizontal direction of +1
        public const float GRAPPLE_ELASTICITY = 4f;      // scale of elasticity, where higher values = more rigid
        public const float GRAPPLE_BOOST = 1.0f;      // boost in momentum after releasing a rope
        //public const float GRAPPLE_HELP = 10f;      // help to push player when no manual input
        //public const float GRAPPLE_HELP_MIN = 3f;  // minimum length of velocity while swinging
        public const float TRAP_COOLDOWN = 4f;     // cooldown for dropping a trap
        public const float TRAP_FORCE = 90f;       // force when trap expires and explodes
        public const int TRAP_PARTICLES = 20;    // number of particles when trap explodes

        public const float BLINK_COOLDOWN = 2.8f;     // cooldown for blink ability
        public const float BLINK_DIST = 14f;       // when blinking, player will move this many meters
        public const float TIMEWARP_TIME = 2f;      // how many seconds should be reversed
        public const float TIMEWARP_COOLDOWN = 5f;  // cooldown for timewarp ability

        public const float JETPACK_ACCEL_UP = 15f + GRAVITY;  // m/s^2 -- upwards acceleration while jetpacking and going up
        public const float JETPACK_ACCEL_DOWN = 60f + GRAVITY;  // m/s^2 -- updwards acceleration while jetpacking and going down
        public const float JETPACK_TIME = 0.9f; // jetpack lasts for this long without touching ground
        public const float JETPACK_PARTICLES = 1 / 40f; // number of particles spawned by jetpack per second
        public const float ROCKET_COOLDOWN = 4f;    // cooldown for rocket explosive ability
        public const float ROCKET_X = 25f;          // initial x velocity of rocket (w/o player)
        public const float ROCKET_Y = 3f;          // initial y velocity of rocket (w/o player)
        public const float ROCKET_SCALE = 0.3f;     // scale of player's velocity in rocket when firing
        public const float ROCKET_GRAVITY = 10f;    // gravity for rocket
        public const float ROCKET_PARTICLES = 1 / 60f; // number of particles spawned by rocket per second
        public const int ROCKET_EXPLODE_PART = 20; // number of particles when rocket explodes
        public const float ROCKET_FORCE = 130f;    // force when rocket hits something and explodes

        public const int TOTAL_JUMPS = 3;       // number of jumps the acrobat can do in total (including intial jump)
        public const float AIR_JUMP_SPEED = JUMP_SPEED + 4f;    // velocity of jump when acrobat jumping in air
        public const float BOOMERANG_COOLDOWN = 6f;    // cooldown for boomerang throw ability
        public const float BOOMERANG_X = 20f;          // initial x velocity of boomerang (w/o player)
        public const float BOOMERANG_Y = 18f;          // initial y velocity of boomerang (w/o player)
        public const float BOOMERANG_SCALE = 0.1f;     // scale of player's velocity in boomerang when throwing
        public const float BOOMERANG_ATTRACT = 90f;    // attraction of boomerang back to player (constant with regards to distance)
        //public const float BOOMERANG_GRAVITY = 1f;    // gravity for boomerang
        public const float BOOMERANG_FORCE = 580f;    // force with which boomerang pulls other things inside
        public const float BOOMERANG_ANTIGRAV = 7f;    // m radius after which player is "sucked in" and loses all control
        public const float MAX_FORCE = 105f;        // maximum (m/s^2)^2 for a gravity force
        public const float BOOMERANG_LIFE_CUTOFF = PROJ_LIVE - 1f;  // leway time during which boomerang is leaving player's hands

        //public const int NUM_WORLDS = 3;    // number of worlds to load
        // format of WORLD_LAYERS is:
        //  first array is each individual world
        //  second array is the layers in the world (in order of back to front)
        //  inner array is {speed, center, size} of layer respectively (must have all 3 elements)
        public static float[][][] WORLD_LAYERS =
        {
            new float[][]
            {
                new float[] { 0.08f, 0.5f,  0.1f },
                new float[] { 0.2f,  0.55f, 0.25f },
                new float[] { 0.4f,  0.6f,  0.5f }
            },
            new float[][]
            {
                new float[] { 0.4f, 0.5f, 1.4f }
            },
            new float[][]
            {
                new float[] { 0f,    0.5f, 4.5f },
                new float[] { 0.07f, 0.5f, 2.5f },
                new float[] { 0.15f, 0.5f, 2.5f },
                new float[] { 0.22f, 0.5f, 2.5f }
            }
        };

        public static string Version
        {
            get
            {
                Version ver = Assembly.GetEntryAssembly().GetName().Version;
                return String.Format("{0}.{1}.{2}", ver.Major, ver.Minor, ver.Build);
            }
        }

        //public static int GetSeed
        //{
        //    get
        //    {
        //        return (DateTime.UtcNow - new DateTime(2015, 1, 1)).Minutes / GameData.NEW_SEED_MINS;
        //    }
        //}

        public enum ControlKey
        {
            Special1, Special2, Special3, Left, Right, JumpHeld, Down
        }

        public interface Controls
        {
            bool Special1 { get; }   // toggle
            bool Special2 { get; }  // toggle
            bool Special3 { get; }  // toggle
            bool Left { get; }     // hold
            bool Right { get; }     // hold
            bool JumpHeld { get; }  // hold
            bool Down { get; }     // hold

            string ToString();
        }

        public class SimulatedControls : Controls
        {
            public bool Special1 { get; set; }
            public bool Special2 { get; set; }
            public bool Special3 { get; set; }
            public bool Left { get; set; }
            public bool Right { get; set; }
            public bool JumpHeld { get; set; }
            public bool Down { get; set; }

            public SimulatedControls(Game1 game)
            {
                Special1 = false;
                Special2 = false;
                Special3 = false;
                Left = false;
                Right = false;
                JumpHeld = false;
                Down = false;
            }
        }

        public class KeyboardControls : Controls
        {
            public bool Special1 { get { return game.ToggleKey(special1); } }
            public bool Special2 { get { return game.ToggleKey(special2); } }
            public bool Special3 { get { return game.ToggleKey(special3); } }
            public bool Left { get { return Keyboard.GetState().IsKeyDown(left); } }
            public bool Right { get { return Keyboard.GetState().IsKeyDown(right); } }
            public bool Jump { get { return game.ToggleKey(jump); } }
            public bool JumpHeld { get { return Keyboard.GetState().IsKeyDown(jump); } }
            public bool Down { get { return Keyboard.GetState().IsKeyDown(down); } }

            private Game1 game;
            private Keys special1, special2, special3, left, right, jump, down;

            public KeyboardControls(Game1 game, Keys special1, Keys special2, Keys special3, Keys left, Keys right, Keys jump, Keys down)
            {
                this.game = game;
                this.special1 = special1;
                this.special2 = special2;
                this.special3 = special3;
                this.left = left;
                this.right = right;
                this.jump = jump;
                this.down = down;
            }

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Left = " + left)
                    .AppendLine("Right = " + right)
                    .AppendLine("Jump = " + jump)
                    .AppendLine("Action = " + down)
                    .AppendLine("Special1 = " + special1)
                    .AppendLine("Special2 = " + special2)
                    .AppendLine("Special3 = " + special3);

                return builder.ToString();
            }
        }

        public class GamePadControls : Controls
        {
            public bool Special1 { get { return game.ToggleButton(playerIndex, special1); } }
            public bool Special2 { get { return game.ToggleButton(playerIndex, special2); } }
            public bool Special3 { get { return game.ToggleButton(playerIndex, special3); } }
            public bool Left { get { return GamePad.GetState(playerIndex, GamePadDeadZone.Circular).IsButtonDown(left); } }
            public bool Right { get { return GamePad.GetState(playerIndex, GamePadDeadZone.Circular).IsButtonDown(right); } }
            public bool JumpHeld { get { return GamePad.GetState(playerIndex, GamePadDeadZone.Circular).IsButtonDown(jump); } }
            public bool Down { get { return GamePad.GetState(playerIndex, GamePadDeadZone.Circular).IsButtonDown(down); } }

            private Game1 game;
            private PlayerIndex playerIndex;
            private Buttons special1, special2, special3, left, right, jump, down;

            public GamePadControls(Game1 game, PlayerIndex playerIndex, Buttons special1, Buttons special2, Buttons special3, Buttons left, Buttons right, Buttons jump, Buttons down)
            {
                this.game = game;
                this.playerIndex = playerIndex;
                this.special1 = special1;
                this.special2 = special2;
                this.special3 = special3;
                this.left = left;
                this.right = right;
                this.jump = jump;
                this.down = down;
            }

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Left = " + left)
                    .AppendLine("Right = " + right)
                    .AppendLine("Jump = " + jump)
                    .AppendLine("Action = " + down)
                    .AppendLine("Special1 = " + special1)
                    .AppendLine("Special2 = " + special2)
                    .AppendLine("Special3 = " + special3);
                return builder.ToString();
            }
        }
    }
}
