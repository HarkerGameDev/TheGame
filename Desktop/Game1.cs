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

using Source.Collisions;
using Source.Graphics;

using GameUILibrary;
using EmptyKeys.UserInterface;
using EmptyKeys.UserInterface.Generated;

using Ini;

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

        public List<GameData.Controls> playerControls;

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
        Vector2 cameraPos;     // previous average position of players
        Vector2 screenOffset; // offset from mouse panning
        float currentZoom = GameData.PIXEL_METER;

        Vector2 screenCenter; // where the player is on the screen
        int playerCenter;   // which player camera is centered at (if in Player mode)
        CameraType cameraType;
        LinkedList<Vector2> cameraPath;
        LinkedListNode<Vector2> cameraNode;

        bool editLevel = false;
        public Platform currentPlatform;
        bool editingPlatform;
        Vector2 startDraw;
        Vector2 endDraw;
        LinkedListNode<Vector2> selectedNode;
        bool lockedX = false;
        bool lockedY = false;

        State state = State.MainMenu;
        bool mainMenu = true;
        int characterSelect = 0;
        int playerSelect = 0;
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

        public enum CameraType
        {
            Average, Player, Path
        }

        public enum State
        {
            Running, Paused, MainMenu, Options, Controls, Character, Setup
        }

        enum Menu
        {
            Main, Options, Controls, Pause, Character, Setup
        }

        public Game1()
        {
            LoadSettings();
            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = GameData.FULLSCREEN;
            graphics.SynchronizeWithVerticalRetrace = GameData.VSYNC;
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
            if (GameData.FULLSCREEN)
            {
                nativeScreenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                nativeScreenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }
            else
            {
                nativeScreenWidth = GameData.WINDOW_WIDTH;
                nativeScreenHeight = GameData.WINDOW_HEIGHT;
            }
            graphics.PreferredBackBufferWidth = nativeScreenWidth;
            graphics.PreferredBackBufferHeight = nativeScreenHeight;

            graphics.PreferMultiSampling = true;
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 16;

            IsFixedTimeStep = true;
#if WINDOWS
            Window.IsBorderless = GameData.BORDERLESS;
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
            ConvertUnits.SetMouseScale((float)GraphicsDevice.Viewport.Width / nativeScreenWidth);
            ConvertUnits.SetResolutionScale((float)nativeScreenWidth / GameData.VIEW_WIDTH);

            Console.WriteLine("Native: {{{0}, {1}}}", nativeScreenWidth, nativeScreenHeight);
            Console.WriteLine("Window: " + Window.ClientBounds.Size);
            //Console.WriteLine("Screen: {{{0}, {1}}}", GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
            Console.WriteLine("Adapter: {{{0}, {1}}}", GraphicsDevice.Adapter.CurrentDisplayMode.Width, GraphicsDevice.Adapter.CurrentDisplayMode.Height);
            Console.WriteLine("Viewport: " + GraphicsDevice.Viewport.Bounds.Size);
            Console.WriteLine("Backbuffer: {{{0}, {1}}}", graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            // Set seed for a scheduled random level (minutes since Jan 1, 2015)
            //randSeed = DateTime.Now.Millisecond;
            //randLevelSeed = GameData.GetSeed;

            // Initialize screen render target
            playerScreens = new RenderTarget2D[GameData.MAX_PLAYERS];
            for (int i = 0; i < playerScreens.Length; i++)
                playerScreens[i] = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

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
                playerControls = new List<GameData.Controls>();
                playerControls.Add(new GameData.KeyboardControls(this, Keys.N, Keys.M, Keys.B, Keys.K, Keys.OemSemicolon, Keys.O, Keys.L));
                playerControls.Add(new GameData.SimulatedControls(this));
                playerControls.Add(new GameData.KeyboardControls(this, Keys.Z, Keys.X, Keys.LeftShift, Keys.A, Keys.D, Keys.W, Keys.S));
                playerControls.Add(new GameData.GamePadControls(this, PlayerIndex.One, Buttons.X, Buttons.B, Buttons.Y, Buttons.LeftThumbstickLeft, Buttons.LeftThumbstickRight, Buttons.A, Buttons.RightTrigger));
                playerControls.Add(new GameData.GamePadControls(this, PlayerIndex.One, Buttons.X, Buttons.B, Buttons.Y, Buttons.LeftThumbstickLeft, Buttons.LeftThumbstickRight, Buttons.A, Buttons.RightTrigger));
            }

            rand = new Random();
            //rand = new Random(randSeed);
            //randLevel = new Random(randLevelSeed);

            base.Initialize();      // This calls LoadContent()
        }

        private void LoadSettings()
        {
            //IniFile ini = new IniFile(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\Settings.ini");
            IniFile ini = new IniFile("Settings.ini");
            //Console.WriteLine("Ini read from {0}: {1}", ini.path, ini.IniReadValue("Video", "Test"));
            GameData.WINDOW_WIDTH = Convert.ToInt32(ini.ReadValue("Video", "Width"));
            GameData.WINDOW_HEIGHT = Convert.ToInt32(ini.ReadValue("Video", "Height"));
            GameData.FULLSCREEN = Convert.ToBoolean(ini.ReadValue("Video", "Fullscreen"));
            GameData.BORDERLESS = Convert.ToBoolean(ini.ReadValue("Video", "Borderless"));
            GameData.VSYNC = Convert.ToBoolean(ini.ReadValue("Video", "VSync"));
            GameData.VOLUME = Convert.ToSingle(ini.ReadValue("Audio", "Volume"));
            GameData.MUTED = Convert.ToBoolean(ini.ReadValue("Audio", "Muted"));
        }

        private void SaveSettings()
        {
            IniFile ini = new IniFile("Settings.ini");
            ini.WriteValue("Video", "Width", GameData.WINDOW_WIDTH);
            ini.WriteValue("Video", "Height", GameData.WINDOW_HEIGHT);
            ini.WriteValue("Video", "Fullscreen", graphics.IsFullScreen);
#if WINDOWS
            ini.WriteValue("Video", "Borderless", Window.IsBorderless);
#endif
            ini.WriteValue("Video", "VSync", graphics.SynchronizeWithVerticalRetrace);
            ini.WriteValue("Audio", "Volume", MediaPlayer.Volume);
            ini.WriteValue("Audio", "Muted", MediaPlayer.IsMuted);
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

            playerControls = new List<GameData.Controls>();
            playerControls.Add(new GameData.SimulatedControls(this));
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
                playerControls = new List<GameData.Controls>();
                playerControls.Add(new GameData.SimulatedControls(this));
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
                    mainMenu = true;
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
                case Menu.Character:
                    state = State.Character;
                    menu = new CharacterMenu();
                    break;
                case Menu.Setup:
                    state = State.Setup;
                    menu = new SetupMenu();
                    break;
            }

            menu.DataContext = viewModel;
        }

        private void StartGame()
        {
            GameData.LEVEL_FILE = viewModel.LevelValue;
            LoadLevel(GameData.LEVEL_FILE);

            players = new List<Player>();
            for (int i = 0; i < GameData.PLAYERS.Length; i++)
            {
                //int character = rand.Next(Character.playerCharacters.Length);
                int character = GameData.PLAYERS[i];
                if (playerControls[i] is GameData.SimulatedControls)
                    players.Add(new AI(Content.Load<Texture2D>("Art/GreenDude"), GameData.PLAYER_START, Character.playerCharacters[character], (GameData.SimulatedControls)playerControls[i], Player.Direction.None));
                else
                    players.Add(new Player(Content.Load<Texture2D>("Art/GreenDude"), GameData.PLAYER_START, Character.playerCharacters[character]));
            }

            cameraPos = GameData.PLAYER_START;
            playerCenter = 0;
            cameraNode = cameraPath.First;
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
            MediaPlayer.Volume = GameData.VOLUME;
            MediaPlayer.IsMuted = GameData.MUTED;

            // Set up user interface
            SpriteFont font = Content.Load<SpriteFont>("Fonts/Segoe_UI_15_Bold");
            FontManager.DefaultFont = Engine.Instance.Renderer.CreateFont(font);
            viewModel = new BasicUIViewModel();
            FontManager.Instance.LoadFonts(Content, "Fonts");

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < GameData.MAX_PLAYERS; i++)
            {
                builder.Append("Player ").AppendLine((i + 1).ToString())
                    .AppendLine(playerControls[i].ToString());
            }
            viewModel.ControlsText = builder.ToString();
            Console.WriteLine(builder.ToString());
            //viewModel.ControlsText = "Hello!\nWhoo!!";

            viewModel.MaxPlayers = GameData.MAX_PLAYERS;
            viewModel.LevelValue = GameData.LEVEL_FILE;
            viewModel.PlayerValue = GameData.DEFAULT_PLAYERS;

            LoadUI(Menu.Main);

            //InitializePlayers();

            // Use this to draw any rectangles
            whiteRect = new Texture2D(GraphicsDevice, 1, 1);
            whiteRect.SetData(new[] { Color.White });

            // Load assets in the Content Manager
            //background = Content.Load<Texture2D>("Art/skyscrapers");
            LoadBackground(2);
            fontSmall = Content.Load<SpriteFont>("Fonts/Score");
            fontBig = Content.Load<SpriteFont>("Fonts/ScoreBig");

            // Create objects
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
        }

        private void LoadLevel(int loadLevel)
        {
            platforms.Clear();
            obstacles.Clear();
            drops.Clear();
            cameraPath = new LinkedList<Vector2>();
            using (BinaryReader file = new BinaryReader(File.Open("Levels/level" + loadLevel, FileMode.Open, FileAccess.Read)))
            {
                GameData.PLAYER_START = new Vector2(file.ReadSingle(), file.ReadSingle());
                GameData.CAMERA_SPEED = GameData.SLOW_CAMERA_SPEED;     // TODO user-customizable camera speed
                int cameraNodes = file.ReadInt32();
                //Console.WriteLine("Camera nodes: {0}", cameraNodes);
                for (int i = 0; i < cameraNodes; i++)
                {
                    cameraPath.AddLast(new Vector2(file.ReadSingle(), file.ReadSingle()));
                    //Console.WriteLine("Loading node: {0}", cameraPath.Last.Value);
                }
                try
                {
                    while (file.BaseStream.Position != file.BaseStream.Length)
                    {
                        Vector2 position = new Vector2(file.ReadSingle(), file.ReadSingle());
                        Vector2 size = new Vector2(file.ReadSingle(), file.ReadSingle());
                        float rotation = file.ReadSingle();
                        platforms.Add(new Platform(whiteRect, position, size, rotation));
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine("Failed to load level\nGot through {0} platforms\n{1}", platforms.Count, e);
                }
                Console.WriteLine("Read {0} bytes from level", file.BaseStream.Position);
            }
        }

        private void SaveLevel(int saveLevel)
        {
#if DEBUG
            //Console.WriteLine("Dir: " + Directory.GetFiles(@"..\..\..\..\Levels")[0]);
            BinaryWriter file = new BinaryWriter(File.Open(@"..\..\..\..\Levels\level" + saveLevel, FileMode.Create, FileAccess.Write));
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
            file.Write(cameraPath.Count);
            foreach (Vector2 node in cameraPath)
            {
                file.Write(node.X);
                file.Write(node.Y);
            }
            foreach (Platform plat in platforms)
            {
                file.Write(plat.Position.X);
                file.Write(plat.Position.Y);
                file.Write(plat.Size.X);
                file.Write(plat.Size.Y);
                file.Write(plat.Rotation);
            }
            Console.WriteLine("Wrote {0} bytes to level", file.BaseStream.Position);

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
#if DEBUG
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
#endif

                    //if (ToggleKey(Keys.I))
                    //    LoadBackground(0);
                    //else if (ToggleKey(Keys.O))
                    //    LoadBackground(1);
                    //else if (ToggleKey(Keys.P))
                    //    LoadBackground(2);

                    if (editLevel)
                    {
                        HandleEditLevel();
                    }
                    else
                    {
                        GameData.CAMERA_SPEED = GameData.SLOW_CAMERA_SPEED;
                    }

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
                                case GameData.ControlKey.Basic1:
                                    control.Basic1 = true;
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
                        control.Basic1 = false;
                    }

                    CheckPlayer();
                    InvertScreen -= deltaTime;
                    totalTime += deltaTime;
                    world.Step(deltaTime);

                    // TODO store previous locations of players
                    foreach (Player player in players)
                        player.PrevStates.Add(Tuple.Create(player.Position, player.Velocity));
                    times.Add(totalTime);

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
                        if (mainMenu)
                            LoadUI(Menu.Main);
                        else
                            LoadUI(Menu.Pause);
                        SaveSettings();
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
                case State.Setup:
                    if (ToggleKey(Keys.Escape))
                        LoadUI(Menu.Main);
                    UpdateMenu(gameTime.ElapsedGameTime.TotalMilliseconds);
                    break;
                case State.Character:
                    if (ToggleKey(Keys.Enter))
                        viewModel.ButtonResult = "NextPlayer";
                    else if (ToggleKey(Keys.Left))
                        characterSelect = characterSelect == 0 ? Character.playerCharacters.Length - 1 : characterSelect - 1;
                    else if (ToggleKey(Keys.Right))
                        characterSelect = (characterSelect + 1) % Character.playerCharacters.Length;
                    else if (ToggleKey(Keys.Escape))
                        LoadUI(Menu.Setup);
                    UpdateMenu(gameTime.ElapsedGameTime.TotalMilliseconds);
                    break;
                default:
                    throw new NotImplementedException("Not processing input for state: " + state);
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
                    mainMenu = false;
                    //MediaPlayer.Play(songs[0]);
                    break;
                case "Options":
                    LoadUI(Menu.Options);
                    break;
                case "Exit":
                    Exit();
                    break;
                case "Controls":
                    LoadUI(Menu.Controls);
                    break;
                case "Pause":
                    LoadUI(Menu.Pause);
                    break;
                case "ExitOptions":
                    if (mainMenu)
                        LoadUI(Menu.Main);
                    else
                        LoadUI(Menu.Pause);
                    SaveSettings();
                    break;
                case "MainMenu":
                    LoadUI(Menu.Main);
                    break;
                case "Setup":
                    LoadUI(Menu.Setup);
                    break;
                case "Average":
                    cameraType = CameraType.Average;
                    goto case "Character";
                case "Player":
                    cameraType = CameraType.Player;
                    goto case "Character";
                case "Path":
                    cameraType = CameraType.Path;
                    goto case "Character";
                case "Character":
                    LoadUI(Menu.Character);
                    playerSelect = 0;
                    characterSelect = 0;
                    viewModel.PlayerText = "Player " + (playerSelect + 1);
                    GameData.PLAYERS = new int[viewModel.PlayerValue];
                    break;
                case "Music":
                    MediaPlayer.IsMuted = !MediaPlayer.IsMuted;
                    //MediaPlayer.Resume();
                    break;
                case "VSync":
                    graphics.SynchronizeWithVerticalRetrace = !graphics.SynchronizeWithVerticalRetrace;
                    graphics.ApplyChanges();
                    break;
                case "CharacterLeft":
                    characterSelect = characterSelect == 0 ? Character.playerCharacters.Length - 1 : characterSelect - 1;
                    break;
                case "CharacterRight":
                    characterSelect = (characterSelect + 1) % Character.playerCharacters.Length;
                    break;
                case "NextPlayer":
                    GameData.PLAYERS[playerSelect] = characterSelect;
                    if (++playerSelect >= GameData.PLAYERS.Length)
                    {
                        StartGame();
                        state = State.Running;
                        mainMenu = false;
                    }
                    else
                    {
                        viewModel.PlayerText = "Player " + (playerSelect + 1);
                    }
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
                        graphics.PreferredBackBufferWidth = GameData.WINDOW_WIDTH;
                        graphics.PreferredBackBufferHeight = GameData.WINDOW_HEIGHT;
                    }
                    graphics.IsFullScreen = !graphics.IsFullScreen;
                    graphics.ApplyChanges();

                    Console.WriteLine("Fullscreen: " + graphics.IsFullScreen);
                    ConvertUnits.SetMouseScale((float)GraphicsDevice.Viewport.Width / nativeScreenWidth);
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
                if (player.Alive)
                    HandlePlayerInput(player, i, deltaTime);
            }

            // Find average velocity across the players
            //Vector2 averageVel = Vector2.Zero;
            //foreach (Player player in players)
            //    averageVel += player.Velocity;
            //averageVel /= players.Count;

            // Calculate screen lag behind players
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
            //else if (ToggleKey(Keys.D1))
            //    players[0] = new Player(Content.Load<Texture2D>("Art/GreenDude"), GameData.PLAYER_START, Character.playerCharacters[0]);
            //else if (ToggleKey(Keys.D2))
            //    players[0] = new Player(Content.Load<Texture2D>("Art/GreenDude"), GameData.PLAYER_START, Character.playerCharacters[1]);
            //else if (ToggleKey(Keys.D3))
            //    players[0] = new Player(Content.Load<Texture2D>("Art/GreenDude"), GameData.PLAYER_START, Character.playerCharacters[2]);
            //else if (ToggleKey(Keys.D4))
            //    players[0] = new Player(Content.Load<Texture2D>("Art/GreenDude"), GameData.PLAYER_START, Character.playerCharacters[3]);
            //else if (ToggleKey(Keys.D5))
            //    players[0] = new Player(Content.Load<Texture2D>("Art/GreenDude"), GameData.PLAYER_START, Character.playerCharacters[4]);
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
                if (controls.Basic1)
                {
                    simTimes.Add(totalTime);
                    keys.Add(GameData.ControlKey.Basic1);
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
                    if (!player.PrevJump)       // toggle jump
                    {
                        if (player.CanJump)    // normal jump
                        {
                            if (player.Velocity.Y > 0)
                                player.Velocity.Y = 0;
                            player.JumpSpeed = player.Velocity.Y * GameData.JUMP_HOLD_PROP - GameData.JUMP_SPEED;

                            player.Velocity.Y -= GameData.JUMP_SPEED;
                            //player.TargetVelocity = player.TargetVelocity * GameData.JUMP_SLOW;
                            player.CurrentState = Player.State.Jumping;
                            player.JumpTime = GameData.JUMP_TIME;
                        }
                        else if (player.WallJump != Player.Direction.None)    // wall jump
                        {
                            if (player.Velocity.Y > 0)
                                player.Velocity.Y = 0;
                            player.JumpSpeed = player.Velocity.Y * GameData.JUMP_HOLD_PROP - GameData.WALL_JUMP_Y;

                            player.Velocity.Y -= GameData.WALL_JUMP_Y;
                            if (player.WallJump == Player.Direction.Left)
                                player.Velocity.X = GameData.WALL_JUMP_X;
                            else
                                player.Velocity.X = -GameData.WALL_JUMP_X;
                            player.JumpTime = GameData.JUMP_TIME;
                            //player.WallJump = Player.Jump.None;
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
                        if (player.Velocity.Y > player.JumpSpeed)
                            player.Velocity.Y = player.JumpSpeed;
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
                    player.TargetVelocity = Player.Direction.Right;
                else if (controls.Left)
                    player.TargetVelocity = Player.Direction.Left;
                else
                    player.TargetVelocity = Player.Direction.None;

                // activate (or toggle) special abilities
                if (player.AbilityTwoTime < 0 && controls.Special1)
                {
                    switch (player.CurrentCharacter.Ability2)
                    {
                        case Character.AbilityTwo.Clone:
                            player.AbilityTwoTime = GameData.CLONE_COOLDOWN;
                            GameData.SimulatedControls simulatedControls = new GameData.SimulatedControls(this);
                            playerControls.Insert(players.Count, simulatedControls);
                            AI clone = new AI(player.texture, player.Position, player.CurrentCharacter, simulatedControls, player.TargetVelocity);
                            players.Add(clone);
                            player.ClonedPlayer = clone;
                            player.CloneTime = GameData.CLONE_TIME;
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
                                    if (target != player && target.Alive)
                                    {
                                        Tuple<Vector2, Vector2> state = target.PrevStates[i];
                                        target.MoveToPosition(state.Item1);
                                        target.Velocity = state.Item2;
                                    }
                                }
                            }
                            break;
                        case Character.AbilityTwo.Hook:
                            player.AbilityTwoTime = GameData.HOOK_COOLDOWN;
                            Vector2 vel = new Vector2(player.FacingRight ? GameData.HOOK_X : -GameData.HOOK_X, GameData.HOOK_Y)
                                + player.Velocity * GameData.HOOK_SCALE;
                            player.Projectiles.Add(new Projectile(whiteRect, player.Position, Color.DarkSlateGray, Projectile.Types.Hook, vel));
                            player.HookedLocation = Vector2.Zero;
                            player.HookedPlayer = null;
                            break;
                        case Character.AbilityTwo.Rocket:
                            // TODO tweak rocket so it's better
                            player.AbilityTwoTime = GameData.ROCKET_COOLDOWN;
                            vel = new Vector2(player.FacingRight ? GameData.ROCKET_X : -GameData.ROCKET_X, GameData.ROCKET_Y)
                                + player.Velocity * GameData.ROCKET_SCALE;
                            player.Projectiles.Add(new Projectile(whiteRect, player.Position, Color.DarkOliveGreen, Projectile.Types.Rocket, vel));
                            break;
                        case Character.AbilityTwo.Boomerang:
                            player.AbilityTwoTime = GameData.BOOMERANG_COOLDOWN;
                            vel = new Vector2(player.FacingRight ? GameData.BOOMERANG_X : -GameData.BOOMERANG_X, GameData.BOOMERANG_Y)
                                + player.Velocity * GameData.BOOMERANG_SCALE;
                            player.Projectiles.Add(new Projectile(whiteRect, player.Position, Color.YellowGreen, Projectile.Types.Boomerang, vel));
                            break;
                    }
                }

                if (player.AbilityThreeTime < 0 && controls.Special2)
                {
                    switch (player.CurrentCharacter.Ability3)
                    {
                        case Character.AbilityThree.Invert:
                            player.AbilityThreeTime = GameData.INVERT_COOLDOWN;
                            InvertScreen = GameData.INVERT_TIME;
                            break;
                        case Character.AbilityThree.Trap:
                            player.AbilityThreeTime = GameData.TRAP_COOLDOWN;
                            drops.Add(new Drop(player, whiteRect, player.Position, 1f, Drop.Types.Trap, Color.Red));
                            break;
                    }
                }

                // Basic attack (with 3 modifiers in both land AND air)
                // TODO hitstun, cooldown, and pause everything for a moment during impact
                if (controls.Basic1)
                {
                    if (controls.Down)                                          // holding down
                    {
                        foreach (Player target in players)
                        {
                            if (target != player && Math.Abs(player.Position.X - target.Position.X) < GameData.ATTACK_DOWN_WIDTH)
                            {
                                if (target.Position.Y > player.Position.Y - player.Size.Y / 2f && target.Position.Y < player.Position.Y + GameData.ATTACK_DOWN_HEIGHT)
                                {
                                    if (target.Velocity.Y < 0)
                                        target.Velocity.Y = 0;
                                    target.Velocity += new Vector2(player.FacingRight ? GameData.ATTACK_DOWN_X : -GameData.ATTACK_DOWN_X, GameData.ATTACK_DOWN_Y)
                                        + player.Velocity * GameData.ATTACK_DOWN_MOMENTUM;
                                }
                            }
                        }
                    }
                    else if (player.TargetVelocity == Player.Direction.None)        // no direction
                    {
                        foreach (Player target in players)
                        {
                            if (target != player && Math.Abs(player.Position.Y - target.Position.Y) < GameData.ATTACK_NORM_HEIGHT)
                            {
                                if (player.FacingRight)
                                {
                                    if (target.Position.X > player.Position.X - player.Size.X / 2f && target.Position.X < player.Position.X + GameData.ATTACK_NORM_WIDTH)
                                    {
                                        if (target.Velocity.X < 0)
                                            target.Velocity.X = 0;
                                        if (target.Velocity.Y > 0)
                                            target.Velocity.Y = 0;
                                        target.Velocity += new Vector2(GameData.ATTACK_NORM_X, GameData.ATTACK_NORM_Y) + player.Velocity * GameData.ATTACK_NORM_MOMENTUM;
                                    }
                                }
                                else
                                {
                                    if (target.Position.X < player.Position.X + player.Size.X / 2f && target.Position.X > player.Position.X - GameData.ATTACK_NORM_WIDTH)
                                    {
                                        if (target.Velocity.X > 0)
                                            target.Velocity.X = 0;
                                        if (target.Velocity.Y > 0)
                                            target.Velocity.Y = 0;
                                        target.Velocity += new Vector2(-GameData.ATTACK_NORM_X, GameData.ATTACK_NORM_Y) + player.Velocity * GameData.ATTACK_NORM_MOMENTUM;
                                    }
                                }
                            }
                        }
                    }
                    else                                                    // holding left or right
                    {
                        foreach (Player target in players)
                        {
                            if (target != player && Math.Abs(player.Position.Y - target.Position.Y) < GameData.ATTACK_SIDE_HEIGHT)
                            {
                                if (player.FacingRight)
                                {
                                    if (target.Position.X > player.Position.X - player.Size.X / 2f && target.Position.X < player.Position.X + GameData.ATTACK_SIDE_WIDTH)
                                    {
                                        if (target.Velocity.X < 0)
                                            target.Velocity.X = 0;
                                        if (target.Velocity.Y > 0)
                                            target.Velocity.Y = 0;
                                        target.Velocity += new Vector2(GameData.ATTACK_SIDE_X, GameData.ATTACK_SIDE_Y) + player.Velocity * GameData.ATTACK_SIDE_MOMENTUM;
                                    }
                                }
                                else
                                {
                                    if (target.Position.X < player.Position.X + player.Size.X / 2f && target.Position.X > player.Position.X - GameData.ATTACK_SIDE_WIDTH)
                                    {
                                        if (target.Velocity.X > 0)
                                            target.Velocity.X = 0;
                                        if (target.Velocity.Y > 0)
                                            target.Velocity.Y = 0;
                                        target.Velocity += new Vector2(-GameData.ATTACK_SIDE_X, GameData.ATTACK_SIDE_Y) + player.Velocity * GameData.ATTACK_SIDE_MOMENTUM;
                                    }
                                }
                            }
                        }
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
            //Vector2 averagePos = Vector2.Zero;
            //int count = 0;
            //foreach (Player player in players)
            //{
            //    if (player.Alive)
            //    {
            //        averagePos += player.Position;
            //        count++;
            //    }
            //}
            //averagePos /= count;

            // Snap the mouse position to 1x1 meter grid
            Vector2 mouseSimPos = ConvertUnits.ToSimUnits(ConvertUnits.GetMousePos(mouse) - cameraBounds.Center.ToVector2() - screenOffset) + cameraPos;
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
                            Collisions.Polygon body = world.TestPoint(mouseSimPos);
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

                            platforms.Remove(currentPlatform);
                            currentPlatform = null;
                            editingPlatform = true;
                        }
                        else if (prevMouseState.LeftButton == ButtonState.Released)       // toggle mouse click
                        {
                            // Try to select node
                            selectedNode = null;
                            LinkedListNode<Vector2> node = cameraPath.First;
                            while (node != null)
                            {
                                Vector2 dist = node.Value - mouseSimPos;
                                if (Math.Abs(dist.X) < GameData.NODE_SIZE && Math.Abs(dist.Y) < GameData.NODE_SIZE)
                                {
                                    selectedNode = node;
                                    //Console.WriteLine("Selected node with dist {0}", dist);
                                    break;
                                }
                                node = node.Next;
                            }
                        }
                        else
                        {
                            if (selectedNode == null)           // Move camera
                                screenOffset += ConvertUnits.GetMousePos(mouse) - ConvertUnits.GetMousePos(prevMouseState);
                            else                                // Move node
                                selectedNode.Value = mouseSimPos;
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
                else if (ToggleKey(Keys.V))
                    currentPlatform.Rotate(MathHelper.PiOver4);
                else if (ToggleKey(Keys.C))
                    currentPlatform.Rotate(-MathHelper.PiOver4);
            }
            else if (selectedNode != null)
            {
                if (keyboard.IsKeyDown(Keys.Back))          // Delete selected node
                {
                    cameraPath.Remove(selectedNode);
                    selectedNode = null;
                }
                else if (keyboard.IsKeyDown(Keys.Enter))    // Deseslect node
                    selectedNode = null;
                else if (ToggleKey(Keys.F))
                {
                    LinkedListNode<Vector2> newNode = new LinkedListNode<Vector2>(mouseSimPos);
                    cameraPath.AddAfter(selectedNode, newNode);
                    selectedNode = newNode;
                }
            }

            // Make new camera node at end of path
            if (ToggleKey(Keys.Q))
            {
                cameraPath.AddLast(mouseSimPos);
                selectedNode = cameraPath.Last;
            }

            if (keyboard.IsKeyDown(Keys.RightShift))
                GameData.CAMERA_SPEED = GameData.FAST_CAMERA_SPEED;
            else
                GameData.CAMERA_SPEED = 0f;

            // Zoom in and out
            if (ToggleKey(Keys.OemPlus))
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

                    Vector2 mousePos = ConvertUnits.ToDisplayUnits(mouseSimPos - cameraPos) + cameraBounds.Center.ToVector2() + screenOffset;
                    screenOffset -= mousePos - ConvertUnits.GetMousePos(prevMouseState);
                    //Console.WriteLine("Screen offset: " + screenOffset);
                }
                else if (mouse.ScrollWheelValue < editScroll)
                {
                    float prevZoom = currentZoom;
                    currentZoom /= (editScroll - mouse.ScrollWheelValue) / GameData.SCROLL_STEP;
                    editScroll = mouse.ScrollWheelValue;
                    ConvertUnits.SetDisplayUnitToSimUnitRatio(currentZoom);

                    Vector2 mousePos = ConvertUnits.ToDisplayUnits(mouseSimPos - cameraPos) + cameraBounds.Center.ToVector2() + screenOffset;
                    screenOffset -= mousePos - ConvertUnits.GetMousePos(prevMouseState);
                }
            }

            // Move players
            if (mouse.RightButton == ButtonState.Pressed && prevMouseState.RightButton == ButtonState.Released)
            {
                screenOffset = Vector2.Zero;
                foreach (Player player in players)
                {
                    player.MoveToPosition(mouseSimPos);
                }
                //cameraPos = mouseSimPos;
            }

            // TODO save and load level from UI
            if (keyboard.IsKeyDown(Keys.LeftControl))
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
            //Player max = null;
            float minY = players[0].Position.Y;
            float maxY = minY;

            //Vector2 averagePos = Vector2.Zero;
            //int count = 0;
            //foreach (Player player in players)
            //{
            //    if (player.Alive)
            //    {
            //        averagePos += player.Position;
            //        count++;
            //    }
            //}
            //averagePos /= count;
            //averagePos = ConvertUnits.ToDisplayUnits(averagePos);

            int alivePlayers = 0;

            for (int i = players.Count - 1; i >= 0; i--)
            {
                Player player = players[i];
                if (player.Alive)
                {
                    alivePlayers++;
                    Vector2 dist = ConvertUnits.ToDisplayUnits(player.Position - cameraPos);
                    if (Math.Abs(dist.X) > GraphicsDevice.Viewport.Width / 2f || Math.Abs(dist.Y) > GraphicsDevice.Viewport.Height / 2f)
                    {
                        player.Kill();
                        continue;
                    }
                }

                //if (player.Position.X > levelEnd - GameData.LOAD_NEW)
                //    MakeLevel();

                //if (max == null || player.Position.X > max.Position.X)
                //    max = player;

                //if (player.Position.Y < minY)
                //    minY = player.Position.Y;
                //else if (player.Position.Y > maxY)
                //    maxY = player.Position.Y;
            }

            if (players.Count > 1 && alivePlayers <= 1 || players.Count == 1 && alivePlayers < 1)
            {
                foreach (Player player in players)
                {
                    if (player.Alive)
                        player.Score++;
                    player.MoveToPosition(cameraPos);
                    player.ResetValues();
                }
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

            //float currentX = max == null ? averagePos : max.Position.X;

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
            Vector2 newCameraPos;
            switch (cameraType)
            {
                case CameraType.Average:
                    newCameraPos = Vector2.Zero;
                    int count = 0;
                    foreach (Player player in players)
                    {
                        newCameraPos += player.Position;
                        count++;
                    }
                    newCameraPos /= count;
                    break;
                case CameraType.Player:
                    newCameraPos = players[playerCenter].Position;
                    break;
                case CameraType.Path:
                    // TODO check if the next node should be used
                    newCameraPos = cameraNode == null ? cameraPos : cameraNode.Value;
                    break;
                default:
                    throw new Exception("Camera type " + cameraType + " not implemented");
            }

            // move camera towards newCameraPos
            Vector2 move = newCameraPos - cameraPos;
            float length = move.Length();
            if (length < GameData.CAMERA_SPEED * deltaTime)
            {
                cameraPos = newCameraPos;
                if (cameraType == CameraType.Path && cameraNode != null)
                    cameraNode = cameraNode.Next;
            }
            else
            {
                if (cameraType == CameraType.Path && length != 0)
                    cameraPos += move / length * GameData.CAMERA_SPEED * (float)deltaTime;
                else
                    cameraPos += move /*/ length * (float)Math.Sqrt(length)*/ * GameData.CAMERA_SPEED * (float)deltaTime;
            }

            //float maxX = ConvertUnits.ToSimUnits(GraphicsDevice.Viewport.Width) * GameData.SCREEN_SPACE;
            //float maxY = ConvertUnits.ToSimUnits(GraphicsDevice.Viewport.Height) * GameData.SCREEN_SPACE;
            //bool splitScreen = false;
            //if (!editLevel)
            //{
            //    foreach (Player player in players)
            //    {
            //        Vector2 dist = player.Position - averagePos;
            //        if (Math.Abs(dist.X) > maxX || Math.Abs(dist.Y) > maxY)
            //        {
            //            splitScreen = true;
            //            break;
            //        }
            //    }
            //}


            //if (splitScreen)
            //{
            //    for (int i = 0; i < GameData.PLAYERS.Length; i++)
            //    {
            //        Player player = players[i];
            //        if (!player.Alive)
            //            continue;

            //        GraphicsDevice.SetRenderTarget(playerScreens[i]);

            //        Vector2 dist = cameraPos - player.Position;
            //        dist.Normalize();
            //        dist.X *= ConvertUnits.ToSimUnits(GameData.SCREEN_SPACE * GraphicsDevice.Viewport.Width);
            //        dist.Y *= ConvertUnits.ToSimUnits(GameData.SCREEN_SPACE * GraphicsDevice.Viewport.Height);
            //        Vector2 pos = player.Position + dist;
            //        //Console.WriteLine("Real pos_" + i + " = " + player.Position + "\tPos = " + pos + "\tDist = " + dist);

            //        // Draw background
            //        float zoom = ConvertUnits.GetRatio();
            //        ConvertUnits.SetDisplayUnitToSimUnitRatio(GameData.SHADOW_SCALE);
            //        DrawBackground(ConvertUnits.ToDisplayUnits(cameraPos));
            //        ConvertUnits.SetDisplayUnitToSimUnitRatio(zoom);

            //        DrawScene(deltaTime, ConvertUnits.ToDisplayUnits(pos));
            //    }
            //}
            //else
            //{
            GraphicsDevice.SetRenderTarget(playerScreens[0]);

            // Draw background
            float zoom = ConvertUnits.GetRatio();
            ConvertUnits.SetDisplayUnitToSimUnitRatio(GameData.SHADOW_SCALE);
            DrawBackground(ConvertUnits.ToDisplayUnits(cameraPos));
            ConvertUnits.SetDisplayUnitToSimUnitRatio(zoom);

            DrawScene(deltaTime, ConvertUnits.ToDisplayUnits(cameraPos));
            //}

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.CornflowerBlue);

     //       if (splitScreen)
     //       {
     //           for (int i = 0; i < GameData.PLAYERS.Length; i++)
     //           {
     //               Player player = players[i];
     //               if (!player.Alive)
     //                   continue;

     //               BasicEffect effect = new BasicEffect(graphics.GraphicsDevice);
     //               effect.World = Matrix.Identity;
     //               effect.TextureEnabled = true;
     //               effect.Texture = playerScreens[i];

     //               Vector2 dist = cameraPos - player.Position;
     //               dist = new Vector2(dist.Y, dist.X);
     //               dist /= -Math.Max(Math.Abs(dist.X), Math.Abs(dist.Y));
					//if (dist.Y > 0.999)
					//	dist.Y = 1;
					//else if (dist.Y < -0.999)
					//	dist.Y = -1;
					//if (dist.X > 0.999)
					//	dist.X = 1;
					//else if (dist.X < -0.9999)
					//	dist.X = -1;
     //               //Console.WriteLine("Dist_" + i + " = " + dist);

     //               // for Position, (-1,-1) is bottom-left and (1,1) is top-right
     //               // for TextureCoordinate, (0,0) is top-left and (1,1) is bottom-right
     //               VertexPositionTexture[] vertices = new VertexPositionTexture[4];

     //               if (Math.Abs(dist.Y) == 1)
     //               {
     //                   vertices[0].Position = new Vector3(dist, 0f);
     //                   vertices[1].Position = new Vector3(dist.Y, dist.Y, 0f);
     //                   vertices[2].Position = new Vector3(-dist, 0f);
     //                   vertices[3].Position = new Vector3(dist.Y, -dist.Y, 0f);
     //               }
     //               else
     //               {
     //                   vertices[0].Position = new Vector3(dist, 0f);
     //                   vertices[1].Position = new Vector3(dist.X, -dist.X, 0f);
     //                   vertices[2].Position = new Vector3(-dist, 0f);
     //                   vertices[3].Position = new Vector3(-dist.X, -dist.X, 0f);
     //               }

     //               //vertices[0].Position = new Vector3(-1f, -1f, 0f);
     //               //vertices[1].Position = new Vector3(1f, 1f, 0f);
     //               //vertices[2].Position = new Vector3(1f, -1f, 0f);

     //               for (int j = 0; j < vertices.Length; j++)
     //               {
     //                   vertices[j].TextureCoordinate = new Vector2(vertices[j].Position.X / 2f + 0.5f, -vertices[j].Position.Y / 2f + 0.5f);
     //                   if (InvertScreen > 0)
     //                       vertices[j].TextureCoordinate *= -1;
     //                   //if (InvertScreen > 0)
     //                   //{
     //                   //    vertices[0].TextureCoordinate = new Vector2(1f, 0f);
     //                   //    vertices[1].TextureCoordinate = new Vector2(0f, 1f);
     //                   //    vertices[2].TextureCoordinate = new Vector2(0f, 0f);
     //                   //}
     //                   //else
     //                   //{
     //                   //    vertices[0].TextureCoordinate = new Vector2(0f, 1f);
     //                   //    vertices[1].TextureCoordinate = new Vector2(1f, 0f);
     //                   //    vertices[2].TextureCoordinate = new Vector2(1f, 1f);
     //                   //}
     //               }

     //               foreach (EffectPass pass in effect.CurrentTechnique.Passes)
     //               {
     //                   pass.Apply();

     //                   GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, vertices, 0, 2);
     //               }

     //               spriteBatch.Begin();
     //               float rot = MathHelper.PiOver2 + (float)Math.Atan2(dist.X * GraphicsDevice.Viewport.Width, dist.Y * GraphicsDevice.Viewport.Height);
     //               Vector2 origin = new Vector2(0.5f, 0.5f);
     //               Vector2 scale = new Vector2(GraphicsDevice.Viewport.Width + GraphicsDevice.Viewport.Height, GameData.SPLIT_HEIGHT);
     //               spriteBatch.Draw(Game1.whiteRect, GraphicsDevice.Viewport.Bounds.Center.ToVector2(), null, Color.Black, rot, origin, scale, SpriteEffects.None, 0f);
     //               spriteBatch.End();
     //           }
     //       }
     //       else
     //       {
            spriteBatch.Begin();
            spriteBatch.Draw(playerScreens[0], GraphicsDevice.Viewport.Bounds, null, Color.White, 0f, Vector2.Zero,
                InvertScreen > 0 ? SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            spriteBatch.End();
            //}

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

            // Display current played time
            string time = totalTime.ToString("n1") + "s played";
            leftX = GraphicsDevice.Viewport.Width / 2f - fontSmall.MeasureString(time).X / 2f;
            spriteBatch.DrawString(fontSmall, time, new Vector2(leftX, 0f), Color.LightSkyBlue);

            // Display high score
            //string high = "High Score: " + highScore.ToString("n1");
            //leftX = GraphicsDevice.Viewport.Width / 2f - fontSmall.MeasureString(high).X / 2f;
            //spriteBatch.DrawString(fontSmall, high, new Vector2(leftX, 40f), Color.Yellow);

            // Display player scores
            for (int i=0; i<players.Count; i++)
            {
                StringBuilder text = new StringBuilder("Player ");
                text.Append(i + 1).Append(" Score: ").Append(players[i].Score);
                spriteBatch.DrawString(fontSmall, text, new Vector2(10f, 10f + 40 * i), Color.BlanchedAlmond);
            }

            // Display version number
            Vector2 botRight = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height) - fontSmall.MeasureString(GameData.Version);
            spriteBatch.DrawString(fontSmall, GameData.Version, botRight, Color.LightSalmon);

            // Show "Editing" text
            if (editLevel)
            {
                string editing = "Editing level";
                leftX = GraphicsDevice.Viewport.Width / 2f - fontBig.MeasureString(editing).X / 2f;
                spriteBatch.DrawString(fontBig, editing, new Vector2(leftX, 100f), Color.Chartreuse);
            }

            spriteBatch.End();

