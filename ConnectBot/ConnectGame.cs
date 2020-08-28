using ConnectBot.GameMenus;
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
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        
        /// <summary>
        /// Dictionary of int to texture for easily grabbing
        /// repeating displayable images.
        /// </summary>
        Dictionary<string, Texture2D> _imageDict;
        SpriteFont _consolas24;

        /// <summary>
        /// Array of board column objects, contains game board state
        /// and drawing functionality.
        /// </summary>
        BoardColumn[] _boardColumns = new BoardColumn[NUM_COLUMNS];

        protected DiscColor CurrentTurn { get; set; }
        protected DiscColor PlayerTurn { get; set; }
        protected DiscColor BotTurn { get; set; }

        private double _timeSinceLastMove;

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
            Start
        };

        private MenuState CurrentMenu { get; set; }

        private DiscColor _winner;

        private ConnectAI Bot { get; set; }

        private bool _botThinking = false;

        /// <summary>
        /// Menus classes to handle various inputs.
        /// </summary>
        private PlayAgainMenu _playAgainMenu;
        private StartMenu _startMenu;

        public ConnectGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 900;
            _graphics.PreferredBackBufferHeight = 740;
            _graphics.ApplyChanges();
            
            Content.RootDirectory = "Content";
        }

        public BitBoard GetBitBoard()
        {
            ulong redDiscs = 0;
            ulong blackDiscs = 0;

            for (int column = 0; column < NUM_COLUMNS; column++)
            {
                redDiscs |= (_boardColumns[column].GetBitColumn().RedDiscs << (column * 6));
                blackDiscs |= (_boardColumns[column].GetBitColumn().BlackDiscs << (column * 6));
            }

            return new BitBoard(redDiscs, blackDiscs);
        }

        protected void VictoryConfirmed(DiscColor winner)
        {
            if (winner != DiscColor.None)
            {
                ShowPlayAgainMenu(winner);
            }
            else
            {
                if (BitBoardHelpers.GetOpenColumns(GetBitBoard()).Count == 0)
                {
                    ShowPlayAgainDrawnMenu();
                }
            }
        }

        protected void ShowPlayAgainDrawnMenu()
        {
            CurrentMenu = MenuState.PlayAgainDrawn;
        }

        protected void ShowPlayAgainMenu(DiscColor winner)
        {
            _winner = winner; 
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
            IsMouseVisible = true;
            
            // Create and load space objects
            int xPos = DrawingConstants.X_BOARD_BUFFER;

            // Add space size added to account for blue arrow
            int yPos = DrawingConstants.Y_BOARD_BUFFER + DrawingConstants.SPACE_SIZE;

            for (int col = 0; col < NUM_COLUMNS; col++)
            {
                _boardColumns[col] = new BoardColumn(
                    xPos, 
                    yPos, 
                    _imageDict[ImageNames.COLUMN_HOLDER],
                    _imageDict[ImageNames.HIGHLIGHTED_COLUMN_HOLDER],
                    _imageDict[ImageNames.BLUE_ARROW]);

                // Move to next column.
                xPos += DrawingConstants.SPACE_SIZE;
            }

            CurrentMenu = MenuState.Start;
            _winner = DiscColor.None;

            _startMenu = new StartMenu(_consolas24);
            _playAgainMenu = new PlayAgainMenu(_consolas24, GraphicsDevice);

            ResetGame();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Populate image dictionary
            _imageDict = new Dictionary<string, Texture2D>();
            _consolas24 = Content.Load<SpriteFont>("consolas24");

            foreach (string img in ImageNames.AllImageNames)
            {
                _imageDict[img] = Content.Load<Texture2D>(img);
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                EndGame();
            }

            _timeSinceLastMove += gameTime.ElapsedGameTime.TotalSeconds;

            // TODO should this be a seperate method?
            // Handle user clicks that could be attempted moves.
            // Contain mouse will not allow clicks on a full column.
            LastMouseState = MouseState;
            MouseState = Mouse.GetState();
            Point mousePosition = new Point(MouseState.X, MouseState.Y);
            
            switch (CurrentMenu)
            {
                case MenuState.Start:
                    if (LastMouseState.LeftButton == ButtonState.Pressed &&
                            MouseState.LeftButton == ButtonState.Released)
                    {
                        if (_startMenu.BlackDiscContainsMouse(mousePosition))
                        {
                            SetGameColors(DiscColor.Black);
                            break;
                        }
                        else if (_startMenu.RedDiscContainsMouse(mousePosition))
                        {
                            SetGameColors(DiscColor.Red);
                            break;
                        }
                    }
                    break;

                case MenuState.None:
                    // Check for movement clicks
                    if (_timeSinceLastMove > 0.5)
                    {
                        if (CurrentTurn == PlayerTurn)
                        {
                            for (int col = 0; col < NUM_COLUMNS; col++)
                            {
                                if (_boardColumns[col].ContainMouse(mousePosition))
                                {
                                    _boardColumns[col].IsFocused = true;

                                    if (LastMouseState.LeftButton == ButtonState.Pressed &&
                                        MouseState.LeftButton == ButtonState.Released)
                                    {
                                        //Perform move and change turn.
                                        _boardColumns[col].SetSpace(PlayerTurn);
                                        _boardColumns[col].IsFocused = false;
                                        _timeSinceLastMove = 0.0;

                                        ChangeTurn();
                                        var winner = BitBoardHelpers.CheckVictory(GetBitBoard());
                                        VictoryConfirmed(winner);

                                        UpdateBotBoard();
                                    }
                                }
                                else
                                {
                                    _boardColumns[col].IsFocused = false;
                                }
                            }
                        }
                        else if (CurrentTurn == BotTurn)
                        {
                            if (!_botThinking)
                            {
                                _botThinking = true;
                                GetBotMoveAsync();
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
                        if (_playAgainMenu.YesButtonContainsMouse(mousePosition))
                        {
                            ResetGame();
                            break;
                        }
                        else if (_playAgainMenu.NoButtonContainsMouse(mousePosition))
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
        /// Prompts bot to calculate move in the background 
        /// and updates game state accordingly after.
        /// </summary>
        protected async Task GetBotMoveAsync()
        {
            int botMove = await Task.Run(() => Bot.Move());

            if (botMove == -1)
                throw new InvalidOperationException("The bot did not return a valid column.");

            _boardColumns[botMove].SetSpace(BotTurn);
            ChangeTurn();

            var winner = BitBoardHelpers.CheckVictory(GetBitBoard());
            VictoryConfirmed(winner);

            _botThinking = false;
        }

        /// <summary>
        /// Updates the player and bot disc colors and the 
        /// menu state to reflect that the player has chosen
        /// which color they would like to play.
        /// </summary>
        /// <param name="playerChoice"></param>
        void SetGameColors(DiscColor playerChoice)
        {
            PlayerTurn = playerChoice;
            BotTurn = ChangeTurnColor(playerChoice);

            Bot = new ConnectAI(BotTurn);
            CurrentMenu = MenuState.None;
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
            GraphicsDevice.Clear(DrawingConstants.BACKGROUND_COLOR);
            _spriteBatch.Begin();

            switch (CurrentMenu)
            {
                case MenuState.Start:
                    _startMenu.Draw(_spriteBatch, _imageDict);
                    break;

                case MenuState.PlayAgain:
                    DrawBoard();
                    _playAgainMenu.Draw(_spriteBatch, _imageDict, _winner);
                    break;

                case MenuState.PlayAgainDrawn:
                    DrawBoard();
                    _playAgainMenu.Draw(_spriteBatch, _imageDict, DiscColor.None, true);
                    break;

                case MenuState.None:
                    DrawBoard();
                    break;
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawBoard()
        {
            for (int col = 0; col < NUM_COLUMNS; col++)
            {
                _boardColumns[col].Draw(
                    _spriteBatch,
                    _imageDict,
                    // Used to delay the appearance of the blue arrow for improved visuals
                    (_timeSinceLastMove > 0.5 && CurrentTurn != BotTurn));
            }
        }

        /// <summary>
        /// Resets game board and state.
        /// </summary>
        protected void ResetGame()
        {
            CurrentTurn = DiscColor.Black;
            CurrentMenu = MenuState.Start;

            for (int c = 0; c < NUM_COLUMNS; c++)
            {
                _boardColumns[c].ResetSpaces();
            }

            UpdateBotBoard();
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
            Bot?.UpdateBoard(GetBitBoard());
        }
    }
}
