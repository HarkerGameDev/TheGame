//#define LIGHTING

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;

using Source.Collisions;
using Source.Graphics;

using EmptyKeys.UserInterface;
using EmptyKeys.UserInterface.Generated;

using Ziggyware;

namespace Source
{
    /// <summary>
    /// This is the main type for the game.
    /// 
    /// IMPORTANT NOTES - PLEASE READ ALL:
    /// - Please use (0,0) as the bottom left for all levels -- you will see a green box there when editing
    /// - (0,0) is in the top left for drawing to screen, and (0,0) is in the center of a physics object
    /// - Physics uses meters, monogame uses pixels -- use ConvertUnits to convert
    /// - Please follow the style guide in place, which is
    ///   * ALL_CAPS for global constants (define in GameData)
    ///   * UpperCamelCase for members (instance fields and methods)
    ///   * lowerCamelCase for other variables
    /// - Search for 'TODO' for things that need to be finished
    /// - For things that need to be added, look at the bottom of the google doc, https://docs.google.com/document/d/1ofddsIU92CeK2RtJ5eg3PWEG8U2o49VdmNxmAJwwMMg/edit
    /// - Make good commit messages
    /// - There are no "assigned tasks" anymore, just subteams - do what needs to be done on the TOOD list (google doc)
    /// - No magic numbers permitted!
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        KeyboardState prevKeyState;
        GamePadState[] prevPadStates;
        MouseState prevMouseState;

        GameData.Controls[] playerControls;

        public static Texture2D whiteRect;
        Tuple<Texture2D, float, float, float>[] prevBackground, background;
        float fadeTime;
        public SpriteFont fontSmall, fontBig;

#if LIGHTING
        QuadRenderComponent quadRender;
        ShadowmapResolver shadowmapResolver;
        LightArea lightArea;
        RenderTarget2D screenShadows;
#endif

        public Random rand;
        Random randLevel;
        World world;

        Rectangle cameraBounds; // limit of where the player can be on screen
        Vector2 screenCenter; // where the player is on the screen
        Vector2 screenOffset; // offset from mouse panning
        float currentZoom = GameData.PIXEL_METER;

        bool editLevel = false;
        public Floor currentFloor;
        bool editingFloor;
        Vector2 startDraw;
        Vector2 endDraw;

        State state = State.MainMenu;
        float totalTime = 0;
        float highScore = 0;

        public List<Player> players;
        public List<Floor> floors;
        public List<Particle> particles;
        public List<Obstacle> obstacles;
        public List<Drop> drops;

        List<Button> pauseMenu;
        List<Button> optionsMenu;
        List<Button> controlsMenu;

        int levelEnd = 0;

        MainMenu mainMenu;

        int nativeScreenWidth;
        int nativeScreenHeight;

        List<float> times;
        List<GameData.ControlKey> keys;
        int randSeed, randLevelSeed;
        bool prevLeft = false;
        bool prevRight = false;
        bool prevJump = false;
        bool simulating = false;
        int simIndex = 0;
        int currentReplay = 0;

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

#if LIGHTING
            quadRender = new QuadRenderComponent(this);
            Components.Add(quadRender);
            Components.Add(new GamerServicesComponent(this));
#endif
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

            IsFixedTimeStep = true;
            graphics.SynchronizeWithVerticalRetrace = false;
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
            ConvertUnits.SetDisplayUnitToSimUnitRatio(currentZoom);

            // Set seed for a scheduled random level (minutes since Jan 1, 2015)
            randSeed = DateTime.Now.Millisecond;
            randLevelSeed = GameData.GetSeed;

            // Set variables
            times = new List<float>();
            keys = new List<GameData.ControlKey>();

            // Initialize previous keyboard and gamepad states
            prevKeyState = new KeyboardState();
            prevMouseState = new MouseState();
            prevPadStates = new GamePadState[4];
            for (int i = 0; i < prevPadStates.Length; i++)
                prevPadStates[i] = new GamePadState();

