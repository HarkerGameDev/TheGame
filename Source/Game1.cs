using System;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision.Shapes;

namespace Source
{
	/// <summary>
	/// This is the main type for the game.
    /// 
    /// IMPORTANT NOTES - PLEASE READ ALL:
    /// - (0,0) is in the top left for drawing to screen, and (0,0) is in the center for Farseer physics
    /// - Farseer uses meters, monogame uses pixels -- use ConvertUnits to convert
    /// - Please follow the style guide in place, which is
    ///   * ALL_CAPS for global constants
    ///   * UpperCamelCase for members (instance fields and methods)
    ///   * lowerCamelCase for other variables
    /// - Search for 'TODO' for things that need to be finished
    /// - For things that need to be added, look at the google doc, https://docs.google.com/document/d/1ofddsIU92CeK2RtJ5eg3PWEG8U2o49VdmNxmAJwwMMg/edit
    /// - Make good commit messages
    /// - PLEEAASSEEE do your assigned task, or say you can't do it so that someone else can
	/// </summary>
	public class Game1 : Game
	{
#if MONOMAC
        private const String LEVEL_FILE = "../../../../../../test.lvl";
#else
        private const String LEVEL_FILE = "../../../../test.lvl";
#endif

        // Farseer user data
        private const int PLAYER = 0;
        private const int FLOOR = 1;

        private const float MIN_VELOCITY = 1f;  // m/s -- what can be considered 0 horizontal velocity
        private const float MAX_VELOCITY = 13f; // m/s -- approximate Usaine Bolt speed
        private const float SLOWDOWN = 0.7f;    // N/s -- impulse applied in opposite direction of travel to simulate friction
        private const double JUMP_WAIT = 1;

		private GraphicsDeviceManager graphics;
		private SpriteBatch spriteBatch;
        private KeyboardState prevKeyState;
        private GamePadState prevPadState;
        private MouseState prevMouseState;
		private Texture2D whiteRect;
        private SpriteFont font, fontBig;

        private Random rand;
        private World world;

        private const float CAMERA_SCALE = 20f;
        private const float MAX_CAMERA_SPEED_X = 5f; //5
        private const float MAX_CAMERA_SPEED_Y = 3f; //3
        private const float SCREEN_LEFT = 0.1f;
        private const float SCREEN_RIGHT = 0.35f;
        private const float SCREEN_TOP = 0.3f;
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
        
		public class Floor
		{
            public Body Body;
            public Vector2 Origin;
            public Vector2 Scale;

            private Vector2 start;
            
            public Floor(World world, Vector2 position1, Vector2 position2)
            {
                start = position1;
                Vector2 dist = position2 - position1;
                float width = dist.Length();
                Vector2 center = position1 + dist / 2;
                float rotation = (float)Math.Atan2(dist.Y, dist.X);

                MakeFloor(world, width, center, rotation);
            }

            public Floor(World world, float width, Vector2 center, float rotation)
            {
                MakeFloor(world, width, center, rotation);
            }

            private void MakeFloor(World world, float width, Vector2 center, float rotation)
            {
                Body = BodyFactory.CreateRectangle(world, width, 0.2f, 1f, center, FLOOR);
                Scale = new Vector2(width, 0.2f);
                Origin = new Vector2(0.5f, 0.5f);

                Body.BodyType = BodyType.Static;
                Body.IsStatic = true;
                Body.FixedRotation = true;
                Body.Rotation = rotation;
                Body.Friction = 0.7f;
                Body.Restitution = 0.1f;    // Bounciness. Everything is ever so slightly bouncy so it doesn't feel like a rock with VHB tape.
            }

            public void SetEnd(Vector2 end)
            {
                Vector2 dist = end - start;
                float width = dist.Length();
                float height = 0.2f;
                Vector2 center = start + dist / 2;
                float rotation = (float)Math.Atan2(dist.Y, dist.X);

                Scale = new Vector2(width, height);
                Body.Rotation = rotation;
                Body.Position = center;
            }
		}

