using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using System.Drawing;

namespace kurs
{
    class Game : GameWindow
    {
        Blok blok1, blok2;
        Ball ball;
        Paddle paddle;
        Random rnd;
        List<Blok> blocks = new List<Blok>();
        int dx, dy;
        bool gameOver = false;
        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeSettings)
        : base(gameWindowSettings, nativeSettings)
        {

        }
        protected override void OnLoad()
        {
            VSync = VSyncMode.On;
            base.OnLoad();
            blok1 = new Blok(0f, 0f, 0.5f, 0.5f, Color4.DarkOrange);
            blok2 = new Blok(-1f, 0f, 0.5f, 0.5f, Color4.DarkOrange);
            blocks.Add(blok1);
            blocks.Add(blok2);
            ball = new Ball(-0.1f, -0.80f, 0.1f, 0.1f, Color4.Yellow);
            paddle = new Paddle(-0.3f, -0.97f, 0.6f, 0.05f, Color4.Aqua);
            rnd = new Random();
            dx = rnd.Next(-3, 3);
            dy = 1;
        }
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            if (!gameOver)
            {
                if (KeyboardState.IsKeyDown(Keys.Escape))
                    Close();
                if (KeyboardState.IsKeyDown(Keys.Right))
                    ball.Move(new Vector2(0.01f, 0));
                if (KeyboardState.IsKeyDown(Keys.Left))
                    ball.Move(new Vector2(-0.01f, 0));
                if (KeyboardState.IsKeyDown(Keys.Down))
                    ball.Move(new Vector2(0, -0.01f));
                if (KeyboardState.IsKeyDown(Keys.Up))
                    ball.Move(new Vector2(0, 0.01f));
                if (KeyboardState.IsKeyDown(Keys.E))
                    dx = rnd.Next(-3, 3);



                if (KeyboardState.IsKeyDown(Keys.R))
                {
                    ball.Move(new Vector2(dx / 1000f, dy / 1000f));
                }
                CheackAllBlocks();
                BallCollisionOnWallsAndPaddleChecker();
            }




        }
        protected override void OnUnload()
        {
            base.OnUnload();
            blok1.DeleteVBO();
            blok2.DeleteVBO();
            ball.DeleteVBO();
            paddle.DeleteVBO();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.LineWidth(9.0f);
            //Random rnd = new Random();
            //for(float i =0.1f;i<1.0f;i+=0.1f)
            //{
            //    //GL.Color3(rnd.Next(0,255)/255f, rnd.Next(0, 255)/255f, rnd.Next(0, 255)/255f);
            //    GL.Begin(PrimitiveType.Polygon);
            //        GL.Vertex2(i, 0f);
            //        GL.Vertex2(i+0.1f, 0f);
            //        GL.Vertex2(i+0.1f, 0.5f);
            //        GL.Vertex2(i, 0.5f);
            //    GL.End();
            //}
            blok1.DrawVBO();
            blok2.DrawVBO();
            ball.DrawVBO();
            paddle.DrawVBO();
            SwapBuffers();
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
            if (ball.Cords[4] >= 1) // столкновение с границей справа
                dx = -Math.Abs(dx);
            if (ball.Cords[0] <= -1) //столкновение с границей слева
                dx = Math.Abs(dx);
            if (ball.Cords[3] >= 1) // сверху
                dy = -Math.Abs(dy);
            if (ball.Cords[1] <= -1) // что делаем если шарик упал
            {
                //gameOver = true;
            }

            for (int i = 0; i < ball.Cords.Length; i+=2)
            {
                if (paddle.IsPointIn(ball.Cords[i], ball.Cords[i + 1]))
                {
                    //Debug.WriteLine(DateTime.Now.ToString());
                    //float centerBlokX = paddle.X + paddle.Width / 2; float CenterBlockY = paddle.Y + paddle.Height / 2; // центр вписанной окружности в блок
                    //float centerBallX = ball.X + ball.Width / 2; float centerBallY = ball.Y + ball.Height / 2; // центр вписанной окружности в снаряд
                    //float x = Math.Abs(centerBlokX - centerBallX); float y = Math.Abs(CenterBlockY - centerBallY); // расчёт расстояния между центрами
                    //if (x > y) // определение плоскости столкновения в данном случае если true то это либо право либо лево
                    //{
                    //    if (centerBallX > centerBlokX) // справа
                    //        dx = Math.Abs(dx);
                    //    else
                    //        dx = -Math.Abs(dx); // слева
                    //}
                    //if (ball.Y == paddle.Y + paddle + paddle.Height)
                    //{
                    //    Debug
                    //}

                    //if (ball.X > paddle.X && ball.X < paddle.X + paddle.Width / 3)
                    //{
                    //    Debug.WriteLine("Первая треть");
                    //}
                    //else if (ball.X > paddle.X + paddle.Width / 3 && ball.X < paddle.X + 2 * paddle.Width / 3)
                    //{
                    //    Debug.WriteLine("Вторая треть");
                    //} 
                    //else if (ball.X > paddle.X + 2 * paddle.Width / 3 && ball.X < paddle.X + paddle.Width)
                    //{
                    //    Debug.WriteLine("Третья треть");
                    //}
                    if (ball.X > paddle.X && ball.X < paddle.X + paddle.Width / 3 || ball.X+ball.Width > paddle.X && ball.X+ ball.Width < paddle.X + paddle.Width / 3)
                    {
                        Debug.WriteLine("Первая треть"); dy = Math.Abs(dy); dx = -Math.Abs(dx);
                    }
                    else if (ball.X > paddle.X + paddle.Width / 3 && ball.X < paddle.X + 2 * paddle.Width / 3 || ball.X + ball.Width > paddle.X + paddle.Width / 3 && ball.X + ball.Width < paddle.X + 2 * paddle.Width / 3)
                    {
                        Debug.WriteLine("Вторая треть"); dy = Math.Abs(dy); dx = 0;
                    }
                    else if (ball.X > paddle.X + 2 * paddle.Width / 3 && ball.X < paddle.X + paddle.Width || ball.X + ball.Width > paddle.X + 2 * paddle.Width / 3 && ball.X + ball.Width < paddle.X + paddle.Width)
                    {
                        Debug.WriteLine("Третья треть"); dy = +Math.Abs(dy); dx = +Math.Abs(dx);
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
        public float X { get => x; set => x = value; }
        public float Y { get => y; set => y = value; }
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

            GL.VertexPointer(2, VertexPointerType.Float, 0, 0);

            GL.Color4(color);
            GL.DrawArrays(PrimitiveType.Polygon, 0, 4);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DisableClientState(ArrayCap.VertexArray);
        }
        public void DeleteVBO()
        {
            GL.DeleteBuffer(BufferID);
        }
        public bool IsPointIn(float pointX, float pointY)
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
        //int BufferID;
        //float[] cords;
        //float width, height;
        //Color4 color;

        public float[] Cords { get => cords; set => cords = value; }
        //public float X { get => cords[0]; set => cords[0] = value; }
        //public float Y { get => cords[1]; set => cords[1] = value; }
        //public float Width { get => width; set => width = value; }
        //public float Height { get => height; set => height = value; }
        //public Ball(float x, float y, float width, float height, Color4 color)
        //{
        //    this.width = width;
        //    this.height = height;
        //    this.color = color;
        //    cords = new float[] { x, y, x, y + height, x + width, y + height, x + width, y };
        //    BufferID = GL.GenBuffer();// генерация индефикатора
        //    GL.BindBuffer(BufferTarget.ArrayBuffer, BufferID);//тип буфера, индефикатор
        //    GL.BufferData(BufferTarget.ArrayBuffer, cords.Length * sizeof(float), cords, BufferUsageHint.StaticDraw);//тип, байты, данны, тип доступа
        //    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        //}
        public Ball(float x, float y, float width, float height, Color4 color) : base (x,y,width,height,color)
        {

        }
        //public void DrawVBO()
        //{
        //    GL.EnableClientState(ArrayCap.VertexArray);
        //    GL.BindBuffer(BufferTarget.ArrayBuffer, BufferID);

        //    GL.VertexPointer(2, VertexPointerType.Float, 0, 0);

        //    GL.Color4(color);
        //    GL.DrawArrays(PrimitiveType.Polygon, 0, 4);

        //    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        //    GL.DisableClientState(ArrayCap.VertexArray);
        //}
        //public void DeleteVBO()
        //{
        //    GL.DeleteBuffer(BufferID);
        //}
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
        public Paddle(float x, float y, float width, float height, Color4 color) : base(x,y,width,height,color)
        {

        }
        //int BufferID;
        //float[] cords;
        //Color4 color;
        //float width, height;

        //public float Width { get => width; set => width = value; }
        //public float Height { get => height; set => height = value; }
        //public float X { get => cords[0]; set => cords[0] = value; }
        //public float Y { get => cords[1]; set => cords[1] = value; }

        //public Paddle(float x, float y, float width, float height, Color4 color)
        //{
        //    this.height = height;
        //    this.width = width;
        //    this.color = color;
        //    cords = new float[] { x, y, x, y + height, x + width, y + height, x + width, y };
        //    BufferID = GL.GenBuffer();// генерация индефикатора
        //    GL.BindBuffer(BufferTarget.ArrayBuffer, BufferID);//тип буфера, индефикатор
        //    GL.BufferData(BufferTarget.ArrayBuffer, cords.Length * sizeof(float), cords, BufferUsageHint.StaticDraw);//тип, байты, данны, тип доступа
        //    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        //}
        //public void DrawVBO()
        //{
        //    GL.EnableClientState(ArrayCap.VertexArray);
        //    GL.BindBuffer(BufferTarget.ArrayBuffer, BufferID);

        //    GL.VertexPointer(2, VertexPointerType.Float, 0, 0);

        //    GL.Color4(color);
        //    GL.DrawArrays(PrimitiveType.Polygon, 0, 4);

        //    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        //    GL.DisableClientState(ArrayCap.VertexArray);
        //}
        //public void DeleteVBO()
        //{
        //    GL.DeleteBuffer(BufferID);
        //}
        //public void Move(Vector2 vector)
        //{
        //    for (int i = 0; i < cords.Length; i += 2)
        //    {
        //        cords[i] += vector.X;
        //        cords[i + 1] += vector.Y;
        //    }
        //    GL.BindBuffer(BufferTarget.ArrayBuffer, BufferID);//тип буфера, индефикатор
        //    GL.BufferData(BufferTarget.ArrayBuffer, cords.Length * sizeof(float), cords, BufferUsageHint.StaticDraw);//тип, байты, данны, тип доступа
        //    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        //}
        //public bool IsPointIn(float pointX, float pointY)
        //{
        //    if (pointX < cords[0]) return false;
        //    if (pointY < cords[1]) return false;
        //    if (pointX > cords[0] + width) return false;
        //    if (pointY > cords[1] + height) return false;
        //    return true;
        //}
    }
}