            if (simulating)
            {
                LoadReplay(currentReplay);
            }
            else
            {
                playerControls = new GameData.Controls[] {
                                                       new GameData.KeyboardControls(this, Keys.OemComma, Keys.OemPeriod, Keys.OemQuestion, Keys.Left, Keys.Right, Keys.Up, Keys.Left),
                                                       new GameData.KeyboardControls(this, Keys.D1, Keys.D2, Keys.D3, Keys.A, Keys.D, Keys.W, Keys.A),
                                                       new GameData.GamePadControls(this, PlayerIndex.One, Buttons.X, Buttons.B, Buttons.Y, Buttons.LeftThumbstickLeft, Buttons.LeftThumbstickRight, Buttons.RightTrigger, Buttons.A)
                                                  };
            }

            rand = new Random(randSeed);
            randLevel = new Random(randLevelSeed);

            base.Initialize();      // This calls LoadContent()
        }

        private void LoadReplay(int replay)
        {
            // TODO load replays with UI
            IAsyncResult result = StorageDevice.BeginShowSelector(null, null);
            result.AsyncWaitHandle.WaitOne();
            StorageDevice device = StorageDevice.EndShowSelector(result);
            result.AsyncWaitHandle.Close();
            if (device != null && device.IsConnected)
            {
                result = device.BeginOpenContainer("Game", null, null);
                result.AsyncWaitHandle.WaitOne();
                StorageContainer container = device.EndOpenContainer(result);
                result.AsyncWaitHandle.Close();

                string directory = "Replays";
                string filename = directory + @"\replay";
                int max = container.GetFileNames(filename + "*").Length - 1;
                if (replay > max)
                    replay = max;
                filename += replay + ".rep";
                Console.WriteLine("Max: " + max);

                BinaryReader file = new BinaryReader(container.OpenFile(filename, FileMode.Open));
                randSeed = file.ReadInt32();
                randLevelSeed = file.ReadInt32();
                while (file.BaseStream.Position != file.BaseStream.Length)
                {
                    times.Add(file.ReadSingle());
                    keys.Add((GameData.ControlKey)file.ReadByte());
                    Console.WriteLine("Loading: " + times[times.Count - 1]);
                }
                Console.WriteLine("Loaded replay" + replay + ".rep");
                Console.WriteLine("Loading\trandSeed: " + randSeed + "\trandLevelSeed: " + randLevelSeed);

                file.Close();
                container.Dispose();
            }

            playerControls = new GameData.Controls[] { new GameData.SimulatedControls(this), new GameData.SimulatedControls(this) };
        }

        private void Reset()
        {
            if (!simulating)
            {
                if (totalTime > highScore)
                    highScore = totalTime;

                // save replay
                IAsyncResult result = StorageDevice.BeginShowSelector(null, null);
                result.AsyncWaitHandle.WaitOne();
                StorageDevice device = StorageDevice.EndShowSelector(result);
                result.AsyncWaitHandle.Close();
                if (device != null && device.IsConnected)
                {
                    result = device.BeginOpenContainer("Game", null, null);
                    result.AsyncWaitHandle.WaitOne();
                    StorageContainer container = device.EndOpenContainer(result);
                    result.AsyncWaitHandle.Close();

                    string directory = "Replays";
                    string filename = directory + @"\replay";
                    filename += container.GetFileNames(filename + "*").Length + ".rep";

                    BinaryWriter file = new BinaryWriter(container.OpenFile(filename, FileMode.Create));
                    file.Write(randSeed);
                    file.Write(randLevelSeed);
                    for (int i = 0; i < times.Count; i++)
                    {
                        file.Write(times[i]);
                        file.Write((byte)keys[i]);
                    }

                    file.Close();
                    container.Dispose();
                }
            }

            randSeed = rand.Next();
            randLevelSeed = rand.Next();
            if (simulating)
                LoadReplay(++currentReplay);
            Console.WriteLine("Using:\trandSeed: " + randSeed + "\trandLevelSeed: " + randLevelSeed);
            rand = new Random(randSeed);
            randLevel = new Random(randLevelSeed);
            foreach (Player player in players)
            {
                player.MoveToPosition(new Vector2(GameData.PLAYER_START, -rand.Next(GameData.MIN_SPAWN, GameData.MAX_SPAWN)));
                player.ResetValues();
            }
            floors.Clear();
            particles.Clear();
            obstacles.Clear();
            drops.Clear();
            levelEnd = 0;
            totalTime = 0;

            if (!simulating)
            {
                times.Clear();
                keys.Clear();
            }
            else
            {
                simIndex = 0;
                playerControls = new GameData.Controls[] { new GameData.SimulatedControls(this), new GameData.SimulatedControls(this) };
                //simulating = false;
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Content.RootDirectory = "Content";
            spriteBatch = new SpriteBatch(GraphicsDevice);

#if LIGHTING
            //Initialize shadows
            shadowmapResolver = new ShadowmapResolver(GraphicsDevice, quadRender, ShadowmapSize.Size256, ShadowmapSize.Size1024);
            shadowmapResolver.LoadContent(Content);
            lightArea = new LightArea(GraphicsDevice, ShadowmapSize.Size128);
            screenShadows = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
#endif

            // Load menus
            SpriteFont font = Content.Load<SpriteFont>("Fonts/Segoe_UI_15_Bold");
            FontManager.DefaultFont = Engine.Instance.Renderer.CreateFont(font);
            mainMenu = new MainMenu();
            FontManager.Instance.LoadFonts(Content, "Fonts");

            // Use this to draw any rectangles
            whiteRect = new Texture2D(GraphicsDevice, 1, 1);
            whiteRect.SetData(new[] { Color.White });

            // Load assets in the Content Manager
            //background = Content.Load<Texture2D>("Art/skyscrapers");
            LoadLevel(2);
            fontSmall = Content.Load<SpriteFont>("Fonts/Score");
            fontBig = Content.Load<SpriteFont>("Fonts/ScoreBig");

            // Create objects
            players = new List<Player>();
            for (int i = 0; i < GameData.numPlayers; i++)
            {
                // create a player with color specified in GameData and random color
                Vector2 spawnLoc = new Vector2(GameData.PLAYER_START, -rand.Next(GameData.MIN_SPAWN, GameData.MAX_SPAWN));
				players.Add(new Player(Content.Load<Texture2D>("Art/GreenDude"), spawnLoc, Character.playerCharacters[rand.Next(Character.playerCharacters.Length)]));
            }
            floors = new List<Floor>();
            particles = new List<Particle>();
            obstacles = new List<Obstacle>();
            drops = new List<Drop>();
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
                    state = State.Controls;
                }, Color.Maroon, fontSmall, "Controls", Color.Chartreuse));
            optionsMenu.Add(new Button(whiteRect, new Vector2(left, centerY), new Vector2(buttonWidth, buttonHeight),
                delegate() {
                    simulating = true;
                    currentReplay = int.MaxValue;
                    LoadReplay(currentReplay);
                    Reset();
                    state = State.Running;
                }, Color.Maroon,
                fontSmall, "Instant replay", Color.Chartreuse));
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
            //LoadLevel();
            MakeLevel();

            //// Load the song
            //Song song = Content.Load<Song>("Music/" + GameData.SONG);
            //MediaPlayer.IsRepeating = true;
            //MediaPlayer.Volume = GameData.VOLUME;
            //MediaPlayer.Play(song);
        }

        /// <summary>
        /// Loads a level and stores it into background
        /// </summary>
        /// <param name="loadLevel"></param>
        private void LoadLevel(int loadLevel)
        {
            float[][] level = GameData.WORLD_LAYERS[loadLevel];
            background = new Tuple<Texture2D, float, float, float>[level.Length];
            for (int i = 0; i < level.Length; i++)
            {
                float[] layer = level[i];
                background[i] = new Tuple<Texture2D, float, float, float>(
                    Content.Load<Texture2D>(string.Format("Worlds/World{0}/layer{1}", loadLevel, i)),
                    layer[0], layer[1], layer[2]);
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

                    if (ToggleKey(Keys.I))
                        LoadLevel(0);
                    else if (ToggleKey(Keys.O))
                        LoadLevel(1);
                    else if (ToggleKey(Keys.P))
                        LoadLevel(2);

                    if (editLevel)
                    {
                        HandleEditLevel();
                    }
                    else
                    {
                        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (simulating)
                        {
                            while (simIndex < times.Count && times[simIndex] - totalTime <= 0)
                            {
                                //deltaTime += diff;
                                Console.Write("Sim: " + totalTime + "\t\tReplay: " + times[simIndex] +
                                    "\t\tDelta: " + deltaTime + "\t\tDiff: " + (times[simIndex] - totalTime) + "\t\t");
                                GameData.SimulatedControls control = (GameData.SimulatedControls)playerControls[0];
                                GameData.ControlKey key = keys[simIndex];
                                switch (key)
                                {
                                    case GameData.ControlKey.Special1:
                                        control.Special1 = true;
                                        break;
                                    case GameData.ControlKey.Special2:
                                        control.Special2 = true;
                                        break;
                                    case GameData.ControlKey.Special3:
                                        control.Special3 = true;
                                        break;
                                    case GameData.ControlKey.Left:
                                        control.Left = !control.Left;
                                        break;
                                    case GameData.ControlKey.Right:
                                        control.Right = !control.Right;
                                        break;
                                    case GameData.ControlKey.Jump:
                                        control.Jump = !control.Jump;
                                        break;
                                    case GameData.ControlKey.Action:
                                        control.Action = true;
                                        break;
                                }
                                simIndex++;
                            }
                        }

                        if (currentFloor == null)
                            HandleInput(deltaTime);

                        if (simulating)
                        {
                            GameData.SimulatedControls control = (GameData.SimulatedControls)playerControls[0];
                            control.Special1 = false;
                            control.Special2 = false;
                            control.Special3 = false;
                            control.Action = false;
                        }

                        CheckPlayer();
                        totalTime += deltaTime;
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
            for (int i = 0; i < players.Count; i++)
            {
                Player player = players[i];
                if (player.TimeSinceDeath < GameData.PHASE_TIME)
                {
                    HandlePlayerInput(player, i, deltaTime);
                }

                if (player.TimeSinceDeath > 0)
                {
                    player.TimeSinceDeath -= deltaTime;
                    //if (player.TimeSinceDeath < 0)
                    //{
                    //    Body spawnProtect = new Floor(whiteRect,
                    //        new Vector2(player.Position.X + GameData.SPAWN_PROTECT / 2f - player.Size.X, player.Position.Y), GameData.SPAWN_PROTECT);
                    //    for (int x = walls.Count - 1; x >= 0; x--)
                    //    {
                    //        Wall wall = walls[x];
                    //        if (spawnProtect.Intersects(wall) != Vector2.Zero)
                    //            walls.RemoveAt(x);
                    //    }
                    //}
                }
            }

            // Find average velocity across the players
            Vector2 averageVel = Vector2.Zero;
            foreach (Player player in players)
                averageVel += player.Velocity;
            averageVel /= players.Count;

            // Calculate wobble-screen
            //float deltaX = ((cameraBounds.Center.X - screenCenter.X) / cameraBounds.Width - averageVel.X / GameData.RUN_VELOCITY) * GameData.CAMERA_SCALE_X;
            //deltaX = MathHelper.Clamp(deltaX, -GameData.MAX_CAMERA_SPEED_X, GameData.MAX_CAMERA_SPEED_X);
            //screenCenter.X += deltaX * deltaTime;
            //screenCenter.X = MathHelper.Clamp(screenCenter.X, cameraBounds.Left, cameraBounds.Right);

            //float deltaY = ((cameraBounds.Center.Y - screenCenter.Y) / cameraBounds.Height - averageVel.Y / GameData.RUN_VELOCITY) * GameData.CAMERA_SCALE_Y;
            //deltaY = MathHelper.Clamp(deltaY, -GameData.MAX_CAMERA_SPEED_Y, GameData.MAX_CAMERA_SPEED_Y);
            //screenCenter.Y += deltaY * deltaTime;
            //screenCenter.Y = MathHelper.Clamp(screenCenter.Y, cameraBounds.Top, cameraBounds.Bottom);

            if (ToggleKey(Keys.R))                        // reset
            {
                Reset();
            }
        }

        /// <summary>
        /// Handles input for a single player for given input keys
        /// </summary>
        /// <param name="player"></param>
        /// <param name="controller"></param>
        private void HandlePlayerInput(Player player, int controller, float deltaTime)
        {
            GameData.Controls controls = playerControls[controller];

            if (!simulating)
            {
                if (controls.Special1)
                {
                    times.Add(totalTime);
                    keys.Add(GameData.ControlKey.Special1);
                }
                if (controls.Special2)
                {
                    times.Add(totalTime);
                    keys.Add(GameData.ControlKey.Special2);
                }
                if (controls.Special3)
                {
                    times.Add(totalTime);
                    keys.Add(GameData.ControlKey.Special3);
                }
                if (controls.Action)
                {
                    times.Add(totalTime);
                    keys.Add(GameData.ControlKey.Action);
                }
                if (controls.Left != prevLeft)
                {
                    times.Add(totalTime);
                    prevLeft = !prevLeft;
                    keys.Add(GameData.ControlKey.Left);
                }
                if (controls.Right != prevRight)
                {
                    times.Add(totalTime);
                    prevRight = !prevRight;
                    keys.Add(GameData.ControlKey.Right);
                }
                if (controls.Jump != prevJump)
                {
                    times.Add(totalTime);
                    prevJump = !prevJump;
                    keys.Add(GameData.ControlKey.Jump);
                }
            }

            if (player.CurrentState != Player.State.Stunned)
            {
                if (controls.Jump)     // jump
                {
                    if (player.CanJump)
                    {
                        player.Velocity.Y = -GameData.JUMP_SPEED;
                        player.TargetVelocity = player.TargetVelocity * GameData.JUMP_SLOW;
                        player.CurrentState = Player.State.Jumping;
                        player.JumpTime = GameData.JUMP_TIME;
                    }
                    else if (player.JumpTime > 0)
                        player.Velocity.Y = -GameData.JUMP_SPEED;
                }
                else if (player.JumpTime > 0)
                    player.JumpTime = 0;

                if (!player.InAir)
                {
                    if (controls.Right)     // move
                        player.TargetVelocity = GameData.RUN_VELOCITY;
                    else if (controls.Left)
                        player.TargetVelocity = -GameData.RUN_VELOCITY;
                    else
                        player.TargetVelocity = 0;
                }
                //else
                //{
                //    if (player.CurrentState == Player.State.Jumping && controls.Jump && player.JumpTime > GameData.JETPACK_CUTOFF)      // jetpack
                //    {
                //        player.Velocity.Y -= GameData.JETPACK_ACCEL * deltaTime;
                //    }
                //}

                // activate (or toggle) special abilities
                if (controls.Special1)
                    player.Ability1 = !player.Ability1;
                if (controls.Special2)
                    player.Ability2 = !player.Ability2;
                if (controls.Special3)
                    player.Ability3 = !player.Ability3;
            }
        }

        private void wobbleScreen(float amplifier)
        {
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
                    currentFloor.Color = currentFloor.Color == Color.Crimson ? Color.LightGoldenrodYellow : Color.Crimson;
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
            if (keyboard.IsKeyDown(Keys.LeftControl))           // TODO save and load level
            {

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
                    float newX = MathHelper.Lerp(targetX, player.Position.X, val);
                    float newY = MathHelper.Lerp(player.SpawnY, player.Position.Y, val);

                    player.MoveToPosition(new Vector2(newX, newY));
                }
                else if (player.Position.X < averageX - ConvertUnits.ToSimUnits(GameData.DEAD_DIST))
                {
                    player.Kill(rand);
                    if (max != null)
                        max.Score++;
                }
            }

            float currentX = max == null ? averageX : max.Position.X;

            //if (totalTime > GameData.WIN_TIME)  // win. TODO -- do more than reset
            //{
            //    Reset();
            //    if (max != null)
            //        max.Score += GameData.WIN_SCORE;
            //}

            float dist = maxY - minY;
            if (dist * currentZoom / GameData.SCREEN_SPACE > GraphicsDevice.Viewport.Height)
                ConvertUnits.SetDisplayUnitToSimUnitRatio(GraphicsDevice.Viewport.Height * GameData.SCREEN_SPACE / dist);
            else
                ConvertUnits.SetDisplayUnitToSimUnitRatio(currentZoom);
        }

        private void MakeLevel()
        {
            int width = randLevel.Next(GameData.MIN_LEVEL_WIDTH, GameData.MAX_LEVEL_WIDTH);
            int numFloors = randLevel.Next(GameData.MIN_NUM_FLOORS, GameData.MAX_NUM_FLOORS);

            int minStep = randLevel.Next(GameData.MIN_LEVEL_STEP, GameData.MAX_LEVEL_STEP);
            int maxStep = randLevel.Next(GameData.MIN_LEVEL_STEP, GameData.MAX_LEVEL_STEP);
            if (maxStep < minStep)
            {
                int temp = maxStep;
                maxStep = minStep;
                minStep = temp;
            }

            float step = randLevel.Next(minStep, maxStep);
            float y = -step;
            for (int i = 0; i < numFloors; i++) // do this for each layer
            {
                float dist = 0;
                while (dist < width)    // make the floors
                {
                    float floorSize = (float)randLevel.NextDouble() * GameData.MAX_FLOOR_DIST + GameData.MIN_FLOOR_DIST;
                    if (floorSize > width - dist)   // floor exceeds limit of level
                    {
                        floorSize = width - dist;
                        if (floorSize < GameData.MIN_FLOOR_WIDTH)   // end of floors for the layer
                            break;
                    }
                    floors.Add(new Floor(whiteRect, new Vector2(levelEnd + dist + floorSize / 2, y), floorSize));   // make the floor
                    float holeSize = (float)randLevel.NextDouble() * GameData.MAX_FLOOR_HOLE + GameData.MIN_FLOOR_HOLE;     // add a hole
                    dist += floorSize + holeSize;

                    // add stairs
                    if (dist < width && randLevel.NextDouble() < GameData.STAIR_CHANCE && holeSize > GameData.MIN_STAIR_DIST)   // add a stair onto the hole
                    {
                        Floor stair = new Floor(whiteRect, new Vector2(levelEnd + dist - GameData.STAIR_WIDTH, y + step), new Vector2(levelEnd + dist, y));

                        // check if there is something on the floor below the stair can start at
                        int numCollisions = 0;
                        foreach (Floor floor in floors)
                        {
                            if (stair.Intersects(floor) != Vector2.Zero)
                            {
                                if (++numCollisions > 0)
                                    break;
                            }
                        }

                        // make the stair
                        if (numCollisions > 0)
                        {
                            stair.Color = Color.LightGoldenrodYellow;
                            floors.Add(stair);
                        }
                    }
                }

                //// add walls onto the layer
                //dist = randLevel.Next(GameData.MIN_WALL_DIST, GameData.MAX_WALL_DIST);  // reset dist
                //while (dist < width)
                //{
                //    Wall wall = new Wall(whiteRect, new Vector2(levelEnd + dist, y + step / 2), step - Floor.FLOOR_HEIGHT, GameData.WALL_HEALTH);

                //    // check if the wall does not intersect with a staircase and has a floor on the top and bottom
                //    bool validStair = true;
                //    int numCollisions = 0;
                //    foreach (Floor floor in floors)
                //    {
                //        if (wall.Intersects(floor) != Vector2.Zero)
                //        {
                //            if (floor.Health > 0)
                //            {
                //                wall = null;
                //                validStair = false;
                //                break;
                //            }
                //            else if (++numCollisions > 1 && !validStair)
                //                break;
                //        }
                //    }

                //    if (validStair && numCollisions > 1)    // make the wall
                //       walls.Add(wall);
                //    dist += randLevel.Next(GameData.MIN_WALL_DIST, GameData.MAX_WALL_DIST);
                //}

                // add obstacles to the layer
                dist = randLevel.Next(GameData.MIN_OBSTACLE_DIST, GameData.MAX_OBSTACLE_DIST);  // reset dist
                while (dist < width)
                {
                    Obstacle obstacle = new Obstacle(whiteRect, new Vector2(levelEnd + dist, y + step - Obstacle.OBSTACLE_HEIGHT / 2));

                    // make sure the obstacle does not intersect with a stair or is on a hole
                    bool validStair = true;
                    int numCollisions = 0;
                    foreach (Floor floor in floors)
                    {
                        if (obstacle.Intersects(floor) != Vector2.Zero)
                        {
                            if (floor.Rotation != 0)
                            {
                                obstacle = null;
                                validStair = false;
                                break;
                            }
                            else if (++numCollisions > 0 && !validStair)
                                break;
                        }
                    }

                    if (validStair && numCollisions > 0)
                        obstacles.Add(obstacle);
                    dist += randLevel.Next(GameData.MIN_OBSTACLE_DIST, GameData.MAX_OBSTACLE_DIST);
                }

                step = randLevel.Next(minStep, maxStep);
                y -= step;  // go to the next layer, which has a random height
            }
            levelEnd += width + randLevel.Next(GameData.LEVEL_DIST_MIN, GameData.LEVEL_DIST_MAX);

            // remove previous levels
            if (floors.Count > GameData.MAX_FLOORS)
                floors.RemoveRange(0, floors.Count - GameData.MAX_FLOORS);
            if (obstacles.Count > GameData.MAX_OBSTACLES)
                obstacles.RemoveRange(0, obstacles.Count - GameData.MAX_OBSTACLES);
        }

        private void DrawGame(double deltaTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Find average position across all players
            Vector2 averagePos = Vector2.Zero;
            foreach (Player player in players)
                averagePos += player.Position;
            averagePos /= players.Count;
            //Vector2 averagePos = ConvertUnits.ToDisplayUnits(averagePos);

            float zoom = ConvertUnits.ToDisplayUnits(1);
            ConvertUnits.SetDisplayUnitToSimUnitRatio(GameData.SHADOW_SCALE);

#if LIGHTING
            // Calculate shadows for lightArea
            // TODO actual lights instead of on player
            // TODO optimize lights (use geometric lighting)
            lightArea.LightPosition = ConvertUnits.ToDisplayUnits(players[0].Position);
            lightArea.BeginDrawingShadowCasters();
            DrawCasters(lightArea, ConvertUnits.ToDisplayUnits(averagePos));
            lightArea.EndDrawingShadowCasters();
            shadowmapResolver.ResolveShadows(lightArea.RenderTarget, lightArea.RenderTarget, lightArea.LightPosition);

            // Combine shadows
            GraphicsDevice.SetRenderTarget(screenShadows);
            GraphicsDevice.Clear(GameData.DARK_COLOR); // masking color for things that aren't under light
            spriteBatch.Begin(blendState: BlendState.Additive);
            //screenOffset + screenCenter - averagePos
            float scale = zoom / GameData.SHADOW_SCALE;
            spriteBatch.Draw(lightArea.RenderTarget, screenOffset + screenCenter - ConvertUnits.ToDisplayUnits(averagePos) * scale + lightArea.LightPosition * scale
                - lightArea.LightAreaSize * 0.5f * scale, null, GameData.LIGHT_COLOR, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
#endif

            // Draw background
            DrawBackground(ConvertUnits.ToDisplayUnits(averagePos));
            ConvertUnits.SetDisplayUnitToSimUnitRatio(zoom);

#if LIGHTING
            // Draw shadows to screen
            BlendState blendState = new BlendState();
            blendState.ColorSourceBlend = Blend.DestinationColor;
            blendState.ColorDestinationBlend = Blend.SourceColor;
            spriteBatch.Begin(SpriteSortMode.Immediate, blendState);
            spriteBatch.Draw(screenShadows, Vector2.Zero, Color.White);
            spriteBatch.End();
#endif

            DrawScene(deltaTime, ConvertUnits.ToDisplayUnits(averagePos));

#if (DEBUG && LIGHTING)
            int width = GraphicsDevice.Viewport.Width;
            int height = GraphicsDevice.Viewport.Height;
            spriteBatch.Begin(SpriteSortMode.Immediate);
            spriteBatch.Draw(lightArea.RenderTarget, new Rectangle((int)(width * 0.9), (int)(height * 0.9), (int)(width * 0.1), (int)(height * 0.1)), Color.White);
            spriteBatch.Draw(screenShadows, new Rectangle((int)(width * 0.9), (int)(height * 0.8), (int)(width * 0.1), (int)(height * 0.1)), Color.White);
            spriteBatch.End();
#endif
        }

        private void DrawCasters(LightArea lightArea, Vector2 averagePos)
        {
            //Vector2 screenPos = screenOffset + screenCenter - ConvertUnits.ToDisplayUnits(averagePos);

            Matrix view = Matrix.CreateTranslation(new Vector3((lightArea.LightAreaSize * 0.5f - lightArea.LightPosition), 0f));

            // Draw all objects
            spriteBatch.Begin(transformMatrix: view);
            foreach (Floor floor in floors)
                floor.Draw(spriteBatch, lightArea);
            foreach (Obstacle obstacle in obstacles)
                obstacle.Draw(spriteBatch, lightArea);
            foreach (Drop drop in drops)
                drop.Draw(spriteBatch, lightArea);

            //if (editingFloor)
            //{
            //    Vector2 dist = endDraw - startDraw;
            //    float rotation = (float)Math.Atan2(dist.Y, dist.X);
            //    Vector2 scale = new Vector2(dist.Length(), Floor.FLOOR_HEIGHT);
            //    Vector2 origin = new Vector2(0.5f, 0.5f);
            //    DrawRect(startDraw + dist / 2, Color.Black, rotation, origin, scale);
            //}

            // Draw all particles and dead wall
            foreach (Particle part in particles)
                part.Draw(spriteBatch, lightArea);
            spriteBatch.End();
        }

        private void DrawScene(double deltaTime, Vector2 averagePos)
        {
            Matrix view = Matrix.CreateTranslation(new Vector3(screenOffset + screenCenter - averagePos, 0f));

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
                floor.Draw(spriteBatch);
            foreach (Obstacle obstacle in obstacles)
                obstacle.Draw(spriteBatch);
            foreach (Drop drop in drops)
                drop.Draw(spriteBatch);
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
            spriteBatch.End();


            // Draw all HUD elements
            spriteBatch.Begin();

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

            // Display high score
            string high = "High Score: " + highScore.ToString("n1");
            leftX = GraphicsDevice.Viewport.Width / 2f - fontSmall.MeasureString(high).X / 2f;
            spriteBatch.DrawString(fontSmall, high, new Vector2(leftX, 40f), Color.Yellow);

            // Display version number
            Vector2 pos = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height) - fontSmall.MeasureString(GameData.Version);
            spriteBatch.DrawString(fontSmall, GameData.Version, pos, Color.LightSalmon);

            spriteBatch.End();
        }

        private void DrawBackground(Vector2 averagePos)
        {
            int width = GraphicsDevice.Viewport.Width;
            int height = GraphicsDevice.Viewport.Height;

            GraphicsDevice.Clear(Color.MidnightBlue);
            spriteBatch.Begin(samplerState: SamplerState.LinearWrap);
            foreach (var layer in background)
            {
                Texture2D tex = layer.Item1;
                float speed = layer.Item2;
                float center = layer.Item3;
                float size = layer.Item4;
                spriteBatch.Draw(tex, new Vector2(0, height * center),
                    new Rectangle((int)(averagePos.X * speed), 0, (int)(width / size), tex.Height),
                    Color.White, 0f, new Vector2(0, tex.Height / 2), size, SpriteEffects.None, 0f);
            }
            //spriteBatch.Draw(background, new Vector2(0, height * GameData.BACK3_CENTER),
            //   new Rectangle((int)(averagePos.X * GameData.BACK3_MOVE), 0, (int)(width / GameData.BACK3_SIZE), background.Height),
            //   GameData.BACK3_COLOR, 0f, new Vector2(0, background.Height / 2), GameData.BACK3_SIZE, SpriteEffects.None, 0f);
            //spriteBatch.Draw(background, new Vector2(0, height * GameData.BACK2_CENTER),
            //   new Rectangle((int)(averagePos.X * GameData.BACK2_MOVE), 0, (int)(width / GameData.BACK2_SIZE), background.Height),
            //   GameData.BACK2_COLOR, 0f, new Vector2(0, background.Height / 2), GameData.BACK2_SIZE, SpriteEffects.None, 0f);
            //spriteBatch.Draw(background, new Vector2(0, height * GameData.BACK1_CENTER),
            //    new Rectangle((int)(averagePos.X * GameData.BACK1_MOVE), 0, (int)(width / GameData.BACK1_SIZE), background.Height),
            //    GameData.BACK1_COLOR, 0f, new Vector2(0, background.Height / 2), GameData.BACK1_SIZE, SpriteEffects.None, 0f);
            spriteBatch.End();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;

            switch (state) {
                case State.Running:
                    DrawGame(deltaTime);
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
                }
            }
        }
    }
}