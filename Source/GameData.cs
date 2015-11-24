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
        // TODO stuff some of the user-configurable stuff into a main menu screen (like controls and # of players)
        //public const int LEVEL = -1;            // if this is greater than -1, levels will not be procedurally generated (useful for editing)

        public const int numPlayers = 4;            // number of players
        public static Player.Ability[] playerAbilities = { Player.Ability.GravityPull, Player.Ability.GravityPush };    // pool of abilities for players -- currently randomly chosen at start
        public static Color[] playerColors = { Color.Red, Color.Yellow, Color.Purple, Color.ForestGreen, Color.Khaki };     // colors of each player


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
        public const int LEVEL_DIST_MIN = 5;    // the space between different buildings
        public const int LEVEL_DIST_MAX = 12;

        public const float GRAVITY_FORCE = 150f;  // G (in physics) in essence
        public const float GRAVITY_CUTOFF = 0.5f;    // minimum distance where gravity applies (to avoid super speed)

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
        public const int PLAYER_STEP = 2;       // player collisions will be calculated this many times per tick to avoid  (only if fixed time step)
        public const int PROJ_STEP = 2;         // projectile movement will be split up into this many steps
        public const float SPAWN_PROTECT = 5f;  // stuff this far apart from the player will be destroyed when the player is spawned

        public const float ZOOM_STEP = 1.5f;       // scale by which zoom is changed with + and -
        public const float PIXEL_METER = 24f;      // pixels per meter for normal game
        public const float PIXEL_METER_EDIT = 8f;  // pixels per meter when in edit mode for level
        public const int VIEW_WIDTH = 1280;        // width of unscaled screen in pixels
        public const int VIEW_HEIGHT = 720;        // height of unscaled screen in pixels

        public const float DEAD_START = RUN_VELOCITY - 2f;   // m/s -- the speed of dead wave at start of game
        public const float DEAD_END = RUN_VELOCITY + 0.5f; // m/s -- the speed at which the dead 'wave' on the left moves by the end of the game
        public const float DEAD_MAX = 40f; // m -- maximum distance between player and death if player is doing well
        public const int DEAD_WIDTH = 600;
        public const int DEAD_HEIGHT = 2000;
        public const float WIN_TIME = 60f;  // s -- survive for this long to win
        public const float MAX_SPEED_SCALE = 1.4f; // game is this much faster by the end of win
        public const int WIN_SCORE = 10;    // player gets 1 point for every WIN_SCORE seconds they survive
        public const int LOSE_SCORE = 5;    // score to lose when hit by purple
        public const int DEATH_LOSS = 2;    // score to lose when the purple only catches 1 player

#if DEBUG
        public const float BOOST_SPEED = -RUN_VELOCITY; // m/s -- horizontal velocity when boosting
#else
        public const float BOOST_SPEED = 26f; // m/s -- horizontal velocity when boosting
