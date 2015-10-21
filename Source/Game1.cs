using System;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using Source.Collisions;

namespace Source
{
    /// <summary>
    /// This is the main type for the game.
    /// 
    /// IMPORTANT NOTES - PLEASE READ ALL:
    /// - Please use (0,0) as the bottom left for all levels -- you will see a green box there
    /// - (0,0) is in the top left for drawing to screen, and (0,0) is in the center for Farseer physics
    /// - Farseer uses meters, monogame uses pixels -- use ConvertUnits to convert
    /// - Please follow the style guide in place, which is
    ///   * ALL_CAPS for global constants
    ///   * UpperCamelCase for members (instance fields and methods)
    ///   * lowerCamelCase for other variables
    /// - Search for 'TODO' for things that need to be finished
    /// - For things that need to be added, look at the google doc, https://docs.google.com/document/d/1ofddsIU92CeK2RtJ5eg3PWEG8U2o49VdmNxmAJwwMMg/edit
    /// - Make good commit messages
    /// - There are no "assigned tasks" anymore, just subteams - do what needs to be done on the TOOD list (google doc)
    /// - No magic numbers permitted!
    /// </summary>
    public class Game1 : Game
    {
        // LEVEL_FILE should point to "test.lvl" in the root project directory

#if MONOMAC
        private String LEVELS_DIR = "../../../../../../";
#else
        private String LEVELS_DIR = "../../../../";
#endif

        // Farseer user data - basically, just use this as if it were an enum
        private const int PLAYER = 0;
        private const int FLOOR = 1;

        private const float PIXEL_METER = 32f;      // pixels per meter for normal game
        private const float PIXEL_METER_EDIT = 8f;  // pixels per meter when in edit mode for level
        private const int VIEW_WIDTH = 1280;        // width of unscaled screen in pixels
        private const int VIEW_HEIGHT = 720;        // height of unscaled screen in pixels
        private const float FLOOR_HEIGHT = 0.2f;    // height in meters of a floor

        private const float GRAVITY = 26f;      // N   -- downwards gravity for the world
        private const float MIN_VELOCITY = 1f;  // m/s -- what can be considered 0 horizontal velocity
        private const float MAX_VELOCITY = 40f; // m/s -- approximate Usaine Bolt speed
        private const float MAX_IMPULSE = 1f;   // N/s -- the impulse which is applied when player starts moving after standing still
        private const double IMPULSE_POW = 0.5; //     -- the player's horizontal input impulse is taken to the following power for extra smoothing
        private const float JUMP_IMPULSE = 26f; // N/s -- the upwards impulse applied when player jumps
        private const float SLOWDOWN = 2f;      // N/s -- impulse applied in opposite direction of travel to simulate friction
        private const float AIR_RESIST = 0.75f; //     -- air resistance on a scale of 0 to 1, where 1 is as if player is on ground
        private const double JUMP_WAIT = 0.5;   // s   -- how long the player needs to wait before jumping again
        private const float PUSH_VEL = 1f;      // m/s -- the player is given a little push going down platforms under this velocity
        private const float PUSH_POW = 10f;     // N/s -- the impulse applied to the player to get down a platform
        private const float MIN_WOBBLE = 2.5f;  //     -- the minimum ratio between max velocity and (max - current velocity) for wobbling
        private const float MAX_WOBBLE = 5f;    //     -- the maximum ratio for wobbling; we don't want wobble amplifier 40x


        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private KeyboardState prevKeyState;
        private GamePadState prevPadState;
        private MouseState prevMouseState;
        private Texture2D whiteRect;
        private SpriteFont font, fontBig;

        private Random rand;
        private World world;

        private const float CAMERA_SCALE = 20f;         // how fast the camera moves
        private const float MAX_CAMERA_SPEED_X = 5f;    // maximum x speed of camera
        private const float MAX_CAMERA_SPEED_Y = 3f;    // maximum y speed of camera
        private const float SCREEN_LEFT = 0.2f;         // defines how far left the player can be on wobble-screen
        private const float SCREEN_RIGHT = 0.35f;       // defines the right limit of the player on wobble-screen
        private const float SCREEN_TOP = 0.3f;          // defines the distance from the top or bottom of the screen for the player in wobble-screen
        private Rectangle cameraBounds;
        private Vector2 screenCenter;

