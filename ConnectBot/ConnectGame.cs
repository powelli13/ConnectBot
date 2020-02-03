using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
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
        Board.BoardColumn[] boardColumns = new Board.BoardColumn[LogicalBoardHelpers.NUM_COLUMNS];

        /// <summary>
        /// Represents which players turn it is, 1 for black, -1 for red.
        /// For this games purposes as of now black will go first.
        /// </summary>
        protected int CurrentTurn { get; set; }
        

        protected int PlayerTurn { get; set; }
        protected int BotTurn { get; set; }

        private double timeSinceLastMove;

        /// <summary>
        /// Used to handle click inputs.
        /// </summary>
        private MouseState MouseState { get; set; }
        private MouseState LastMouseState { get; set; }

        /// <summary>
        /// Enum used to keep track of which menu is up if any.
        /// </summary>
        enum MenuState
        {
            None,
            PlayAgain,
            SelectColor
        };

        private MenuState CurrentMenu { get; set; }

        private ConnectAI Bot { get; set; }

        // TODO remove this and check the thread result
        private bool botThinking = false;

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
            int[,] retBoard = new int[LogicalBoardHelpers.NUM_COLUMNS, LogicalBoardHelpers.NUM_ROWS];

            for (int c = 0; c < LogicalBoardHelpers.NUM_COLUMNS; c++)
            {
                for (int r = 0; r < LogicalBoardHelpers.NUM_ROWS; r++)
                {
                    retBoard[c, r] = boardColumns[c].GetSpace(r);
                }
            }

            return retBoard;
        }

        protected void VictoryConfirmed(int winner)
        {
            if (winner != 0)
                ShowPlayAgainMenu();
        }

        protected void ShowPlayAgainMenu()
        {
            CurrentMenu = MenuState.PlayAgain;
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

            for (int col = 0; col < LogicalBoardHelpers.NUM_COLUMNS; col++)
            {
                boardColumns[col] = new Board.BoardColumn(
                    xPos, 
                    yPos, 
                    imageDict[ImageNames.COLUMN_HOLDER], 
                    imageDict[ImageNames.BLUE_ARROW]);

                // Move to next column.
                xPos += SpaceSize;
            }

            ResetGame();
            
            //TODO menu to decide which color bot plays
            PlayerTurn = LogicalBoardHelpers.DISC_COLOR_BLACK;
            BotTurn = LogicalBoardHelpers.DISC_COLOR_RED;

            // TODO what to pass as column here? how to show null move at board start?
            // TODO how to signify to AI that they should build for specific color does root node determine it?
            Bot = new ConnectAI(BotTurn);
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

            foreach (string img in ImageNames.AllImageNames)
            {
                imageDict[img] = Content.Load<Texture2D>(img);
            }
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
                //if (bot != null)
                //{
                //    bot.Stop();
                //}
            }

            timeSinceLastMove += gameTime.ElapsedGameTime.TotalSeconds;

            // TODO should this be a seperate method?
            // Handle user clicks that could be attempted moves.
            // Contain mouse will not allow clicks on a full column.
            LastMouseState = MouseState;
            MouseState = Mouse.GetState();
            Point mousePosition = new Point(MouseState.X, MouseState.Y);
            //Task<int> botMove;
            //bool botMoveRunning = false;

            switch (CurrentMenu)
            {
                case MenuState.None:
                    // Check for movement clicks
                    if (timeSinceLastMove > 0.5)
                    {
                        if (CurrentTurn == PlayerTurn)
                        {
                            //bot.AISelfTest();
                            for (int col = 0; col < LogicalBoardHelpers.NUM_COLUMNS; col++)
                            {
                                if (boardColumns[col].ContainMouse(mousePosition))
                                {
                                    if (LastMouseState.LeftButton == ButtonState.Pressed &&
                                        MouseState.LeftButton == ButtonState.Released)
                                    {
                                        //bot.AISelfTest();
                                        //Perform move and change turn.
                                        boardColumns[col].SetSpace(PlayerTurn);
                                        timeSinceLastMove = 0.0;

                                        ChangeTurn();
                                        int winner = LogicalBoardHelpers.CheckVictory(GetTextBoard());
                                        VictoryConfirmed(winner);

                                        UpdateBotBoard(col);
                                    }
                                }
                            }
                        }
                        else if (CurrentTurn == BotTurn)
                        {
                            //bot.AISelfTest();

                            if (!botThinking)
                            {
                                botThinking = true;
                                GetBotMove();
                            }
                        }
                    }
                    break;

                case MenuState.PlayAgain:

                    // Detect a click on either of the buttons and respond accordingly.
                    if (LastMouseState.LeftButton == ButtonState.Pressed &&
                            MouseState.LeftButton == ButtonState.Released)
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

        protected async void GetBotMove()
        {
            // TODO double check that this is necessary. the commented await seems to block momentarily sometimes
            //int botMove = await Bot.Move();
            int botMove = -1;
            await Task.Run(() => botMove = Bot.Move().Result);

            // TODO ensure bot made valid move if it tried to cheat request another
            // maybe the board state should be passed into the bot at move request
            boardColumns[botMove].SetSpace(BotTurn);

            ChangeTurn();
            int winner = LogicalBoardHelpers.CheckVictory(GetTextBoard());
            VictoryConfirmed(winner);

            botThinking = false;
        }

        protected void ChangeTurn()
        {
            CurrentTurn = LogicalBoardHelpers.ChangeTurnColor(CurrentTurn);
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

            if (timeSinceLastMove > 0.5 && CurrentTurn != BotTurn)
            {
                drawBlueArrow = true;
            }

            for (int col = 0; col < LogicalBoardHelpers.NUM_COLUMNS; col++)
            {
                boardColumns[col].Draw(spriteBatch, imageDict, drawBlueArrow);
            }

            switch (CurrentMenu)
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
            CurrentTurn = 1;
            CurrentMenu = MenuState.None;

            for (int c = 0; c < LogicalBoardHelpers.NUM_COLUMNS; c++)
            {
                boardColumns[c].ResetSpaces();
            }
        }

        /// <summary>
        /// Clean up resources and exit the game.
        /// </summary>
        protected void EndGame()
        {
            Exit();
        }

        /// <summary>
        /// Send the updated boardstate to the bot.
        /// </summary>
        protected void UpdateBotBoard(int columnMoved)
        {
            int[,] botBoard = GetTextBoard();
            Bot.UpdateBoard(botBoard, columnMoved);
        }
    }
}
