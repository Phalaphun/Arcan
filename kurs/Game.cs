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
        //Blok blok1, blok2;
        Ball ball;
        Paddle paddle;
        Random rnd;
        List<Blok> blocks = new List<Blok>();
        int dx, dy;
        bool gameOver = false;
        int ortoWidth = 600;
        int ortoHeight = 800;
        int ballTextureID, BrickTextureID;
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

            VSync = VSyncMode.On;
            base.OnLoad();
            ball = new Ball(300, 30, 10, 10, Color4.Yellow);
            paddle = new Paddle(250, 10, 100, 10, Color4.Aqua);
            rnd = new Random();
            dx = rnd.Next(-3, 3);
            dy = 1;
            for (int i = 0; i< 6; i++)
            {
                for(int j=0;j<8;j++)
                {
                    blocks.Add(new Blok(50+60*j,350+60*i,50,50,Color4.DarkMagenta));
                }
            }

            ballTextureID = ContentPipe.LoadTexture(@"Content\ball.png");
            BrickTextureID = ContentPipe.LoadTexture(@"Content\brick.jpg");
        }
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            if (!gameOver)
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
                    delBlock.DeleteVBO();
                    blocks.Remove(delBlock);
                    delBlock = null;
                }
                
                if (KeyboardState.IsKeyDown(Keys.Escape))
                    Close();
                if (KeyboardState.IsKeyDown(Keys.Right))
                    ball.Move(new Vector2(1f, 0));
                if (KeyboardState.IsKeyDown(Keys.Left))
                    ball.Move(new Vector2(-1f, 0));
                if (KeyboardState.IsKeyDown(Keys.Down))
                    ball.Move(new Vector2(0, -1f));
                if (KeyboardState.IsKeyDown(Keys.Up))
                    ball.Move(new Vector2(0, 1f));
                if (KeyboardState.IsKeyDown(Keys.E))
                    dx = rnd.Next(-3, 3);
                if(KeyboardState.IsKeyDown(Keys.A))
                    paddle.Move(new Vector2(-1f, 0));
                if (KeyboardState.IsKeyDown(Keys.D))
                    paddle.Move(new Vector2(1f, 0));
                //if (KeyboardState.IsKeyDown(Keys.R))
                //{
                //    ball.Move(new Vector2(dx/2f, dy/2f));
                //}
                ball.Move(new Vector2(dx / 2f, dy / 2f));
                CheackAllBlocks();
                BallCollisionOnWallsAndPaddleChecker();
            }
        }
        protected override void OnUnload()
        {
            base.OnUnload();
            foreach (var blok in blocks)
                blok.DeleteVBO();
            ball.DeleteVBO();
            paddle.DeleteVBO();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            foreach (var blok in blocks)
                blok.DrawVBO();
            ball.DrawVBO();
            paddle.DrawVBO();
            SwapBuffers();
        }
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
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
                //gameOver = true;
            }

            for (int i = 0; i < ball.Cords.Length; i+=2)
            {
                if (paddle.IsPointIn(ball.Cords[i], ball.Cords[i + 1]))
                {
                    float ballCenterX = ball.X + ball.Width / 2;
                    //if (ball.X > paddle.X && ball.X < paddle.X + paddle.Width / 3 || ball.X+ball.Width > paddle.X && ball.X+ ball.Width < paddle.X + paddle.Width / 3)
                    //{
                    //    Debug.WriteLine("Первая треть"); dy = Math.Abs(dy); dx = -Math.Abs(dx);
                    //}
                    //else if (ball.X > paddle.X + paddle.Width / 3 && ball.X < paddle.X + 2 * paddle.Width / 3 || ball.X + ball.Width > paddle.X + paddle.Width / 3 && ball.X + ball.Width < paddle.X + 2 * paddle.Width / 3)
                    //{
                    //    Debug.WriteLine("Вторая треть"); dy = Math.Abs(dy); dx = 0;
                    //}
                    //else if (ball.X > paddle.X + 2 * paddle.Width / 3 && ball.X < paddle.X + paddle.Width || ball.X + ball.Width > paddle.X + 2 * paddle.Width / 3 && ball.X + ball.Width < paddle.X + paddle.Width)
                    //{
                    //    Debug.WriteLine("Третья треть"); dy = +Math.Abs(dy); dx = +Math.Abs(dx);
                    //}
                    if (ballCenterX > paddle.X && ballCenterX < paddle.X + paddle.Width / 3)
                    {
                        Debug.WriteLine("Первая треть"); dy = Math.Abs(dy); dx = -Math.Abs(dx);
                    }
                    else if (ballCenterX > paddle.X + paddle.Width / 3 && ballCenterX < paddle.X + 2 * paddle.Width / 3)
                    {
                        Debug.WriteLine("Вторая треть"); dy = Math.Abs(dy); dx = 0;
                    }
                    else if (ballCenterX > paddle.X + 2 * paddle.Width / 3 && ballCenterX < paddle.X + paddle.Width)
                    {
                        Debug.WriteLine("Третья треть"); dy = +Math.Abs(dy); dx = +Math.Abs(dx);
                    }
                    else if (ball.X > paddle.X + 2 * paddle.Width / 3 && ball.X < paddle.X + paddle.Width)
                    {
                        Debug.WriteLine("Третья треть"); dy = +Math.Abs(dy); dx = +Math.Abs(dx);
                    }
                    else if (ball.X + ball.Width > paddle.X && ball.X < paddle.X + paddle.Width / 3)
                    {
                        Debug.WriteLine("Первая треть"); dy = Math.Abs(dy); dx = -Math.Abs(dx);
                    }


                }
            }

        }
        private void GameOver()
        {
            Debug.WriteLine("GameOver");
        }
    }
    public class Blok
    {
        float x, y, height, width;
        Color4 color;
        protected int BufferID;
        bool status = true;
        protected float[] cords;
        public bool Status { get => status; set => status = value; }
        public virtual float X { get => x; set => x = value; }
        public virtual float Y { get => y; set => y = value; }
        public float Height { get => height; set => height = value; }
        public float Width { get => width; set => width = value; }
        public Blok() { }
        public Blok(float x, float y, float width, float height, Color4 color)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.color = color;
            cords = new float[] { x, y, x, y + height, x + width, y + height, x + width, y };
            BufferID = GL.GenBuffer();// генерация индефикатора
            GL.BindBuffer(BufferTarget.ArrayBuffer, BufferID);//тип буфера, индефикатор
            GL.BufferData(BufferTarget.ArrayBuffer, cords.Length * sizeof(float), cords, BufferUsageHint.StaticDraw);//тип, байты, данны, тип доступа
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
        public void DrawVBO()
        {
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, BufferID);

            GL.VertexPointer(2, VertexPointerType.Float,0,0);

            GL.Color4(color);
            GL.DrawArrays(PrimitiveType.Polygon,0,4);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DisableClientState(ArrayCap.VertexArray);
        }
        public void DeleteVBO()
        {
            GL.DeleteBuffer(BufferID);
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
        public override float  X { get => cords[0]; set => cords[0] = value; }
        public override float Y { get => cords[1]; set => cords[1] = value; }
        public Ball(float x, float y, float width, float height, Color4 color) : base (x,y,width,height,color)
        {

        }
        public void Move(Vector2 vector)
        {
            for (int i = 0; i < cords.Length; i += 2)
            {
                cords[i] += vector.X;
                cords[i + 1] += vector.Y;
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, BufferID);//тип буфера, индефикатор
            GL.BufferData(BufferTarget.ArrayBuffer, cords.Length * sizeof(float), cords, BufferUsageHint.StaticDraw);//тип, байты, данны, тип доступа
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
    }
    public class Paddle : Blok
    {
        public override float X { get => cords[0]; set => cords[0] = value; }
        public override float Y { get => cords[1]; set => cords[1] = value; }
        public Paddle(float x, float y, float width, float height, Color4 color) : base(x,y,width,height,color)
        {

        }
        public void Move(Vector2 vector)
        {
            for (int i = 0; i < cords.Length; i += 2)
            {
                cords[i] += vector.X;
                cords[i + 1] += vector.Y;
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, BufferID);//тип буфера, индефикатор
            GL.BufferData(BufferTarget.ArrayBuffer, cords.Length * sizeof(float), cords, BufferUsageHint.StaticDraw);//тип, байты, данны, тип доступа
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
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

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Rgba,
                PixelType.UnsignedByte,
                data.Scan0);

            bmp.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            return id;
        }
    }
}