        private bool editLevel;
        public Floor currentFloor;
        private bool editingFloor;
        private Vector2 startDraw;
        private Vector2 endDraw;
        private Vector2 screenOffset;

        private bool paused;
        private Player player;
        private List<Floor> floors;

        private const float LOAD_NEW = 100f;     // the next level will be loaded when the player is this far from the current end
        private const int LEVEL = -1;            // if this is greater than 0, levels will not be procedurally generated (useful for editing)
        private const string SONG = "Chiptune dash";    // the song to play, over, and over, and over again. NEVER STOP THE PARTY!
        private int levelEnd;
        private FloorData levels;

        /// <summary>
        /// Stores data for each level in memory
        /// </summary>
        public class FloorData
        {
            private List<Vector4>[] data;
            private World world;
            private Texture2D texture;
            private List<Floor> floors;
            private int[] max;
            private Random rand;

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
                data = new List<Vector4>[totalLevels];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = new List<Vector4>();
                }
                max = new int[totalLevels];
                rand = new Random();
            }

            /// <summary>
            /// Adds a new floor to the specified level
            /// </summary>
            /// <param name="width">Width of the floor</param>
            /// <param name="center">Center of the floor</param>
            /// <param name="rotation">Rotation of the floor</param>
            /// <param name="level">The level in which this floor is located</param>
            public void AddFloor(float width, Vector2 center, float rotation, int level)
            {
                data[level].Add(new Vector4(center, width, rotation));
                float end = center.X + (float)Math.Cos(rotation) * width / 2f;
                if (end > max[level]) max[level] = (int)Math.Round(end);
            }

            /// <summary>
            /// Loads a level into the world and floors list
            /// </summary>
            /// <param name="levelEnd">The current end of the game</param>
            /// <returns>The amount by which levelEnd should be incremented</returns>
            public int LoadLevel(int levelEnd)
            {
                int i;
                if (LEVEL < 0)
                    i = rand.Next(data.Length);
                else
                    i = LEVEL;
                foreach (Vector4 floor in data[i])
                {
                    Floor item = new Floor(texture, new Vector2(floor.X + levelEnd, floor.Y), floor.Z, floor.W);
                    world.Add(item);    // Note: world is collisions
                    floors.Add(item);   // Note: floors is drawing (this may be combined with world if it's a good idea)
                }
                return max[i];
            }
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Modify screen size
            graphics.PreferredBackBufferWidth = VIEW_WIDTH;
            graphics.PreferredBackBufferHeight = VIEW_HEIGHT;
            graphics.IsFullScreen = false;
            IsMouseVisible = true;
            graphics.ApplyChanges();

            // Sets how many pixels is a meter for Farseer
            ConvertUnits.SetDisplayUnitToSimUnitRatio(PIXEL_METER);

            // Set variables
            paused = false;
            editLevel = false;


            // Initialize previous keyboard and gamepad states
            prevKeyState = new KeyboardState();
            prevPadState = new GamePadState();
            prevMouseState = new MouseState();

            base.Initialize();      // This calls LoadContent()
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Content.RootDirectory = "Content";
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Use this to draw any rectangles
            whiteRect = new Texture2D(GraphicsDevice, 1, 1);
            whiteRect.SetData(new[] { Color.White });

            // Load assets in the Content Manager
            font = Content.Load<SpriteFont>("Fonts/Score");
            fontBig = Content.Load<SpriteFont>("Fonts/ScoreBig");

            // Create objects
            player = new Player(whiteRect, new Vector2(0, -10));
            world = new World(player);
            rand = new Random();
            floors = new List<Floor>();

