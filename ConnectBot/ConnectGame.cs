using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

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
        /// Column to hold spaces and column animations.
        /// Handles click detection for moves in a column.
        /// </summary>
        protected class BoardColumn
        {
            private Texture2D blueArrow;
            private Texture2D columnHolder;
            private Space[] columnSpaces;

            /// <summary>
            /// Determines if the column can still be played in.
            /// </summary>
            private bool movable;

            // Rectangles for the blue arrow and column holder.
            private Rectangle columnHolderRect;
            private Rectangle blueArrowRect;

            /// <summary>
            /// Constructor for the column container and click handler.
            /// </summary>
            /// <param name="x">x position in pixels of rectangle</param>
            /// <param name="y">y position in pixels of rectangle</param>
            /// <param name="ch">sprite of column holder</param>
            /// <param name="ba">sprite of blue arrow</param>
            public BoardColumn(int x, int y, Texture2D ch, Texture2D ba)
            {
                // Create rectangles and save sprites.
                blueArrowRect = new Rectangle(x, y - spaceSize, spaceSize, spaceSize);
                columnHolderRect = new Rectangle(x, y, spaceSize, spaceSize * numRows);
                
                blueArrow = ba;
                columnHolder = ch;

                // Initialize spaces in column.
                columnSpaces = new Space[6];

                int yPos = y + numRows * spaceSize;
                
                for (int row = 0; row < numRows; row++)
                {
                    // Add space and move up column.
                    // Subtract first because x, y in constructor are the top left of rectangle TODO is this thie case?
                    // Move up to ensure that we appear inside column container.
                    yPos -= spaceSize;
                    columnSpaces[row] = new Space(x, yPos);
                }

                movable = true;
            }

            /// <summary>
            /// Sets the spaces at rowIndex in the column to be disc.
            /// </summary>
            /// <param name="disc"></param>
            public void SetSpace(int disc)
            {
                // TODO begin animation drop sequence
                // TODO determine pixel drop height for animation
                // Fill the first available space with the given disc.
                for (int t = 0; t < numRows; t++)
                {
                    if (columnSpaces[t].DiscColor == 0)
                    {
                        columnSpaces[t].DiscColor = disc;

                        // Determine if column is full or still movable.
                        if (t == numRows - 1)
                        {
                            movable = false;
                        }

                        return;
                    }
                }
            }

            /// <summary>
            /// Retrieve the disc color at a given row.
            /// </summary>
            /// <param name="row"></param>
            /// <returns></returns>
            public int GetSpace(int row)
            {
                return columnSpaces[row].DiscColor;
            }

            /// <summary>
            /// Clears the spaces in the column.
            /// </summary>
            public void ResetSpaces()
            {
                for (int r = 0; r < numRows; r++)
                {
                    columnSpaces[r].DiscColor = 0;
                }

                movable = true;
            }
            
            /// <summary>
            /// Determines if the column contians the given mouse point.
            /// </summary>
            /// <param name="p">Point representing mouse's location.</param>
            /// <returns>True if the column is clickable and contains point.</returns>
            public bool ContainMouse(Point p)
            {
                if (movable)
                {
                    if (columnHolderRect.Contains(p) || blueArrowRect.Contains(p))
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// Draws column holder, blue arrow if applicable
            /// and all contained spaces to the screen.
            /// </summary>
            /// <param name="sb"></param>
            /// <param name="images"></param>
            public void Draw(SpriteBatch sb, Dictionary<int, Texture2D> images)
            {
                for (int i = 0; i < columnSpaces.Length; i++)
                {
                    columnSpaces[i].Draw(sb, images);
                }

                sb.Draw(columnHolder, columnHolderRect, Color.White);
                if (movable)
                {
                    sb.Draw(blueArrow, blueArrowRect, Color.White);
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

        /// <summary>
        /// Array of board column objects, contains game board state
        /// and drawing functionality.
        /// </summary>
        BoardColumn[] boardColumns = new BoardColumn[numColumns];

        /// <summary>
        /// Represents which players turn it is, 1 for black, 2 for red.
        /// </summary>
        protected int turn;
        protected int playerTurn;
        protected int botTurn;

        /// <summary>
        /// Used to handle click inputs.
        /// </summary>
        private MouseState mouseState;
        private MouseState lastMouseState;

        /// <summary>
        /// Bot to play against.
        /// </summary>
        private ConnectAI bot;

        public ConnectGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 900;
            graphics.PreferredBackBufferHeight = 740;
            graphics.ApplyChanges();
            
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Return array of integers representing board without buffers.
        /// The first six elements are the first column, 
        /// each section of six in the reduced board is a column.
        /// The bottom of the columns comes first.
        /// </summary>
        /// <returns></returns>
        /// TODO rename also the slim board is probably not worth the hassle, make it mutli-dimensional
        public int[] GetBufferedBoard()
        {
            int[] retBoard = new int[42];
            int ix = 0;

            for (int c = 0; c < numColumns; c++)
            {
                for (int r = numRows - 1; r > -1; r--)
                {
                    retBoard[ix] = boardColumns[c].GetSpace(r);
                    ix++;
                }
            }

            return retBoard;
        }

        /// <summary>
        /// Determines if a color won and reset if ther is a winner.
        /// </summary>
        public void CheckVictory()
        {
            // when checking up add 1, 2, 3 to rows
            //      check up on 0 - 6 columns
            //      check up on 0 - 2 rows
            // when checking right add 1, 2, 3, to columns
            //      check right on 0 - 3 columns
            //      check right on 0 - 6 rows
            // when checking up left subtract 1 from cols, add 1 to rows
            //      check up left on columns 3 - 6
            //      check up right on rows 0 - 2
            // when checking up right add 1 to cols, add 1 to rows
            //      check up right on columns 0 - 3
            //      check up right on rows 0 - 2
            // TODO could only check when latest move is in the mix
            int first;
            int second;
            int third;
            int fourth;
            
            // check verticals
            for (int chkUpCol = 0; chkUpCol < numColumns; chkUpCol++)
            {
                for (int chkUpRow = 0; chkUpRow < 3; chkUpRow++)
                {
                    first = boardColumns[chkUpCol].GetSpace(chkUpRow);
                    second = boardColumns[chkUpCol].GetSpace(chkUpRow + 1);
                    third = boardColumns[chkUpCol].GetSpace(chkUpRow + 2);
                    fourth = boardColumns[chkUpCol].GetSpace(chkUpRow + 3);

                    if (first != 0 &&
                        first == second &&
                        first == third &&
                        first == fourth)
                    {
                        System.Console.WriteLine("Player {0} has won! Restarting.", first);
                        ResetGame();
                        return;
                    }
                }
            }

            // check horizontals
            for (int chkCrossCol = 0; chkCrossCol < 4; chkCrossCol++)
            {
                for (int chkCrossRow = 0; chkCrossRow < numRows; chkCrossRow++)
                {
                    first = boardColumns[chkCrossCol].GetSpace(chkCrossRow);
                    second = boardColumns[chkCrossCol + 1].GetSpace(chkCrossRow);
                    third = boardColumns[chkCrossCol + 2].GetSpace(chkCrossRow);
                    fourth = boardColumns[chkCrossCol + 3].GetSpace(chkCrossRow);

                    if (first != 0 &&
                        first == second &&
                        first == third &&
                        first == fourth)
                    {
                        System.Console.WriteLine("Player {0} has won! Restarting.", first);
                        ResetGame();
                        return;
                    }
                }
            }

            // check left diagonal
            for (int chkLDiagCol = 3; chkLDiagCol < numColumns; chkLDiagCol++)
            {
                for (int chkLDiagRow = 0; chkLDiagRow < 3; chkLDiagRow++)
                {
                    first = boardColumns[chkLDiagCol].GetSpace(chkLDiagRow);
                    second = boardColumns[chkLDiagCol - 1].GetSpace(chkLDiagRow + 1);
                    third = boardColumns[chkLDiagCol - 2].GetSpace(chkLDiagRow + 2);
                    fourth = boardColumns[chkLDiagCol - 3].GetSpace(chkLDiagRow + 3);

                    if (first != 0 &&
                        first == second &&
                        first == third &&
                        first == fourth)
                    {
                        System.Console.WriteLine("Player {0} has won! Restarting.", first);
                        ResetGame();
                        return;
                    }
                }
            }

            // check right diagonal
            for (int chkRDiagCol = 0; chkRDiagCol < 4; chkRDiagCol++)
            {
                for (int chkRDiagRow = 0; chkRDiagRow < 3; chkRDiagRow++)
                {
                    first = boardColumns[chkRDiagCol].GetSpace(chkRDiagRow);
                    second = boardColumns[chkRDiagCol + 1].GetSpace(chkRDiagRow + 1);
                    third = boardColumns[chkRDiagCol + 2].GetSpace(chkRDiagRow + 2);
                    fourth = boardColumns[chkRDiagCol + 3].GetSpace(chkRDiagRow + 3);

                    if (first != 0 &&
                        first == second &&
                        first == third &&
                        first == fourth)
                    {
                        System.Console.WriteLine("Player {0} has won! Restarting.", first);
                        ResetGame();
                        return;
                    }
                }
            }

            //System.Console.WriteLine("No winner yet, keep playing!");
            // TODO play again menu?
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
            int xPos = sideBuffer;
            // space size added to account for blue arrow
            int yPos = topBuffer + spaceSize;
            
            for (int col = 0; col < numColumns; col++)
            {
                boardColumns[col] = new BoardColumn(xPos, yPos, imageDict[4], imageDict[3]);

                // Move to next column.
                xPos += spaceSize;
            }

            ResetGame();

            int[] botBoard = GetBufferedBoard();

            //TODO menu to decide which color bot plays
            playerTurn = 1;
            botTurn = 2;

            bot = new ConnectAI(botTurn);
            bot.UpdateBoard(botBoard);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Populate image dictionary
            imageDict = new Dictionary<int, Texture2D>();

            imageDict[1] = Content.Load<Texture2D>("black_disc");
            imageDict[2] = Content.Load<Texture2D>("red_disc");
            imageDict[3] = Content.Load<Texture2D>("blue_arrow");
            imageDict[4] = Content.Load<Texture2D>("column_holder");
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
            {
                Exit();
            }

            // Handle user clicks that could be on columns.
            lastMouseState = mouseState;
            mouseState = Mouse.GetState();
            Point mousePosition = new Point(mouseState.X, mouseState.Y);

            //if (squares[i].rect.Contains(mousePosition) &&
            //    lastMouseState.LeftButton == ButtonState.Pressed &&
            //    mouseState.LeftButton == ButtonState.Released)

            // TODO clicks
            // iterate columns
            // if column contains mouse pos and is clickable
            // if click
            // send click to column with turn
            if (turn == playerTurn)
            {
                for (int col = 0; col < numColumns; col++)
                {
                    if (boardColumns[col].ContainMouse(mousePosition))
                    {
                        if (lastMouseState.LeftButton == ButtonState.Pressed &&
                            mouseState.LeftButton == ButtonState.Released)
                        {
                            // Perform move and change turn.
                            boardColumns[col].SetSpace(playerTurn);
                            turn = (turn == 1 ? 2 : 1);
                            CheckVictory();
                        }
                    }
                }
            }
            else if (turn == botTurn)
            {
                // TODO ensure bot doesn't cheat 
                int botMove = bot.Move();
                boardColumns[botMove].SetSpace(botTurn);
                turn = (turn == 1 ? 2 : 1);
                CheckVictory();
            }


            
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
                boardColumns[col].Draw(spriteBatch, imageDict);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Resets game board and state.
        /// </summary>
        protected void ResetGame()
        {
            turn = 1;

            for (int c = 0; c < numColumns; c++)
            {
                boardColumns[c].ResetSpaces();
            }
        }
    }
}
