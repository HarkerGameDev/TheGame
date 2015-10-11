using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Source
{
	/// <summary>
	/// This is the main type for the game.
    /// 
    /// IMPORTANT NOTES - PLEASE READ ALL:
    /// - (0,0) is in the top left for everything
    /// - 1 unit is 1 cm (look at GRAVIY)
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
		// Game related constants
		const int GRAVITY = 980;

		// Player related constants
		const int PLAYER_SPEED = 200;
        const int JUMP_VEL = 500;

		GraphicsDeviceManager graphics;     // this is always here
		SpriteBatch spriteBatch;            // so is this
		Texture2D whiteRect;                // useful for drawing rectangles
        SpriteFont font, fontBig;           // fonts need to always be generated and loaded using content manager

		bool paused;
        bool pausePressed;
        bool gameRunning;
		Player player;
		Random rand;
        double _gameTime;   //Total game time of play - high scores
        List<Rectangle> floors;

		class Player
		{
            public Rectangle Rect;
            public Vector2 Velocity;
            public bool Falling;

            public Player()
            {
                Rect = new Rectangle(100, 100, 20, 50);
                Velocity = new Vector2(0, 0);
                Falling = false;
            }
		}

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			// set game to fullscreen and match monitor resolution
			graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width / 2;
			graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height / 2;
			graphics.IsFullScreen = false;
			graphics.ApplyChanges();

			// initialize all global variables
			rand = new Random();
			paused = false;
            pausePressed = false;
            gameRunning = true;

            // initialize objects
            player = new Player();
            floors = new List<Rectangle>();
            floors.Add(new Rectangle(50, 300, 400, 50)); // TODO !!FOR TESTING ONLY!! - make a proper level file or random generation
            floors.Add(new Rectangle(50, 200, 200, 20));

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			// create a white rectangle by making a 1x1 pixel block. Size can be 
			// changed during drawing by scaling. Color is white because color masking
			// in spritebatch (Draw) uses White as the color to replace by the mask.
			whiteRect = new Texture2D(GraphicsDevice, 1, 1);
			whiteRect.SetData(new[] { Color.White });

			// Load fonts. Important note, fonts need to be loaded and created from the
			// content manager. Look up "monogame fonts" on the internet for more info.
			font = Content.Load<SpriteFont>("Score");
			fontBig = Content.Load<SpriteFont>("ScoreBig");
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
		/// Moves the player based on input from gamepad, if connected, or keyboard. Use
        /// velocity for physics based movement, and the player will be moved here accordingly
		/// </summary>
		/// <param name="deltaTime">Time since last call</param>
		private void MovePlayer(float deltaTime)
        {
            // use GamePadCapabilities to see if connected gamepad has required inputs
            GamePadCapabilities capabilities = GamePad.GetCapabilities(PlayerIndex.One);

            if (capabilities.IsConnected)
            {
                // Important, ALWAYS multiply speed by delta time when moving
                float speed = deltaTime * PLAYER_SPEED;

                // add deadzone and move. Note, Y axis is inverted
                GamePadState state = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);
                player.Rect.X += (int)(state.ThumbSticks.Left.X * speed);
                player.Rect.Y -= (int)(state.ThumbSticks.Left.Y * speed);
                if (state.IsButtonDown(Buttons.A) && !player.Falling)
                    player.Velocity.Y = -JUMP_VEL;
            }
            else // use keyboard
            {
                // again, ALWAYS scale anything that moves by deltaTime
                int speed = (int)(deltaTime * PLAYER_SPEED);

                KeyboardState state = Keyboard.GetState();
                if (state.IsKeyDown(Keys.Right) || state.IsKeyDown(Keys.D))
                    player.Rect.X += speed;
                if (state.IsKeyDown(Keys.Left) || state.IsKeyDown(Keys.A))
                    player.Rect.X -= speed;
                if (!player.Falling && (state.IsKeyDown(Keys.Up) || state.IsKeyDown(Keys.W)))
                    player.Velocity.Y = -JUMP_VEL;
            }

            // for now, use Velocity only for physics related things (like gravity)
            player.Rect.Offset(player.Velocity * deltaTime);
		}

        /// <summary>
        /// Check collisions to the floors/ ceilings
        /// </summary>
        private void CheckFloors()
        {
            player.Falling = true;
            foreach (Rectangle rect in floors)
            {
                if (player.Rect.Intersects(rect))
                {
                    
                    
                    // check if floor is a ceiling or a floor
                    if (player.Rect.Center.Y < rect.Top)
                    {
                        player.Velocity.Y = 0;
                        player.Rect.Y = rect.Top - player.Rect.Height;
                        player.Falling = false;
                    }
                    else if(player.Rect.Center.Y > rect.Bottom)
                    {
                        player.Velocity.Y = 0;
                        player.Rect.Y = rect.Bottom;
                    }// check sideways collisions
                    else if (player.Rect.Center.X < rect.Left)
                    {
                        player.Velocity.X = 0;
                        player.Rect.X = rect.Left-player.Rect.Width;
                    }
                    else if (player.Rect.Center.X > rect.Right)
                    {
                        player.Velocity.X = 0;
                        player.Rect.X = rect.Right;
                    }

                    

                }
            }
        }

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// ALWAYS do this
			float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Handle end game
            // TODO put this in pause menu later
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

            // Handle toggle pause
            // TODO open pause menu
            if (Keyboard.GetState().IsKeyDown(Keys.Space) || GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed)
            {
                if (!pausePressed)
                {
                    paused = !paused;
                    pausePressed = true;
                }
            }
            else
            {
                pausePressed = false;
            }

			// This is pretty much how pause always works
			if (!paused && gameRunning) {
				_gameTime += deltaTime;

                player.Velocity.Y += GRAVITY * deltaTime;
                MovePlayer(deltaTime);

                CheckFloors();      // do this after moving the player so it doesn't look like the player is in the floor
			}

			base.Update(gameTime);
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

			spriteBatch.Begin();

			// Draw the player
			spriteBatch.Draw(whiteRect, player.Rect, Color.Green);

			// Draw the floors
			foreach (Rectangle rect in floors)
			{
                spriteBatch.Draw(whiteRect, rect, Color.Azure);
			}

			// Show paused screen if game is paused
			if (paused)
            {
                float centerY = width / 2.0f - fontBig.MeasureString("Paused").X / 2.0f;
				spriteBatch.DrawString(fontBig, "Paused", new Vector2(height * 0.1f, centerY), Color.Yellow);
            }

			spriteBatch.End();
			base.Draw(gameTime);
		}

        ///// <summary>
        ///// Dynamically creates a circle with given radius
        ///// </summary>
        ///// <param name="radius">Desired radius of circle</param>
        ///// <returns>A Texture2D circle colored white for masking</returns>
        //private Texture2D CreateCircle(int radius)
        //{
        //    int outerRadius = radius * 2 + 2; // So circle doesn't go out of bounds
        //    Texture2D texture = new Texture2D(GraphicsDevice, outerRadius, outerRadius);

        //    Color[] data = new Color[outerRadius * outerRadius];

        //    // Colour the entire texture transparent first.
        //    for (int i = 0; i < data.Length; i++)
        //        data[i] = Color.Transparent;

        //    // Work out the minimum step necessary using trigonometry + sine approximation.
        //    double angleStep = 1f / radius;

        //    for (double angle = 0; angle < Math.PI; angle += angleStep)
        //    {
        //        // Use the parametric definition of a circle: http://en.wikipedia.org/wiki/Circle#Cartesian_coordinates
        //        int x = (int)Math.Round(radius + radius * Math.Cos(angle));
        //        int y = (int)Math.Round(radius + radius * Math.Sin(angle));

        //        // Fill in the pixels in between the points. I am using vertical fills here
        //        for (int i=radius*2 - y; i<=y; i++)
        //            data[i * outerRadius + x + 1] = Color.White;
        //    }

        //    texture.SetData(data);
        //    return texture;
        //}
	}
}
