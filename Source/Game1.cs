using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;

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
		private Texture2D whiteRect;
        private SpriteFont font, fontBig;

        private Random rand;
        private World world;

        private const float CAMERA_SCALE = 30f;
        private const float MAX_CAMERA_SPEED_X = 6f;
        private const float MAX_CAMERA_SPEED_Y = 4f;
        private Rectangle cameraBounds;
        private Vector2 screenCenter;

		private bool paused;
		private Player player;
        private List<Floor> floors;
        
		class Floor
		{
            public Body Body;
            public Vector2 Origin;
            public Vector2 Scale;

            protected Floor()
            {
                Origin = new Vector2(0.5f, 0.5f);
            }
            
            public Floor(World world, Vector2 position1, Vector2 position2)
            {
                Vector2 dist = position2 - position1;
                float width = dist.Length();
                float height = 0.2f;
                Vector2 center = position1 + dist / 2;
                float rotation = (float)Math.Atan2(dist.Y, dist.X);

                Body = BodyFactory.CreateRectangle(world, width, height, 1f, center, FLOOR);
                Scale = ConvertUnits.ToDisplayUnits(new Vector2(width, height));
                Origin = new Vector2(0.5f, 0.5f);

                Body.BodyType = BodyType.Static;
                Body.IsStatic = true;
                Body.FixedRotation = true;
                Body.Rotation = rotation;
                Body.Friction = 0.7f;
                Body.Restitution = 0.1f;    // Bounciness. Everything is ever so slightly bouncy so it doesn't feel like a rock with VHB tape.
            }
		}

        class Player : Floor
        {
            public bool CanJump
            {
                get
                {
                    return Collisions > 0 && JumpWait < 0;
                }
            }
            private int Collisions;
            public double JumpWait;

            public Player(World world) : base()
            {
                float width = 0.8f;
                float height = 1.8f;
                Body = BodyFactory.CreateCapsule(world, height - width, width / 2, 1f, PLAYER);
                Body.Position = new Vector2(3f, 8f);
                Scale = ConvertUnits.ToDisplayUnits(new Vector2(width, height));

                //Falling = true;
                Collisions = 0;
                JumpWait = 0;

                Fixture foot = FixtureFactory.AttachRectangle(1f, 0.3f, 0f, new Vector2(0f, 0.8f), Body);
                foot.IsSensor = true;
                foot.OnCollision += YesJump;
                foot.OnSeparation += NoJump;

                Body.BodyType = BodyType.Kinematic;
                Body.IsStatic = false;
                Body.FixedRotation = true;
                Body.Friction = 0f;     // Friction is handled for the player individually because it decreases speed otherwise
            }

            private bool YesJump(Fixture f1, Fixture f2, Contact contact)
            {
                Collisions++;
                //Falling = false;
                return true;
            }

            private void NoJump(Fixture f1, Fixture f2)
            {
                Collisions--;
                //if (Collisions <= 0)
                //    Falling = true;
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

            // Initialize previous keyboard and gamepad states
            prevKeyState = new KeyboardState();
            prevPadState = new GamePadState();

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
            cameraBounds = new Rectangle((int)(width * 0.15), (int)(height * 0.25), (int)(width * 0.3), (int)(height * 0.5));
            screenCenter = cameraBounds.Center.ToVector2();

			// Use this to draw rectangles
			whiteRect = new Texture2D(GraphicsDevice, 1, 1);
			whiteRect.SetData(new[] { Color.White });

			// Load fonts using Content Manager
			font = Content.Load<SpriteFont>("Score");
			fontBig = Content.Load<SpriteFont>("ScoreBig");

            // Farseer level hard-coding
            // TODO load some file instead of hardcoding
            floors.Add(new Floor(world, new Vector2(1f, 15f), new Vector2(29f, 15f)));
            floors.Add(new Floor(world, new Vector2(1f, 9f), new Vector2(12f, 9f)));
            floors.Add(new Floor(world, new Vector2(18f, 9f), new Vector2(29f, 9f)));
            floors.Add(new Floor(world, new Vector2(8f, 13f), new Vector2(15f, 10f)));
            floors.Add(new Floor(world, new Vector2(1f, 3f), new Vector2(12f, 3f)));
            floors.Add(new Floor(world, new Vector2(18f, 3f), new Vector2(29f, 3f)));
            floors.Add(new Floor(world, new Vector2(8f, 7f), new Vector2(15f, 4f)));
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

            //float impulse = 0.5f * (2 - Math.Abs(player.Body.LinearVelocity.X) / MAX_VELOCITY);
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
                    player.Body.ApplyLinearImpulse(new Vector2(Math.Sign(player.Body.LinearVelocity.X) * -SLOWDOWN, 0f));
            }
            if (state.IsKeyDown(Keys.Up) && player.CanJump)
            {
                player.JumpWait = JUMP_WAIT;
                player.Body.ApplyLinearImpulse(new Vector2(0f, -13f));
            }

            //screenCenter.X = MathHelper.SmoothStep(cameraBounds.Left, cameraBounds.Right, player.Body.LinearVelocity.X / MAX_VELOCITY / 2f + 0.5f);
            //screenCenter.Y = MathHelper.SmoothStep(cameraBounds.Top, cameraBounds.Bottom, player.Body.LinearVelocity.Y / MAX_VELOCITY / 2f + 0.5f);

            // TODO ever so slight camera shake when going fast
            float deltaX = ((cameraBounds.Center.X - screenCenter.X) / cameraBounds.Width - player.Body.LinearVelocity.X / MAX_VELOCITY) * CAMERA_SCALE;
            deltaX = MathHelper.Clamp(deltaX, -MAX_CAMERA_SPEED_X, MAX_CAMERA_SPEED_X);
            screenCenter.X += deltaX;
            screenCenter.X = MathHelper.Clamp(screenCenter.X, cameraBounds.Left, cameraBounds.Right);
            //Console.WriteLine(string.Format("deltaX: {0}", deltaX));

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
            Matrix view = Matrix.CreateTranslation(new Vector3(screenCenter - ConvertUnits.ToDisplayUnits(player.Body.Position), 0f));

            // Draw player and floors
            spriteBatch.Begin(transformMatrix: view);
            spriteBatch.Draw(whiteRect, ConvertUnits.ToDisplayUnits(player.Body.Position), null, Color.Red, player.Body.Rotation, player.Origin, player.Scale, SpriteEffects.None, 0f);
			foreach (Floor item in floors)
			{
                spriteBatch.Draw(whiteRect, ConvertUnits.ToDisplayUnits(item.Body.Position), null, Color.Azure, item.Body.Rotation, item.Origin, item.Scale, SpriteEffects.None, 0f);
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
