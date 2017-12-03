using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace PongGame
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        KeyboardState state = new KeyboardState();

        // Tekstury dla stołu, lewej deski, prawej deski i piłki
        private Texture2D table, firstBoard, secondBoard, ball;

        // Zmienne mówiące czy gra wystartowała, została zakończona i czy jest w trybie single player
        private bool startGame = false, gameOver = false, singlePlayer = false;

        // Czcionki do wyświetlenia wyniku oraz tekstów
        private SpriteFont fontGame, fontScore;

        // Zmienne przechowujące liczbę punktów oraz numer gracza, który wygrał w rywalizacji multiplayer
        private int score = 0, playerNumber;

        // Dźwiękowy efekt odbicia piłki
        private Song pongEffect;

        // Początkowe położenie lewej deski
        private int firstBoardPositionX = 20, firstBoardPositionY = 220;

        // Początkowe położenie prawej deski
        private int secondBoardPositionX = 950, secondBoardPositionY = 220;

        // Wymiary deski 
        private int boardWidth = 30, boardHeight = 100;

        // Początkowe położenie piłki oraz jej prędkość poruszania względem osi X i Y
        private int ballPositionX, ballPositionY = 50, ballSpeedX = 8, ballSpeedY = 3;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
       
            // Ustawienie rozdzielczości ekranu zgodnej z teksturą grafiki stołu
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft;
            graphics.PreferredBackBufferWidth = 1000;
            graphics.PreferredBackBufferHeight = 550;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            IsMouseVisible = true;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Wczytanie wszystkich plików wykorzystywanych w grze
            table = Content.Load<Texture2D>("Images/PongTable");
            firstBoard = Content.Load<Texture2D>("Images/Board");
            secondBoard = Content.Load<Texture2D>("Images/Board");
            ball = Content.Load<Texture2D>("Images/PingPongBallSmall");
            fontGame = Content.Load<SpriteFont>("Fonts/FontGame");
            fontScore = Content.Load<SpriteFont>("Fonts/FontScore");
            pongEffect = Content.Load<Song>("Sounds/Pong");

            // Ustalenie środkowego położenia piłki na osi X (połowa szerokości ekranu - połowa szerokości piłki)
            ballPositionX = (graphics.PreferredBackBufferWidth / 2) - (ball.Height / 2);
        }

        protected override void UnloadContent()
        {
        }
    
        protected override void Update(GameTime gameTime)
        {
            // Pobranie wciśniętego klawisza
            state = Keyboard.GetState();

            // Jeśli gra jeszcze nie wystartowała i wciśnięto spację, to rozpocznij w trybie single player (w przeciwnym razie multiplayer)
            if (!startGame && state.IsKeyDown(Keys.Space))
            {
                startGame = true;
                singlePlayer = true;
            }
            else if (!startGame && (state.IsKeyDown(Keys.RightShift) || state.IsKeyDown(Keys.LeftShift)))
            {
                startGame = true;
                singlePlayer = false;
            }

            // Jeśli gra jest zakończona i wciśnięto Enter, to przywróć parametry początkowe
            if (gameOver && state.IsKeyDown(Keys.Enter))
            {
                startGame = false;
                gameOver = false;
                score = 0;
                firstBoardPositionY = 220;
                secondBoardPositionY = 220;
                ballPositionX = (graphics.PreferredBackBufferWidth / 2) - (ball.Height / 2);
                ballPositionY = 50;
            }

            // Jeśli gra wystartowała i nie jest gameOver, to steruj rozgrywką
            if (startGame && !gameOver)
            {
                // W zależności od wciśniętego klawisza deska wędruje w górę lub w dół (jest zablokowane wydostanie
                // się deski poza ekran)
                if (state.IsKeyDown(Keys.Z))
                {
                    if (firstBoardPositionY > 448)
                    {
                        firstBoardPositionY = 450;
                    }
                    else
                    {
                        firstBoardPositionY += 5;
                    }
                }
                else if (state.IsKeyDown(Keys.A))
                {
                    if (firstBoardPositionY < 3)
                    {
                        firstBoardPositionY = 0;
                    }
                    else
                    {
                        firstBoardPositionY -= 5;
                    }
                }

                // Zmiana położenia piłki
                ballPositionY += ballSpeedY;
                ballPositionX += ballSpeedX;

                // Odbicie piłki od górnej lub dolnej krawędzi (z uwzględnieniem rozmiarów piłki)
                if (ballPositionY > (GraphicsDevice.Viewport.Bounds.Height - ball.Height))
                {
                    ballSpeedY = ballSpeedY * (-1);
                }
                else if (ballPositionY < (0))
                {
                    ballSpeedY = ballSpeedY * (-1);
                }

                // Jeśli jest tryb single player, to prawa deska podąża sama za piłką - jeśli multiplayer to sterujemy prawą deską
                if (singlePlayer)
                {
                    if ((secondBoardPositionY + boardHeight / 2) > (ballPositionY + (ball.Height / 2)))
                    {
                        if (secondBoardPositionY > 0)
                        {
                            secondBoardPositionY -= 3;
                        }
                    }
                    else
                    {
                        if (secondBoardPositionY + boardHeight < graphics.PreferredBackBufferHeight)
                        {
                            secondBoardPositionY += 3;
                        }
                    }
                } else
                {
                    if (state.IsKeyDown(Keys.Down))
                    {
                        if (secondBoardPositionY > 448)
                        {
                            secondBoardPositionY = 450;
                        }
                        else
                        {
                            secondBoardPositionY += 5;
                        }
                    }
                    else if (state.IsKeyDown(Keys.Up))
                    {
                        if (secondBoardPositionY < 3)
                        {
                            secondBoardPositionY = 0;
                        }
                        else
                        {
                            secondBoardPositionY -= 5;
                        }
                    }
                }

                // Sprawdzenie, czy piłka dotknęła lewej krawędzi prawej deski - jeśli tak to odbicie, jeśli nie to koniec gry
                if (ballPositionX + ball.Width >= secondBoardPositionX)
                {
                    // Piłka musi nachodzić minimum 0.3 częścią swojej wysokości na deskę w celu odbicia
                    if ((ballPositionY + (ball.Height * 0.7) >= secondBoardPositionY) && (ballPositionY + (ball.Height * 0.3) / 1 <= secondBoardPositionY + boardHeight))
                    {
                        ballSpeedX = ballSpeedX * (-1);
                        MediaPlayer.Play(pongEffect);
                    }
                    else
                    {
                        gameOver = true;
                        playerNumber = 1;
                    }
                }

                // Sprawdzenie, czy piłka dotknęła prawej krawędzi lewej deski - jeśli tak to odbicie, jeśli nie to koniec gry
                if (ballPositionX <= firstBoardPositionX + boardWidth)
                {
                    // Piłka musi nachodzić minimum 0.3 częścią swojej wysokości na deskę w celu odbicia
                    if ((ballPositionY + (ball.Height * 0.7) >= firstBoardPositionY) && (ballPositionY + (ball.Height * 0.3) <= firstBoardPositionY + boardHeight))
                    {
                        ballSpeedX = ballSpeedX * (-1);
                        MediaPlayer.Play(pongEffect);
                        if (singlePlayer) {
                            score++;
                        }
                    }
                    else
                    {
                        gameOver = true;
                        playerNumber = 2;
                    }
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            // Jeśli gra jeszcze nie wystartowała to wyświetlone będą napisy początkowe, jeśli jest zakończona to końcowe
            if (!startGame)
            {
                spriteBatch.Draw(table, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.White);
                spriteBatch.DrawString(fontGame, "SPACJA - ZAGRAJ Z KOMPUTEREM!", new Vector2(130, 200), Color.White);
                spriteBatch.DrawString(fontGame, "SHIFT - ZAGRAJ Z PRZECIWNIKIEM!", new Vector2(125, 300), Color.White);
            }
            else if (gameOver)
            {
                spriteBatch.Draw(table, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.Red);
                if (singlePlayer)
                {
                    spriteBatch.DrawString(fontGame, "KONIEC GRY! WYNIK: " + (int)score, new Vector2(240, 200), Color.White);
                } else
                {
                    spriteBatch.DrawString(fontGame, "KONIEC GRY! LEPSZY JEST GRACZ " + playerNumber + "!", new Vector2(110, 200), Color.White);
                }
                spriteBatch.DrawString(fontGame, "ENTER - NOWA GRA", new Vector2(270, 280), Color.White);
            }
            else
            {
                spriteBatch.Draw(table, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.White);
            }

            // Rysowanie desek i piłki z odświeżanymi współrzędnymi
            spriteBatch.Draw(firstBoard, new Rectangle(firstBoardPositionX, firstBoardPositionY, boardWidth, boardHeight), Color.White);
            spriteBatch.Draw(secondBoard, new Rectangle(secondBoardPositionX, secondBoardPositionY, boardWidth, boardHeight), Color.White);
            spriteBatch.Draw(ball, new Vector2(ballPositionX, ballPositionY), Color.White);

            // W trybie single player na dole wyświetla się wynik
            if (singlePlayer)
            {
                spriteBatch.DrawString(fontScore, "Wynik: " + (int)score, new Vector2(50, 500), Color.White);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