            // Load the levels into memory
            string[] levelFiles = Directory.GetFiles(LEVELS_DIR, "level*.lvl");
            levels = new FloorData(world, whiteRect, floors, levelFiles.Length);
            for (int i = 0; i < levelFiles.Length; i++)
            {
                using (BinaryReader reader = new BinaryReader(File.Open(levelFiles[i], FileMode.Open)))
                {
                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        float floorWidth = reader.ReadSingle();
                        Vector2 center = new Vector2(reader.ReadSingle() + levelEnd, reader.ReadSingle());
                        float rotation = reader.ReadSingle();
                        levels.AddFloor(floorWidth, center, rotation, i);
                    }
                }
            }

            // Initialize camera
            int width = graphics.GraphicsDevice.Viewport.Width;
            int height = graphics.GraphicsDevice.Viewport.Height;
            cameraBounds = new Rectangle((int)(width * SCREEN_LEFT), (int)(height * SCREEN_TOP), (int)(width * (SCREEN_RIGHT - SCREEN_LEFT)), (int)(height * (1 - 2 * SCREEN_TOP)));
            screenCenter = cameraBounds.Center.ToVector2();
            screenOffset = Vector2.Zero;

            // Load the level stored in LEVEL_FILE
            levelEnd = 0;
            LoadLevel();

            // Load the song
            Song song = Content.Load<Song>("Music/" + SONG);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(song);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // content created manually must be disposed manually, with object.Dispose()
            whiteRect.Dispose();

            // all content loaded fron Content.Load can simply be unloaded with Content.Unload
            Content.Unload();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {

            // Handle end game
            // TODO put this in a pause menu
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Handle toggle pause
            // TODO open pause menu
            if (Keyboard.GetState().IsKeyDown(Keys.Space) && !prevKeyState.IsKeyDown(Keys.Space))
                paused = !paused;

            // Toggle edit level
            // TODO much better level editing/ creation
            if (Keyboard.GetState().IsKeyDown(Keys.E) && !prevKeyState.IsKeyDown(Keys.E))
            {
                editLevel = !editLevel;
                if (editLevel)
                    ConvertUnits.SetDisplayUnitToSimUnitRatio(PIXEL_METER_EDIT);
                else
                {
                    ConvertUnits.SetDisplayUnitToSimUnitRatio(PIXEL_METER);
                    screenOffset = Vector2.Zero;
                    currentFloor = null;
                }
            }

            if (editLevel)
            {
                HandleEditLevel();
            }

            if (!paused)
            {
                if (currentFloor == null)
                    HandleKeyboard();

                //CheckPlayer();
                //player.Update(gameTime.ElapsedGameTime.TotalSeconds);

                world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);

                float wobbleRatio = MAX_VELOCITY / (MAX_VELOCITY - player.Velocity.X);
                if (wobbleRatio >= MAX_WOBBLE)
                    wobbleScreen(MAX_WOBBLE);
                else if (wobbleRatio >= MIN_WOBBLE)
                    wobbleScreen(wobbleRatio);
            }

            prevKeyState = Keyboard.GetState();
            prevPadState = GamePad.GetState(0);

            base.Update(gameTime);
        }

        /// <summary>
        /// Handles all keyboard input for the game. Moves the player and recalculates wobble-screen.
        /// </summary>
        private void HandleKeyboard()
        {
            //KeyboardState state = Keyboard.GetState();

            //float impulse = MathHelper.SmoothStep(MAX_IMPULSE, 0f, Math.Abs(player.Body.LinearVelocity.X) / MAX_VELOCITY);
            //impulse = (float)Math.Pow(impulse, IMPULSE_POW);

            //if (state.IsKeyDown(Keys.Right))                    // move right
            //{
            //    player.Body.ApplyLinearImpulse(new Vector2(impulse, 0f));
            //    if (player.Body.LinearVelocity.X < 0f && player.CanJump)  // change direction quicker
            //        player.Body.ApplyLinearImpulse(new Vector2(SLOWDOWN, 0f));
            //}
            //else if (state.IsKeyDown(Keys.Left))                // move left
            //{
            //    player.Body.ApplyLinearImpulse(new Vector2(-impulse, 0f));
            //    if (player.Body.LinearVelocity.X > 0f && player.CanJump)  // change direction quickler
            //    {
            //        player.Body.ApplyLinearImpulse(new Vector2(-SLOWDOWN, 0f));
            //    }
            //}
            //else                            // air resistance and friction
            //{
            //    float slow = SLOWDOWN;
            //    if (!player.CanJump)
            //    {
            //        slow = SLOWDOWN * AIR_RESIST;
            //    }
            //    if (Math.Abs(player.Body.LinearVelocity.X) < MIN_VELOCITY)
            //        player.Body.LinearVelocity = new Vector2(0f, player.Body.LinearVelocity.Y);
            //    else
            //    {
            //        int playerVelSign = Math.Sign(player.Body.LinearVelocity.X);
            //        player.Body.ApplyLinearImpulse(new Vector2(Math.Sign(player.Body.LinearVelocity.X) * -slow, 0f));
            //    }
            //}
            //if (state.IsKeyDown(Keys.Up) && player.CanJump)     // jump
            //{
            //    player.JumpWait = JUMP_WAIT;
            //    player.Body.ApplyLinearImpulse(new Vector2(0f, -JUMP_IMPULSE));
            //}
            //if (state.IsKeyDown(Keys.Down) && player.CanJump && !player.Ghost)
            //{                                                   // fall
            //    if (player.Body.LinearVelocity.Y <= PUSH_VEL)
            //        player.Body.ApplyLinearImpulse(new Vector2(0f, PUSH_POW));
            //    player.Ghost = true;
            //    player.oldY = player.Body.Position.Y;
            //}

            // Calculate wobble-screen
            // TODO ever so slight camera shake when going fast
            float deltaX = ((cameraBounds.Center.X - screenCenter.X) / cameraBounds.Width - player.Velocity.X / MAX_VELOCITY) * CAMERA_SCALE;
            deltaX = MathHelper.Clamp(deltaX, -MAX_CAMERA_SPEED_X, MAX_CAMERA_SPEED_X);
            screenCenter.X += deltaX / 5;
            screenCenter.X = MathHelper.Clamp(screenCenter.X, cameraBounds.Left, cameraBounds.Right);

            float deltaY = ((cameraBounds.Center.Y - screenCenter.Y) / cameraBounds.Height - player.Velocity.Y / MAX_VELOCITY) * CAMERA_SCALE;
            deltaY = MathHelper.Clamp(deltaY, -MAX_CAMERA_SPEED_Y, MAX_CAMERA_SPEED_Y);
            screenCenter.Y += deltaY;
            screenCenter.Y = MathHelper.Clamp(screenCenter.Y, cameraBounds.Top, cameraBounds.Bottom);
        }

        private void wobbleScreen(float amplifier)
        {
            //Wobble screen with given amplifier
            //Ain't nobody got time for those calculations
            screenCenter.X += (float)((rand.NextDouble() - 0.5) * amplifier * 2);
            screenCenter.Y += (float)((rand.NextDouble() - 0.5) * amplifier * 2);
        }

        /// <summary>
        /// Handles all keypresses for editing a level. These are the controls:
        /// - Press 'E' to enter edit level mode.
        /// - Shift-drag to select a floor (move your mouse slowly)
        /// - Backspace to delete selected floor
        /// - Alt-drag to modify currently selected floor
        /// - Tap left, right, up, or down to move selected floor
        /// - Ctrl-drag to create a new floor
        /// - Ctrl-S to save your current level as test.lvl in the root project directory (be sure to copy and rename it if you want to keep it) - confirmation in console
        /// - Ctrl-O to open the level in test.lvl -- NOTE, this will override ANY changes you made, so be careful
        /// - + and - zoom in and out
        /// - Drag mouse with nothing held down to pan camera
        /// </summary>
        private void HandleEditLevel()
        {
            KeyboardState keyboard = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

            // Snap the mouse position to 1x1 meter grid
            Vector2 mouseSimPos = ConvertUnits.ToSimUnits(mouse.Position.ToVector2() - cameraBounds.Center.ToVector2() - screenOffset) + player.Position;
            mouseSimPos.X = (float)Math.Round(mouseSimPos.X);
            mouseSimPos.Y = (float)Math.Round(mouseSimPos.Y);

            if (mouse.LeftButton == ButtonState.Pressed)
            {
                if (editingFloor)                                   // Draw the floor
                {
                    endDraw = mouseSimPos;
                }
                else
                {
                    if (keyboard.IsKeyDown(Keys.LeftControl))       // Start drawing a floor
                    {
                        editingFloor = true;
                        startDraw = mouseSimPos;
                        endDraw = mouseSimPos;
                    }
                    else
                    {
                        if (keyboard.IsKeyDown(Keys.LeftShift))     // Select a floor
                        {
                            Body body = world.TestPoint(ConvertUnits.ToSimUnits(mouse.Position.ToVector2() - cameraBounds.Center.ToVector2() - screenOffset) + player.Position);
                            if (body != null && body is Floor)
                                currentFloor = (Floor)body;
                        }
                        else if (keyboard.IsKeyDown(Keys.LeftAlt) && currentFloor != null)
                        {                                           // Resize selected floor
                            float rotation = currentFloor.Rotation;
                            Vector2 center = currentFloor.Position;
                            float width = currentFloor.Size.X;
                            Vector2 offset = new Vector2(width * (float)Math.Cos(rotation), width * (float)Math.Sin(rotation)) / 2;
                            if (offset.X < 0) offset *= -1;
                            if (mouseSimPos.X > center.X)
                                startDraw = center - offset;
                            else
                                startDraw = center + offset;
                            endDraw = mouseSimPos;
                            //currentFloor.Body.Dispose();
                            floors.Remove(currentFloor);
                            currentFloor = null;
                            editingFloor = true;
                        }
                        else                                        // Move camera
                        {
                            screenOffset += mouse.Position.ToVector2() - prevMouseState.Position.ToVector2();
                        }
                    }
                }

            }
            else if (editingFloor)                                  // Make the floor
            {
                if (startDraw != endDraw)
                {
                    currentFloor = new Floor(whiteRect, startDraw, endDraw);
                    floors.Add(currentFloor);
                }
                editingFloor = false;
            }
            else if (currentFloor != null)
            {                                                       // Delete selected floor
                if (keyboard.IsKeyDown(Keys.Back))
                {
                    //currentFloor.Body.Dispose();
                    floors.Remove(currentFloor);
                    currentFloor = null;
                }
                else if (ToggleKey(Keys.Up))
                    currentFloor.Position += new Vector2(0f, -1f);
                else if (ToggleKey(Keys.Left))
                    currentFloor.Position += new Vector2(-1f, 0f);
                else if (ToggleKey(Keys.Right))
                    currentFloor.Position = new Vector2(1f, 0f);
                else if (ToggleKey(Keys.Down))
                    currentFloor.Position += new Vector2(0f, 1f);
                else if (keyboard.IsKeyDown(Keys.Enter))
                    currentFloor = null;
            }
            if (keyboard.IsKeyDown(Keys.LeftControl))               // Save and load level
            {
                if (ToggleKey(Keys.S) && LEVEL >= 0)
                {
                    SaveLevel();
                }
                else if (ToggleKey(Keys.O))
                {
                    LoadLevel();
                }
            }
            else if (ToggleKey(Keys.OemPlus))                       // Zoom in and out
                ConvertUnits.SetDisplayUnitToSimUnitRatio(ConvertUnits.ToDisplayUnits(1f) * 2);
            else if (ToggleKey(Keys.OemMinus))
                ConvertUnits.SetDisplayUnitToSimUnitRatio(ConvertUnits.ToDisplayUnits(1f) / 2);

            prevMouseState = mouse;
        }

        private bool ToggleKey(Keys key)
        {
            return Keyboard.GetState().IsKeyDown(key) && prevKeyState.IsKeyUp(key);
        }

        /// <summary>
        /// Checks if the user is off the level, and resets the player if it is
        /// </summary>
        private void CheckPlayer()
        {
            //if (player.Body.Position.Y > 10f)
            //{
            //    player = new Player(world);
            //    if (LEVEL < 0)
            //    {
            //        levelEnd = 0;
            //        currentFloor = null;
            //        foreach (Floor floor in floors)
            //            floor.Body.Dispose();
            //        floors.Clear();
            //    }
            //}
            //else if (player.Body.Position.X > levelEnd - LOAD_NEW && LEVEL < 0)
            //    LoadLevel();
        }

        /// <summary>
        /// Saves a level (the floors) to the file in LEVEL_FILE
        /// </summary>
        private void SaveLevel()
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(LEVELS_DIR + "level" + LEVEL, FileMode.Create)))
            {
                foreach (Floor floor in floors)
                {
                    writer.Write(floor.Size.X);
                    writer.Write(floor.Position.X);
                    writer.Write(floor.Position.Y);
                    writer.Write(floor.Rotation);
                }
            }
            Console.WriteLine("Saved");
        }

        /// <summary>
        /// Load the level specified in level and increments levelEnd
        /// </summary>
        private void LoadLevel()
        {
            levelEnd += levels.LoadLevel(levelEnd);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Calculate camera location matrix
            Matrix view;
            if (editLevel)
                view = Matrix.CreateTranslation(new Vector3(screenOffset + cameraBounds.Center.ToVector2() - ConvertUnits.ToDisplayUnits(player.Position), 0f));
            else
                view = Matrix.CreateTranslation(new Vector3(screenOffset + screenCenter - ConvertUnits.ToDisplayUnits(player.Position), 0f));

            // Draw player and floors
            spriteBatch.Begin(transformMatrix: view);
            player.Draw(spriteBatch);
            //DrawRect(player.Body.Position + new Vector2(0f, 0.9f), Color.Gold, 0f, new Vector2(0.5f, 0.5f), new Vector2(0.6f, 0.5f));
            foreach (Floor item in floors)
                item.Draw(spriteBatch);
            //if (currentFloor != null)
            //    DrawRect(currentFloor.Body, Color.Green, currentFloor.Origin, currentFloor.Scale + new Vector2(0, FLOOR_HEIGHT));
            if (editingFloor)
            {
                Vector2 dist = endDraw - startDraw;
                float rotation = (float)Math.Atan2(dist.Y, dist.X);
                Vector2 scale = new Vector2(dist.Length(), FLOOR_HEIGHT);
                Vector2 origin = new Vector2(0.5f, 0.5f);
                DrawRect(startDraw + dist / 2, Color.Azure, rotation, origin, scale);
            }
            if (editLevel)
                DrawRect(Vector2.Zero, Color.LightGreen, 0f, new Vector2(0.5f, 0.5f), new Vector2(1, 1));
            spriteBatch.End();

            // Show paused screen if game is paused
            // TODO display a proper pause menu
            spriteBatch.Begin();
            if (paused)
            {
                float centerX = GraphicsDevice.Viewport.Width / 2.0f - fontBig.MeasureString("Paused").X / 2.0f;
                spriteBatch.DrawString(fontBig, "Paused", new Vector2(centerX, GraphicsDevice.Viewport.Height * 0.1f), Color.Yellow);
            }

            //if (levelAnnounceWaitAt > 0)
            //{
            //    float centerX = GraphicsDevice.Viewport.Width / 2.0f - fontBig.MeasureString("Level " + currentLevel).X / 2.0f;
            //    spriteBatch.DrawString(fontBig, "Level " + currentLevel, new Vector2(centerX, GraphicsDevice.Viewport.Height * 0.1f), Color.Aqua);
            //    levelAnnounceWaitAt -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            //}
            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Draws a rectangle. Make sure this is called AFTER spriteBatch.Begin()
        /// </summary>
        /// <param name="position">The center of the rectangle</param>
        /// <param name="color"></param>
        /// <param name="rotation"></param>
        /// <param name="origin">The center of the texture</param>
        /// <param name="scale">The horizontal and vertical scale for the rectangle</param>
        private void DrawRect(Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale)
        {
            spriteBatch.Draw(whiteRect, ConvertUnits.ToDisplayUnits(position), null, color, rotation, origin, ConvertUnits.ToDisplayUnits(scale), SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws a rectangle. Make sure this is called AFTER spriteBatch.Begin()
        /// </summary>
        /// <param name="body">The body for the rectangle to draw</param>
        /// <param name="color"></param>
        /// <param name="origin">The center for the texture to use</param>
        /// <param name="scale">The horizontal and vertical scale for the rectangle</param>
        private void DrawRect(Body body, Color color, Vector2 origin, Vector2 scale)
        {
            spriteBatch.Draw(whiteRect, ConvertUnits.ToDisplayUnits(body.Position), null, color, body.Rotation, origin, ConvertUnits.ToDisplayUnits(scale), SpriteEffects.None, 0f);
        }
    }
}