        class Player
        {
            public Body Body;
            public Vector2 Origin;
            public Vector2 Scale;

            public bool CanJump
            {
                get
                {
                    return Collisions > 0 && JumpWait < 0;
                }
            }
            private int Collisions;
            public double JumpWait;

            public Player(World world)
            {
                float width = 0.8f;
                float height = 1.8f;
                Body = BodyFactory.CreateCapsule(world, height - width, width / 2, 1f, PLAYER);
                Body.Position = new Vector2(3f, 8f);
                Scale = new Vector2(width, height);
                Origin = new Vector2(0.5f, 0.5f);

                //Falling = true;
                Collisions = 0;
                JumpWait = 0;

                Fixture foot = FixtureFactory.AttachRectangle(1f, 0.3f, 0f, new Vector2(0f, 0.8f), Body);
                foot.IsSensor = true;
                foot.OnCollision += StartTouch;
                foot.OnSeparation += EndTouch;

                Body.BodyType = BodyType.Kinematic;
                Body.IsStatic = false;
                Body.FixedRotation = true;
                Body.Friction = 0f;     // Friction is handled for the player individually because it decreases speed otherwise
            }

            private bool StartTouch(Fixture f1, Fixture f2, Contact contact)
            {
                Collisions++;
                return true;
            }

