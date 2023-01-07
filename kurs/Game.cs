using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kurs
{
    class Game : GameWindow
    {
        Blok blok1;
        Blok blok2;
        //Ball ball1;
        Ball ball2;
        Random rnd;
        List<Blok> blocks = new List<Blok>();
        int dx, dy;
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
            //ball1 = new Ball(35,0.1f ,0, 0, Color4.Green);
            ball2 = new Ball(-1f, -1f, 0.1f, 0.1f, Color4.Yellow,true);
            rnd = new Random();
            dx = rnd.Next(-3, 3);
            dy = 1;
        }
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            if (KeyboardState.IsKeyDown(Keys.Escape))
                Close();
            //if (KeyboardState.IsKeyDown(Keys.Right))
            //    ball1.MoveRight();
            if (KeyboardState.IsKeyDown(Keys.Right))
                ball2.Move(new Vector2(0.01f,0));
            if (KeyboardState.IsKeyDown(Keys.Left))
                ball2.Move(new Vector2(-0.01f, 0));
            if (KeyboardState.IsKeyDown(Keys.Down))
                ball2.Move(new Vector2(0, -0.01f));
            if (KeyboardState.IsKeyDown(Keys.Up))
                ball2.Move(new Vector2(0, 0.01f));
            if (KeyboardState.IsKeyDown(Keys.E))
                dx = rnd.Next(-3, 3);



            if (KeyboardState.IsKeyDown(Keys.R))
            {
                ball2.Move(new Vector2(dx/1000f, dy/1000f));
            }
            CheackAllBlocks();




        }
        protected override void OnUnload()
        {
            base.OnUnload();
            blok1.DeleteVBO();
            blok2.DeleteVBO();
            //ball1.DeleteVBO();
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
            //ball1.DrawVBO();
            ball2.DrawVBO();
            SwapBuffers();
        }


        private void CheackAllBlocks()
        {
            foreach (var block in blocks)
            {
                for (int i = 0; i < ball2.Cords.Length; i += 2)
                {
                    if (block.IsPointIn(ball2.Cords[i], ball2.Cords[i + 1]))
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
            float centerBallX = ball2.X + ball2.Width / 2; float centerBallY = ball2.Y + ball2.Height / 2; // центр вписанной окружности в снаряд
            float x = Math.Abs(centerBlokX - centerBallX); float y = Math.Abs(CenterBlockY - centerBallY); // расчёт расстояния между центрами
            if(x>y) // определение плоскости столкновения в данном случае если true то это либо право либо лево
            {
                if (centerBallX > centerBlokX) // справа
                    dx = Math.Abs(dx);
                else
                    dx = -Math.Abs(dx); // слева
            }
            else
            {
                if (centerBallY> CenterBlockY) // низ
                    dy = Math.Abs(dy);
                else // вверх
                    dy = -Math.Abs(dy);
            }
        }
        
        
    }
    public class Blok
    {
        float x, y, height, width;
        Color4 color;
        int BufferID;
        bool status = true;

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
            float[] vertex = { x, y, x, y + height, x + width, y + height, x + width, y };
            BufferID = GL.GenBuffer();// генерация индефикатора
            GL.BindBuffer(BufferTarget.ArrayBuffer, BufferID);//тип буфера, индефикатор
            GL.BufferData(BufferTarget.ArrayBuffer, vertex.Length * sizeof(float), vertex, BufferUsageHint.StaticDraw);//тип, байты, данны, тип доступа
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
        public bool IsPointIn(float pointX,float pointY)
        {
            if (pointX < x) return false;
            if (pointY < y) return false;
            if (pointX > x + width) return false;
            if (pointY > y + height) return false;
            return true;
        }

    }

    public class Ball
    {
        int kolvo, BufferID;
        //float r, xCentr, yCentr, x, y;
        float[] cords;
        float width, height;
        Color4 color;

        public float[] Cords { get => cords; set => cords = value; }
        public float X { get => cords[0]; set => cords[0] = value; }
        public float Y { get => cords[1]; set => cords[1] = value; }
        public float Width { get => width; set => width = value; }
        public float Height { get => height; set => height = value; }

        //public Ball(int kolvo, float r, float xCentr, float yCentr, Color4 color)
        //{
        //    this.kolvo = kolvo;
        //    this.r = r;
        //    this.xCentr = xCentr;
        //    this.yCentr = yCentr;
        //    this.color = color;
        //    List<float> temp = new List<float>() { }; // Создаем список куда перенесем все координаты
        //    for (int i = -1; i < kolvo; i++)
        //    {
        //        float a = ((float)Math.PI * 2) / kolvo;
        //        x = (((float)Math.Sin(a * i) * r)) + xCentr;
        //        y = (((float)Math.Cos(a * i) * r)) + yCentr;
        //        temp.Add(x);
        //        temp.Add(y);
        //    }
        //    cords = temp.ToArray(); // превращаем список в постоянный массив
        //    BufferID = GL.GenBuffer();// генерация индефикатора
        //    GL.BindBuffer(BufferTarget.ArrayBuffer, BufferID);//тип буфера, индефикатор
        //    GL.BufferData(BufferTarget.ArrayBuffer, cords.Length * sizeof(float), cords, BufferUsageHint.StaticDraw);//тип, байты, данны, тип доступа
        //    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        //}
        public Ball(float x, float y, float width, float height, Color4 color,bool s)
        {
            this.width = width;
            this.height = height;
            this.color = color;
            cords = new float[] { x, y, x, y + height, x + width, y + height, x + width, y };
            BufferID = GL.GenBuffer();// генерация индефикатора
            GL.BindBuffer(BufferTarget.ArrayBuffer, BufferID);//тип буфера, индефикатор
            GL.BufferData(BufferTarget.ArrayBuffer, cords.Length * sizeof(float), cords, BufferUsageHint.StaticDraw);//тип, байты, данны, тип доступа
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
        //public void DrawVBO()
        //{
        //    GL.EnableClientState(ArrayCap.VertexArray);
        //    GL.BindBuffer(BufferTarget.ArrayBuffer, BufferID);

        //    GL.VertexPointer(2, VertexPointerType.Float, 0, 0);

        //    GL.Color4(color);
        //    GL.DrawArrays(PrimitiveType.Polygon, 0, kolvo);

        //    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        //    GL.DisableClientState(ArrayCap.VertexArray);
        //}
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
}
