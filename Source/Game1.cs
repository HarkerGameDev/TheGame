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

        public Random rand;
        //Random randLevel;
        World world;

        Rectangle cameraBounds; // limit of where the player can be on screen
        Vector2 screenCenter; // where the player is on the screen
        Vector2 screenOffset; // offset from mouse panning
        float currentZoom = GameData.PIXEL_METER;

        bool editLevel = false;
        public Platform currentPlatform;
        bool editingPlatform;
        Vector2 startDraw;
        Vector2 endDraw;

        State state = State.MainMenu;
        float totalTime = 0;
        float highScore = 0;

        public List<Player> players;
        public List<Platform> platforms;
        public List<Particle> particles;
        public List<Obstacle> obstacles;
        public List<Drop> drops;

        List<Button> pauseMenu;
        List<Button> optionsMenu;
        List<Button> controlsMenu;

        MainMenu mainMenu;

        int nativeScreenWidth;
        int nativeScreenHeight;

        List<float> times;
        List<GameData.ControlKey> keys;
        //int randSeed, randLevelSeed;
        bool prevJumpHeld = false;
        bool prevLeft = false;
        bool prevRight = false;
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

#if DEBUG
            IsFixedTimeStep = false;
#else
            IsFixedTimeStep = true;
#endif
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
            //randSeed = DateTime.Now.Millisecond;
            //randLevelSeed = GameData.GetSeed;

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
                                                       new GameData.GamePadControls(this, PlayerIndex.One, Buttons.X, Buttons.B, Buttons.Y, Buttons.LeftThumbstickLeft, Buttons.LeftThumbstickRight, Buttons.A, Buttons.RightTrigger)
                                                  };
            }

            rand = new Random();
            //rand = new Random(randSeed);
            //randLevel = new Random(randLevelSeed);

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
                //randSeed = file.ReadInt32();
                //randLevelSeed = file.ReadInt32();
                while (file.BaseStream.Position != file.BaseStream.Length)
                {
                    times.Add(file.ReadSingle());
                    keys.Add((GameData.ControlKey)file.ReadByte());
                    Console.WriteLine("Loading: " + times[times.Count - 1]);
                }
                Console.WriteLine("Loaded replay" + replay + ".rep");
                //Console.WriteLine("Loading\trandSeed: " + randSeed + "\trandLevelSeed: " + randLevelSeed);

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
                    container.CreateDirectory(directory);

                    string filename = directory + @"\replay";
                    filename += container.GetFileNames(filename + "*").Length + ".rep";

                    BinaryWriter file = new BinaryWriter(container.OpenFile(filename, FileMode.Create));
                    //file.Write(randSeed);
                    //file.Write(randLevelSeed);
                    for (int i = 0; i < times.Count; i++)
                    {
                        file.Write(times[i]);
                        file.Write((byte)keys[i]);
                    }

                    file.Close();
                    container.Dispose();
                }
            }

            //randSeed = rand.Next();
            //randLevelSeed = rand.Next();
            if (simulating)
                LoadReplay(++currentReplay);
            //Console.WriteLine("Using:\trandSeed: " + randSeed + "\trandLevelSeed: " + randLevelSeed);
            //rand = new Random(randSeed);
            //randLevel = new Random(randLevelSeed);
            foreach (Player player in players)
            {
                player.MoveToPosition(GameData.PLAYER_START);
                player.ResetValues();
            }
            platforms.Clear();
            particles.Clear();
            obstacles.Clear();
            drops.Clear();
            //levelEnd = 0;
            totalTime = 0;

            if (!simulating)
            {
                times.Clear();
                keys.Clear();
            }
            else
            {
                simIndex = 0;
                playerControls = new GameData.Controls[] { new GameData.SimulatedControls(this), new GameData.SimulatedControls(this), new GameData.SimulatedControls(this) };
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
            LoadBackground(2);
            fontSmall = Content.Load<SpriteFont>("Fonts/Score");
            fontBig = Content.Load<SpriteFont>("Fonts/ScoreBig");

            // Create objects
            players = new List<Player>();
            for (int i = 0; i < GameData.numPlayers; i++)
            {
                //int character = rand.Next(Character.playerCharacters.Length);
#if DEBUG
                int character = 1;
#else
                int character = i;
#endif
                players.Add(new Player(Content.Load<Texture2D>("Art/GreenDude"), GameData.PLAYER_START, Character.playerCharacters[character]));
            }
            platforms = new List<Platform>();
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
            LoadLevel(0);

            //// Load the song
            //Song song = Content.Load<Song>("Music/" + GameData.SONG);
            //MediaPlayer.IsRepeating = true;
            //MediaPlayer.Volume = GameData.VOLUME;
            //MediaPlayer.Play(song);
        }

        private void LoadLevel(int loadLevel)
        {
            platforms.Clear();
            obstacles.Clear();
            drops.Clear();
            using (BinaryReader file = new BinaryReader(File.Open("Levels/level" + loadLevel, FileMode.Open, FileAccess.Read)))
            {
                GameData.PLAYER_START = new Vector2(file.ReadSingle(), file.ReadSingle());
                while (file.BaseStream.Position != file.BaseStream.Length)
                {
                    Vector2 position = new Vector2(file.ReadSingle(), file.ReadSingle());
                    Vector2 size = new Vector2(file.ReadSingle(), file.ReadSingle());
                    platforms.Add(new Platform(whiteRect, position, size));
                }
            }
        }

        private void SaveLevel(int saveLevel)
        {
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

                string directory = "Levels";
                container.CreateDirectory(directory);

                string filename = directory + @"\level" + saveLevel;
                //filename += container.GetFileNames(filename + "*").Length + ".rep";

                BinaryWriter file = new BinaryWriter(container.OpenFile(filename, FileMode.Create));
                file.Write(GameData.PLAYER_START.X);
                file.Write(GameData.PLAYER_START.Y);
                foreach (Platform plat in platforms)
                {
                    file.Write(plat.Position.X);
                    file.Write(plat.Position.Y);
                    file.Write(plat.Size.X);
                    file.Write(plat.Size.Y);
                }

                file.Close();
                container.Dispose();
            }
        }

        /// <summary>
        /// Loads a level and stores it into background
        /// </summary>
        /// <param name="loadLevel"></param>
        private void LoadBackground(int loadLevel)
        {
            float[][] level = GameData.WORLD_LAYERS[loadLevel];
            background = new Tuple<Texture2D, float, float, float>[level.Length];
            for (int i = 0; i < level.Length; i++)
            {
                float[] layer = level[i];
                background[i] = new Tuple<Texture2D, float, float, float>(
                    Content.Load<Texture2D>(string.Format("Backgrounds/Background{0}/layer{1}", loadLevel, i)),
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
                            currentPlatform = null;
                            currentZoom = GameData.PIXEL_METER;
                        }
                        else
                            currentZoom = GameData.PIXEL_METER_EDIT;
                        ConvertUnits.SetDisplayUnitToSimUnitRatio(currentZoom);
                    }

                    if (ToggleKey(Keys.I))
                        LoadBackground(0);
                    else if (ToggleKey(Keys.O))
                        LoadBackground(1);
                    else if (ToggleKey(Keys.P))
                        LoadBackground(2);

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
                                        control.Jump = true;
                                        break;
                                    case GameData.ControlKey.JumpHeld:
                                        control.JumpHeld = !control.JumpHeld;
                                        break;
                                    case GameData.ControlKey.Action:
                                        control.Action = true;
                                        break;
                                }
                                simIndex++;
                            }
                        }

                        if (currentPlatform == null)
                            HandleInput(deltaTime);

                        if (simulating)
                        {
                            GameData.SimulatedControls control = (GameData.SimulatedControls)playerControls[0];
                            control.Special1 = false;
                            control.Special2 = false;
                            control.Special3 = false;
                            control.Action = false;
                            control.Jump = false;
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
                HandlePlayerInput(player, i, deltaTime);
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
                //if (controls.Jump)
                //{
                //    times.Add(totalTime);
                //    keys.Add(GameData.ControlKey.Jump);
                //}
                if (controls.JumpHeld != prevJumpHeld)
                {
                    times.Add(totalTime);
                    prevJumpHeld = !prevJumpHeld;
                    keys.Add(GameData.ControlKey.JumpHeld);
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
            }

            if (player.CurrentState != Player.State.Stunned)
            {
                if (controls.JumpHeld)
                {
                    if (!player.PrevJump)
                    {
                        if (player.WallJump != Player.Jump.None)    // wall jump
                        {
                            player.Velocity.Y = -GameData.WALL_JUMP_Y;
                            if (player.WallJump == Player.Jump.Left)
                                player.Velocity.X = -GameData.WALL_JUMP_X;
                            else
                                player.Velocity.X = GameData.WALL_JUMP_X;
                            player.WallJump = Player.Jump.None;
                        }
                        else if (player.CanJump)    // normal jump
                        {
                            player.Velocity.Y = -GameData.JUMP_SPEED;
                            player.TargetVelocity = player.TargetVelocity * GameData.JUMP_SLOW;
                            player.CurrentState = Player.State.Jumping;
                            player.JumpTime = GameData.JUMP_TIME;
                        }
                        else if (player.AbilityOneTime < 0)        // jump abilities
                        {
                            switch (player.CurrentCharacter.Ability1)
                            {
                                case Character.AbilityOne.Platform:
                                    Platform plat = new Platform(whiteRect, player.Position + new Vector2(0, GameData.PLATFORM_DIST),
                                        new Vector2(GameData.PLATFORM_WIDTH, GameData.PLATFORM_HEIGHT));
                                    platforms.Add(plat);
                                    player.AbilityOneTime = GameData.PLATFORM_COOLDOWN;
                                    player.PlatformTime = GameData.PLATFORM_LIFE;
                                    player.SpawnedPlatform = plat;
                                    break;
                                case Character.AbilityOne.Grapple:
                                    // TODO do a grapple animation
                                    //player.GrappleTarget = player.Position + new Vector2(10f, -10f);
                                    player.GrappleTarget = Raycast(player.Position, new Vector2(player.Flip == SpriteEffects.None ? 1f : -1f, GameData.GRAPPLE_ANGLE));
                                    player.TargetRadius = Vector2.Distance(player.Position, player.GrappleTarget);
                                    player.GrappleRight = player.Flip == SpriteEffects.None;
                                    break;
                                case Character.AbilityOne.Blink:
                                    player.AbilityOneTime = GameData.BLINK_COOLDOWN;
                                    player.Blink = true;    // this is used in World later
                                    break;
                            }
                        }
                    }
                    else if (player.JumpTime > 0)   // hold jump in air
                    {
                        player.Velocity.Y -= GameData.JUMP_ACCEL * deltaTime;
                    }
                }
                else        // not jumping
                {
                    player.JumpTime = 0;
                    player.GrappleTarget = Vector2.Zero;
                }

                if (controls.Right)     // move
                    player.TargetVelocity = GameData.RUN_VELOCITY;
                else if (controls.Left)
                    player.TargetVelocity = -GameData.RUN_VELOCITY;
                else
                    player.TargetVelocity = 0;
                //else
                //{
                //    if (player.CurrentState == Player.State.Jumping && controls.Jump && player.JumpTime > GameData.JETPACK_CUTOFF)      // jetpack
                //    {
                //        player.Velocity.Y -= GameData.JETPACK_ACCEL * deltaTime;
                //    }
                //}

                // activate (or toggle) special abilities
                //if (controls.Special1)
                //    player.Ability1 = !player.Ability1;
                //if (controls.Special2)
                //    player.Ability2 = !player.Ability2;
                //if (controls.Special3)
                //    player.Ability3 = !player.Ability3;
            }
            player.PrevJump = controls.JumpHeld;
        }

        /// <summary>
        /// Casts a ray from start in the given direction, returning found point or Vector2.Zero
        /// </summary>
        /// <param name="start"></param>
        /// <param name="dir"></param>
        /// <returns>The nearest point of collision, or Vector2.Zero if nothing</returns>
        private Vector2 Raycast(Vector2 start, Vector2 dir)
        {
            float minT = float.MaxValue;
            foreach (Platform plat in platforms)
            {
                float t = plat.Raycast(start, dir);
                if (t < minT)
                    minT = t;
            }
            if (minT < GameData.MAX_GRAPPLE)
                return start + dir * minT;
            else
                return Vector2.Zero;
        }

        private void wobbleScreen(float amplifier)
        {
            screenCenter.X += (float)((rand.NextDouble() - 0.5) * amplifier * 2);
            screenCenter.Y += (float)((rand.NextDouble() - 0.5) * amplifier * 2);
        }

        /// <summary>
        /// Handles all keypresses for editing a level. These are the controls:
        /// - Press 'E' to enter edit level mode.
        /// - Shift-drag to select a platform (move your mouse slowly)
        /// - Backspace to delete selected platform
        /// - Alt-drag to modify currently selected platform
        /// - Tap left, right, up, or down to move selected platform
        /// - Ctrl-drag to create a new platform
        /// - Ctrl-S to save your current level as a new level in the Levels folder
        /// - Ctrl-O to open the last level -- NOTE, this will override ANY changes you made, so be careful
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
                if (editingPlatform)                                   // Draw the platform
                {
                    endDraw = mouseSimPosRound;
                }
                else
                {
                    if (keyboard.IsKeyDown(Keys.LeftControl))       // Start drawing a platform
                    {
                        editingPlatform = true;
                        startDraw = mouseSimPosRound;
                        endDraw = mouseSimPosRound;
                    }
                    else
                    {
                        if (keyboard.IsKeyDown(Keys.LeftShift))     // Select a platform
                        {
                            Body body = world.TestPoint(mouseSimPos);
                            if (body != null && body is Platform)
                                currentPlatform = (Platform)body;
                        }
                        else if (keyboard.IsKeyDown(Keys.LeftAlt) && currentPlatform != null)
                        {                                           // Resize selected platform
                            // TODO make resizing work
                            float rotation = currentPlatform.Rotation;
                            Vector2 center = currentPlatform.Position;
                            float width = currentPlatform.Size.X;
                            Vector2 offset = new Vector2(width * (float)Math.Cos(rotation), width * (float)Math.Sin(rotation)) / 2;
                            if (offset.X < 0) offset *= -1;
                            if (mouseSimPosRound.X > center.X)
                                startDraw = center - offset;
                            else
                                startDraw = center + offset;
                            endDraw = mouseSimPosRound;
                            platforms.Remove(currentPlatform);
                            currentPlatform = null;
                            editingPlatform = true;
                        }
                        else                                        // Move camera
                        {
                            screenOffset += mouse.Position.ToVector2() - prevMouseState.Position.ToVector2();
                        }
                    }
                }
            }
            else if (editingPlatform)                                  // Make the platform
            {
                if (startDraw != endDraw)
                {
                    Vector2 size = endDraw - startDraw;
                    currentPlatform = new Platform(whiteRect, startDraw + size / 2f, size);
                    platforms.Add(currentPlatform);
                }
                editingPlatform = false;
            }
            else if (currentPlatform != null)
            {                                                       // Delete selected platform
                if (keyboard.IsKeyDown(Keys.Back))
                {
                    platforms.Remove(currentPlatform);
                    currentPlatform = null;
                }
                else if (keyboard.IsKeyDown(Keys.Up))           // Move platform
                    currentPlatform.MovePosition(-Vector2.UnitY);
                else if (keyboard.IsKeyDown(Keys.Left))
                    currentPlatform.MovePosition(-Vector2.UnitX);
                else if (keyboard.IsKeyDown(Keys.Right))
                    currentPlatform.MovePosition(Vector2.UnitX);
                else if (keyboard.IsKeyDown(Keys.Down))
                    currentPlatform.MovePosition(Vector2.UnitY);
                else if (keyboard.IsKeyDown(Keys.Enter))        // Deselect platform
                    currentPlatform = null;
                else if (ToggleKey(Keys.F))
                {
                    currentPlatform.Color = currentPlatform.Color == Color.Crimson ? Color.LightGoldenrodYellow : Color.Crimson;
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
                if (ToggleKey(Keys.S))
                {
                    SaveLevel(0);
                }
                else if (ToggleKey(Keys.O))
                {
                    LoadLevel(0);
                }
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
                //if (player.Position.X > levelEnd - GameData.LOAD_NEW)
                //    MakeLevel();

                if (max == null || player.Position.X > max.Position.X)
                    max = player;

                if (player.Position.Y < minY)
                    minY = player.Position.Y;
                else if (player.Position.Y > maxY)
                    maxY = player.Position.Y;
            }

            //foreach (Player player in players)
            //{
            //    if (player.TimeSinceDeath > 0)
            //    {
            //        bool allDead = max == null;

            //        float val = (float)(player.TimeSinceDeath / GameData.DEAD_TIME);
            //        float targetX = allDead ? averageX : max.Position.X;
            //        float newX = MathHelper.Lerp(targetX, player.Position.X, val);
            //        float newY = MathHelper.Lerp(player.SpawnY, player.Position.Y, val);

            //        player.MoveToPosition(new Vector2(newX, newY));
            //    }
            //    else if (player.Position.X < averageX - ConvertUnits.ToSimUnits(GameData.DEAD_DIST))
            //    {
            //        player.Kill(rand);
            //        if (max != null)
            //            max.Score++;
            //    }
            //}

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

            // Draw background
            DrawBackground(ConvertUnits.ToDisplayUnits(averagePos));
            ConvertUnits.SetDisplayUnitToSimUnitRatio(zoom);

            DrawScene(deltaTime, ConvertUnits.ToDisplayUnits(averagePos));
        }

        //private void DrawCasters(LightArea lightArea, Vector2 averagePos)
        //{
        //    //Vector2 screenPos = screenOffset + screenCenter - ConvertUnits.ToDisplayUnits(averagePos);

        //    Matrix view = Matrix.CreateTranslation(new Vector3((lightArea.LightAreaSize * 0.5f - lightArea.LightPosition), 0f));

        //    // Draw all objects
        //    spriteBatch.Begin(transformMatrix: view);
        //    foreach (Platform platform in platforms)
        //        platform.Draw(spriteBatch, lightArea);
        //    foreach (Obstacle obstacle in obstacles)
        //        obstacle.Draw(spriteBatch, lightArea);
        //    foreach (Drop drop in drops)
        //        drop.Draw(spriteBatch, lightArea);

        //    //if (editingFloor)
        //    //{
        //    //    Vector2 dist = endDraw - startDraw;
        //    //    float rotation = (float)Math.Atan2(dist.Y, dist.X);
        //    //    Vector2 scale = new Vector2(dist.Length(), Floor.FLOOR_HEIGHT);
        //    //    Vector2 origin = new Vector2(0.5f, 0.5f);
        //    //    DrawRect(startDraw + dist / 2, Color.Black, rotation, origin, scale);
        //    //}

        //    // Draw all particles and dead wall
        //    foreach (Particle part in particles)
        //        part.Draw(spriteBatch, lightArea);
        //    spriteBatch.End();
        //}

        private void DrawScene(double deltaTime, Vector2 averagePos)
        {
            Matrix view = Matrix.CreateTranslation(new Vector3(screenOffset + screenCenter - averagePos, 0f));

            // Draw players
            spriteBatch.Begin(transformMatrix: view);
            foreach (Player player in players)
            {
                player.Sprite.Update(deltaTime);
                player.Draw(spriteBatch);
                foreach (Projectile proj in player.Projectiles)
                    proj.Draw(spriteBatch);
            }
            spriteBatch.End();

            // Draw all objects
            spriteBatch.Begin(transformMatrix: view);
            spriteBatch.Draw(whiteRect, new Rectangle(-(int)view.Translation.X, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.LightGray);
            foreach (Platform platform in platforms)
                platform.Draw(spriteBatch);
            foreach (Obstacle obstacle in obstacles)
                obstacle.Draw(spriteBatch);
            foreach (Drop drop in drops)
                drop.Draw(spriteBatch);
            if (currentPlatform != null)
                DrawRect(currentPlatform.Position, Color.Green, currentPlatform.Rotation, currentPlatform.Origin, currentPlatform.Size);
            if (editingPlatform)
            {
                Vector2 dist = endDraw - startDraw;
                Vector2 origin = new Vector2(0.5f, 0.5f);
                DrawRect(startDraw + dist / 2, Color.Azure, 0, origin, dist);
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
            //System.Text.StringBuilder text = new System.Text.StringBuilder();
            //text.AppendLine("Scores");
            //for (int i = 0; i < players.Count; i++)
            //{
            //    text.AppendLine(string.Format("Player {0}: {1}", i + 1, players[i].Score));
            //}
            //spriteBatch.DrawString(fontSmall, text, new Vector2(10, 10), Color.Green);

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