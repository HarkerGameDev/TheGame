using System;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using Source.Collisions;
using Source.Graphics;

using EmptyKeys.UserInterface;
using EmptyKeys.UserInterface.Generated;

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
        private GamePadState[] prevPadStates;
        private MouseState prevMouseState;

        private GameData.Controls[] playerControls;

        private static Texture2D whiteRect;
        public SpriteFont fontSmall, fontBig;

        public Random rand;
        private Random randLevel;
        private World world;

        private Rectangle cameraBounds;
        private Vector2 screenCenter;
        private float currentZoom;

        private bool editLevel;
        public Floor currentFloor;
        private bool editingFloor;
        private Vector2 startDraw;
        private Vector2 endDraw;

        private Vector2 screenOffset;

        //private bool paused;
        private State state;
        private float totalTime;

        public List<Player> players;
        public List<Floor> floors;
        public List<Wall> walls;
        public List<Particle> particles;
        public List<Obstacle> obstacles;

        private List<Button> pauseMenu;
        //private List<Button> mainMenu;
        private List<Button> optionsMenu;
        private List<Button> controlsMenu;

        private int levelEnd;
        private float death;

        private MainMenu mainMenu;

        private int nativeScreenWidth;
        private int nativeScreenHeight;

        public enum State
        {
            Running, Paused, MainMenu, Options, Controls
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.DeviceCreated += graphics_DeviceCreated;
            graphics.PreparingDeviceSettings += graphics_PreparingDeviceSettings;
            IsMouseVisible = true;
        }

        private void graphics_DeviceCreated(object sender, EventArgs e)
        {
            Engine engine = new MonoGameEngine(GraphicsDevice, nativeScreenWidth, nativeScreenHeight);
        }

        private void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            nativeScreenWidth = graphics.PreferredBackBufferWidth;
            nativeScreenHeight = graphics.PreferredBackBufferHeight;

            graphics.PreferredBackBufferWidth = GameData.VIEW_WIDTH;
            graphics.PreferredBackBufferHeight = GameData.VIEW_HEIGHT;
            graphics.PreferMultiSampling = true;
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.SynchronizeWithVerticalRetrace = true;
            graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 16;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Sets how many pixels is a meter
            currentZoom = GameData.PIXEL_METER;
            ConvertUnits.SetDisplayUnitToSimUnitRatio(currentZoom);

            // Set seed for a scheduled random level (minutes since Jan 1, 2015)
            rand = new Random();
            randLevel = new Random(GameData.GetSeed);

            // Set variables
            //paused = false;
            state = State.MainMenu;
            editLevel = false;
            death = -GameData.DEAD_MAX;
            totalTime = 0;

            // Initialize previous keyboard and gamepad states
            prevKeyState = new KeyboardState();
            prevMouseState = new MouseState();
            prevPadStates = new GamePadState[4];
            for (int i = 0; i < prevPadStates.Length; i++)
                prevPadStates[i] = new GamePadState();

            // Load controls (hardcoded for now)
            playerControls = new GameData.Controls[] {
                                                       new GameData.KeyboardControls(this, Keys.Left, Keys.Right, Keys.Up, Keys.Down, Keys.RightShift),
                                                       new GameData.KeyboardControls(this, Keys.A, Keys.D, Keys.W, Keys.S, Keys.LeftShift),
                                                       new GameData.KeyboardControls(this, Keys.J, Keys.L, Keys.I, Keys.K, Keys.O),
                                                       new GameData.GamePadControls(this, PlayerIndex.One, Buttons.LeftTrigger, Buttons.LeftThumbstickRight, Buttons.RightTrigger, Buttons.LeftThumbstickDown, Buttons.A)
                                                  };

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

            // Load menus
            SpriteFont font = Content.Load<SpriteFont>("Fonts/Segoe_UI_15_Bold");
            FontManager.DefaultFont = Engine.Instance.Renderer.CreateFont(font);
            mainMenu = new MainMenu();
            FontManager.Instance.LoadFonts(Content, "Fonts");

            // Use this to draw any rectangles
            whiteRect = new Texture2D(GraphicsDevice, 1, 1);
            whiteRect.SetData(new[] { Color.White });

            // Load assets in the Content Manager
            fontSmall = Content.Load<SpriteFont>("Fonts/Score");
            fontBig = Content.Load<SpriteFont>("Fonts/ScoreBig");

            // Create objects
            players = new List<Player>();
            for (int i = 0; i < GameData.numPlayers; i++)
            {
                Vector2 spawnLoc = new Vector2(GameData.PLAYER_START, -rand.Next(GameData.MIN_SPAWN, GameData.MAX_SPAWN));
				players.Add(new Player(Content.Load<Texture2D>("Art/GreenDude"), spawnLoc, GameData.playerColors[i], GameData.playerAbilities[rand.Next(GameData.playerAbilities.Length)]));
            }
            floors = new List<Floor>();
            walls = new List<Wall>();
            particles = new List<Particle>();
            obstacles = new List<Obstacle>();
            world = new World(this);

            // Initialize camera
            int width = graphics.GraphicsDevice.Viewport.Width;
            int height = graphics.GraphicsDevice.Viewport.Height;
            cameraBounds = new Rectangle((int)(width * GameData.SCREEN_LEFT), (int)(height * GameData.SCREEN_TOP),
                (int)(width * (GameData.SCREEN_RIGHT - GameData.SCREEN_LEFT)), (int)(height * (1 - 2 * GameData.SCREEN_TOP)));
            screenCenter = cameraBounds.Center.ToVector2();
            screenOffset = Vector2.Zero;

            // Make menus
            float buttonWidth = width * GameData.BUTTON_WIDTH;
            float buttonHeight = height * GameData.BUTTON_HEIGHT;
            float left = (width - buttonWidth) / 2f;
            float centerY = (height - buttonHeight) / 2f;

            pauseMenu = new List<Button>();
            pauseMenu.Add(new Button(whiteRect, new Vector2(left, buttonHeight), new Vector2(buttonWidth, buttonHeight),
                delegate() { state = State.Running; }, Color.RoyalBlue,
                fontSmall, "Continue", Color.Red));
            pauseMenu.Add(new Button(whiteRect, new Vector2(left, centerY), new Vector2(buttonWidth, buttonHeight),
                delegate() { state = State.Options; }, Color.RoyalBlue,
                fontSmall, "Options", Color.Red));
            pauseMenu.Add(new Button(whiteRect, new Vector2(left, height - buttonHeight * 2), new Vector2(buttonWidth, buttonHeight),
                delegate() { Exit(); }, Color.RoyalBlue,
                fontSmall, "Exit", Color.Red));

            optionsMenu = new List<Button>();
            optionsMenu.Add(new Button(whiteRect, new Vector2(left, buttonHeight), new Vector2(buttonWidth, buttonHeight),
                delegate() {
                    graphics.ToggleFullScreen();
                    //graphics.PreferredBackBufferWidth = graphics.IsFullScreen ? GraphicsDevice.DisplayMode.Width : GameData.VIEW_WIDTH;
                    //graphics.PreferredBackBufferHeight = graphics.IsFullScreen ? GraphicsDevice.DisplayMode.Height : GameData.VIEW_HEIGHT;
                    //graphics.ApplyChanges();
                }, Color.Maroon, fontSmall, "Toggle fullscreen", Color.Chartreuse));
            optionsMenu.Add(new Button(whiteRect, new Vector2(left, centerY), new Vector2(buttonWidth, buttonHeight),
                delegate() { state = State.Controls; }, Color.Maroon,
                fontSmall, "Controls", Color.Chartreuse));
            optionsMenu.Add(new Button(whiteRect, new Vector2(left, height - buttonHeight * 2), new Vector2(buttonWidth, buttonHeight),
                delegate() { state = State.Paused; }, Color.Maroon,
                fontSmall, "Back", Color.Chartreuse));

            controlsMenu = new List<Button>();
            controlsMenu.Add(new Button(whiteRect, new Vector2(buttonHeight, 0), new Vector2(width / 2f - buttonHeight + 1, height - buttonHeight * 3),
                delegate() { }, Color.DarkGray,
                fontSmall, "Player 1 controls\n" + playerControls[0], Color.Chartreuse));
            controlsMenu.Add(new Button(whiteRect, new Vector2(width / 2f, 0), new Vector2(width / 2f - buttonHeight, height - buttonHeight * 3),
                delegate() { }, Color.DarkGray,
                fontSmall, "Player 2 controls\n" + playerControls[1], Color.Chartreuse));
            controlsMenu.Add(new Button(whiteRect, new Vector2(left, height - buttonHeight * 2), new Vector2(buttonWidth, buttonHeight),
                delegate() { state = State.Options; }, Color.DarkGray,
                fontSmall, "Back", Color.Chartreuse));

            // Load the level stored in LEVEL_FILE
            levelEnd = 0;
            //LoadLevel();
            MakeLevel();

            // Load the song
            Song song = Content.Load<Song>("Music/" + GameData.SONG);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = GameData.VOLUME;
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
            switch (state) {
                case State.Running:
                    if (ToggleKey(Keys.E))
                    {
                        editLevel = !editLevel;
                        if (!editLevel)
                        {
                            screenOffset = Vector2.Zero;
                            currentFloor = null;
                            currentZoom = GameData.PIXEL_METER;
                        }
                        else
                            currentZoom = GameData.PIXEL_METER_EDIT;
                        ConvertUnits.SetDisplayUnitToSimUnitRatio(currentZoom);
                    }

                    if (editLevel)
                    {
                        HandleEditLevel();
                    }
                    else
                    {
                        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                        if (currentFloor == null)
                            HandleInput(deltaTime);

                        totalTime += deltaTime;
                        float remaining = totalTime / GameData.WIN_TIME;
                        deltaTime = deltaTime * MathHelper.Lerp(1f, GameData.MAX_SPEED_SCALE, remaining);
                        if (remaining > 1)
                            remaining = 1;
                        death += MathHelper.Lerp(GameData.DEAD_START, GameData.DEAD_END, remaining) * deltaTime;

                        CheckPlayer();
                        //player.Update(gameTime.ElapsedGameTime.TotalSeconds);

                        world.Step(deltaTime);
                    }
                    if (ToggleKey(Keys.Space))
                        state = State.Paused;
                    break;
                case State.Paused:
                    HandleMenu(pauseMenu);
                    if (ToggleKey(Keys.Space))
                        state = State.Running;
                    else if (ToggleKey(Keys.Escape))
                        Exit();
                    break;
                case State.MainMenu:
                    mainMenu.UpdateInput(gameTime.ElapsedGameTime.TotalMilliseconds);
                    mainMenu.UpdateLayout(gameTime.ElapsedGameTime.TotalMilliseconds);
                    if (ToggleKey(Keys.Enter))
                        state = State.Running;
                    else if (ToggleKey(Keys.Escape))
                        Exit();
                    break;
                case State.Options:
                    HandleMenu(optionsMenu);
                    if (ToggleKey(Keys.Escape))
                        state = State.MainMenu;
                    break;
                case State.Controls:
                    HandleMenu(controlsMenu);
                    if (ToggleKey(Keys.Escape))
                        state = State.Options;
                    break;
            }

            prevKeyState = Keyboard.GetState();
            prevMouseState = Mouse.GetState();
            for (int i = 0; i < prevPadStates.Length; i++)
                prevPadStates[i] = GamePad.GetState((PlayerIndex)i, GamePadDeadZone.Circular);

            base.Update(gameTime);
        }

        /// <summary>
        /// Handles all keyboard and gamepad input for the game. Moves all players and recalculates wobble-screen.
        /// </summary>
        private void HandleInput(float deltaTime)
        {
            KeyboardState state = Keyboard.GetState();

            for (int i = 0; i < players.Count; i++)
            {
                Player player = players[i];
                if (player.TimeSinceDeath < GameData.PHASE_TIME)
                {
                    HandleKeyboard(deltaTime, player, i);
                }

                if (player.TimeSinceDeath > 0)
                {
                    player.TimeSinceDeath -= deltaTime;
                    if (player.TimeSinceDeath < 0)
                    {
                        Body spawnProtect = new Floor(whiteRect,
                            new Vector2(player.Position.X + GameData.SPAWN_PROTECT / 2f - player.Size.X, player.Position.Y), GameData.SPAWN_PROTECT);
                        for (int x = walls.Count - 1; x >= 0; x--)
                        {
                            Wall wall = walls[x];
                            if (spawnProtect.Intersects(wall) != Vector2.Zero)
                                walls.RemoveAt(x);
                        }
                    }
                }
            }

            if (ToggleKey(Keys.R))                        // reset
            {
                Reset();
            }

            // Find average velocity across the players
            Vector2 averageVel = Vector2.Zero;
            foreach (Player player in players)
                averageVel += player.Velocity;
            averageVel /= players.Count;

            // Calculate wobble-screen
            float deltaX = ((cameraBounds.Center.X - screenCenter.X) / cameraBounds.Width - averageVel.X / GameData.RUN_VELOCITY) * GameData.CAMERA_SCALE_X;
            deltaX = MathHelper.Clamp(deltaX, -GameData.MAX_CAMERA_SPEED_X, GameData.MAX_CAMERA_SPEED_X);
            screenCenter.X += deltaX * deltaTime;
            screenCenter.X = MathHelper.Clamp(screenCenter.X, cameraBounds.Left, cameraBounds.Right);

            float deltaY = ((cameraBounds.Center.Y - screenCenter.Y) / cameraBounds.Height - averageVel.Y / GameData.RUN_VELOCITY) * GameData.CAMERA_SCALE_Y;
            deltaY = MathHelper.Clamp(deltaY, -GameData.MAX_CAMERA_SPEED_Y, GameData.MAX_CAMERA_SPEED_Y);
            screenCenter.Y += deltaY * deltaTime;
            screenCenter.Y = MathHelper.Clamp(screenCenter.Y, cameraBounds.Top, cameraBounds.Bottom);

            float wobbleRatio = GameData.RUN_VELOCITY / (GameData.RUN_VELOCITY - averageVel.X);
            if (wobbleRatio >= GameData.MAX_WOBBLE)
                wobbleScreen(GameData.MAX_WOBBLE);
            else if (wobbleRatio >= GameData.MIN_WOBBLE)
                wobbleScreen(wobbleRatio);
        }

        private void Reset()
        {
            foreach (Player player in players)
            {
                player.MoveToPosition(new Vector2(GameData.PLAYER_START, -rand.Next(GameData.MIN_SPAWN, GameData.MAX_SPAWN)));
                player.Velocity = Vector2.Zero;
                player.TimeSinceDeath = 0;
                player.BoostTime = GameData.BOOST_LENGTH;
            }
            floors.Clear();
            walls.Clear();
            obstacles.Clear();
            levelEnd = 0;
            totalTime = 0;
            death = -GameData.DEAD_MAX;
            rand = new Random();
            randLevel = new Random(GameData.GetSeed);
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
            GameData.Controls controls = playerControls[controller];

            //float impulse = GameData.MAX_ACCEL * deltaTime;
            //float impulse = MathHelper.SmoothStep(MAX_IMPULSE, 0f, Math.Abs(player.Velocity.X) / MAX_VELOCITY) * deltaTime;
            //impulse = (float)Math.Pow(impulse, IMPULSE_POW);

            float slow = GameData.SLOWDOWN * deltaTime;
            if (player.CurrentState == Player.State.Jumping)
            {
                slow *= GameData.AIR_RESIST;
            }

            if (!player.InAir)
            {
                if (controls.Boost)                    // boost
                {
                    if (player.BoostTime > GameData.BOOST_LENGTH / 2)
                    {
                        player.TargetVelocity = GameData.BOOST_SPEED;
                        player.CurrentState = Player.State.Boosting;
                    }
                    else if (player.CurrentState != Player.State.Boosting)
                        player.TargetVelocity = GameData.RUN_VELOCITY;
                }
                else                           // normal run
                {
                    player.TargetVelocity = GameData.RUN_VELOCITY;
                    player.CurrentState = Player.State.Walking;
                }
                if (controls.Jump)     // jump
                {
                    //if (player.CurrentState == Player.State.Boosting)
                    //{
                        //Console.WriteLine("Boosting jump");
                        //player.BoostTime -= GameData.JUMP_COST;
                        //if (player.BoostTime < 0)
                        //    player.BoostTime = 0;
                    //}

                    //Console.WriteLine("Player state: " + player.CurrentState);
                    player.Velocity.Y = -GameData.JUMP_SPEED;
                    player.TargetVelocity = player.TargetVelocity * GameData.JUMP_SLOW;
                    player.CurrentState = Player.State.Jumping;
                }
                else if (controls.Slam)
                {
                    player.CurrentState = Player.State.Slamming;
                    if (player.Velocity.Y < GameData.SLAM_SPEED)
                        player.Velocity.Y = GameData.SLAM_SPEED;
                }
            }
            else
            {
                if (controls.Slam)
                {
                    player.CurrentState = Player.State.Slamming;
                    if (player.Velocity.Y < GameData.SLAM_SPEED)
                        player.Velocity.Y = GameData.SLAM_SPEED;
                }
                else
                {
                    player.CurrentState = Player.State.Jumping;
                }
                //if (player.Velocity.X == 0)
                //    player.Velocity.X = GameData.RUN_VELOCITY;
            }
            if (controls.Shoot && player.TimeSinceDeath <= 0 && player.BoostTime > GameData.SHOOT_COST)
            {
                player.Projectiles.Add(new Projectile(whiteRect, new Vector2(player.Position.X - player.Size.X / 2f, player.Position.Y), player.Color));
                player.BoostTime -= GameData.SHOOT_COST;
                //Console.WriteLine("Shooting!");
            }
            if (controls.Special)                // activate (or toggle) special
            {
                player.AbilityActive = !player.AbilityActive;
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
                else if (keyboard.IsKeyDown(Keys.Enter))
                    currentFloor = null;
                else if (ToggleKey(Keys.F))
                {
                    currentFloor.Health = currentFloor.Health == 0 ? GameData.STAIR_HEALTH : 0;
                    //Console.WriteLine("Made floor" + (currentFloor.Breakable ? "" : " not") + " breakable");
                    currentFloor.Color = currentFloor.Health != 0 ? Color.LightGoldenrodYellow : Color.Azure;
                }
            }
            if (ToggleKey(Keys.OemPlus))                       // Zoom in and out
            {
                currentZoom *= GameData.ZOOM_STEP;
                ConvertUnits.SetDisplayUnitToSimUnitRatio(currentZoom);
            }
            else if (ToggleKey(Keys.OemMinus))
            {
                currentZoom /= GameData.ZOOM_STEP;
                ConvertUnits.SetDisplayUnitToSimUnitRatio(currentZoom);
            }
        }

        public bool ToggleKey(Keys key)
        {
            return Keyboard.GetState().IsKeyDown(key) && prevKeyState.IsKeyUp(key);
        }

        public bool ToggleButton(PlayerIndex playerIndex, Buttons button)
        {
            return GamePad.GetState(playerIndex, GamePadDeadZone.Circular).IsButtonDown(button) && prevPadStates[(int)playerIndex].IsButtonUp(button);
        }

        /// <summary>
        /// Checks if the user is off the level, and resets the player if it is. Also, updates player-related game things (documentation WIP)
        /// </summary>
        private void CheckPlayer()
        {
            Player max = null;
            float minY = players[0].Position.Y;
            float maxY = minY;

            float averageX = 0;
            foreach (Player player in players)
                averageX += player.Position.X;
            averageX /= players.Count;

            if (averageX - death > GameData.DEAD_MAX)
                death = averageX - GameData.DEAD_MAX;

            foreach (Player player in players)
            {
                if (player.Position.X > levelEnd - GameData.LOAD_NEW)
                    MakeLevel();

                if ((max == null || player.Position.X > max.Position.X) && player.TimeSinceDeath <= 0)
                    max = player;

                if (player.Position.Y < minY)
                    minY = player.Position.Y;
                else if (player.Position.Y > maxY)
                    maxY = player.Position.Y;
            }

            foreach (Player player in players)
            {
                if (player.TimeSinceDeath > 0)
                {
                    bool allDead = max == null;

                    float val = (float)(player.TimeSinceDeath / GameData.DEAD_TIME);
                    float targetX = allDead ? averageX : max.Position.X;
                    float targetY = allDead ? -GameData.RESPAWN_DIST : max.Position.Y;
                    float newX = MathHelper.Lerp(targetX, player.Position.X, val);
                    //float newY = MathHelper.Lerp(targetY, allDead ? 0 : player.Position.Y, val);
                    float newY = MathHelper.Lerp(player.SpawnY, player.Position.Y, val);

                    player.MoveToPosition(new Vector2(newX, newY));
                    //player.Velocity = new Vector2(newX - player.Position.X, newY - player.Position.Y) * val;
                    //Console.WriteLine("Player moving to " + new Vector2(newX, newY));
                    //Console.WriteLine("Target " + new Vector2(targetX, targetY));
                    //Console.WriteLine("Max: " + max == null);
                }
                else if (player.Position.X < averageX - ConvertUnits.ToSimUnits(GameData.DEAD_DIST))
                {
                    player.Kill(rand);
                    if (max != null)
                        max.Score++;
                    //if (--player.Score < 0)
                    //    player.Score = 0;
                }
#if !DEBUG
                else if (player.Position.X < death)
                {
                    player.Kill(rand);
                    player.Score -= GameData.DEATH_LOSS;
                    //if (player.Score < 0)
                    //    player.Score = 0;
                }
#endif
            }

            float currentX = max == null ? averageX : max.Position.X;
#if !DEBUG
            if (currentX < death)   // lose
            {
                foreach (Player player in players)
                {
                    player.Score += (int)totalTime / GameData.WIN_SCORE;
                    //if (player.Score < 0)
                    //    player.Score = 0;
                }
                Reset();
            }
#endif

            //if (totalTime > GameData.WIN_TIME)  // win. TODO -- do more than reset
            //{
            //    Reset();
            //    if (max != null)
            //        max.Score += GameData.WIN_SCORE;
            //}

            //float currentRatio = editLevel ? GameData.PIXEL_METER_EDIT : GameData.PIXEL_METER;
            float dist = maxY - minY;
            if (dist * currentZoom / GameData.SCREEN_SPACE > GraphicsDevice.Viewport.Height)
                ConvertUnits.SetDisplayUnitToSimUnitRatio(GraphicsDevice.Viewport.Height * GameData.SCREEN_SPACE / dist);
            else
                ConvertUnits.SetDisplayUnitToSimUnitRatio(currentZoom);
        }

        ///// <summary>
        ///// Load the level specified in level and increments levelEnd
        ///// </summary>
        //private void LoadLevel()
        //{
        //    levelEnd += levels.LoadLevel(levelEnd) + GameData.LEVEL_DIST;
        //    Console.WriteLine("LevelEnd: " + levelEnd);
        //}

        private void MakeLevel()
        {
            int width = randLevel.Next(GameData.MIN_LEVEL_WIDTH, GameData.MAX_LEVEL_WIDTH);
            int numFloors = randLevel.Next(GameData.MIN_NUM_FLOORS, GameData.MAX_NUM_FLOORS);

            int minStep = randLevel.Next(GameData.MIN_LEVEL_STEP, GameData.MAX_LEVEL_STEP);
            int maxStep = randLevel.Next(GameData.MIN_LEVEL_STEP, GameData.MAX_LEVEL_STEP);
            //int minStep = GameData.MIN_LEVEL_STEP;
            //int maxStep = GameData.MAX_LEVEL_STEP;
            if (maxStep < minStep)
            {
                int temp = maxStep;
                maxStep = minStep;
                minStep = temp;
            }

            //float x = levelEnd + width / 2f;
            float step = randLevel.Next(minStep, maxStep);
            float y = -step;
            for (int i = 0; i < numFloors; i++)
            {
                walls.Add(new Wall(whiteRect, new Vector2(levelEnd + Wall.WALL_WIDTH / 2, y + step / 2 + Floor.FLOOR_HEIGHT / 2), step, GameData.WINDOW_HEALTH));
                walls.Add(new Wall(whiteRect, new Vector2(levelEnd + width - Wall.WALL_WIDTH / 2, y + step / 2 + Floor.FLOOR_HEIGHT / 2), step, GameData.WINDOW_HEALTH));

                float dist = 0;
                while (dist < width)
                {
                    float floorSize = (float)randLevel.NextDouble() * GameData.MAX_FLOOR_DIST + GameData.MIN_FLOOR_DIST;
                    if (floorSize > width - dist)
                    {
                        floorSize = width - dist;
                        if (floorSize < GameData.MIN_FLOOR_WIDTH)
                            break;
                    }
                    floors.Add(new Floor(whiteRect, new Vector2(levelEnd + dist + floorSize / 2, y), floorSize));
                    float holeSize = (float)randLevel.NextDouble() * GameData.MAX_FLOOR_HOLE + GameData.MIN_FLOOR_HOLE;
                    dist += floorSize + holeSize;

                    if (dist < width && randLevel.NextDouble() < GameData.STAIR_CHANCE && holeSize > GameData.MIN_STAIR_DIST)
                    {
                        Floor stair = new Floor(whiteRect, new Vector2(levelEnd + dist - GameData.STAIR_WIDTH, y + step), new Vector2(levelEnd + dist, y));

                        int numCollisions = 0;
                        foreach (Floor floor in floors)
                        {
                            if (stair.Intersects(floor) != Vector2.Zero)
                            {
                                //floor.Color = Color.Green;
                                if (++numCollisions > 0)
                                    break;
                            }
                        }

                        if (numCollisions > 0)
                        {
                            stair.Health = GameData.STAIR_HEALTH;
                            stair.Color = Color.LightGoldenrodYellow;
                            floors.Add(stair);
                        }
                    }
                }

                dist = randLevel.Next(GameData.MIN_WALL_DIST, GameData.MAX_WALL_DIST);
                while (dist < width)
                {
                    Wall wall = new Wall(whiteRect, new Vector2(levelEnd + dist, y + step / 2), step - Floor.FLOOR_HEIGHT, GameData.WALL_HEALTH);

                    bool validStair = true;
                    int numCollisions = 0;
                    foreach (Floor floor in floors)
                    {
                        if (wall.Intersects(floor) != Vector2.Zero)
                        {
                            //floor.Color = Color.Plum;
                            if (floor.Health > 0)
                            {
                                wall = null;
                                validStair = false;
                                //Console.WriteLine("Floor is colliding with wall " + floor.Position);
                                break;
                            }
                            else
                            {
                                if (++numCollisions > 1 && !validStair)
                                    break;
                            }
                        }
                    }

                    if (validStair && numCollisions > 1)
                       walls.Add(wall);
                    dist += randLevel.Next(GameData.MIN_WALL_DIST, GameData.MAX_WALL_DIST);
                }

                step = randLevel.Next(minStep, maxStep);
                y -= step;
            }
            levelEnd += width + randLevel.Next(GameData.LEVEL_DIST_MIN, GameData.LEVEL_DIST_MAX);

            if (floors.Count > GameData.MAX_FLOORS)
                floors.RemoveRange(0, floors.Count - GameData.MAX_FLOORS);
            if (walls.Count > GameData.MAX_WALLS)
                walls.RemoveRange(0, walls.Count - GameData.MAX_WALLS);
            if (obstacles.Count > GameData.MAX_OBSTACLES)
                obstacles.RemoveRange(0, obstacles.Count - GameData.MAX_OBSTACLES);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            double deltaTime = (double)gameTime.ElapsedGameTime.TotalSeconds;
            GraphicsDevice.Clear(Color.CornflowerBlue);

            switch (state) {
                case State.Running:
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


                    // Draw players
                    spriteBatch.Begin(transformMatrix: view);
                    foreach (Player player in players)
                    {
                        if (player.TimeSinceDeath < GameData.PHASE_TIME)
                        {
                            player.Sprite.Update(deltaTime);
                            player.Draw(spriteBatch);
                            foreach (Projectile proj in player.Projectiles)
                                proj.Draw(spriteBatch);
                        }
                    }
                    spriteBatch.End();


                    // Draw all objects
                    spriteBatch.Begin(transformMatrix: view);
                    spriteBatch.Draw(whiteRect, new Rectangle(-(int)view.Translation.X, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.LightGray);
                    foreach (Floor floor in floors)
                    {
                        floor.Draw(spriteBatch);
                        //DrawRect(floor, Color.Green, Vector2.Zero, new Vector2(0.6f));
                    }
                    foreach (Wall wall in walls)
                        wall.Draw(spriteBatch);
                    foreach (Obstacle obstacle in obstacles)
                        obstacle.Draw(spriteBatch);
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


                    // Draw all particles and dead wall
                    spriteBatch.Begin(transformMatrix: view);
                    foreach (Particle part in particles)
                        part.Draw(spriteBatch);
                    spriteBatch.Draw(whiteRect, new Rectangle((int)ConvertUnits.ToDisplayUnits(death) - GameData.DEAD_WIDTH, -GameData.DEAD_HEIGHT, GameData.DEAD_WIDTH, GameData.DEAD_HEIGHT), Color.Purple); // please excuse these magic numbers, they are meaningless
                    spriteBatch.End();


                    // Draw all HUD elements
                    // TODO display a proper pause menu
                    spriteBatch.Begin();
                    //if (paused)
                    //{
                    //    float centerX = GraphicsDevice.Viewport.Width / 2.0f - fontBig.MeasureString("Paused").X / 2.0f;
                    //    spriteBatch.DrawString(fontBig, "Paused", new Vector2(centerX, GraphicsDevice.Viewport.Height * 0.1f), Color.Yellow);
                    //}

                    // Display scores in the top left
                    System.Text.StringBuilder text = new System.Text.StringBuilder();
                    text.AppendLine("Scores");
                    for (int i = 0; i < players.Count; i++)
                    {
                        text.AppendLine(string.Format("Player {0}: {1}", i + 1, players[i].Score));
                    }
                    spriteBatch.DrawString(fontSmall, text, new Vector2(10, 10), Color.Green);

                    // Display frames per second in the top right
                    string frames = (1f / deltaTime).ToString("n2");
                    float leftX = GraphicsDevice.Viewport.Width - fontSmall.MeasureString(frames).X;
                    spriteBatch.DrawString(fontSmall, frames, new Vector2(leftX, 0f), Color.LightGray);

                    // Display current survived time
                    string time = totalTime.ToString("n1") + "s survived";
                    leftX = GraphicsDevice.Viewport.Width / 2f - fontSmall.MeasureString(time).X / 2f;
                    spriteBatch.DrawString(fontSmall, time, new Vector2(leftX, 0f), Color.LightSkyBlue);

                    // Display version number
                    Vector2 pos = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height) - fontSmall.MeasureString(GameData.Version);
                    spriteBatch.DrawString(fontSmall, GameData.Version, pos, Color.LightSalmon);

                    //if (levelAnnounceWaitAt > 0)
                    //{
                    //    float centerX = GraphicsDevice.Viewport.Width / 2.0f - fontBig.MeasureString("Level " + currentLevel).X / 2.0f;
                    //    spriteBatch.DrawString(fontBig, "Level " + currentLevel, new Vector2(centerX, GraphicsDevice.Viewport.Height * 0.1f), Color.Aqua);
                    //    levelAnnounceWaitAt -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    //}
                    spriteBatch.End();
                    break;
                case State.Paused:
                    GraphicsDevice.Clear(Color.Yellow);
                    spriteBatch.Begin();
                    foreach (Button button in pauseMenu)
                        button.Draw(spriteBatch);
                    spriteBatch.End();
                    break;
                case State.MainMenu:
                    GraphicsDevice.Clear(Color.Turquoise);
                    mainMenu.Draw(gameTime.ElapsedGameTime.TotalMilliseconds);
                    break;
                case State.Options:
                    GraphicsDevice.Clear(Color.Chocolate);
                    spriteBatch.Begin();
                    foreach (Button button in optionsMenu)
                        button.Draw(spriteBatch);
                    spriteBatch.End();
                    break;
                case State.Controls:
                    GraphicsDevice.Clear(Color.Plum);
                    spriteBatch.Begin();
                    foreach (Button button in controlsMenu)
                        button.Draw(spriteBatch);
                    spriteBatch.End();
                    break;
            }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="position">Bottom left of rectangle</param>
        /// <param name="color"></param>
        /// <param name="scale">Rectangle will be scaled from the bottom left</param>
        public static void DrawRectangle(SpriteBatch spriteBatch, Vector2 position, Color color, Vector2 scale)
        {
            spriteBatch.Draw(whiteRect, ConvertUnits.ToDisplayUnits(position), null, color, 0f, Vector2.Zero, ConvertUnits.ToDisplayUnits(scale), SpriteEffects.None, 0f);
        }

        private void HandleMenu(List<Button> menu)
        {
            foreach (Button button in menu)
            {
                MouseState mouse = Mouse.GetState();
                if (button.TestPoint(mouse.Position))
                {
                    if (mouse.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
                    {
                        button.OnClick();
                    }
                    else
                    {
                        // TODO some hover over display
                    }
                }
            }
        }
    }
}