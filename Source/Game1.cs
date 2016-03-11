using System;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;

using Source.Collisions;
using Source.Graphics;
using Source.Properties;

using GameUILibrary;
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
        int editScroll;

        GameData.Controls[] playerControls;

        public static Texture2D whiteRect;
        Tuple<Texture2D, float, float, float>[] prevBackground, background;
        float fadeTime;
        public SpriteFont fontSmall, fontBig;
        //List<Song> songs;

        RenderTarget2D[] playerScreens;

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
        bool lockedX = false;
        bool lockedY = false;

        State state = State.MainMenu;
        float totalTime = 0;
        float highScore = 0;

        public List<Player> players;
        public List<Platform> platforms;
        public List<Particle> particles;
        public List<Obstacle> obstacles;
        public List<Drop> drops;

        //List<Button> pauseMenu;
        //List<Button> optionsMenu;
        //List<Button> controlsMenu;

        BasicUIViewModel viewModel;
        EmptyKeys.UserInterface.Controls.UIRoot menu;

        int nativeScreenWidth;
        int nativeScreenHeight;

        List<float> simTimes;
        List<GameData.ControlKey> keys;
        //int randSeed, randLevelSeed;
        bool prevDown = false;
        bool prevJumpHeld = false;
        bool prevLeft = false;
        bool prevRight = false;
        bool simulating = false;
        int simIndex = 0;
        int currentReplay = 0;

        float InvertScreen = -1;
        List<float> times;

        public enum State
        {
            Running, Paused, MainMenu, Options, Controls
        }

        enum Menu
        {
            Main, Options, Controls, Pause
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
            if (Settings.Default.Fullscreen)
            {
                nativeScreenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                nativeScreenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }
            else
            {
                nativeScreenWidth = Settings.Default.WindowWidth;
                nativeScreenHeight = Settings.Default.WindowHeight;
            }
            graphics.PreferredBackBufferWidth = nativeScreenWidth;
            graphics.PreferredBackBufferHeight = nativeScreenHeight;

            graphics.PreferMultiSampling = true;
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 16;

            IsFixedTimeStep = true;
            graphics.SynchronizeWithVerticalRetrace = Settings.Default.VSync;
            graphics.IsFullScreen = Settings.Default.Fullscreen;
#if WINDOWS
            Window.IsBorderless = Settings.Default.Borderless;
#endif
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
            ConvertUnits.SetMouseScale(graphics.IsFullScreen, (float)Window.ClientBounds.Width / nativeScreenWidth);
            ConvertUnits.SetResolutionScale((float)nativeScreenWidth / GameData.VIEW_WIDTH);

            // Set seed for a scheduled random level (minutes since Jan 1, 2015)
            //randSeed = DateTime.Now.Millisecond;
            //randLevelSeed = GameData.GetSeed;

            // Set variables
            simTimes = new List<float>();
            keys = new List<GameData.ControlKey>();
            times = new List<float>();

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
                                                       new GameData.KeyboardControls(this, Keys.OemComma, Keys.OemPeriod, Keys.OemQuestion, Keys.Left, Keys.Right, Keys.Up, Keys.Down),
                                                       new GameData.KeyboardControls(this, Keys.LeftControl, Keys.LeftShift, Keys.Z, Keys.A, Keys.D, Keys.W, Keys.S),
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
                    simTimes.Add(file.ReadSingle());
                    keys.Add((GameData.ControlKey)file.ReadByte());
                    Console.WriteLine("Loading: " + simTimes[simTimes.Count - 1]);
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
                    for (int i = 0; i < simTimes.Count; i++)
                    {
                        file.Write(simTimes[i]);
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
            //levelEnd = 0;
            totalTime = 0;

            //LoadLevel(GameData.LEVEL_FILE);

            if (!simulating)
            {
                simTimes.Clear();
                keys.Clear();
            }
            else
            {
                simIndex = 0;
                playerControls = new GameData.Controls[] { new GameData.SimulatedControls(this), new GameData.SimulatedControls(this), new GameData.SimulatedControls(this) };
                //simulating = false;
            }
        }

        private void LoadUI(Menu type)
        {
            switch (type)
            {
                case Menu.Main:
                    state = State.MainMenu;
                    //MediaPlayer.Play(songs[1]);
                    menu = new MainMenu();
                    break;
                case Menu.Controls:
                    state = State.Controls;
                    menu = new ControlsMenu();
                    break;
                case Menu.Options:
                    state = State.Options;
                    menu = new OptionsMenu();
                    break;
                case Menu.Pause:
                    state = State.Paused;
                    menu = new PauseMenu();
                    break;
            }

            menu.DataContext = viewModel;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Content.RootDirectory = "Content";
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO figure out why multiple songs do not work well with muting
            // Load the songs
            //songs = new List<Song>();
            //songs.Add(Content.Load<Song>("Music/Air Skate"));
            //songs.Add(Content.Load<Song>("Music/Main Menu"));
            //songs.Add(Content.Load<Song>("Music/Character Select"));
            Song song = Content.Load<Song>("Music/Air Skate");
            MediaPlayer.Play(song);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = Settings.Default.Volume;
            MediaPlayer.IsMuted = Settings.Default.Muted;

            // Set up user interface
            SpriteFont font = Content.Load<SpriteFont>("Fonts/Segoe_UI_15_Bold");
            FontManager.DefaultFont = Engine.Instance.Renderer.CreateFont(font);
            viewModel = new BasicUIViewModel();
            FontManager.Instance.LoadFonts(Content, "Fonts");

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < GameData.NUM_PLAYERS; i++)
            {
                builder.Append("Player ").AppendLine(i.ToString())
                    .AppendLine(playerControls[i].ToString());
            }
            viewModel.ControlsText = builder.ToString();
            Console.WriteLine(builder.ToString());
            //viewModel.ControlsText = "Hello!\nWhoo!!";

            LoadUI(Menu.Main);

            // Initialize screen render target
            playerScreens = new RenderTarget2D[GameData.NUM_PLAYERS];
            for (int i=0; i<GameData.NUM_PLAYERS; i++)
                playerScreens[i] = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

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
            for (int i = 0; i < GameData.NUM_PLAYERS; i++)
            {
                //int character = rand.Next(Character.playerCharacters.Length);
#if DEBUG
                int character = GameData.CHARACTER;
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

            // Load the level stored in LEVEL_FILE
            LoadLevel(GameData.LEVEL_FILE);
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
#if DEBUG
            //Console.WriteLine("Dir: " + Directory.GetFiles(@"..\..\..\..\Source\Levels")[0]);
            BinaryWriter file = new BinaryWriter(File.Open(@"..\..\..\..\Source\Levels\level" + saveLevel, FileMode.Open, FileAccess.Write));
#else
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
#endif
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
#if !DEBUG
                container.Dispose();
            }
#endif
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
                    if (ToggleKey(Keys.Escape))
                    {
                        LoadUI(Menu.Pause);
                        //MediaPlayer.Play(songs[2]);
                    }
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
                        {
                            currentZoom = GameData.PIXEL_METER_EDIT;
                            editScroll = Mouse.GetState().ScrollWheelValue;
                        }
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
                            while (simIndex < simTimes.Count && simTimes[simIndex] - totalTime <= 0)
                            {
                                //deltaTime += diff;
                                Console.Write("Sim: " + totalTime + "\t\tReplay: " + simTimes[simIndex] +
                                    "\t\tDelta: " + deltaTime + "\t\tDiff: " + (simTimes[simIndex] - totalTime) + "\t\t");
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
                                    case GameData.ControlKey.JumpHeld:
                                        control.JumpHeld = !control.JumpHeld;
                                        break;
                                    case GameData.ControlKey.Down:
                                        control.Down = !control.Down;
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
                        }

                        CheckPlayer();
                        InvertScreen -= deltaTime;
                        totalTime += deltaTime;
                        world.Step(deltaTime);

                        // TODO store previous locations of players
                        foreach (Player player in players)
                            player.PrevStates.Add(Tuple.Create(player.Position, player.Velocity));
                        times.Add(totalTime);
                    }
                    break;
                case State.Paused:
                    if (ToggleKey(Keys.Escape))
                    {
                        state = State.Running;
                        //MediaPlayer.Play(songs[0]);
                    }
                    UpdateMenu(gameTime.ElapsedGameTime.TotalMilliseconds);
                    break;
                case State.Options:
                    if (ToggleKey(Keys.Escape))
                    {
                        LoadUI(Menu.Pause);
                        Settings.Default.Save();
                    }
                    UpdateMenu(gameTime.ElapsedGameTime.TotalMilliseconds);
                    break;
                case State.Controls:
                    if (ToggleKey(Keys.Escape))
                        LoadUI(Menu.Options);
                    UpdateMenu(gameTime.ElapsedGameTime.TotalMilliseconds);
                    break;
                case State.MainMenu:
                    if (ToggleKey(Keys.Escape))
                        Exit();
                    UpdateMenu(gameTime.ElapsedGameTime.TotalMilliseconds);
                    break;
            }

            prevKeyState = Keyboard.GetState();
            prevMouseState = Mouse.GetState();
            for (int i = 0; i < prevPadStates.Length; i++)
                prevPadStates[i] = GamePad.GetState((PlayerIndex)i, GamePadDeadZone.Circular);

            base.Update(gameTime);
        }

        private void UpdateMenu(double elapsedTime)
        {
            menu.UpdateInput(elapsedTime);
            menu.UpdateLayout(elapsedTime);
            ParseButtons();
            viewModel.ButtonResult = null;
        }

        private void ParseButtons()
        {
            switch (viewModel.ButtonResult)
            {
                case "Start":
                    state = State.Running;
                    //MediaPlayer.Play(songs[0]);
                    break;
                case "Options":
                    LoadUI(Menu.Options);
                    break;
                case "Exit":
                    Exit();
                    break;
                case "Fullscreen":
                    // TODO toggle fullscreen
                    if (!graphics.IsFullScreen)
                    {
                        graphics.PreferredBackBufferWidth = GraphicsDevice.Adapter.CurrentDisplayMode.Width;
                        graphics.PreferredBackBufferHeight = GraphicsDevice.Adapter.CurrentDisplayMode.Height;
                    }
                    else
                    {
                        graphics.PreferredBackBufferWidth = Settings.Default.WindowWidth;
                        graphics.PreferredBackBufferHeight = Settings.Default.WindowHeight;
                    }
                    graphics.IsFullScreen = !graphics.IsFullScreen;
                    graphics.ApplyChanges();

                    ConvertUnits.SetMouseScale(graphics.IsFullScreen, (float)Window.ClientBounds.Width / nativeScreenWidth);
                    //ConvertUnits.SetResolutionScale((float)nativeScreenWidth / GameData.VIEW_WIDTH);
                    //Window.BeginScreenDeviceChange(true);
                    //Console.WriteLine("Fullscreen: {0}\tEngine: {1}", graphics.IsFullScreen, Engine.Instance.Renderer.IsFullScreen);
                    Engine.Instance.Renderer.ResetNativeSize();
                    Console.WriteLine("Native: {{{0}, {1}}}", nativeScreenWidth, nativeScreenHeight);
                    Console.WriteLine("Window: " + Window.ClientBounds.Size);
                    //Console.WriteLine("Screen: {{{0}, {1}}}", GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
                    Console.WriteLine("Adapter: {{{0}, {1}}}", GraphicsDevice.Adapter.CurrentDisplayMode.Width, GraphicsDevice.Adapter.CurrentDisplayMode.Height);
                    Console.WriteLine("Viewport: " + GraphicsDevice.Viewport.Bounds.Size);
                    Console.WriteLine("Backbuffer: {{{0}, {1}}}", graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
                    //GraphicsDevice.Viewport = new Viewport(0, 0, nativeScreenWidth, nativeScreenHeight);

                    menu.Resize(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

                    Settings.Default.Fullscreen = graphics.IsFullScreen;
                    break;
                case "Controls":
                    LoadUI(Menu.Controls);
                    break;
                case "Pause":
                    LoadUI(Menu.Pause);
                    break;
                case "ExitOptions":
                    LoadUI(Menu.Pause);
                    Settings.Default.Save();
                    break;
                case "Music":
                    MediaPlayer.IsMuted = !MediaPlayer.IsMuted;
                    Settings.Default.Muted = MediaPlayer.IsMuted;
                    //MediaPlayer.Resume();
                    break;
                case "VSync":
                    graphics.SynchronizeWithVerticalRetrace = !graphics.SynchronizeWithVerticalRetrace;
                    graphics.ApplyChanges();
                    Settings.Default.VSync = graphics.SynchronizeWithVerticalRetrace;
                    break;
                case null:
                    break;
                default:
                    throw new NotImplementedException("Button command " + viewModel.ButtonResult + " is not implemented.");
            }
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
                Reset();
            else if (ToggleKey(Keys.D1))
                players[0] = new Player(Content.Load<Texture2D>("Art/GreenDude"), GameData.PLAYER_START, Character.playerCharacters[0]);
            else if (ToggleKey(Keys.D2))
                players[0] = new Player(Content.Load<Texture2D>("Art/GreenDude"), GameData.PLAYER_START, Character.playerCharacters[1]);
            else if (ToggleKey(Keys.D3))
                players[0] = new Player(Content.Load<Texture2D>("Art/GreenDude"), GameData.PLAYER_START, Character.playerCharacters[2]);
            else if (ToggleKey(Keys.D4))
                players[0] = new Player(Content.Load<Texture2D>("Art/GreenDude"), GameData.PLAYER_START, Character.playerCharacters[3]);
            else if (ToggleKey(Keys.D5))
                players[0] = new Player(Content.Load<Texture2D>("Art/GreenDude"), GameData.PLAYER_START, Character.playerCharacters[4]);
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
                    simTimes.Add(totalTime);
                    keys.Add(GameData.ControlKey.Special1);
                }
                if (controls.Special2)
                {
                    simTimes.Add(totalTime);
                    keys.Add(GameData.ControlKey.Special2);
                }
                if (controls.Special3)
                {
                    simTimes.Add(totalTime);
                    keys.Add(GameData.ControlKey.Special3);
                }
                if (controls.Down != prevDown)
                {
                    simTimes.Add(totalTime);
                    prevDown = !prevDown;
                    keys.Add(GameData.ControlKey.Down);
                }
                if (controls.JumpHeld != prevJumpHeld)
                {
                    simTimes.Add(totalTime);
                    prevJumpHeld = !prevJumpHeld;
                    keys.Add(GameData.ControlKey.JumpHeld);
                }
                if (controls.Left != prevLeft)
                {
                    simTimes.Add(totalTime);
                    prevLeft = !prevLeft;
                    keys.Add(GameData.ControlKey.Left);
                }
                if (controls.Right != prevRight)
                {
                    simTimes.Add(totalTime);
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
                        if (player.CanJump)    // normal jump
                        {
                            player.Velocity.Y = -GameData.JUMP_SPEED;
                            player.TargetVelocity = player.TargetVelocity * GameData.JUMP_SLOW;
                            player.CurrentState = Player.State.Jumping;
                            player.JumpTime = GameData.JUMP_TIME;
                        }
                        else if (player.WallJump != Player.Jump.None)    // wall jump
                        {
                            player.Velocity.Y = -GameData.WALL_JUMP_Y;
                            if (player.WallJump == Player.Jump.Left)
                                player.Velocity.X = -GameData.WALL_JUMP_X;
                            else
                                player.Velocity.X = GameData.WALL_JUMP_X;
                            player.WallJump = Player.Jump.None;
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
                                    player.GrappleTarget = Raycast(player.Position, new Vector2(player.FacingRight ? 1f : -1f, GameData.GRAPPLE_ANGLE));
                                    player.TargetRadius = Vector2.Distance(player.Position, player.GrappleTarget);
                                    player.GrappleRight = player.Flip == SpriteEffects.None;
                                    break;
                                case Character.AbilityOne.Blink:
                                    player.AbilityOneTime = GameData.BLINK_COOLDOWN;
                                    player.MoveByPosition(new Vector2(player.FacingRight ? GameData.BLINK_DIST : -GameData.BLINK_DIST, 0));
                                    // TODO maybe do some validation to make sure the person isn't 'cheating'
                                    break;
                                case Character.AbilityOne.Jetpack:
                                    player.JetpackEnabled = true;   // Jetpack is handled below
                                    break;
                                case Character.AbilityOne.Jump:
                                    if (--player.JumpsLeft > 0)
                                        player.Velocity.Y = -GameData.AIR_JUMP_SPEED;
                                    break;
                            }
                        }
                    }
                    else if (player.JumpTime > 0)   // hold jump in air
                    {
                        player.Velocity.Y -= GameData.JUMP_ACCEL * deltaTime;
                    }
                    else if (player.JetpackEnabled)
                    {
                        if (player.JetpackTime > 0)
                        {
                            float prevTime = player.JetpackTime;
                            player.JetpackTime -= deltaTime;
                            int particles = (int)(Math.Truncate(prevTime / GameData.JETPACK_PARTICLES) - Math.Truncate(player.JetpackTime / GameData.JETPACK_PARTICLES));
                            //Console.WriteLine("Prev: {0}\tCurr: {1}", prevTime / GameData.JETPACK_PARTICLES, player.JetpackTime / GameData.JETPACK_PARTICLES);
                            world.MakeParticles(new Vector2(player.Position.X, player.Position.Y + player.Size.Y / 2f), whiteRect, particles, 0, 1, Color.WhiteSmoke);
                            player.Velocity.Y -= (player.Velocity.Y > 0 ? GameData.JETPACK_ACCEL_DOWN : GameData.JETPACK_ACCEL_UP) * deltaTime;
                            //Console.WriteLine("Jetpacking: {0} at time: {1}", player.Velocity.Y, player.JetpackTime);
                        }
                    }
                }
                else        // not jumping
                {
                    player.JumpTime = 0;
                    if (player.GrappleTarget != Vector2.Zero)
                    {
                        // TODO boost more for lower speeds and less for higher speeds
                        // TODO fix when player mashes grapple to get super-speed
                        player.Velocity *= GameData.GRAPPLE_BOOST;
                        player.GrappleTarget = Vector2.Zero;
                    }
                    player.JetpackEnabled = false;
                }

                if (controls.Right)
                    player.TargetVelocity = GameData.RUN_VELOCITY;
                else if (controls.Left)
                    player.TargetVelocity = -GameData.RUN_VELOCITY;
                else
                    player.TargetVelocity = 0;

                // activate (or toggle) special abilities
                if (player.AbilityTwoTime < 0 && controls.Special1)
                {
                    switch (player.CurrentCharacter.Ability2)
                    {
                        case Character.AbilityTwo.Invert:
                            player.AbilityTwoTime = GameData.INVERT_COOLDOWN;
                            InvertScreen = GameData.INVERT_TIME;
                            break;
                        case Character.AbilityTwo.Trap:
                            player.AbilityTwoTime = GameData.TRAP_COOLDOWN;
                            drops.Add(new Drop(player, whiteRect, player.Position, 1f, Drop.Types.Trap, Color.Red));
                            break;
                        case Character.AbilityTwo.Timewarp:
                            player.AbilityTwoTime = GameData.TIMEWARP_COOLDOWN;
                            int i = times.Count - 1;
                            while (i >= 0 && times[i] + GameData.TIMEWARP_TIME >= totalTime)
                                i--;
                            if (i >= 0)
                            {
                                Console.WriteLine("Found time: {0}\tCurrent time: {1}", times[i], totalTime);
                                // TODO animate the transition for super-awesome effect
                                foreach (Player target in players)
                                {
                                    if (target != player)
                                    {
                                        Tuple<Vector2, Vector2> state = target.PrevStates[i];
                                        target.MoveToPosition(state.Item1);
                                        target.Velocity = state.Item2;
                                    }
                                }
                            }
                            break;
                        case Character.AbilityTwo.Rocket:
                            // TODO tweak rocket so it's better
                            player.AbilityTwoTime = GameData.ROCKET_COOLDOWN;
                            Vector2 vel = new Vector2(player.FacingRight ? GameData.ROCKET_X : -GameData.ROCKET_X, -GameData.ROCKET_Y);
                            vel += player.Velocity * GameData.ROCKET_SCALE;
                            player.Projectiles.Add(new Projectile(whiteRect, player.Position, Color.DarkOliveGreen, Projectile.Types.Rocket, vel));
                            break;
                        case Character.AbilityTwo.Boomerang:
                            player.AbilityTwoTime = GameData.BOOMERANG_COOLDOWN;
                            vel = new Vector2(player.FacingRight ? GameData.BOOMERANG_X : -GameData.BOOMERANG_X, -GameData.BOOMERANG_Y);
                            vel += player.Velocity * GameData.BOOMERANG_SCALE;
                            player.Projectiles.Add(new Projectile(whiteRect, player.Position, Color.YellowGreen, Projectile.Types.Boomerang, vel));
                            break;
                    }
                }
                if (player.AbilityThreeTime < 0 && controls.Special2)
                {
                    switch (player.CurrentCharacter.Ability3)
                    {
                        //case Character.AbilityThree.Swap:
                        //    player.AbilityThreeTime = GameData.SWAP_COOLDOWN;
                        //    Player first = players[rand.Next(players.Count)];
                        //    Player second = players[rand.Next(players.Count)];
                        //    Vector2 tempPos = first.Position;
                        //    Vector2 tempVel = first.Velocity;

                        //    // TODO somehow indicate to the player that they have switched
                        //    first.MoveToPosition(second.Position);
                        //    first.Velocity = second.Velocity;
                        //    second.MoveToPosition(tempPos);
                        //    second.Velocity = tempVel;
                        //    break;
                    }
                }
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
            Vector2 mouseSimPos = ConvertUnits.ToSimUnits(ConvertUnits.GetMousePos(mouse) - cameraBounds.Center.ToVector2() - screenOffset) + averagePos;
            Vector2 mouseSimPosRound = new Vector2((float)Math.Round(mouseSimPos.X), (float)Math.Round(mouseSimPos.Y));

            if (mouse.LeftButton == ButtonState.Pressed)
            {
                if (editingPlatform)                                   // Draw the platform
                {
                    if (lockedY)
                        endDraw.Y = mouseSimPosRound.Y;
                    else if (lockedX)
                        endDraw.X = mouseSimPosRound.X;
                    else
                        endDraw = mouseSimPosRound;
                }
                else
                {
                    if (keyboard.IsKeyDown(Keys.LeftControl))       // Start drawing a platform
                    {
                        editingPlatform = true;
                        lockedX = lockedY = false;
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
                            float slopeX = currentPlatform.Size.Y / currentPlatform.Size.X * (mouseSimPos.X - currentPlatform.Position.X);
                            float yMax = currentPlatform.Position.Y + slopeX;
                            float yMin = currentPlatform.Position.Y - slopeX;

                            if (yMax < yMin)
                            {
                                float temp = yMax;
                                yMax = yMin;
                                yMin = temp;
                            }

                            //Console.WriteLine("yMin: {0}\tyMax: {1}\tmouseY: {2}", yMin, yMax, mouseSimPos.Y);

                            lockedX = false;
                            lockedY = false;
                            if (yMin < mouseSimPos.Y && mouseSimPos.Y < yMax)
                                lockedX = true;
                            else
                                lockedY = true;

                            Vector2 topLeft = currentPlatform.Position - currentPlatform.Size / 2f;
                            Vector2 botRight = currentPlatform.Position + currentPlatform.Size / 2f;

                            if (lockedX)
                            {
                                if (Math.Abs(topLeft.X - mouseSimPos.X) < Math.Abs(botRight.X - mouseSimPos.X))     // left
                                {
                                    startDraw = botRight;
                                    endDraw = new Vector2(mouseSimPosRound.X, topLeft.Y);
                                }
                                else                // right
                                {
                                    startDraw = topLeft;
                                    endDraw = new Vector2(mouseSimPosRound.X, botRight.Y);
                                }
                            }
                            else if (lockedY)
                            {
                                if (Math.Abs(topLeft.Y - mouseSimPos.Y) < Math.Abs(botRight.Y - mouseSimPos.Y))     // top
                                {
                                    startDraw = botRight;
                                    endDraw = new Vector2(topLeft.X, mouseSimPosRound.Y);
                                }
                                else            // bottom
                                {
                                    startDraw = topLeft;
                                    endDraw = new Vector2(botRight.X, mouseSimPosRound.Y);
                                }
                            }

                            // calculate the starting drawing pivot (which should be on opposite corner of click)
                            //if (Math.Abs(topLeft.X - mouseSimPos.X) < Math.Abs(botRight.X - mouseSimPos.X))
                            //{
                            //    if (Math.Abs(topLeft.Y - mouseSimPos.Y) < Math.Abs(botRight.Y - mouseSimPos.Y))     // top left
                            //    {
                            //        startDraw = botRight;
                            //        if (lockedX)
                            //            endDraw = new Vector2(mouseSimPosRound.X, topLeft.Y);
                            //        else if (lockedY)
                            //            endDraw = new Vector2(topLeft.X, mouseSimPosRound.Y);
                            //    }
                            //    else                                    // bottom left
                            //    {
                            //        startDraw = new Vector2(botRight.X, topLeft.Y);
                            //        if (lockedX)
                            //            endDraw = new Vector2(mouseSimPosRound.X, botRight.Y);
                            //        else if (lockedY)
                            //            endDraw = new Vector2(topLeft.X, mouseSimPosRound.Y);
                            //    }
                            //}
                            //else
                            //{
                            //    if (Math.Abs(topLeft.Y - mouseSimPos.Y) < Math.Abs(botRight.Y - mouseSimPos.Y))     // top right
                            //        startDraw = new Vector2(topLeft.X, botRight.Y);
                            //    else                                    // bottom right
                            //        startDraw = topLeft;
                            //}

                            platforms.Remove(currentPlatform);
                            currentPlatform = null;
                            editingPlatform = true;
                        }
                        else                                        // Move camera
                        {
                            screenOffset += ConvertUnits.GetMousePos(mouse) - ConvertUnits.GetMousePos(prevMouseState);
                        }
                    }
                }
            }
            else if (editingPlatform)                                  // Make the platform
            {
                if (startDraw.X != endDraw.X && startDraw.Y != endDraw.Y)
                {
                    Vector2 topLeft, size;
                    if (startDraw.X > endDraw.X && startDraw.Y < endDraw.Y)     // bottom left
                    {
                        topLeft = new Vector2(endDraw.X, startDraw.Y);
                        size = new Vector2(startDraw.X - endDraw.X, endDraw.Y - startDraw.Y);
                    }
                    else if (startDraw.X < endDraw.X && startDraw.Y > endDraw.Y)    // top right
                    {
                        topLeft = new Vector2(startDraw.X, endDraw.Y);
                        size = new Vector2(endDraw.X - startDraw.X, startDraw.Y - endDraw.Y);
                    }
                    else
                    {
                        topLeft = startDraw;
                        size = endDraw - startDraw;
                    }
                    currentPlatform = new Platform(whiteRect, topLeft + size / 2f, size);
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
                    currentPlatform.MoveByPosition(-Vector2.UnitY);
                else if (keyboard.IsKeyDown(Keys.Left))
                    currentPlatform.MoveByPosition(-Vector2.UnitX);
                else if (keyboard.IsKeyDown(Keys.Right))
                    currentPlatform.MoveByPosition(Vector2.UnitX);
                else if (keyboard.IsKeyDown(Keys.Down))
                    currentPlatform.MoveByPosition(Vector2.UnitY);
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
            else
            {
                if (mouse.ScrollWheelValue > editScroll)
                {
                    float prevZoom = currentZoom;
                    currentZoom *= (mouse.ScrollWheelValue - editScroll) / GameData.SCROLL_STEP;
                    editScroll = mouse.ScrollWheelValue;
                    ConvertUnits.SetDisplayUnitToSimUnitRatio(currentZoom);

                    Vector2 mousePos = ConvertUnits.ToDisplayUnits(mouseSimPos - averagePos) + cameraBounds.Center.ToVector2() + screenOffset;
                    screenOffset -= mousePos - ConvertUnits.GetMousePos(prevMouseState);
                    Console.WriteLine("Screen offset: " + screenOffset);
                }
                else if (mouse.ScrollWheelValue < editScroll)
                {
                    float prevZoom = currentZoom;
                    currentZoom /= (editScroll - mouse.ScrollWheelValue) / GameData.SCROLL_STEP;
                    editScroll = mouse.ScrollWheelValue;
                    ConvertUnits.SetDisplayUnitToSimUnitRatio(currentZoom);

                    Vector2 mousePos = ConvertUnits.ToDisplayUnits(mouseSimPos - averagePos) + cameraBounds.Center.ToVector2() + screenOffset;
                    screenOffset -= mousePos - ConvertUnits.GetMousePos(prevMouseState);
                }
            }
            if (mouse.RightButton == ButtonState.Pressed && prevMouseState.RightButton == ButtonState.Released)
            {
                screenOffset = Vector2.Zero;
                foreach (Player player in players)
                {
                    player.MoveToPosition(mouseSimPos);
                }
            }
            if (keyboard.IsKeyDown(Keys.LeftControl))           // TODO save and load level from UI
            {
                if (ToggleKey(Keys.S))
                {
                    SaveLevel(GameData.LEVEL_FILE);
                }
                else if (ToggleKey(Keys.O))
                {
                    LoadLevel(GameData.LEVEL_FILE);
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

            //float dist = maxY - minY;
            //if (dist * currentZoom / GameData.SCREEN_SPACE > GraphicsDevice.Viewport.Height)
            //    ConvertUnits.SetDisplayUnitToSimUnitRatio(GraphicsDevice.Viewport.Height * GameData.SCREEN_SPACE / dist);
            //else
            //    ConvertUnits.SetDisplayUnitToSimUnitRatio(currentZoom);
        }

        private void DrawGame(double deltaTime)
        {
            // Find average position across all players
            Vector2 averagePos = Vector2.Zero;
            foreach (Player player in players)
                averagePos += player.Position;
            averagePos /= players.Count;
            //Vector2 averagePos = ConvertUnits.ToDisplayUnits(averagePos);

            // TODO don't always splitscreen
            float maxX = ConvertUnits.ToSimUnits(GraphicsDevice.Viewport.Width) * GameData.SCREEN_SPACE;
            float maxY = ConvertUnits.ToSimUnits(GraphicsDevice.Viewport.Height) * GameData.SCREEN_SPACE;
            bool splitScreen = false;
            foreach (Player player in players)
            {
                Vector2 dist = player.Position - averagePos;
                if (Math.Abs(dist.X) > maxX || Math.Abs(dist.Y) > maxY)
                {
                    splitScreen = true;
                    break;
                }
            }


            if (splitScreen)
            {
                for (int i = 0; i < GameData.NUM_PLAYERS; i++)
                {
                    GraphicsDevice.SetRenderTarget(playerScreens[i]);

                    Player player = players[i];
                    Vector2 dist = averagePos - player.Position;
                    dist.Normalize();
                    dist.X *= ConvertUnits.ToSimUnits(GameData.SCREEN_SPACE * GraphicsDevice.Viewport.Width);
                    dist.Y *= ConvertUnits.ToSimUnits(GameData.SCREEN_SPACE * GraphicsDevice.Viewport.Height);
                    Vector2 pos = player.Position + dist;
                    //Console.WriteLine("Real pos_" + i + " = " + player.Position + "\tPos = " + pos + "\tDist = " + dist);

                    // Draw background
                    float zoom = ConvertUnits.GetRatio();
                    ConvertUnits.SetDisplayUnitToSimUnitRatio(GameData.SHADOW_SCALE);
                    DrawBackground(ConvertUnits.ToDisplayUnits(pos));
                    ConvertUnits.SetDisplayUnitToSimUnitRatio(zoom);

                    DrawScene(deltaTime, ConvertUnits.ToDisplayUnits(pos));
                }
            }
            else
            {
                GraphicsDevice.SetRenderTarget(playerScreens[0]);

                // Draw background
                float zoom = ConvertUnits.GetRatio();
                ConvertUnits.SetDisplayUnitToSimUnitRatio(GameData.SHADOW_SCALE);
                DrawBackground(ConvertUnits.ToDisplayUnits(averagePos));
                ConvertUnits.SetDisplayUnitToSimUnitRatio(zoom);

                DrawScene(deltaTime, ConvertUnits.ToDisplayUnits(averagePos));
            }

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (splitScreen)
            {
                for (int i = 0; i < GameData.NUM_PLAYERS; i++)
                {
                    BasicEffect effect = new BasicEffect(graphics.GraphicsDevice);
                    effect.World = Matrix.Identity;
                    effect.TextureEnabled = true;
                    effect.Texture = playerScreens[i];

                    Player player = players[i];
                    Vector2 dist = averagePos - player.Position;
                    dist = new Vector2(dist.Y, dist.X);
                    dist /= -Math.Max(Math.Abs(dist.X), Math.Abs(dist.Y));
					if (dist.Y > 0.999)
						dist.Y = 1;
					else if (dist.Y < -0.999)
						dist.Y = -1;
					if (dist.X > 0.999)
						dist.X = 1;
					else if (dist.X < -0.9999)
						dist.X = -1;
                    //Console.WriteLine("Dist_" + i + " = " + dist);

                    // for Position, (-1,-1) is bottom-left and (1,1) is top-right
                    // for TextureCoordinate, (0,0) is top-left and (1,1) is bottom-right
                    VertexPositionTexture[] vertices = new VertexPositionTexture[4];

                    if (Math.Abs(dist.Y) == 1)
                    {
                        vertices[0].Position = new Vector3(dist, 0f);
                        vertices[1].Position = new Vector3(dist.Y, dist.Y, 0f);
                        vertices[2].Position = new Vector3(-dist, 0f);
                        vertices[3].Position = new Vector3(dist.Y, -dist.Y, 0f);
                    }
                    else
                    {
                        vertices[0].Position = new Vector3(dist, 0f);
                        vertices[1].Position = new Vector3(dist.X, -dist.X, 0f);
                        vertices[2].Position = new Vector3(-dist, 0f);
                        vertices[3].Position = new Vector3(-dist.X, -dist.X, 0f);
                    }

                    //vertices[0].Position = new Vector3(-1f, -1f, 0f);
                    //vertices[1].Position = new Vector3(1f, 1f, 0f);
                    //vertices[2].Position = new Vector3(1f, -1f, 0f);

                    for (int j = 0; j < vertices.Length; j++)
                    {
                        vertices[j].TextureCoordinate = new Vector2(vertices[j].Position.X / 2f + 0.5f, -vertices[j].Position.Y / 2f + 0.5f);
                        if (InvertScreen > 0)
                            vertices[j].TextureCoordinate *= -1;
                        //if (InvertScreen > 0)
                        //{
                        //    vertices[0].TextureCoordinate = new Vector2(1f, 0f);
                        //    vertices[1].TextureCoordinate = new Vector2(0f, 1f);
                        //    vertices[2].TextureCoordinate = new Vector2(0f, 0f);
                        //}
                        //else
                        //{
                        //    vertices[0].TextureCoordinate = new Vector2(0f, 1f);
                        //    vertices[1].TextureCoordinate = new Vector2(1f, 0f);
                        //    vertices[2].TextureCoordinate = new Vector2(1f, 1f);
                        //}
                    }

                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, vertices, 0, 2);
                    }

                    spriteBatch.Begin();
                    float rot = MathHelper.PiOver2 + (float)Math.Atan2(dist.X * GraphicsDevice.Viewport.Width, dist.Y * GraphicsDevice.Viewport.Height);
                    Vector2 origin = new Vector2(0.5f, 0.5f);
                    Vector2 scale = new Vector2(GraphicsDevice.Viewport.Width + GraphicsDevice.Viewport.Height, GameData.SPLIT_HEIGHT);
                    spriteBatch.Draw(Game1.whiteRect, GraphicsDevice.Viewport.Bounds.Center.ToVector2(), null, Color.Black, rot, origin, scale, SpriteEffects.None, 0f);
                    spriteBatch.End();
                }
            }
            else
            {
                spriteBatch.Begin();
                spriteBatch.Draw(playerScreens[0], GraphicsDevice.Viewport.Bounds, null, Color.White, 0f, Vector2.Zero,
                    InvertScreen > 0 ? SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
                spriteBatch.End();
            }

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
            Vector2 botRight = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height) - fontSmall.MeasureString(GameData.Version);
            spriteBatch.DrawString(fontSmall, GameData.Version, botRight, Color.LightSalmon);

            spriteBatch.End();

#if DEBUG
            spriteBatch.Begin();
            for (int i = 0; i < GameData.NUM_PLAYERS; i++)
            {
                spriteBatch.Draw(playerScreens[i], new Rectangle(GraphicsDevice.Viewport.Width / 10 * i, 0, GraphicsDevice.Viewport.Width / 10, GraphicsDevice.Viewport.Height / 10), Color.White);
                spriteBatch.Draw(whiteRect, new Rectangle(GraphicsDevice.Viewport.Width / 10 * i, 0, 10, GraphicsDevice.Viewport.Height / 10), Color.Black);
            }
            spriteBatch.End();
#endif
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
            //spriteBatch.Draw(whiteRect, new Rectangle(-(int)view.Translation.X, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.LightGray);
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
                Vector2 topLeft, size;
                if (startDraw.X > endDraw.X && startDraw.Y < endDraw.Y)     // bottom left
                {
                    topLeft = new Vector2(endDraw.X, startDraw.Y);
                    size = new Vector2(startDraw.X - endDraw.X, endDraw.Y - startDraw.Y);
                }
                else if (startDraw.X < endDraw.X && startDraw.Y > endDraw.Y)    // top right
                {
                    topLeft = new Vector2(startDraw.X, endDraw.Y);
                    size = new Vector2(endDraw.X - startDraw.X, startDraw.Y - endDraw.Y);
                }
                else
                {
                    topLeft = startDraw;
                    size = endDraw - startDraw;
                }
                Vector2 origin = new Vector2(0.5f, 0.5f);
                DrawRect(topLeft + size / 2, Color.Azure, 0, origin, size);
            }
            if (editLevel)
                DrawRect(Vector2.Zero, Color.LightGreen, 0f, new Vector2(0.5f, 0.5f), new Vector2(1, 1));
            spriteBatch.End();


            // Draw all particles and dead wall
            spriteBatch.Begin(transformMatrix: view, blendState: BlendState.NonPremultiplied);
            foreach (Particle part in particles)
                part.Draw(spriteBatch);
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
                float size = layer.Item4 * ConvertUnits.GetResolutionScale();
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
                case State.Options:
                case State.Controls:
                case State.MainMenu:
                    //GraphicsDevice.Clear(Color.Turquoise);
                    menu.Draw(gameTime.ElapsedGameTime.TotalMilliseconds);
                    break;
            }

#if DEBUG
            spriteBatch.Begin();
            EmptyKeys.UserInterface.Input.MouseStateBase mouse = Engine.Instance.InputDevice.MouseState;
            spriteBatch.DrawString(fontSmall, "Mouse -- " + ConvertUnits.GetMousePos(prevMouseState), new Vector2(10, GraphicsDevice.Viewport.Height - 40), Color.White);
            spriteBatch.DrawString(fontSmall, "Mouse -- {" + mouse.NormalizedX + "," + mouse.NormalizedY + "}", new Vector2(10, GraphicsDevice.Viewport.Height - 80), Color.White);
            spriteBatch.DrawString(fontSmall, "Music muted: " + MediaPlayer.IsMuted, new Vector2(10, GraphicsDevice.Viewport.Height - 120), Color.White);
            spriteBatch.End();
#endif

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
    }
}