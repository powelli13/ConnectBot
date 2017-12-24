using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace ConnectBot
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class ConnectGame : Game
    {
        /// <summary>
        /// Class to contain essential game space information.
        /// </summary>
        protected class Space
        {

            private Rectangle rect;
            private Texture2D discSprite;
            private int discColor = 0;

            public int DiscColor
            {
                get
                {
                    return discColor;
                }
                set
                {
                    discColor = value;
                }
            }
            
            /// <summary>
            /// Constructor requires the rect placement in pixels.
            /// </summary>
            public Space(int x, int y)
            {
                rect = new Rectangle(x, y, spaceSize, spaceSize);
            }

            /// <summary>
            /// Draws disc to the screen, only draws if it has a disc.
            /// </summary>
            /// <param name="sb"></param>
            public void Draw(SpriteBatch sb, Dictionary<int, Texture2D> images)
            {
                if (discColor != 0)
                {
                    sb.Draw(images[discColor], rect, Color.Wheat);
                }
            }
        }

        /// <summary>
        /// Spacing display constants.
        /// </summary>
        const int spaceSize = 80;
        const int sideBuffer = 80;
        const int topBuffer = 100;

        const int numRows = 6;
        const int numColumns = 7;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        /// <summary>
        /// Dictionary of int to texture for easily grabbing
        /// repeating displayable images.
        /// </summary>
        Dictionary<int, Texture2D> imageDict;
        Space[][] spaces = new Space[][] {
            new Space[6],
            new Space[6],
            new Space[6],
            new Space[6],
            new Space[6],
            new Space[6],
            new Space[6]
        };

        ConnectLogic gameLogic;

        public ConnectGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 900;
            graphics.PreferredBackBufferHeight = 740;
            graphics.ApplyChanges();
            
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
            base.Initialize();
            
            IsMouseVisible = true;

            // create and load space objects
            int xPos = 0;
            int yPos = numRows * spaceSize;
            
            for (int col = 0; col < numColumns; col++)
            {
                for (int row = 0; row < numRows; row++)
                {
                    spaces[col][row] = new Space(xPos + sideBuffer, yPos + topBuffer);
                    // move up the column
                    yPos -= spaceSize;
                }

                // after drawing rows move over a column
                xPos += spaceSize;
                yPos = numRows * spaceSize;
            }

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //blackDisc = Content.Load<Texture2D>("black_disc");
            //redDisc = Content.Load<Texture2D>("red_disc");
            //blueArrow = Content.Load<Texture2D>("blue_arrow");
            // Populate image dictionary
            imageDict = new Dictionary<int, Texture2D>();

            imageDict[1] = Content.Load<Texture2D>("black_disc");
            imageDict[2] = Content.Load<Texture2D>("red_disc");
            imageDict[3] = Content.Load<Texture2D>("blue_arrow");

            gameLogic = new ConnectLogic();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            //TODO 
            int[][] gameState = gameLogic.GetBoardState();
            
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Wheat);
            
            spriteBatch.Begin();

            for (int col = 0; col < numColumns; col++)
            {
                for (int row = 0; row < numRows; row++)
                {
                    spaces[col][row].Draw(spriteBatch, imageDict);
                }
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