#endif

        public const float GRAVITY = 36f;   // m/s^2 -- gravity for players
        public const float GRAVITY_PART = 15f; // m/s^2 -- gravity for particles
        public const float MIN_VELOCITY = 1f;  // m/s -- what can be considered target velocity
        public const float SLOW_SPEED = 13f; // m/s -- speed player is going at when slowing down
        public const float RUN_VELOCITY = 22f; // m/s -- maximum horizontal velocity for player
        public const float MAX_ACCEL = 40f;   // m/s^2 -- the impulse which is applied when player starts moving after standing still
        public const float JUMP_SPEED = 18f; // m/s -- the initial upwards velocity when player jumps
        public const float JUMP_SLOW = 0.85f;   // -- x velocity is scaled by this when jumping
        public const float SLAM_SPEED = 17f; // m/s -- the speed at which the player goes down when slamming
        public const float SLOWDOWN = 45f;      // m/s^2 -- impulse applied in opposite direction of travel to simulate friction
        public const float AIR_RESIST = 0.75f; //     -- air resistance on a scale of 0 to 1, where 1 is as if player is on ground
        //public const double JUMP_WAIT = 0.5;   // s   -- how long the player needs to wait before jumping again
        //public const float PUSH_VEL = 1f;      // m/s -- the player is given a little push going down platforms under this velocity
        //public const float PUSH_POW = 10f;     // m/s -- the impulse applied to the player to get down a platform
        public const float MIN_WOBBLE = 0f;  //     -- the minimum ratio between max velocity and (max - current velocity) for wobbling
        public const float MAX_WOBBLE = 0f;    //     -- the maximum ratio for wobbling; we don't want wobble amplifier 40x
        public const float RESPAWN_DIST = 10;
        public const float CLIMB_SPEED = 8f;     // m/s -- speed of climbing onto a ledge

        public const float BOOST_LENGTH = 6f;  // how long a player can boost for
        public const float BOOST_REGEN = 8.7f; // boost will be refilled in this time (from 0)
        public const float STUN_LENGTH = 0.5f; // the player is stunned for this long
        public const float STUN_SCALE = 0.6f; // the player speed is scaled by this when stunned
        public const float SHOOT_COST = 0.3f; // boost bar cost of a shot
        //public const float JUMP_COST = 2 * JUMP_SPEED / GRAVITY / (BOOST_REGEN / BOOST_LENGTH); // use five equations to see time it takes to land on equal surface (v = v0 + at)

        public const string SONG = "afln_s_gdc-1";    // the song to play, over, and over, and over again. NEVER STOP THE PARTY!
        public const float VOLUME = 0.0f;                // volume for song

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
        public const int NUM_PART_WALL = 10;        // number of particles to spawn when a wall is exploded
        public const int NUM_PART_FLOOR = 5;        // number of particles to spawn when slamming and a hole is made

        public const float PROJ_SPEED = 60f;    // speed of a projectile
        public const float PROJ_LIVE = 1.0f;     // lifetime of a projectile in s

        public const float BUTTON_WIDTH = 0.4f;  // width of a button in proportion of screen
        public const float BUTTON_HEIGHT = 0.12f;    // height of button in proportion

        public static string Version
        {
            get
            {
                //Assembly asm = Assembly.GetEntryAssembly();
                //FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
                //return String.Format("{0}.{1}.{2}", fvi.ProductMajorPart, fvi.ProductMinorPart, fvi.ProductBuildPart);

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

        public interface Controls
        {

            bool Special { get; }
            bool Boost { get; }
            bool Jump { get; }
            bool Slam { get; }
            bool Shoot { get; }

            string ToString();
        }

        public struct KeyboardControls : Controls
        {
            public bool Special { get { return game.ToggleKey(special); } }
            public bool Boost { get { return Keyboard.GetState().IsKeyDown(boost); } }
            public bool Jump { get { return Keyboard.GetState().IsKeyDown(jump); } }
            public bool Slam { get { return Keyboard.GetState().IsKeyDown(slam); } }
            public bool Shoot { get { return game.ToggleKey(shoot); } }

            private Game1 game;
            private Keys special, boost, jump, slam, shoot;

            public KeyboardControls(Game1 game, Keys special, Keys boost, Keys jump, Keys slam, Keys shoot)
            {
                this.game = game;
                this.special = special;
                this.boost = boost;
                this.jump = jump;
                this.slam = slam;
                this.shoot = shoot;
            }

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Boost = " + boost)
                    .AppendLine("Jump = " + jump)
                    .AppendLine("Slam = " + slam)
                    .AppendLine("Shoot = " + shoot)
                    .AppendLine("Special = " + special);
                return builder.ToString();
            }
        }

        public struct GamePadControls : Controls
        {
            public bool Special { get { return game.ToggleButton(playerIndex, special); } }
            public bool Boost { get { return GamePad.GetState(playerIndex, GamePadDeadZone.Circular).IsButtonDown(boost); } }
            public bool Jump { get { return GamePad.GetState(playerIndex, GamePadDeadZone.Circular).IsButtonDown(jump); } }
            public bool Slam { get { return GamePad.GetState(playerIndex, GamePadDeadZone.Circular).IsButtonDown(slam); } }
            public bool Shoot { get { return game.ToggleButton(playerIndex, shoot); } }

            private Game1 game;
            private PlayerIndex playerIndex;
            private Buttons special, boost, jump, slam, shoot;

            public GamePadControls(Game1 game, PlayerIndex playerIndex, Buttons special, Buttons boost, Buttons jump, Buttons slam, Buttons shoot)
            {
                this.game = game;
                this.playerIndex = playerIndex;
                this.special = special;
                this.boost = boost;
                this.jump = jump;
                this.slam = slam;
                this.shoot = shoot;
            }

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Boost = " + boost)
                    .AppendLine("Jump = " + jump)
                    .AppendLine("Slam = " + slam)
                    .AppendLine("Shoot = " + shoot)
                    .AppendLine("Special = " + special);
                return builder.ToString();
            }
        }
    }
}
