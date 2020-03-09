using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static ConnectBot.LogicalBoardHelpers;

namespace ConnectBot
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class ConnectGame : Game
    {
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
        BoardColumn[] boardColumns = new BoardColumn[NUM_COLUMNS];

        protected DiscColor CurrentTurn { get; set; }
        protected DiscColor PlayerTurn { get; set; }
        protected DiscColor BotTurn { get; set; }

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
            PlayAgainDrawn,
            SelectColor
        };

        private MenuState CurrentMenu { get; set; }

        private ConnectAI Bot { get; set; }

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
        /// Returns multi dimensional array of DiscColor enums
        /// representing the current state of the board.
        /// </summary>
        /// <returns></returns>
        public DiscColor[,] GetTextBoard()
        {
            DiscColor[,] retBoard = new DiscColor[NUM_COLUMNS, NUM_ROWS];

            for (int c = 0; c < NUM_COLUMNS; c++)
            {
                for (int r = 0; r < NUM_ROWS; r++)
                {
                    retBoard[c, r] = boardColumns[c].GetSpace(r);
                }
            }

            return retBoard;
        }

        protected void VictoryConfirmed(DiscColor winner)
        {
            if (winner != DiscColor.None)
            {
                ShowPlayAgainMenu();
            }
            else
            {
                if (GetOpenColumns(GetTextBoard()).Count == 0)
                {
                    ShowPlayAgainDrawnMenu();
                }
            }
        }

        protected void ShowPlayAgainDrawnMenu()
        {
            CurrentMenu = MenuState.PlayAgainDrawn;
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
            // TODO figure out how to resize the window
            IsMouseVisible = true;

            // Create and load space objects
            int xPos = DrawingConstants.XBoardBuffer;

            // Add space size added to account for blue arrow
            int yPos = DrawingConstants.YBoardBuffer + DrawingConstants.SpaceSize;

            for (int col = 0; col < NUM_COLUMNS; col++)
            {
                boardColumns[col] = new BoardColumn(
                    xPos, 
                    yPos, 
                    imageDict[ImageNames.COLUMN_HOLDER],
                    imageDict[ImageNames.HIGHLIGHTED_COLUMN_HOLDER],
                    imageDict[ImageNames.BLUE_ARROW]);

                // Move to next column.
                xPos += DrawingConstants.SpaceSize;
            }

            ResetGame();
            
            PlayerTurn = DiscColor.Black;
            BotTurn = DiscColor.Red;

            Bot = new ConnectAI(BotTurn);
            UpdateBotBoard();

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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                EndGame();
            }

            timeSinceLastMove += gameTime.ElapsedGameTime.TotalSeconds;

            // TODO should this be a seperate method?
            // Handle user clicks that could be attempted moves.
            // Contain mouse will not allow clicks on a full column.
            LastMouseState = MouseState;
            MouseState = Mouse.GetState();
            Point mousePosition = new Point(MouseState.X, MouseState.Y);
            
            switch (CurrentMenu)
            {
                case MenuState.None:
                    // Check for movement clicks
                    if (timeSinceLastMove > 0.5)
                    {
                        if (CurrentTurn == PlayerTurn)
                        {
                            for (int col = 0; col < NUM_COLUMNS; col++)
                            {
                                if (boardColumns[col].ContainMouse(mousePosition))
                                {
                                    boardColumns[col].IsFocused = true;

                                    if (LastMouseState.LeftButton == ButtonState.Pressed &&
                                        MouseState.LeftButton == ButtonState.Released)
                                    {
                                        //Perform move and change turn.
                                        boardColumns[col].SetSpace(PlayerTurn);
                                        boardColumns[col].IsFocused = false;
                                        timeSinceLastMove = 0.0;

                                        ChangeTurn();
                                        DiscColor winner = CheckVictory(GetTextBoard());
                                        VictoryConfirmed(winner);

                                        UpdateBotBoard();
                                    }
                                }
                                // TODO is there a way to improve this using some object with a mouse enters event or something?
                                else
                                {
                                    boardColumns[col].IsFocused = false;
                                }
                            }
                        }
                        else if (CurrentTurn == BotTurn)
                        {
                            if (!botThinking)
                            {
                                botThinking = true;
                                GetBotMove();
                            }
                        }
                    }
                    break;

                case MenuState.PlayAgain:
                case MenuState.PlayAgainDrawn:
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
            int botMove = -1;
            await Task.Run(() => botMove = Bot.Move().Result);

            // TODO ensure bot made valid move if it tried to cheat request another
            // maybe the board state should be passed into the bot at move request
            if (botMove == -1) throw new InvalidOperationException("The bot did not return a valid column.");

            boardColumns[botMove].SetSpace(BotTurn);

            ChangeTurn();
            DiscColor winner = CheckVictory(GetTextBoard());
            VictoryConfirmed(winner);

            botThinking = false;
        }

        protected void ChangeTurn()
        {
            CurrentTurn = ChangeTurnColor(CurrentTurn);
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

            for (int col = 0; col < NUM_COLUMNS; col++)
            {
                boardColumns[col].Draw(spriteBatch, imageDict, drawBlueArrow);
            }

            switch (CurrentMenu)
            {
                case MenuState.PlayAgain:
                    playAgainMenu.Draw(spriteBatch, imageDict);
                    break;
                case MenuState.PlayAgainDrawn:
                    playAgainMenu.Draw(spriteBatch, imageDict, true);
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
            CurrentTurn = DiscColor.Black;
            CurrentMenu = MenuState.None;

            for (int c = 0; c < NUM_COLUMNS; c++)
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
        protected void UpdateBotBoard()
        {
            Bot.UpdateBoard(GetTextBoard());
        }
    }
}
