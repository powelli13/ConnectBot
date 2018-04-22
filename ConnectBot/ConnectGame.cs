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
        #region Class : Space
        /// <summary>
        /// Class to contain essential game space information.
        /// </summary>
        protected class Space
        {
            // Rectangle that represents the space
            private Rectangle rect;
            // Rectangle used to draw discs falling over time
            private Rectangle drawRect;
            private Texture2D discSprite;
            private int discColor = 0;
            private bool falling = false;

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

            public bool Falling
            {
                get
                {
                    return falling;
                }
                set
                {
                    falling = value;
                }
            }

            /// <summary>
            /// Constructor requires the rect placement in pixels.
            /// </summary>
            public Space(int x, int y)
            {
                rect = new Rectangle(x, y, spaceSize, spaceSize);

                // Draw rectangle starting y is the top most disc space for a column
                // Top buffer of board to edge of screen add space size to account for blue arrows
                drawRect = new Rectangle(x, topBuffer + spaceSize, spaceSize, spaceSize);
            }

            /// <summary>
            /// Draws disc to the screen, only draws if it has a disc.
            /// </summary>
            /// <param name="sb"></param>
            public void Draw(SpriteBatch sb, Dictionary<int, Texture2D> images)
            {
                if (discColor != 0)
                {
                    sb.Draw(images[discColor], drawRect, Color.Wheat);

                    if (drawRect.Y < rect.Y /*&& falling*/)
                    {
                        drawRect.Y += 8;
                    }

                    if (drawRect.Y >= rect.Y)
                    {
                        //falling = false;
                        drawRect.Y = rect.Y;
                    }
                }
            }
        }
        #endregion

        #region Class : BoardColumn
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
                // Fill the first available space with the given disc.
                for (int t = 0; t < numRows; t++)
                {
                    if (columnSpaces[t].DiscColor == 0)
                    {
                        columnSpaces[t].DiscColor = disc;
                        //columnSpaces[t].Falling = true;

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
            /// and all contained discs to the screen.
            /// </summary>
            /// <param name="sb"></param>
            /// <param name="images"></param>
            public void Draw(SpriteBatch sb, Dictionary<int, Texture2D> images, bool drawBlueArrow)
            {
                for (int i = 0; i < columnSpaces.Length; i++)
                {
                    columnSpaces[i].Draw(sb, images);
                }

                sb.Draw(columnHolder, columnHolderRect, Color.White);
                if (movable && drawBlueArrow)
                {
                    sb.Draw(blueArrow, blueArrowRect, Color.White);
                }
            }
        }
        #endregion

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
        /// Represents which players turn it is, 1 for black, -1 for red.
        /// </summary>
        protected int turn;
        const int black = 1;
        const int red = -1;

        protected int playerTurn;
        protected int botTurn;

        private double timeSinceLastMove;

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
        /// Returns multi dimensional array of integers
        /// representing the current state of the board.
        /// </summary>
        /// <returns></returns>
        public int[,] GetTextBoard()
        {
            int[,] retBoard = new int[numColumns, numRows];

            for (int c = 0; c < numColumns; c++)
            {
                for (int r = 0; r < numRows; r++)
                {
                    retBoard[c, r] = boardColumns[c].GetSpace(r);
                }
            }

            return retBoard;
        }

        /// <summary>
        /// Determines if a color won and reset if ther is a winner.
        /// </summary>
        public void CheckVictory()
        {
            // TODO display board after victory for a few seconds.
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
                        string winner = "Black";

                        if (first == 2)
                        {
                            winner = "Red";
                        }

                        VictoryConfirmed(first);
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
                        VictoryConfirmed(first);
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
                        VictoryConfirmed(first);
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
                        VictoryConfirmed(first);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Confirms victory to console and resets the game.
        /// </summary>
        /// <param name="winner"></param>
        /// // TODO play again menu?
        /// // TODO announce victory and prompt replay or something
        public void VictoryConfirmed(int winner)
        {
            string winnerColor = "Black";

            if (winner == 2)
            {
                winnerColor = "Red";
            }

            ResetGame();
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

            // Create and load space objects
            int xPos = sideBuffer;
            // Add space size added to account for blue arrow
            int yPos = topBuffer + spaceSize;
            
            for (int col = 0; col < numColumns; col++)
            {
                boardColumns[col] = new BoardColumn(xPos, yPos, imageDict[4], imageDict[3]);

                // Move to next column.
                xPos += spaceSize;
            }

            ResetGame();
            
            //TODO menu to decide which color bot plays
            playerTurn = black;
            botTurn = red;

            bot = new ConnectAI(botTurn);
            UpdateBotBoard();
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
            imageDict[-1] = Content.Load<Texture2D>("red_disc");
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
            // TODO Escape doesn't completed stop execution. 
            // TODO also AI needs to get stopped when the x button is clicked on the window.
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                if (bot != null)
                {
                    bot.Stop();
                }

                Exit();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.J))
            {
                if (bot != null)
                {
                    bot.Stop();
                }
            }

            timeSinceLastMove += gameTime.ElapsedGameTime.TotalSeconds;

            // TODO should this be a seperate method?
            // Handle user clicks that could be attempted moves.
            // Contain mouse will not allow clicks on a full column.
            lastMouseState = mouseState;
            mouseState = Mouse.GetState();
            Point mousePosition = new Point(mouseState.X, mouseState.Y);

            if (timeSinceLastMove > 0.5)
            {
                if (turn == playerTurn)
                {
                    //bot.AISelfTest();
                    for (int col = 0; col < numColumns; col++)
                    {
                        if (boardColumns[col].ContainMouse(mousePosition))
                        {
                            if (lastMouseState.LeftButton == ButtonState.Pressed &&
                                mouseState.LeftButton == ButtonState.Released)
                            {
                                bot.AISelfTest();
                                //Perform move and change turn.
                                boardColumns[col].SetSpace(playerTurn);
                                timeSinceLastMove = 0.0;

                                ChangeTurn();
                                CheckVictory();
                                UpdateBotBoard();
                            }
                        }
                    }
                }
                else if (turn == botTurn)
                {
                    bot.AISelfTest();
                    // TODO ensure bot doesn't cheat
                    int botMove = bot.Move();
                    boardColumns[botMove].SetSpace(botTurn);
                    timeSinceLastMove = 0.0;

                    ChangeTurn();
                    CheckVictory();
                }
            }
            
            base.Update(gameTime);
        }

        /// <summary>
        /// Updates the turn for the game.
        /// </summary>
        protected void ChangeTurn()
        {
            turn = (turn == black ? red : black);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // TODO wait a little while to show game end
            GraphicsDevice.Clear(Color.Wheat);
            spriteBatch.Begin();
            bool drawBlueArrow = false;

            if (timeSinceLastMove > 0.5)
            {
                drawBlueArrow = true;
            }

            for (int col = 0; col < numColumns; col++)
            {
                boardColumns[col].Draw(spriteBatch, imageDict, drawBlueArrow);
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

        /// <summary>
        /// Send the updated boardstate to the bot.
        /// </summary>
        protected void UpdateBotBoard()
        {
            int[,] botBoard = GetTextBoard();
            bot.UpdateBoard(botBoard, turn);
        }
    }
}
