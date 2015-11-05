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
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private KeyboardState prevKeyState;
        private GamePadState prevPadState;
        private MouseState prevMouseState;
        private Texture2D whiteRect;
        private SpriteFont font, fontBig;

        private Random rand;
        private World world;

        private Rectangle cameraBounds;
        private Vector2 screenCenter;

        private bool editLevel;
        public Floor currentFloor;
        private bool editingFloor;
        private Vector2 startDraw;
        private Vector2 endDraw;
        private Vector2 screenOffset;

        private bool paused;
        private List<Player> players;
        private List<Floor> floors;

        private int levelEnd;
        private GameData.FloorData levels;
        private bool onMac = false;

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
            graphics.PreferredBackBufferWidth = GameData.VIEW_WIDTH;
            graphics.PreferredBackBufferHeight = GameData.VIEW_HEIGHT;
            graphics.IsFullScreen = false;
            IsMouseVisible = true;
            graphics.ApplyChanges();

            // Sets how many pixels is a meter for Farseer
            ConvertUnits.SetDisplayUnitToSimUnitRatio(GameData.PIXEL_METER);

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

			string[] foo = Directory.GetFiles(GameData.LEVELS_DIR, "level*.lvl");
			if (foo.Length == 0) {
				onMac = true;
			}

            // Create objects
            players = new List<Player>();
            for (int i = 0; i < GameData.numPlayers; i++)
            {
				players.Add(new Player(Content.Load<Texture2D>("pumpkins/001"), GameData.PLAYER_POSITION, GameData.playerColors[i]));
            }
            GameData.playerColors = null;
            rand = new Random();
            floors = new List<Floor>();
            world = new World(players, floors);

            // Load the levels into memory
            string[] levelFiles = Directory.GetFiles(GameData.LEVELS_DIR, "level*.lvl");
			if (levelFiles.Length == 0) {
                levelFiles = Directory.GetFiles(GameData.LEVELS_DIR2, "level*.lvl");
                onMac = true;
			}
            levels = new GameData.FloorData(world, whiteRect, floors, levelFiles.Length);
            for (int i = 0; i < levelFiles.Length; i++)
            {
                using (BinaryReader reader = new BinaryReader(File.Open(levelFiles[i], FileMode.Open)))
                {
                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        float floorWidth = reader.ReadSingle();
                        Vector2 center = new Vector2(reader.ReadSingle() + levelEnd, reader.ReadSingle());
                        float rotation = reader.ReadSingle();
                        bool solid = reader.ReadBoolean();
                        levels.AddFloor(floorWidth, center, rotation, solid, i);
                    }
                }
            }

            // Initialize camera
            int width = graphics.GraphicsDevice.Viewport.Width;
            int height = graphics.GraphicsDevice.Viewport.Height;
            cameraBounds = new Rectangle((int)(width * GameData.SCREEN_LEFT), (int)(height * GameData.SCREEN_TOP),
                (int)(width * (GameData.SCREEN_RIGHT - GameData.SCREEN_LEFT)), (int)(height * (1 - 2 * GameData.SCREEN_TOP)));
            screenCenter = cameraBounds.Center.ToVector2();
            screenOffset = Vector2.Zero;

            // Load the level stored in LEVEL_FILE
            levelEnd = 0;
            LoadLevel();

            // Load the song
			try {
                Song song = Content.Load<Song>("Music/" + GameData.SONG);
              MediaPlayer.IsRepeating = true;
              MediaPlayer.Volume = GameData.VOLUME;
              MediaPlayer.Play(song);
			} catch (Microsoft.Xna.Framework.Content.ContentLoadException cle) {
			}
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

            //if (Keyboard.GetState().IsKeyDown(Keys.I))
            //{
            //    for (int x = 0; x < floors.Count; x++)
            //    {
            //        Console.WriteLine(floors[x].Intersects(floors[floors.Count-1]));
            //    }
            //}
                

            // Handle toggle pause
            // TODO open pause menu
            if (Keyboard.GetState().IsKeyDown(Keys.Space) && !prevKeyState.IsKeyDown(Keys.Space))
                paused = !paused;

            // Toggle edit level
            // TODO much better level editing/ creation
            if (ToggleKey(Keys.E))
            {
                editLevel = !editLevel;
                if (editLevel)
                    ConvertUnits.SetDisplayUnitToSimUnitRatio(GameData.PIXEL_METER_EDIT);
                else
                {
                    ConvertUnits.SetDisplayUnitToSimUnitRatio(GameData.PIXEL_METER);
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
                float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (currentFloor == null)
                    HandleInput(deltaTime);

                CheckPlayer();
                //player.Update(gameTime.ElapsedGameTime.TotalSeconds);

                world.Step(deltaTime);
            }

            prevKeyState = Keyboard.GetState();
            prevPadState = GamePad.GetState(0);

            base.Update(gameTime);
        }

        /// <summary>
        /// Handles all keyboard and gamepad input for the game. Moves all players and recalculates wobble-screen.
        /// </summary>
        private void HandleInput(float deltaTime)
        {
            KeyboardState state = Keyboard.GetState();

            int currentController = 0;
            int currentKeyboard = 0;
            for (int i = 0; i < players.Count; i++)
            {
                Player player = players[i];

                if (GameData.useController[i])
                {
                    currentController++;
                    if (player.TimeSinceDeath < GameData.PHASE_TIME)
                    {
                        HandleGamepad(deltaTime, player, currentController - 1);
                    }
                }
                else
                {
                    currentKeyboard++;
                    if (player.TimeSinceDeath < GameData.PHASE_TIME)
                    {
                        HandleKeyboard(deltaTime, player, currentKeyboard - 1);
                    }
                }

                if (player.TimeSinceDeath > 0)
                {
                    player.TimeSinceDeath -= deltaTime;
                }
            }

            if (state.IsKeyDown(Keys.R))                        // reset
            {
                foreach (Player player in players)
                {
                    player.MoveToPosition(GameData.PLAYER_POSITION);
                    player.Velocity = Vector2.Zero;
                }
            }

            // Find average velocity across the players
            Vector2 averageVel = Vector2.Zero;
            foreach (Player player in players)
                averageVel += player.Velocity;
            averageVel /= players.Count;

            // Calculate wobble-screen
            float deltaX = ((cameraBounds.Center.X - screenCenter.X) / cameraBounds.Width - averageVel.X / GameData.MAX_VELOCITY) * GameData.CAMERA_SCALE;
            deltaX = MathHelper.Clamp(deltaX, -GameData.MAX_CAMERA_SPEED_X, GameData.MAX_CAMERA_SPEED_X);
            screenCenter.X += deltaX / 5;
            screenCenter.X = MathHelper.Clamp(screenCenter.X, cameraBounds.Left, cameraBounds.Right);

            float deltaY = ((cameraBounds.Center.Y - screenCenter.Y) / cameraBounds.Height - averageVel.Y / GameData.MAX_VELOCITY) * GameData.CAMERA_SCALE;
            deltaY = MathHelper.Clamp(deltaY, -GameData.MAX_CAMERA_SPEED_Y, GameData.MAX_CAMERA_SPEED_Y);
            screenCenter.Y += deltaY;
            screenCenter.Y = MathHelper.Clamp(screenCenter.Y, cameraBounds.Top, cameraBounds.Bottom);

            float wobbleRatio = GameData.MAX_VELOCITY / (GameData.MAX_VELOCITY - averageVel.X);
            if (wobbleRatio >= GameData.MAX_WOBBLE)
                wobbleScreen(GameData.MAX_WOBBLE);
            else if (wobbleRatio >= GameData.MIN_WOBBLE)
                wobbleScreen(wobbleRatio);
        }

        /// <summary>
        /// Handles input for a single player for given input keys
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="player"></param>
        /// <param name="controller"></param>
        private void HandleKeyboard(float deltaTime, Player player, int controller)
        {
            KeyboardState state = Keyboard.GetState();
            GameData.Controls controls = GameData.keyboardControls[controller];

            float impulse = GameData.MAX_IMPULSE * deltaTime;
            //float impulse = MathHelper.SmoothStep(MAX_IMPULSE, 0f, Math.Abs(player.Velocity.X) / MAX_VELOCITY) * deltaTime;
            //impulse = (float)Math.Pow(impulse, IMPULSE_POW);

            float slow = GameData.SLOWDOWN * deltaTime;
            if (!player.CanJump)
            {
                slow *= GameData.AIR_RESIST;
            }

            if (state.IsKeyDown(controls.right))                    // move right
            {
                player.Velocity += (new Vector2(impulse, 0f));
                if (player.Velocity.X < 0f && player.CanJump)  // change direction quicker
                    player.Velocity += (new Vector2(slow, 0f));
            }
            else if (state.IsKeyDown(controls.left))                // move left
            {
                player.Velocity += (new Vector2(-impulse, 0f));
                if (player.Velocity.X > 0f && player.CanJump)  // change direction quickler
                {
                    player.Velocity += (new Vector2(-slow, 0f));
                }
            }
            else                            // air resistance and friction
            {
                if (Math.Abs(player.Velocity.X) < GameData.MIN_VELOCITY)
                    player.Velocity = new Vector2(0f, player.Velocity.Y);
                else
                {
                    int playerVelSign = Math.Sign(player.Velocity.X);
                    player.Velocity += (new Vector2(Math.Sign(player.Velocity.X) * -slow, 0f));
                }
            }
            if (state.IsKeyDown(controls.up) && player.CanJump)     // jump
            {
                player.Velocity = (new Vector2(player.Velocity.X, -GameData.JUMP_IMPULSE));
            }

            if (state.IsKeyDown(controls.down))
            {                                                   // fall
                //if (player.Velocity.Y <= PUSH_VEL)
                //    player.Velocity = (new Vector2(player.Velocity.X, PUSH_POW));
                player.Ghost = true;
            }
            if (ToggleKey(controls.shoot) && player.TimeSinceDeath <= 0)
            {
                player.Projectiles.Add(new Projectile(whiteRect, new Vector2(player.Position.X, player.Position.Y), player.Color));
                //Console.WriteLine("Shooting!");
            }
        }

        /// <summary>
        /// Handles input for a single player for given input keys
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="player"></param>
        /// <param name="controller">Must be from 0 to 3</param>
        private void HandleGamepad(float deltaTime, Player player, int controller)
        {
            GamePadState state = GamePad.GetState((PlayerIndex)controller, GamePadDeadZone.Circular);

            float impulse = GameData.MAX_IMPULSE * deltaTime;
            //float impulse = MathHelper.SmoothStep(MAX_IMPULSE, 0f, Math.Abs(player.Velocity.X) / MAX_VELOCITY) * deltaTime;
            //impulse = (float)Math.Pow(impulse, IMPULSE_POW);

            float slow = GameData.SLOWDOWN * deltaTime;
            if (!player.CanJump)
            {
                slow *= GameData.AIR_RESIST;
            }

            if (state.ThumbSticks.Left.X == 0f)         // air resistance and friction
            {
                if (Math.Abs(player.Velocity.X) < GameData.MIN_VELOCITY)
                    player.Velocity = new Vector2(0f, player.Velocity.Y);
                else
                {
                    int playerVelSign = Math.Sign(player.Velocity.X);
                    player.Velocity += (new Vector2(Math.Sign(player.Velocity.X) * -slow, 0f));
                }
            }
            else
            {
                // move right and left
                player.Velocity += (new Vector2(impulse * state.ThumbSticks.Left.X, 0f));

                if (player.CanJump) // change direction quicker
                {
                    if (player.Velocity.X > 0f && state.ThumbSticks.Left.X > 0f)
                        player.Velocity += (new Vector2(-slow, 0f));
                    else if (player.Velocity.X < 0f && state.ThumbSticks.Left.X < 0f)
                        player.Velocity += (new Vector2(slow, 0f));
                }
            }

            if (state.IsButtonDown(Buttons.A) && player.CanJump)     // jump
            {
                player.Velocity = (new Vector2(player.Velocity.X, -GameData.JUMP_IMPULSE));
            }

            if (state.ThumbSticks.Left.Y > 0f)
            {                                                   // fall
                //if (player.Velocity.Y <= PUSH_VEL)
                //    player.Velocity = (new Vector2(player.Velocity.X, PUSH_POW));
                player.Ghost = true;
            }

            if (state.IsButtonDown(Buttons.X) && prevPadState.IsButtonUp(Buttons.X) && player.TimeSinceDeath <= 0)
            {
                player.Projectiles.Add(new Projectile(whiteRect, new Vector2(player.Position.X, player.Position.Y), player.Color));
                //Console.WriteLine("Shooting!");
            }
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
        /// - Press f when a floor is selected to change its solid state
        /// </summary>
        private void HandleEditLevel()
        {
            KeyboardState keyboard = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

            // Find average position across all players
            Vector2 averagePos = Vector2.Zero;
            foreach (Player player in players)
                averagePos += player.Position;
            averagePos /= players.Count;

            // Snap the mouse position to 1x1 meter grid
            Vector2 mouseSimPos = ConvertUnits.ToSimUnits(mouse.Position.ToVector2() - cameraBounds.Center.ToVector2() - screenOffset) + averagePos;
            Vector2 mouseSimPosRound = new Vector2((float)Math.Round(mouseSimPos.X), (float)Math.Round(mouseSimPos.Y));

            if (mouse.LeftButton == ButtonState.Pressed)
            {
                if (editingFloor)                                   // Draw the floor
                {
                    endDraw = mouseSimPosRound;
                }
                else
                {
                    if (keyboard.IsKeyDown(Keys.LeftControl))       // Start drawing a floor
                    {
                        editingFloor = true;
                        startDraw = mouseSimPosRound;
                        endDraw = mouseSimPosRound;
                    }
                    else
                    {
                        if (keyboard.IsKeyDown(Keys.LeftShift))     // Select a floor
                        {
                            Body body = world.TestPoint(mouseSimPos);
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
                            if (mouseSimPosRound.X > center.X)
                                startDraw = center - offset;
                            else
                                startDraw = center + offset;
                            endDraw = mouseSimPosRound;
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
                else if (keyboard.IsKeyDown(Keys.Up))
                    currentFloor.MovePosition(-Vector2.UnitY);
                else if (keyboard.IsKeyDown(Keys.Left))
                    currentFloor.MovePosition(-Vector2.UnitX);
                else if (keyboard.IsKeyDown(Keys.Right))
                    currentFloor.MovePosition(Vector2.UnitX);
                else if (keyboard.IsKeyDown(Keys.Down))
                    currentFloor.MovePosition(Vector2.UnitY);
                else if (ToggleKey(Keys.F))
                {
                    currentFloor.Solid = !currentFloor.Solid;
                    if (currentFloor.Solid)
                        Console.WriteLine("Floor is now solid");
                    else
                        Console.WriteLine("Floor is no longer solid");
                }
                else if (keyboard.IsKeyDown(Keys.Enter))
                    currentFloor = null;
            }
            if (keyboard.IsKeyDown(Keys.LeftControl))               // Save and load level
            {
                if (ToggleKey(Keys.S) && GameData.LEVEL >= 0)
				{
					SaveLevel ();
				}
				else if (ToggleKey (Keys.O))
				{
					LoadLevel ();
				}
				else if (ToggleKey (Keys.C))						//Clear the level
				{
					floors.Clear ();
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
        /// Checks if the user is off the level, and resets the player if it is. Also, updates player-related game things (documentation WIP)
        /// </summary>
        private void CheckPlayer()
        {
            Player max = players[0];

            float averageX = 0;
            foreach (Player player in players)
                averageX += player.Position.X;
            averageX /= players.Count;

            foreach (Player player in players)
            {
                player.Velocity.X = MathHelper.Clamp(player.Velocity.X, -GameData.MAX_VELOCITY, GameData.MAX_VELOCITY);
                if (player.Position.Y > 10f)
                {
                    player.MoveToPosition(GameData.PLAYER_POSITION);
                    player.Velocity = Vector2.Zero;
                    if (GameData.LEVEL < 0)
                    {
                        levelEnd = 0;
                        currentFloor = null;
                        //foreach (Floor floor in floors)
                        //    floor.Body.Dispose();
                        floors.Clear();
                    }
                }
                else if (player.Position.X > levelEnd - GameData.LOAD_NEW && GameData.LEVEL < 0)
                    LoadLevel();

                if (player.Position.X > max.Position.X)
                    max = player;
            }

            foreach (Player player in players)
            {
                if (player.TimeSinceDeath > 0)
                {
                    float val = (float)(player.TimeSinceDeath / GameData.DEAD_TIME);
                    float newX = MathHelper.Lerp(max.Position.X, player.Position.X, val);
                    float newY = MathHelper.Lerp(max.Position.Y, player.Position.Y, val);
                    player.MoveToPosition(new Vector2(newX, newY));
                }
                else if (player.Position.X < averageX - GameData.DEAD_DIST)
                {
                    player.TimeSinceDeath = GameData.DEAD_TIME;
                    player.Projectiles.Clear();
                    max.Score++;
                }
            }
        }

        /// <summary>
        /// Saves a level (the floors) to the file in LEVEL_FILE
        /// </summary>
        private void SaveLevel()
        {
			String dir;
			if (onMac) {
                dir = GameData.LEVELS_DIR2;
			} else {
                dir = GameData.LEVELS_DIR;
			}
            using (BinaryWriter writer = new BinaryWriter(File.Open(dir + "level" + GameData.LEVEL + ".lvl", FileMode.Create)))
            {
                foreach (Floor floor in floors)
                {
                    bool wall = false;
                    if (floor.Size.Y > floor.Size.X)
                        wall = true;

                    if (wall)
                        writer.Write(floor.Size.Y);
                    else
                        writer.Write(floor.Size.X);

                    writer.Write(floor.Position.X);
                    writer.Write(floor.Position.Y);

                    if (wall)
                        writer.Write(MathHelper.PiOver2);
                    else
                        writer.Write(floor.Rotation);

                    writer.Write(floor.Solid);
                }
            }
            Console.WriteLine("Saved");
        }

        /// <summary>
        /// Load the level specified in level and increments levelEnd
        /// </summary>
        private void LoadLevel()
        {
            levelEnd += levels.LoadLevel(levelEnd) + GameData.LEVEL_DIST;
            Console.WriteLine("LevelEnd: " + levelEnd);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Find average position across all players
            Vector2 averagePos = Vector2.Zero;
            foreach (Player player in players)
                averagePos += player.Position;
            averagePos /= players.Count;

            // Calculate camera location matrix
            Matrix view;
            if (editLevel)
                view = Matrix.CreateTranslation(new Vector3(screenOffset + cameraBounds.Center.ToVector2() - ConvertUnits.ToDisplayUnits(averagePos), 0f));
            else
                view = Matrix.CreateTranslation(new Vector3(screenOffset + screenCenter - ConvertUnits.ToDisplayUnits(averagePos), 0f));

            // Draw player and floors
            spriteBatch.Begin(transformMatrix: view);
            
            spriteBatch.Draw(whiteRect, new Rectangle(-(int)view.Translation.X, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.LightGray);

            //DrawRect(player.Body.Position + new Vector2(0f, 0.9f), Color.Gold, 0f, new Vector2(0.5f, 0.5f), new Vector2(0.6f, 0.5f));
            foreach (Floor item in floors)
                item.Draw(spriteBatch);
            foreach (Player player in players)
            {
                if (player.TimeSinceDeath < GameData.PHASE_TIME)
                    player.Draw(spriteBatch);
                foreach (Projectile proj in player.Projectiles)
                    proj.Draw(spriteBatch);
            }
            if (currentFloor != null)
                DrawRect(currentFloor.Position, Color.Green, currentFloor.Rotation, currentFloor.Origin, currentFloor.Size);
            if (editingFloor)
            {
                Vector2 dist = endDraw - startDraw;
                float rotation = (float)Math.Atan2(dist.Y, dist.X);
                Vector2 scale = new Vector2(dist.Length(), Floor.FLOOR_HEIGHT);
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

            // Display scores in the top left
            System.Text.StringBuilder text = new System.Text.StringBuilder();
            text.AppendLine("Scores");
            for (int i = 0; i < players.Count; i++)
            {
                text.AppendLine(string.Format("Player {0}: {1}", i + 1, players[i].Score));
            }
            spriteBatch.DrawString(font, text, new Vector2(10, 10), Color.Green);

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