            private void EndTouch(Fixture f1, Fixture f2)
            {
                Collisions--;
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
            graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width / 2;
            graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height / 2;
            graphics.IsFullScreen = false;
            IsMouseVisible = true;
            graphics.ApplyChanges();

            // Sets how many pixels is a meter for Farseer
            ConvertUnits.SetDisplayUnitToSimUnitRatio(32f);

            // Create objects
            world = new World(new Vector2(0, 13f));     // gravity in N
            rand = new Random();
            player = new Player(world);
            floors = new List<Floor>();

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
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

            // Initialize camera
            int width = graphics.GraphicsDevice.Viewport.Width;
            int height = graphics.GraphicsDevice.Viewport.Height;
            cameraBounds = new Rectangle((int)(width * SCREEN_LEFT), (int)(height * SCREEN_TOP), (int)(width * (SCREEN_RIGHT - SCREEN_LEFT)), (int)(height * (1 - 2 * SCREEN_TOP)));
            screenCenter = cameraBounds.Center.ToVector2();
            screenOffset = Vector2.Zero;

			// Use this to draw rectangles
			whiteRect = new Texture2D(GraphicsDevice, 1, 1);
			whiteRect.SetData(new[] { Color.White });

			// Load fonts using Content Manager
			font = Content.Load<SpriteFont>("Score");
			fontBig = Content.Load<SpriteFont>("ScoreBig");

            // Loads level stored in LEVEL_FILE
            LoadLevel();
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
            // TODO put this in pause menu later
			if (Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

            // Handle toggle pause
            // TODO open pause menu
            if (Keyboard.GetState().IsKeyDown(Keys.Space) && !prevKeyState.IsKeyDown(Keys.Space))
                paused = !paused;

            // Toggle edit level
            if (Keyboard.GetState().IsKeyDown(Keys.E) && !prevKeyState.IsKeyDown(Keys.E))
            {
                editLevel = !editLevel;
                if (editLevel)
                    ConvertUnits.SetDisplayUnitToSimUnitRatio(8f);
                else
                {
                    ConvertUnits.SetDisplayUnitToSimUnitRatio(32f);
                    screenOffset = Vector2.Zero;
                    currentFloor = null;
                }
            }

            if (editLevel)
            {
                HandleEditLevel();
            }

            // This is pretty much how pause always works
            if (!paused)
            {
                HandleKeyboard();

                CheckPlayer();
                player.JumpWait -= gameTime.ElapsedGameTime.TotalSeconds;

                world.Step((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f);
            }

            prevKeyState = Keyboard.GetState();
            prevPadState = GamePad.GetState(0);

			base.Update(gameTime);
		}

        private void HandleKeyboard()
        {
            KeyboardState state = Keyboard.GetState();

            float impulse = MathHelper.SmoothStep(0.5f, 0f, Math.Abs(player.Body.LinearVelocity.X) / MAX_VELOCITY);
            impulse = (float)Math.Pow(impulse, 0.5);

            if (state.IsKeyDown(Keys.Right))
            {
                player.Body.ApplyLinearImpulse(new Vector2(impulse, 0f));
                if (player.Body.LinearVelocity.X < 0f && player.CanJump)  // change direction quickly
                    player.Body.ApplyLinearImpulse(new Vector2(SLOWDOWN, 0f));
            }
            else if (state.IsKeyDown(Keys.Left))
            {
                player.Body.ApplyLinearImpulse(new Vector2(-impulse, 0f));
                if (player.Body.LinearVelocity.X > 0f && player.CanJump)  // change direction quickly
                {
                    player.Body.ApplyLinearImpulse(new Vector2(-SLOWDOWN, 0f));
                }
            }
            else if (player.CanJump)   // player is on the ground
            {
                if (Math.Abs(player.Body.LinearVelocity.X) < MIN_VELOCITY)
                    player.Body.LinearVelocity = new Vector2(0f, player.Body.LinearVelocity.Y);
                else
                {
                    int playerVelSign = Math.Sign(player.Body.LinearVelocity.X);
                    player.Body.ApplyLinearImpulse(new Vector2(Math.Sign(player.Body.LinearVelocity.X) * -SLOWDOWN, 0f));
                }
            }
            if (state.IsKeyDown(Keys.Up) && player.CanJump)
            {
                player.JumpWait = JUMP_WAIT;
                player.Body.ApplyLinearImpulse(new Vector2(0f, -13f));
            }

            // TODO ever so slight camera shake when going fast
            float deltaX = ((cameraBounds.Center.X - screenCenter.X) / cameraBounds.Width - player.Body.LinearVelocity.X / MAX_VELOCITY) * CAMERA_SCALE;
            deltaX = MathHelper.Clamp(deltaX, -MAX_CAMERA_SPEED_X, MAX_CAMERA_SPEED_X);
            screenCenter.X += deltaX;
            screenCenter.X = MathHelper.Clamp(screenCenter.X, cameraBounds.Left, cameraBounds.Right);

            float deltaY = ((cameraBounds.Center.Y - screenCenter.Y) / cameraBounds.Height - player.Body.LinearVelocity.Y / MAX_VELOCITY) * CAMERA_SCALE;
            deltaY = MathHelper.Clamp(deltaY, -MAX_CAMERA_SPEED_Y, MAX_CAMERA_SPEED_Y);
            screenCenter.Y += deltaY;
            screenCenter.Y = MathHelper.Clamp(screenCenter.Y, cameraBounds.Top, cameraBounds.Bottom);
        }

        private void CheckPlayer()
        {
            if (player.Body.Position.Y > ConvertUnits.ToSimUnits(graphics.GraphicsDevice.Viewport.Height))
                player = new Player(world);
        }

        private void HandleEditLevel()
        {
            KeyboardState keyboard = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();
            Vector2 mouseSimPos = ConvertUnits.ToSimUnits(mouse.Position.ToVector2() - cameraBounds.Center.ToVector2() - screenOffset) + player.Body.Position;
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
                            Fixture fix = world.TestPoint(ConvertUnits.ToSimUnits(mouse.Position.ToVector2() - cameraBounds.Center.ToVector2() - screenOffset) + player.Body.Position);
                            if (fix != null && (int)fix.Body.UserData == FLOOR)
                                currentFloor = floors.Find(f => f.Body == fix.Body);
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
                currentFloor = new Floor(world, startDraw, endDraw);
                floors.Add(currentFloor);
                editingFloor = false;
            }
            else if (keyboard.IsKeyDown(Keys.Back) && currentFloor != null)
            {                                                       // Delete selected floor
                currentFloor.Body.Dispose();
                floors.Remove(currentFloor);
                currentFloor = null;
            }
            else if (keyboard.IsKeyDown(Keys.LeftControl))          // Save and load level
            {
                if (keyboard.IsKeyDown(Keys.S) && !prevKeyState.IsKeyDown(Keys.S))
                {
                    SaveLevel();
                }
                else if (keyboard.IsKeyDown(Keys.O) && !prevKeyState.IsKeyDown(Keys.O))
                {
                    LoadLevel();
                }
            }

            prevMouseState = mouse;
        }

        private void SaveLevel()
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(LEVEL_FILE, FileMode.Create)))
            {
                foreach (Floor floor in floors)
                {
                    writer.Write(floor.Scale.X);
                    writer.Write(floor.Body.Position.X);
                    writer.Write(floor.Body.Position.Y);
                    writer.Write(floor.Body.Rotation);
                }
            }
            Console.WriteLine("Saved");
        }