//#if DEBUG
//            spriteBatch.Begin();
//            for (int i = 0; i < GameData.PLAYERS.Length; i++)
//            {
//                spriteBatch.Draw(playerScreens[i], new Rectangle(GraphicsDevice.Viewport.Width / 10 * i, 0, GraphicsDevice.Viewport.Width / 10, GraphicsDevice.Viewport.Height / 10), Color.White);
//                spriteBatch.Draw(whiteRect, new Rectangle(GraphicsDevice.Viewport.Width / 10 * i, 0, 10, GraphicsDevice.Viewport.Height / 10), Color.Black);
//            }
//            spriteBatch.End();
//#endif
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
                if (player.Alive)
                {
                    player.Sprite.Update(deltaTime);
                    player.Draw(spriteBatch);
                }
                foreach (Projectile proj in player.Projectiles)
                    proj.Draw(spriteBatch);
            }

            // Draw all objects
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
            {
                DrawRect(Vector2.Zero, Color.LightGreen, 0f, new Vector2(0.5f, 0.5f), new Vector2(1, 1));
                LinkedListNode<Vector2> node = cameraPath.First;
                if (node != null)
                {
                    while (node.Next != null)
                    {
                        Vector2 dist = node.Next.Value - node.Value;
                        float rot = (float)Math.Atan2(dist.Y, dist.X);
                        Vector2 origin = new Vector2(0f, 0.5f);
                        Vector2 scale = new Vector2(ConvertUnits.ToDisplayUnits(dist.Length()), 3f);
                        spriteBatch.Draw(Game1.whiteRect, ConvertUnits.ToDisplayUnits(node.Value), null, Color.Yellow, rot, origin, scale, SpriteEffects.None, 0f);
                        DrawRect(node.Value, Color.YellowGreen, 0f, new Vector2(0.5f, 0.5f), new Vector2(GameData.NODE_SIZE));
                        node = node.Next;
                    }
                    DrawRect(node.Value, Color.YellowGreen, 0f, new Vector2(0.5f, 0.5f), new Vector2(GameData.NODE_SIZE));
                    if (selectedNode != null)
                    {
                        DrawRect(selectedNode.Value, Color.Plum, 0f, new Vector2(0.5f, 0.5f), new Vector2(GameData.NODE_SIZE * 2));
                    }
                }
            }
            spriteBatch.End();

            // Draw all particles
            spriteBatch.Begin(transformMatrix: view, blendState: BlendState.NonPremultiplied);
            foreach (Particle part in particles)
                part.Draw(spriteBatch);
            spriteBatch.End();

