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
        public const int numPlayers = 1;            // number of players

        public static bool[] useController = { false, false, false };    // true means the player at the index will be using a controller.
        public static Controls[] keyboardControls = {            // defines the keyboard controls which will be used
                                                       new Controls(Keys.Left, Keys.Right, Keys.Up, Keys.Down, Keys.RightShift),
                                                       new Controls(Keys.A, Keys.D, Keys.W, Keys.S, Keys.LeftShift),
                                                       new Controls(Keys.J, Keys.L, Keys.I, Keys.K, Keys.O)};

        public const float LOAD_NEW = 50f;     // the next level will be loaded when the player is this far from the current end
        public const int MAX_FLOORS = 50;    // maximum number of floors at any given time
        public const int MAX_WALLS = 90;    // maximum number of walls

        public const int MIN_LEVEL_WIDTH = 20;  // width of levels
        public const int MAX_LEVEL_WIDTH = 50;
        public const float MIN_FLOOR_HOLE = 5f; // size of a hole inside a building
        public const float MAX_FLOOR_HOLE = 9f - MIN_FLOOR_HOLE;
        public const float MIN_FLOOR_DIST = 15; // width of a floor until a hole is reached
        public const float MAX_FLOOR_DIST = 80 - MIN_FLOOR_DIST;
        public const int MIN_LEVEL_STEP = 5;    // size of each floor in building
        public const int MAX_LEVEL_STEP = 10;
        public const int MIN_NUM_FLOORS = 5;    // number of floors per building
        public const int MAX_NUM_FLOORS = 11;
        public const int MIN_WALL_DIST = 10;    // distance between walls inside rooms
        public const int MAX_WALL_DIST = 90;

        public const float FLOOR_HOLE = 3.5f;   // size in m of hole to make when slamming
        public const int WINDOW_HEALTH = 1;     // windows are on the side of buildings
        public const int WALL_HEALTH = 3;       // walls are inside the buildings themselves
        public const float MIN_FLOOR_WIDTH = 2f;  // floors cannot be smaller than this (by random generation or slamming)
        public const double STAIR_CHANCE = 0.8; // chance a hole will have a stair to it (from 0 to 1)
        public const float STAIR_WIDTH = MIN_FLOOR_DIST;   // horizontal distance of a stair
        public const float MIN_STAIR_DIST = MIN_FLOOR_HOLE + 2f;

        public const float PLAYER_START = 2f;   // starting x position of player
        public const int MIN_SPAWN = MAX_LEVEL_STEP + 1;  // minimum spawning position (vertically)
        public const int MAX_SPAWN = MIN_LEVEL_STEP * MIN_NUM_FLOORS + 10;  // maximum spawning position (vertically)

        public const float ZOOM_STEP = 1.5f;       // scale by which zoom is changed with + and -
        public const float PIXEL_METER = 24f;      // pixels per meter for normal game
        public const float PIXEL_METER_EDIT = 8f;  // pixels per meter when in edit mode for level
        public const int VIEW_WIDTH = 1280;        // width of unscaled screen in pixels
        public const int VIEW_HEIGHT = 720;        // height of unscaled screen in pixels
        public static Color[] playerColors = { Color.Red, Color.Yellow, Color.Purple };     // colors of each player

        public const float DEAD_START = RUN_VELOCITY;   // m/s -- the speed of dead wave at start of game
        public const float DEAD_SPEED = RUN_VELOCITY * 1.11f; // m/s -- the speed at which the dead 'wave' on the left moves
        public const float DEAD_MAX = 40f; // m -- maximum distance between player and death if player is doing well
        public const int DEAD_WIDTH = 600;
        public const int DEAD_HEIGHT = 2000;
        public const float WIN_TIME = 60f;  // s -- survive for this long to win

        public const float GRAVITY = 26f;   // m/s^2 -- gravity for players
        public const float GRAVITY_PART = 15f; // m/s^2 -- gravity for particles
        //public const float MIN_VELOCITY = 1f;  // m/s -- what can be considered 0 horizontal velocity
        public const float SLOW_SPEED = 13f; // m/s -- speed player is going at when slowing down
        public const float RUN_VELOCITY = 22f; // m/s -- maximum horizontal velocity for player
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
        public const float CLIMB_SPEED = 8f;     // m/s -- speed of climbing onto a ledge

        public const float BOOST_LENGTH = 4.8f;  // how long a player can boost for
        public const float BOOST_REGEN = 5.6f; // boost will be refilled in this time (from 0)
        public const float STUN_LENGTH = 0.5f; // the player is stunned for this long
        public const float STUN_SCALE = 0.6f; // the player speed is scaled by this when stunned
        public const float SHOOT_COST = 0.3f; // boost bar cost of a shot
        public const float JUMP_COST = 2 * JUMP_SPEED / GRAVITY / (BOOST_REGEN / BOOST_LENGTH); // use five equations to see time it takes to land on equal surface (v = v0 + at)

        public const string SONG = "afln_s_gdc-1";    // the song to play, over, and over, and over again. NEVER STOP THE PARTY!
        public const float VOLUME = 0.5f;                // volume for song

        public const float CAMERA_SCALE = 20f;         // how fast the camera moves
        public const float MAX_CAMERA_SPEED_X = 5f;    // maximum x speed of camera
        public const float MAX_CAMERA_SPEED_Y = 3f;    // maximum y speed of camera
        public const float SCREEN_LEFT = 0.2f;         // defines how far left the player can be on wobble-screen
        public const float SCREEN_RIGHT = 0.35f;       // defines the right limit of the player on wobble-screen
        public const float SCREEN_SPACE = 0.45f;        // camera will begin zooming out when the player are SCREEN_SPACE % of the screen apart from each other
        public const float SCREEN_TOP = 0.3f;          // defines the distance from the top or bottom of the screen for the player in wobble-screen
        public const float DEAD_DIST = 10f;            // players this distance or more behind the average x will move to the maximum player
        public const double DEAD_TIME = 3;             // respawn time when a player gets behind the cutoff
        public const double PHASE_TIME = 1;            // the point at which the player will be visible again after dying to get the player prepared

        public const float PARTICLE_WIDTH = .125f;  // width of a particle (as a square)
        public const float PARTICLE_LIFETIME = 1.2f;  // how long a particle lasts for (in s)
        public const float PARTICLE_LIFETIME_TEXT = 0.3f; // lifetime of a text particle
        public const float PARTICLE_MAX_SPIN = 10f; // maximum angular velocity (in rad/s)
        public const float PARTICLE_X = 4f;         // maximum x velocity of a particle when randomly generating in either direction
        public const float PARTICLE_Y = 5f;         // maximum y velocity of a particle in either direction
        public const int NUM_PART_WALL = 10;        // number of particles to spawn when a wall is exploded
        public const int NUM_PART_FLOOR = 5;        // number of particles to spawn when slamming and a hole is made

        public const float PROJ_SPEED = 60f;    // speed of a projectile
        public const float PROJ_LIVE = 1.0f;     // lifetime of a projectile in s

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