        private void LoadLevel()
        {
            floors.Clear();
            using (BinaryReader reader = new BinaryReader(File.Open(LEVEL_FILE, FileMode.Open)))
            {
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    float width = reader.ReadSingle();
                    Vector2 center = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    float rotation = reader.ReadSingle();
                    floors.Add(new Floor(world, width, center, rotation));
                }
            }
        }

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

            int width = GraphicsDevice.Viewport.Width;
            int height = GraphicsDevice.Viewport.Height;

            // Calculate camera location matrix
            Matrix view;
            if (editLevel)
                view = Matrix.CreateTranslation(new Vector3(screenOffset + cameraBounds.Center.ToVector2() - ConvertUnits.ToDisplayUnits(player.Body.Position), 0f));
            else
                view = Matrix.CreateTranslation(new Vector3(screenOffset + screenCenter - ConvertUnits.ToDisplayUnits(player.Body.Position), 0f));                

            // Draw player and floors
            spriteBatch.Begin(transformMatrix: view);
            spriteBatch.Draw(whiteRect, ConvertUnits.ToDisplayUnits(player.Body.Position), null, Color.Red, player.Body.Rotation, player.Origin, ConvertUnits.ToDisplayUnits(player.Scale), SpriteEffects.None, 0f);
			foreach (Floor item in floors)
                spriteBatch.Draw(whiteRect, ConvertUnits.ToDisplayUnits(item.Body.Position), null, Color.Azure, item.Body.Rotation, item.Origin, ConvertUnits.ToDisplayUnits(item.Scale), SpriteEffects.None, 0f);
            if (currentFloor != null)
                spriteBatch.Draw(whiteRect, ConvertUnits.ToDisplayUnits(currentFloor.Body.Position), null, Color.Green, currentFloor.Body.Rotation,
                    currentFloor.Origin, ConvertUnits.ToDisplayUnits(currentFloor.Scale + new Vector2(0, 0.2f)), SpriteEffects.None, 0f);
            if (editingFloor) {
                Vector2 dist = ConvertUnits.ToDisplayUnits(endDraw - startDraw);
                float rotation = (float)Math.Atan2(dist.Y, dist.X);
                Vector2 scale = new Vector2(dist.Length(), ConvertUnits.ToDisplayUnits(0.2f));
                Vector2 origin = new Vector2(0.5f, 0.5f);
                spriteBatch.Draw(whiteRect, ConvertUnits.ToDisplayUnits(startDraw) + dist / 2, null, Color.Azure, rotation, origin, scale, SpriteEffects.None, 0f);
            }
            spriteBatch.End();

			// Show paused screen if game is paused
            spriteBatch.Begin();
			if (paused)
            {
                float centerY = width / 2.0f - fontBig.MeasureString("Paused").X / 2.0f;
				spriteBatch.DrawString(fontBig, "Paused", new Vector2(height * 0.1f, centerY), Color.Yellow);
            }
			spriteBatch.End();

            base.Draw(gameTime);
		}
	}
}