//#if DEBUG
//            spriteBatch.Begin(transformMatrix: view);
//            spriteBatch.Draw(whiteRect, ConvertUnits.ToDisplayUnits(cameraPos), null, Color.Olive, 0f, new Vector2(0.5f), ConvertUnits.ToDisplayUnits(3), SpriteEffects.None, 0f);
//            spriteBatch.Draw(whiteRect, averagePos, null, Color.DarkGreen, 0f, new Vector2(0.5f), ConvertUnits.ToDisplayUnits(2), SpriteEffects.None, 0f);
//            spriteBatch.End();
//#endif
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
                spriteBatch.Draw(tex, new Vector2(0, height * center + averagePos.Y * speed * GameData.BACKGROUND_Y_SCALE),
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

        private void DrawCharacter()
        {
            int width = GraphicsDevice.Viewport.Width;
            int height = GraphicsDevice.Viewport.Height;
            
            GraphicsDevice.Clear(Color.MidnightBlue);

            spriteBatch.Begin();
            // TODO draw current profile
            spriteBatch.DrawString(fontBig, Character.playerCharacters[characterSelect].Name, new Vector2(300, 200), Character.playerCharacters[characterSelect].Color);
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
                case State.Character:
                    DrawCharacter();
                    menu.Draw(gameTime.ElapsedGameTime.TotalMilliseconds);
                    break;
                    //goto case State.MainMenu;
                case State.Paused:
                case State.Options:
                case State.Controls:
                case State.Setup:
                case State.MainMenu:
                    //GraphicsDevice.Clear(Color.Turquoise);
                    menu.Draw(gameTime.ElapsedGameTime.TotalMilliseconds);
                    break;
                default:
                    throw new NotImplementedException("Not drawing state: " + state);
            }

#if DEBUG
            spriteBatch.Begin();
            EmptyKeys.UserInterface.Input.MouseStateBase mouse = Engine.Instance.InputDevice.MouseState;
            spriteBatch.DrawString(fontSmall, "Mouse -- " + ConvertUnits.GetMousePos(prevMouseState), new Vector2(10, GraphicsDevice.Viewport.Height - 40), Color.White);
            spriteBatch.DrawString(fontSmall, "Mouse -- {" + mouse.NormalizedX + "," + mouse.NormalizedY + "}", new Vector2(10, GraphicsDevice.Viewport.Height - 80), Color.White);
            spriteBatch.DrawString(fontSmall, "Music muted: " + MediaPlayer.IsMuted, new Vector2(10, GraphicsDevice.Viewport.Height - 120), Color.White);
            spriteBatch.DrawString(fontSmall, "Screen center: " + screenCenter, new Vector2(10, GraphicsDevice.Viewport.Height - 160), Color.White);
            spriteBatch.DrawString(fontSmall, "Screen offset: " + screenOffset, new Vector2(10, GraphicsDevice.Viewport.Height - 200), Color.White);
            if (players != null)
            {
                spriteBatch.DrawString(fontSmall, "Player 0 Velocity: " + players[0].Velocity, new Vector2(10, GraphicsDevice.Viewport.Height - 240), Color.White);
                spriteBatch.DrawString(fontSmall, "Player 0 Position: " + players[0].Position, new Vector2(10, GraphicsDevice.Viewport.Height - 280), Color.White);
                spriteBatch.DrawString(fontSmall, "Player 0 State: " + players[0].CurrentState, new Vector2(10, GraphicsDevice.Viewport.Height - 320), Color.White);
            }
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
    }
}