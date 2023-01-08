using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace kurs
{
    class Game : GameWindow
    {
        Ball ball;
        Paddle paddle;
        Random rnd;
        List<Blok> blocks = new List<Blok>();
        List<Blok> uberBlocks = new List<Blok>();
        List<Button> buttons = new List<Button>();
        Button gameOverButton;
        Button gameWinButton;
        int dx, dy, ortoWidth = 600, ortoHeight = 800, ballTextureID, brickTextureID,uberBrickTextureID, StartGameID, CloseGameID, Level1ID,Level2ID,scores, GameOverID,GameWinID;
        double deltaX, deltaY;
        Vector2 cursor;
        bool gameOver = false, isPaused = false, levelChoose=false,level=false, clearButtons=false, gameWin=false;
        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeSettings)
        : base(gameWindowSettings, nativeSettings)
        {

        }
        protected override void OnLoad()
        {
            GL.MatrixMode(MatrixMode.Projection); //Как я понял тут выбирается локальная матрица над которой сейчас будет работа происходить.
            GL.LoadIdentity(); // Загружаем матрицу по умолчанию. Вроде бы единичная матрица
            GL.Ortho(0, ortoWidth, 0, ortoHeight, -1, 1); // 0;0 находится в левом нижнем углу. У направлена вверх, х - направо. Перемножаю матрицу на новую.
            GL.MatrixMode(MatrixMode.Modelview); // Выбираю снова глобальную матрицу 

            ballTextureID = ContentPipe.LoadTexture(@"Content\ball.png");
            brickTextureID = ContentPipe.LoadTexture(@"Content\brick.jpg");
            uberBrickTextureID = ContentPipe.LoadTexture(@"Content\UberBlock.png");
            StartGameID = ContentPipe.LoadTexture(@"Content\Start.png");
            CloseGameID = ContentPipe.LoadTexture(@"Content\Close.png");
            Level1ID = ContentPipe.LoadTexture(@"Content\1Level.png");
            Level2ID = ContentPipe.LoadTexture(@"Content\2Level.png");

            GameOverID = ContentPipe.LoadTexture(@"Content\Lose.png");
            GameWinID = ContentPipe.LoadTexture(@"Content\Win.png");
            gameOverButton = new Button(230, 500, 100, 100, Color4.Gray, GameOverID);
            gameOverButton.OnMouseDown += ReturnToMainMenu;
            gameWinButton = new Button(230, 500, 100, 100, Color4.Gray, GameWinID);
            gameWinButton.OnMouseDown += ReturnToMainMenu;
            VSync = VSyncMode.On;
            base.OnLoad();
            
            rnd = new Random();
            
            StartScreen(); 
        }
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            if(clearButtons)
            {
                buttons.Clear();
                clearButtons = false;
            }
            if(levelChoose)
            {
                buttons.Add(new Button(230, 500, 100, 100, Color4.Gray, Level1ID));
                buttons.Add(new Button(230, 300, 100, 100, Color4.Gray, Level2ID));
                buttons.Add(new Button(230, 100, 100, 100, Color4.Gray, CloseGameID));
                buttons[0].OnMouseDown += Level1;
                buttons[1].OnMouseDown += Level2;
                buttons[2].OnMouseDown += CloseGame;
                levelChoose = false;
            }
            if(level)
            {
                GameMaster();
            } 
        }
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            if(level)
            {
                GameMasterRender();
            }
            ButtonRender();
            SwapBuffers();
        }
        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (gameOver)
                return;
            else
            {
                switch (e.Key)
                {
                    case Keys.A:
                        if (!isPaused)
                        {
                            paddle.Move(new Vector2(-8f, 0)); 
                        }
                        break;
                    case Keys.D:
                        if (!isPaused)
                        {
                            paddle.Move(new Vector2(8f, 0)); 
                        }
                        break;
                    case Keys.P:
                        isPaused = !isPaused;
                        break;
                    case Keys.Escape:
                        CloseGame();
                        break;
                }
            }
        }
        protected override void OnUnload()
        {
            base.OnUnload();
            foreach (var blok in blocks)
                blok.Delete();
            foreach (var blok in uberBlocks)
                blok.Delete();
            if (ball != null || paddle!=null)
            {
                ball.Delete();
                paddle.Delete(); 
            }
            GL.DeleteTexture(ballTextureID);
            GL.DeleteTexture(brickTextureID);
            GL.DeleteTexture(uberBrickTextureID);
            GL.DeleteTexture(StartGameID);
            GL.DeleteTexture(CloseGameID);
            GL.DeleteTexture(Level1ID);
            GL.DeleteTexture(Level2ID);
            GL.DeleteTexture(GameOverID);
            GL.DeleteTexture(GameWinID);
            gameWinButton.Delete();
            gameOverButton.Delete();
            foreach (var b in buttons)
                b.Delete();
        }
        protected override void OnResize(ResizeEventArgs e)
        {
            deltaX = e.Width / (float)ortoWidth;
            deltaY = e.Height / (float)ortoHeight;
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
        }
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
            cursor = new Vector2((float)(e.Position.X / deltaX), (float)ortoHeight - (float)(e.Position.Y / deltaY));
        }
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            foreach(var but in buttons)
            {
                if(but.IsPointIn(cursor.X,cursor.Y))
                {
                    but.OnMouseDown?.Invoke();
                }
            }
            if (gameOver)
                if (gameOverButton.IsPointIn(cursor.X, cursor.Y))
                    gameOverButton.OnMouseDown?.Invoke();
            if (gameWin)
                if (gameOverButton.IsPointIn(cursor.X, cursor.Y))
                    gameOverButton.OnMouseDown?.Invoke();
        }
        private void CheackAllBlocks()
        {
            foreach (var block in blocks)
            {
                for (int i = 0; i < ball.Cords.Length; i += 2)
                {
                    if (block.IsPointIn(ball.Cords[i], ball.Cords[i + 1]))
                    {
                        block.Status = false;
                        ChangeBallVector(block);
                    }
                }
            }
            foreach (var block in uberBlocks)
            {
                for (int i = 0; i < ball.Cords.Length; i += 2)
                {
                    if (block.IsPointIn(ball.Cords[i], ball.Cords[i + 1]))
                    {
                        ChangeBallVector(block);
                    }
                }
            }
        }
        private void ChangeBallVector(Blok currentBlok)
        {
            float centerBlokX = currentBlok.X + currentBlok.Width / 2; float CenterBlockY = currentBlok.Y + currentBlok.Height / 2; // центр вписанной окружности в блок
            float centerBallX = ball.X + ball.Width / 2; float centerBallY = ball.Y + ball.Height / 2; // центр вписанной окружности в снаряд
            float x = Math.Abs(centerBlokX - centerBallX); float y = Math.Abs(CenterBlockY - centerBallY); // расчёт расстояния между центрами
            if (x > y) // определение плоскости столкновения в данном случае если true то это либо право либо лево
            {
                if (centerBallX > centerBlokX) // справа
                    dx = Math.Abs(dx);
                else
                    dx = -Math.Abs(dx); // слева
            }
            else
            {
                if (centerBallY > CenterBlockY) // низ
                    dy = Math.Abs(dy);
                else // вверх
                    dy = -Math.Abs(dy);
            }
        }
        private void BallCollisionOnWallsAndPaddleChecker()
        {
            if (ball.Cords[4] >= ortoWidth) // столкновение с границей справа
                dx = -Math.Abs(dx);
            if (ball.Cords[0] <= 0) //столкновение с границей слева
                dx = Math.Abs(dx);
            if (ball.Cords[3] >= ortoHeight) // сверху
                dy = -Math.Abs(dy);
            if (ball.Cords[1] <= 0) // что делаем если шарик упал
            {
                gameOver = true;
            }

            for (int i = 0; i < ball.Cords.Length; i += 2)
            {
                if (paddle.IsPointIn(ball.Cords[i], ball.Cords[i + 1]))
                {
                    float ballCenterX = ball.X + ball.Width / 2;
                    if (ballCenterX > paddle.X && ballCenterX < paddle.X + paddle.Width / 3)
                    {
                        Debug.WriteLine("Первая треть"); dy = 1; dx = -3;
                    }
                    else if (ballCenterX > paddle.X + paddle.Width / 3 && ballCenterX < paddle.X + 2 * paddle.Width / 3)
                    {
                        Debug.WriteLine("Вторая треть"); dy = 1; dx = 0;
                    }
                    else if (ballCenterX > paddle.X + 2 * paddle.Width / 3 && ballCenterX < paddle.X + paddle.Width)
                    {
                        Debug.WriteLine("Третья треть"); dy = +1; dx = +1;
                    }
                    else if (ball.X > paddle.X + 2 * paddle.Width / 3 && ball.X < paddle.X + paddle.Width)
                    {
                        Debug.WriteLine("Третья треть"); dy = +1; dx = +1;
                    }
                    else if (ball.X + ball.Width > paddle.X && ball.X < paddle.X + paddle.Width / 3)
                    {
                        Debug.WriteLine("Первая треть"); dy = 1; dx = -3;
                    }

                }
            }

        }
        private void ButtonRender()
        {
            foreach (var but in buttons)
            {
                but.Draw();
            }
        }
        private void GameMaster()
        {
            if (!isPaused)
            {
                if (!gameOver && !gameWin)
                {
                    Blok delBlock = null;
                    foreach (var blok in blocks)
                    {
                        if (blok.Status == false)
                        {
                            delBlock = blok;

                        }
                    }
                    if (delBlock != null)
                    {
                        delBlock.Delete();
                        blocks.Remove(delBlock);
                        scores += 10;
                        Title = "Acranoid           Scores: " + scores.ToString();
                        delBlock = null;
                    }
#if DEBUG
                    if (KeyboardState.IsKeyDown(Keys.Right))
                        ball.Move(new Vector2(2f, 0));
                    if (KeyboardState.IsKeyDown(Keys.Left))
                        ball.Move(new Vector2(-2f, 0));
                    if (KeyboardState.IsKeyDown(Keys.Down))
                        ball.Move(new Vector2(0, -2f));
                    if (KeyboardState.IsKeyDown(Keys.Up))
                        ball.Move(new Vector2(0, 2f));
                    if (KeyboardState.IsKeyDown(Keys.E))
                        dx = rnd.Next(-3, 3);
#endif
                    ball.Move(new Vector2(dx / 2f, dy / 2f));

                    CheackAllBlocks();
                    BallCollisionOnWallsAndPaddleChecker();
                    if (blocks.Count == 0)
                    {
                        gameWin = true;
                    }

                }
            }
        }
        private void GameMasterRender()
        {
            foreach (var blok in blocks)
                blok.Draw();
            foreach (var blok in uberBlocks)
                blok.Draw();
            ball.Draw();
            paddle.Draw();
            if (gameOver)
            {
                gameOverButton.Draw();
            }
            if (gameWin)
            {
                gameWinButton.Draw();
            }
        }
        private void StartScreen()
        {
            buttons.Add(new Button(230, 500, 100, 100, Color4.Gray, StartGameID));
            buttons.Add(new Button(230, 300, 100, 100, Color4.Gray, CloseGameID));
            buttons[1].OnMouseDown += CloseGame;
            buttons[0].OnMouseDown += Start;
        }
        private void CloseGame()
        {
            OnUnload();
            Close();
        }
        private void Start()
        {
            levelChoose = true;
            clearButtons = true;
            foreach (var but in buttons)
                but.Delete();
        }
        private void Level1()
        {
            level = true;
            scores = 0;
            foreach (var but in buttons)
                but.Delete();
            clearButtons = true;
            ball = new Ball(300, 30, 10, 10, Color4.Yellow, ballTextureID);
            paddle = new Paddle(250, 10, 100, 10, Color4.Aqua, brickTextureID, ortoWidth);

            dx = rnd.Next(-3, 3);
            dy = 1;
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    blocks.Add(new Blok(50 + 60 * j, 350 + 60 * i, 50, 50, Color4.DarkMagenta, brickTextureID));
                }
            }
        }
        private void Level2()
        {
            level = true;
            scores = 0;
            foreach (var but in buttons)
                but.Delete();
            clearButtons = true;
            ball = new Ball(300, 30, 10, 10, Color4.Yellow, ballTextureID);
            paddle = new Paddle(250, 10, 100, 10, Color4.Aqua, brickTextureID, ortoWidth);
            dx = rnd.Next(-3, 3);
            dy = 1;
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (i == 0 && j == 0 || i == 0 && j == 7 || i == 5 && j == 0 || i == 5 && j == 7)
                        continue;
                    blocks.Add(new Blok(50 + 60 * j, 350 + 60 * i, 50, 50, Color4.DarkMagenta, brickTextureID));
                }
            }
            uberBlocks.Add(new Blok(50 + 60 * 0, 350 + 60 * 0, 50, 50, Color4.DarkMagenta, uberBrickTextureID));
            uberBlocks.Add(new Blok(50 + 60 * 7, 350 + 60 * 0, 50, 50, Color4.DarkMagenta, uberBrickTextureID));
            uberBlocks.Add(new Blok(50 + 60 * 7, 350 + 60 * 5, 50, 50, Color4.DarkMagenta, uberBrickTextureID));
            uberBlocks.Add(new Blok(50 + 60 * 0, 350 + 60 * 5, 50, 50, Color4.DarkMagenta, uberBrickTextureID));
            for(int i=0;i<10;i+=2)
            {
                uberBlocks.Add(new Blok(50 + 60 * i, 250 , 50, 50, Color4.DarkMagenta, uberBrickTextureID));
            }
        }
        private void ReturnToMainMenu()
        {
            level = false;   
            levelChoose = false;
            clearButtons = false;
            foreach (var but in buttons)
                but.Delete();
            gameOver = false;
            gameWin = false;
            Title = "Acranoid";

            blocks.Clear();
            uberBlocks.Clear();

            StartScreen();
        }

    }
    public class Blok
    {
        float x, y, height, width;
        Color4 color;
        protected int bufferCordsID, bufferTextureID, vaoID, textureID;
        bool status = true;
        protected float[] cords, textCords;
        public bool Status { get => status; set => status = value; }
        public virtual float X { get => x; set => x = value; }
        public virtual float Y { get => y; set => y = value; }
        public float Height { get => height; set => height = value; }
        public float Width { get => width; set => width = value; }
        public Blok() { }
        public Blok(float x, float y, float width, float height, Color4 color, int textureID)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.color = color;
            this.textureID = textureID;
            cords = new float[] { x, y, x, y + height, x + width, y + height, x + width, y };
            textCords = new float[] { 0f, 1f, 0f, 0f, 1f, 0f, 1f, 1f };
            bufferCordsID = GL.GenBuffer();// генерация индефикатора
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferCordsID);//тип буфера, индефикатор
            GL.BufferData(BufferTarget.ArrayBuffer, cords.Length * sizeof(float), cords, BufferUsageHint.StaticDraw);//тип, байты, данны, тип доступа
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            bufferTextureID = GL.GenBuffer(); // генерация идефикатора для текстурного массива
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferTextureID);
            GL.BufferData(BufferTarget.ArrayBuffer, textCords.Length * sizeof(float), textCords, BufferUsageHint.StaticDraw);//тип, байты, данны, тип доступа
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            vaoID = GL.GenVertexArray();
            GL.BindVertexArray(vaoID);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferTextureID);
            GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferCordsID);
            GL.VertexPointer(2, VertexPointerType.Float, 0, 0);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.TextureCoordArray);

        }
        public void Draw()
        {

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);// Подключаем режим отображения текстур для работы с прозрачностью
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha); // делаем так чтобы прозрачность была прозрачной а не просто белой 
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);

            GL.BindTexture(TextureTarget.Texture2D, textureID);

            GL.BindVertexArray(vaoID);

            GL.DrawArrays(PrimitiveType.Polygon, 0, 4);
            GL.BindVertexArray(0);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.Disable(EnableCap.Blend);
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.TextureCoordArray);
            GL.Disable(EnableCap.Texture2D);

        }
        public virtual void Delete()
        {
            GL.DeleteBuffer(bufferCordsID);
            GL.DeleteBuffer(bufferTextureID);
            GL.DeleteVertexArray(vaoID);
        }
        public virtual bool IsPointIn(float pointX, float pointY)
        {
            if (pointX < x) return false;
            if (pointY < y) return false;
            if (pointX > x + width) return false;
            if (pointY > y + height) return false;
            return true;
        }
    }
    public class Ball : Blok
    {
        public float[] Cords { get => cords; set => cords = value; }
        public override float X { get => cords[0]; set => cords[0] = value; }
        public override float Y { get => cords[1]; set => cords[1] = value; }
        public Ball(float x, float y, float width, float height, Color4 color, int textureID) : base(x, y, width, height, color, textureID)
        {
        }
        public void Move(Vector2 vector)
        {
            for (int i = 0; i < cords.Length; i += 2)
            {
                cords[i] += vector.X;
                cords[i + 1] += vector.Y;
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferCordsID);//тип буфера, индефикатор
            GL.BufferData(BufferTarget.ArrayBuffer, cords.Length * sizeof(float), cords, BufferUsageHint.StaticDraw);//тип, байты, данны, тип доступа
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
    }
    public class Paddle : Blok
    {
        public override float X { get => cords[0]; set => cords[0] = value; }
        public override float Y { get => cords[1]; set => cords[1] = value; }
        int ortoWidth;
        public Paddle(float x, float y, float width, float height, Color4 color, int textureID, int ortoWidth) : base(x, y, width, height, color, textureID)
        {
            this.ortoWidth = ortoWidth;
        }
        public void Move(Vector2 vector)
        {
            if (cords[0] + vector.X>= 0 && cords[0]+Width + vector.X <= ortoWidth)
            {
                for (int i = 0; i < cords.Length; i += 2)
                {
                    cords[i] += vector.X;
                    cords[i + 1] += vector.Y;
                }
                GL.BindBuffer(BufferTarget.ArrayBuffer, bufferCordsID);//тип буфера, индефикатор
                GL.BufferData(BufferTarget.ArrayBuffer, cords.Length * sizeof(float), cords, BufferUsageHint.StaticDraw);//тип, байты, данны, тип доступа
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0); 
            }
        }
        public override bool IsPointIn(float pointX, float pointY)
        {
            if (pointX < cords[0]) return false;
            if (pointY < cords[1]) return false;
            if (pointX > cords[0] + Width) return false;
            if (pointY > cords[1] + Height) return false;
            return true;
        }
    }
    public class ContentPipe
    {
        static public int LoadTexture(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"File not found at '{path}'");
            }

            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            Bitmap bmp = new Bitmap(path);
            BitmapData data = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D,0, PixelInternalFormat.Rgba,
                data.Width, data.Height,0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte,
                data.Scan0);

            bmp.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            return id;
        }

    }
    delegate void MouseDelegate();
    class Button: Blok
    {
        float x, y, width, height;
        Color4 color;
        public MouseDelegate OnMouseDown;
        public Button(float x, float y, float width, float height, Color4 color, int textureID) : base(x, y, width, height, color, textureID)
        {
        }
        public override bool IsPointIn(float pointX, float pointY)
        {
            if (pointX < cords[0]) return false;
            if (pointY < cords[1]) return false;
            if (pointX > cords[0] + Width) return false;
            if (pointY > cords[1] + Height) return false;
            return true;
        }
    }
}