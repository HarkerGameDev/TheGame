﻿using System;
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

        public static Vector2 PLAYER_START = new Vector2(1f, -10f);

        public const float ZOOM_STEP = 1.5f;       // scale by which zoom is changed with + and -
        public const float PIXEL_METER = 24f;      // pixels per meter for normal game
        public const float PIXEL_METER_EDIT = 8f;  // pixels per meter when in edit mode for level
        public const int VIEW_WIDTH = 1280;        // width of unscaled screen in pixels
        public const int VIEW_HEIGHT = 720;        // height of unscaled screen in pixels

        public const float SHADOW_SCALE = PIXEL_METER / 3.4f; // what light calculations will be normalized to (smaller scale = larger light)
        public static Color DARK_COLOR = new Color(new Vector3(0.1f));  // color mask for non-lit areas
        public static Color LIGHT_COLOR = Color.Wheat;

        public const float JUMP_SPEED = 16f; // m/s -- the initial upwards velocity when player jumps
        public const float JUMP_ACCEL = GRAVITY + 3f;   // m/s^2 -- acceleration when holding jump
        public const float JUMP_TIME = 0.2f;  // s -- can hold jump for this long and still have upwards velocity
        public const float WALL_JUMP_Y = 12f;   // m/s -- vertical jump off a wall
        public const float WALL_JUMP_X = 16f;   // m/s -- horizontal jump off a wall
        public const float WALL_JUMP_LEWAY = 0.3f;    // s -- time after which player can no longer wall jump after leaving a wall
        public const float WALL_STICK_SCALE = 0.5f; // -- scale of vertical velocity when beginning to wall slide
        public const float WALL_SLIDE_SCALE = 0.2f; // -- gravity scale when sliding on the wall
        //public const float JETPACK_ACCEL = 7f;  // m/s^2 -- upwards acceleration while jetpacking
        //public const float JETPACK_SPEED = 14f; // m/s -- upwards speed while using jetpack
        //public const float JETPACK_REGEN = 10f; // jetpack will be refilled in this time

        public const float GRAVITY = 36f;   // m/s^2 -- gravity for players
        public const float GRAVITY_PART = 15f; // m/s^2 -- gravity for particles

        public const float MIN_VELOCITY = 1f;  // m/s -- what can be considered target velocity
        public const float RUN_VELOCITY = 22f; // m/s -- maximum horizontal velocity for player
        public const float MAX_ACCEL = 30f;   // m/s^2 -- acceleration applied when reaching TargetVelocity
        public const float AIR_ACCEL = 15f;   // m/s^2 -- acceleration while in air
        public const float OBSTACLE_JUMP = 25f; // m/s -- initial upwards velocity after vaulting off of an obstacle succesfully
        public const float CLIMB_SPEED = 8f;     // m/s -- speed of climbing onto a ledge

        public const float JUMP_SLOW = 0.85f;   // -- x velocity is scaled by this when jumping
        public const float OBSTACLE_SLOW = 0.2f;    // -- player speed is reduced to this ratio when an obstacle is hit incorrectly
        //public const float WINDOW_SLOW = 0.2f;    // -- player speed is reduced to this ratio when a window is hit
        //public const float WALL_SLOW = 0.2f;    // -- player speed is reduced to this ratio when a wall is hit while flying

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

        public const float PARTICLE_WIDTH = .125f;  // width of a particle (as a square)
        public const float PARTICLE_LIFETIME = 1.4f;  // how long a particle lasts for (in s)
        public const float PARTICLE_LIFETIME_TEXT = 0.3f; // lifetime of a text particle
        public const float PARTICLE_MAX_SPIN = 10f; // maximum angular velocity (in rad/s)
        public const float PARTICLE_X = 4f;         // maximum x velocity of a particle when randomly generating in either direction
        public const float PARTICLE_Y = 5f;         // maximum y velocity of a particle in either direction
        //public const int NUM_PART_WALL = 10;        // number of particles to spawn when a wall is exploded
        //public const int NUM_PART_FLOOR = 5;        // number of particles to spawn when slamming and a hole is made
        public const int NUM_PART_OBSTACLE = 3;     // number of particles to spawn when an obstacle is hit or destroyed

        public const float PROJ_WIDTH = 1f;     // width of a projectile in m
        public const float PROJ_HEIGHT = 0.25f; // height of a projectile in m
        public const float PROJ_SPEED = 60f;    // speed of a projectile
        public const float PROJ_LIVE = 1.0f;     // lifetime of a projectile in s

        public const float DROP_LIVE = 1f;      // lifetime of a drop
        public const float DROP_SPEED_X = 8f;   // initial x speed of drop
        public const float DROP_SPEED_Y = -14f;  // initial y speed of drop
        public const float DROP_FRICTION = 0.6f; // ratio of speed lost per second for drop

        public const float STUN_TIME = 0.5f;    // time of stun
        public const float OBSTACLE_STUN = 0.5f;    // time of stun after vaulting off obstacle
        public const float OBSTACLE_HIT_STUN = 0.4f;    // time of stun after hitting an obstacle
        public const float STUN_RADIUS = 3.3f;    // radius within which a player will be stuned from an explosion (in m)

        public const float GRAVITY_FORCE = 150f;  // G (in physics) in essence
        public const float BOMB_FORCE = 200f;        // force in m/s when a bomb explodes
        public const float MAX_FORCE = 40f;    // maximum (m/s^2)^2 for a gravity force

        public const float BUTTON_WIDTH = 0.4f;  // width of a button in proportion of screen
        public const float BUTTON_HEIGHT = 0.12f;    // height of button in proportion

        // Character constants
        public const float PLATFORM_DIST = 1.5f;    // distance from player to y center of platform
        public const float PLATFORM_WIDTH = 10f;    // width of platform
        public const float PLATFORM_HEIGHT = 1f;    // height of platform
        public const float PLATFORM_COOLDOWN = 5f;  // cooldown for platform ability
        public const float PLATFORM_LIFE = 1.2f;      // how long the platform lasts before despawning

        public const float GRAPPLE_HEIGHT = 3f;     // pixel height of grapple rope
        public const float MAX_GRAPPLE = 20f;       // maximum grapple distance (in m)
        public const float GRAPPLE_ANGLE = -1f;     // direction height of grapple assuming horizontal direction of 1

        //public const int NUM_WORLDS = 3;    // number of worlds to load
        // format of WORLD_LAYERS is:
        //  first array is each individual world
        //  second array is the layers in the world (in order of back to front)
        //  inner array is {speed, center, size} of layer respectively (must be size 3)
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
            Special1, Special2, Special3, Left, Right, Jump, JumpHeld, Action
        }

        public interface Controls
        {
            bool Special1 { get; }   // toggle
            bool Special2 { get; }  // toggle
            bool Special3 { get; }  // toggle
            bool Left { get; }     // hold
            bool Right { get; }     // hold
            bool JumpHeld { get; }  // hold
            bool Action { get; }     // toggle

            string ToString();
        }

        public class SimulatedControls : Controls
        {
            public bool Special1 { get; set; }
            public bool Special2 { get; set; }
            public bool Special3 { get; set; }
            public bool Left { get; set; }
            public bool Right { get; set; }
            public bool Jump { get; set; }
            public bool JumpHeld { get; set; }
            public bool Action { get; set; }

            public SimulatedControls(Game1 game)
            {
                Special1 = false;
                Special2 = false;
                Special3 = false;
                Left = false;
                Right = false;
                Jump = false;
                JumpHeld = false;
                Action = false;
            }
        }

        public struct KeyboardControls : Controls
        {
            public bool Special1 { get { return game.ToggleKey(special1); } }
            public bool Special2 { get { return game.ToggleKey(special2); } }
            public bool Special3 { get { return game.ToggleKey(special3); } }
            public bool Left { get { return Keyboard.GetState().IsKeyDown(left); } }
            public bool Right { get { return Keyboard.GetState().IsKeyDown(right); } }
            public bool Jump { get { return game.ToggleKey(jump); } }
            public bool JumpHeld { get { return Keyboard.GetState().IsKeyDown(jump); } }
            public bool Action { get { return game.ToggleKey(action); } }

            private Game1 game;
            private Keys special1, special2, special3, left, right, jump, action;

            public KeyboardControls(Game1 game, Keys special1, Keys special2, Keys special3, Keys left, Keys right, Keys jump, Keys action)
            {
                this.game = game;
                this.special1 = special1;
                this.special2 = special2;
                this.special3 = special3;
                this.left = left;
                this.right = right;
                this.jump = jump;
                this.action = action;
            }

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Left = " + left)
                    .AppendLine("Right = " + right)
                    .AppendLine("Jump = " + jump)
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
            public bool Left { get { return GamePad.GetState(playerIndex, GamePadDeadZone.Circular).IsButtonDown(left); } }
            public bool Right { get { return GamePad.GetState(playerIndex, GamePadDeadZone.Circular).IsButtonDown(right); } }
            public bool JumpHeld { get { return GamePad.GetState(playerIndex, GamePadDeadZone.Circular).IsButtonDown(jump); } }
            public bool Action { get { return game.ToggleButton(playerIndex, action); } }

            private Game1 game;
            private PlayerIndex playerIndex;
            private Buttons special1, special2, special3, left, right, jump, action;

            public GamePadControls(Game1 game, PlayerIndex playerIndex, Buttons special1, Buttons special2, Buttons special3, Buttons left, Buttons right, Buttons jump, Buttons action)
            {
                this.game = game;
                this.playerIndex = playerIndex;
                this.special1 = special1;
                this.special2 = special2;
                this.special3 = special3;
                this.left = left;
                this.right = right;
                this.jump = jump;
                this.action = action;
            }

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Left = " + left)
                    .AppendLine("Right = " + right)
                    .AppendLine("Jump = " + jump)
                    .AppendLine("Action = " + action)
                    .AppendLine("Special1 = " + special1)
                    .AppendLine("Special2 = " + special2)
                    .AppendLine("Special3 = " + special3);
                return builder.ToString();
            }
        }
    }
}
