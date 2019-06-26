using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectBot
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class ConnectGame : Game
    {
        /// <summary>
        /// Spacing display constants.
        /// </summary>
        const int SpaceSize = 80;
        const int XBoardBuffer = 80;
        const int YBoardBuffer = 100;

        const int numRows = 6;
        const int numColumns = 7;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        /// <summary>
        /// Dictionary of int to texture for easily grabbing
        /// repeating displayable images.
        /// </summary>
        Dictionary<string, Texture2D> imageDict;

        /// <summary>
        /// Array of board column objects, contains game board state
        /// and drawing functionality.
        /// </summary>
        Board.BoardColumn[] boardColumns = new Board.BoardColumn[numColumns];

        /// <summary>
        /// Represents which players turn it is, 1 for black, -1 for red.
        /// For this games purposes as of now black will go first.
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
        /// Enum used to keep track of which menu is up if any.
        /// </summary>
        enum MenuState
        {
            None,
            PlayAgain,
            SelectColor
        };
        private MenuState currentMenu;
            
        /// <summary>
        /// Bot to play against.
        /// </summary>
        private ConnectAI bot;

        /// <summary>
        /// Menus classes to handle various inputs.
        /// </summary>
        private GameMenus.PlayAgainMenu playAgainMenu;

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

        // TODO this should move to logical board
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
        protected void VictoryConfirmed(int winner)
        {
            string winnerColor = "Black";

            if (winner == 2)
            {
                winnerColor = "Red";
            }

            ShowPlayAgainMenu();
            
        }

        /// <summary>
        /// Display the play again menu sprites.
        /// </summary>
        protected void ShowPlayAgainMenu()
        {

            currentMenu = MenuState.PlayAgain;

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
            // TODO figure out how to resize the window. looks terrible on my new laptop
            IsMouseVisible = true;

            // Create and load space objects
            int xPos = XBoardBuffer;

            // Add space size added to account for blue arrow
            int yPos = YBoardBuffer + SpaceSize;

            for (int col = 0; col < numColumns; col++)
            {
                boardColumns[col] = new Board.BoardColumn(xPos, yPos, imageDict["column_holder"], imageDict["blue_arrow"]);

                // Move to next column.
                xPos += SpaceSize;
            }

            ResetGame();
            
            //TODO menu to decide which color bot plays
            playerTurn = black;
            botTurn = red;

            // TODO what to pass as column here? how to show null move at board start?
            // TODO how to signify to AI that they should build for specific color does root node determine it?
            bot = new ConnectAI(botTurn, GetTextBoard(), -1, turn);
            UpdateBotBoard(-1);

            playAgainMenu = new GameMenus.PlayAgainMenu();

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
            imageDict = new Dictionary<string, Texture2D>();

            // TODO could this be an enum so that strings do not get misspelled? make it namespace wide so that other classes can use it
            string[] imageNames = {
                "black_disc",
                "red_disc",
                "blue_arrow",
                "column_holder",
                "play_again_background",
                "play_again_question",
                "yes_button",
                "no_button"
            };

            foreach (string img in imageNames)
            {
                imageDict[img] = Content.Load<Texture2D>(img);
            }

            //imageDict[1] = Content.Load<Texture2D>("black_disc");
            //imageDict[-1] = Content.Load<Texture2D>("red_disc");
            //imageDict[3] = Content.Load<Texture2D>("blue_arrow");
            //imageDict[4] = Content.Load<Texture2D>("column_holder");

            //imageDict[5] = Content.Load<Texture2D>("play_again_background");
            //imageDict[6] = Content.Load<Texture2D>("play_again_question");
            //imageDict[7] = Content.Load<Texture2D>("yes_button");
            //imageDict[8] = Content.Load<Texture2D>("no_button");
            
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
                EndGame();
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
            bool botMoveRunning = false;
            
            switch (currentMenu)
            {
                case MenuState.None:
                    // Check for movement clicks
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
                                        //bot.AISelfTest();
                                        //Perform move and change turn.
                                        boardColumns[col].SetSpace(playerTurn);
                                        timeSinceLastMove = 0.0;

                                        ChangeTurn();
                                        CheckVictory();
                                        UpdateBotBoard(col);
                                    }
                                }
                            }
                        }
                        else if (turn == botTurn)
                        {
                            //bot.AISelfTest();
                            // TODO ensure bot makes valie move

                            //int bfotMove = bot.Move();
                            // TODO this is definitely not the way to do this
                            if (!botMoveRunning)
                            {
                                Task<int> botMove = bot.Move();
                            }

                            //boardColumns[botMove].SetSpace(botTurn);
                            //timeSinceLastMove = 0.0;

                            //ChangeTurn();
                            //CheckVictory();
                        }
                    }

                    break;

                case MenuState.PlayAgain:

                    // Detect a click on either of the buttons and respond accordingly.
                    if (lastMouseState.LeftButton == ButtonState.Pressed &&
                            mouseState.LeftButton == ButtonState.Released)
                    {
                        if (playAgainMenu.YesButtonContainsMouse(mousePosition))
                        {
                            ResetGame();
                            break;
                        }
                        else if (playAgainMenu.NoButtonContainsMouse(mousePosition))
                        {
                            EndGame();
                            break;
                        }
                    }

                    break;
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

            switch (currentMenu)
            {
                case MenuState.PlayAgain:

                    playAgainMenu.Draw(spriteBatch, imageDict);

                    break;
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
            currentMenu = MenuState.None;

            for (int c = 0; c < numColumns; c++)
            {
                boardColumns[c].ResetSpaces();
            }
        }

        /// <summary>
        /// Clean up resources and exit the game.
        /// </summary>
        protected void EndGame()
        {
            if (bot != null)
            {
                bot.Stop();
            }

            Exit();
        }

        /// <summary>
        /// Send the updated boardstate to the bot.
        /// </summary>
        protected void UpdateBotBoard(int columnMoved)
        {
            // TODO this should probably be moved to the bot class
            int[,] botBoard = GetTextBoard();
            bot.UpdateBoard(botBoard, turn, columnMoved);
        }
    }
}
