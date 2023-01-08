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
        Ball ball;  // Создаём мяч, но не выделяем под него память. Просто говорим компилятору что будет такая переменная чтобы спокойно ею пользоваться в пределах класса
        Paddle paddle;// Создаём отскок-панельку, но не выделяем под него память. Просто говорим компилятору что будет такая переменная чтобы спокойно ею пользоваться в пределах класса
        Random rnd;// Создаём рандомайзер, но не выделяем под него память. Просто говорим компилятору что будет такая переменная чтобы спокойно ею пользоваться в пределах класса
        List<Blok> blocks = new List<Blok>(); //Создаем динамический массив в котором будем хранить блоки
        List<Blok> uberBlocks = new List<Blok>();//Создаем динамический массив в котором будем хранить неразрушаемые блоки
        List<Button> buttons = new List<Button>(); //Создаем динамический массив в котором будем хранить кнопки для менюшек, крмое кнопок гейм овера и победы
        Button gameOverButton; // Создаем кнопку гейм овер
        Button gameWinButton; // Создаем кнопку гейм вина 
        int dx, dy, ortoWidth = 600, ortoHeight = 800, ballTextureID, brickTextureID,uberBrickTextureID, StartGameID, CloseGameID, Level1ID,Level2ID,scores, GameOverID,GameWinID; //создаём кучу разных переменных dx dy -преращения для измненеия координат шарика ortoWidth и ortoHeight это размеры сетки игрового поля - пределы координатных осей типа, всё что с ID в конце - айдишники дял текстур, score - очки, отобразаются в названии формы
        double deltaX, deltaY; // коэфициенты для перерасчёта координат экрана к координатам формы. Считаются в OnResize. 
        Vector2 cursor; // Вектор, которых хранит координаты курсора мыши с учетом deltaX deltaY. Считается в OnMouseMove.
        bool gameOver = false, isPaused = false, levelChoose=false,level=false, clearButtons=false, gameWin=false; // 1-Флаг показывающий что игра проиграна, 2- что игра на паузе 3- что человек начал выбирать уровень и нужно переключить менюшку 4) что чел выбрал уровень(не важно какой) 5) что нужно очистить лист от старых кнопок(используется в момент когда сменился экран и нужно новые кнопки отобразить) 6) показывает что чел выиграл игру
        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeSettings)
        : base(gameWindowSettings, nativeSettings)
        {

        }
        protected override void OnLoad() // Данный метод вызывается один раз при запуска игры и производит первоначальную настройку.
        {
            GL.MatrixMode(MatrixMode.Projection); //Как я понял тут выбирается локальная матрица над которой сейчас будет работа происходить.
            GL.LoadIdentity(); // Загружаем матрицу по умолчанию. Вроде бы единичная матрица
            GL.Ortho(0, ortoWidth, 0, ortoHeight, -1, 1); // 0;0 находится в левом нижнем углу. У направлена вверх, х - направо. Перемножаю матрицу на новую(новая матрица задаётся в скобках). И так получаю как бы другую матрицу которая и становится координатными осями.
            GL.MatrixMode(MatrixMode.Modelview); // Выбираю снова глобальную матрицу и типо все изменения выше на неё переносятся. 

            ballTextureID = ContentPipe.LoadTexture(@"Content\ball.png"); // Загружаю текстуру в видеопамять и сохраняю её айди. Текстура мячика
            brickTextureID = ContentPipe.LoadTexture(@"Content\brick.jpg"); // Текстура кирпича
            uberBrickTextureID = ContentPipe.LoadTexture(@"Content\UberBlock.png"); // Текстура неломаемого кирпича
            StartGameID = ContentPipe.LoadTexture(@"Content\Start.png"); // Текстура кнопки Старт
            CloseGameID = ContentPipe.LoadTexture(@"Content\Close.png"); // Текстура кнопки Закрыть
            Level1ID = ContentPipe.LoadTexture(@"Content\1Level.png"); // Текстура кнопки выбрать первый уровень
            Level2ID = ContentPipe.LoadTexture(@"Content\2Level.png"); // Текстура кнопки выбрать второй уровень

            GameOverID = ContentPipe.LoadTexture(@"Content\Lose.png"); // Текстура кнопки Проиграл
            GameWinID = ContentPipe.LoadTexture(@"Content\Win.png"); // Текстура кнопки Победил
            gameOverButton = new Button(230, 500, 100, 100, GameOverID); // Вот выше мы сказали что у нас будет кнопка, а тут мы ей выделили память (слово new означает выделить память) и вызвали конструктор.
            gameOverButton.OnMouseDown += ReturnToMainMenu; // К делегату внутри кнопки привязали Метод который находится в классе игры.
            gameWinButton = new Button(230, 500, 100, 100, GameWinID);
            gameWinButton.OnMouseDown += ReturnToMainMenu;
            VSync = VSyncMode.On; // Включили ограничение скорость игры в 60 фпс (или в стоклько фпс колько у монитора пользователя частота развёртки). Нужно чтобы игра не игралась быстрее, иначе бы шарик летал с космической скрость и нужно было бы придумывать как это ограничить.
            base.OnLoad(); // Мы как бы перегрузили метод от наследуемого класса GameWindow (посмотри на самый вверх,  где мы только только начали в классе писать что-то) и тут обращаемся к этому же методу, но к его реализации в базовом классе. То есть сначала выполнится весь тот код который мы написали выше, а потом выполнится весь тот код который написал в методе OnLoad класса GameWindow. Обратится к чему-то от базового класса можно с помощью ключевого слова base. это типо как this для твоего класса но для обращение не к себе а к родителю
            
            rnd = new Random(); // Выделяем память у рандомайзера и вызываем его конструктор
            
            StartScreen(); // Запускается метод который создаёт стартовые две кнопки Start и Close
        }
        protected override void OnUpdateFrame(FrameEventArgs args) // Данный метод вызывается в бесконечном цикле и в нём нужно обрабатывать влю логику игры - передвижения итд. Сразу после UpdateFrame вызывается RenderFrame - поэтому все изменения в логике игры тут же отрисуются
        {
            base.OnUpdateFrame(args); // Обращение к методы базового класса
            if(clearButtons) // Если нужно очистить массив кнопок - очищаем его
            {
                buttons.Clear(); // Сама очистка
                clearButtons = false; // очистку выполнили больше повторять не нужно. 
            }
            if(levelChoose) // Если пользователь перешел к следующему меню тут наверзу будет true и нужно создать кнопки для выбора уровня и выхода 
            {
                buttons.Add(new Button(230, 500, 100, 100,  Level1ID)); // Создаём в недавно очищенном массиве новые кнопки - 1 и 2 уровни
                buttons.Add(new Button(230, 300, 100, 100, Level2ID));
                buttons.Add(new Button(230, 100, 100, 100, CloseGameID)); // кнопка выхода
                buttons[0].OnMouseDown += Level1; // Подключаем к кнопке метод из класса Game который нужно будет выполнить при нажатии на кнопку. Само нажатие проверяется в другом месте. Подключение происходит с помощью прибавления к делегату ссыллки на метод.
                buttons[1].OnMouseDown += Level2;
                buttons[2].OnMouseDown += CloseGame;
                levelChoose = false; // Раз мы уже создали кнопки то ещё раз этот if выполнять не нужно, поэтому меняем флаг на false 
            }
            if(level) // Если чел выбрал уровень то начинаем обрабатывать логику игры - двигать мяч, проверять коллизию(по русски столкновение) с блоками стенами и отражалкой).
            {
                GameMaster(); // Соответственно вызываем функцию где весь "контент" по обработке логики
            } 
        }
        protected override void OnRenderFrame(FrameEventArgs args) //Метод вызывается в цикле и в нём происходит отрисовка всего что нужно отрисовать.
        {
            base.OnRenderFrame(args); // обращается в методу базового класса
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); // Очищаем буфер от цвета и того, что рисовали в прошлом кадре, иначе бы кадры друг на друга наслаивались бы( посмотри на ютубе что происходило с окнами программы в Windows XP если они зависали - они при передвижении свой след оставляли)
            if(level) // Если чел выбрал уровень то рисуем все объекты
            {
                GameMasterRender(); // Функция в которой все объекты именно игры рисуются
            }
            ButtonRender();  // Рисуем все кнопки. Если чел играет уже в урвоень то массив кнопок пуст, потому никакие кнопки не рисуются.
            SwapBuffers(); // Выше всё что орисовалось оно как бы рисовалось не на экран сразу, а рисовалось в некоторый буфер, теперь же кадр на экране и буфер меняются местами. и дальше по новой начинается подготовка и рисовка следующего кадра в буфер.
        }
        protected override void OnKeyDown(KeyboardKeyEventArgs e) // Метод вызывается как только человек нажмёт на клавишу
        {
            base.OnKeyDown(e); // обращается в методу базового класса
            if (gameOver || gameWin)// Если игра проиграна или выиграна то мы максимум реагируем на нажатие Escape чтобы человек выше из игры 
            {
                if (e.Key == Keys.Escape) // Провермяем какая клавиша нажата и вызываем соответствующее действие
                    CloseGame(); // Метод который закрывает игру
                return; // Если человек проиграл или выиграл игру и нажимает просто так на кнопки то нам даже не нужно проверять какие кнопки он нажимает и мы резко выходим из метода
            }
            else // Если игра не проиграна и не выиграна - то есть играется ещё то можно и обработать кнопки 
            {
                switch (e.Key)
                {
                    case Keys.A:
                        if (!isPaused)
                        {
                            paddle.Move(new Vector2(-8f, 0)); // если чел нажал клавишу английскую А то двигаем платформу влево
                        }
                        break;
                    case Keys.D:
                        if (!isPaused)
                        {
                            paddle.Move(new Vector2(8f, 0)); // вправо
                        }
                        break;
                    case Keys.P:
                        isPaused = !isPaused; // переключаем флаг паузы на противоположный
                        break;
                    case Keys.Escape:
                        CloseGame();
                        break;
                }
            }
        }
        protected override void OnUnload() // Метод вызывается автоматически при закрытии окна игры
        {
            base.OnUnload(); // обращается в методу базового класса
            foreach (var blok in blocks) // проходимся по всем уцелевшим блокам и очищаем у них буферы в видеокарте
                blok.Delete();
            foreach (var blok in uberBlocks) // проходимся по всем неразрушаемым блокам и очищаем у них буферы в видеокарте
                blok.Delete();
            if (ball != null || paddle!=null) // если мяч и отражалка существуют то очищаем их (они могут не существовать если игрок запустил приложение но ни разу не зашел в уровень, ведь эти две штуки создаются только по клику на уровень 1 или урвоень 2
            {
                ball.Delete();
                paddle.Delete(); 
            }
            GL.DeleteTexture(ballTextureID); // удаляем все текстуры в видеопамяти
            GL.DeleteTexture(brickTextureID);
            GL.DeleteTexture(uberBrickTextureID);
            GL.DeleteTexture(StartGameID);
            GL.DeleteTexture(CloseGameID);
            GL.DeleteTexture(Level1ID);
            GL.DeleteTexture(Level2ID);
            GL.DeleteTexture(GameOverID);
            GL.DeleteTexture(GameWinID);
            gameWinButton.Delete(); //  очищаем буфер кнопок выиграл игру и проиграл
            gameOverButton.Delete();
            foreach (var b in buttons) // на всякий случаей если ВДРУГ КАКИМ-ТО ЧУДОМ остались кнопки в массив - очищаем их.
                b.Delete();
        }
        protected override void OnResize(ResizeEventArgs e) // Метод вызывается при изменениее размеров окна
        {
            deltaX = e.Width / (float)ortoWidth; // Считаем коэфициент во сколько раз отличается ширина окна от ширины игрового поля
            deltaY = e.Height / (float)ortoHeight;// Считаем коэфициент во сколько раз отличается высота окна от высоты игрового поля
            base.OnResize(e); // Обращаемся в базовму методу
            GL.Viewport(0, 0, e.Width, e.Height); // тут пересчитывается viewport - типо когда мы изменяем размер окна именно тут все блоки увеличиваются или уменьшаются ну крч под размер подстариваются
        }
        protected override void OnMouseMove(MouseMoveEventArgs e) // Вызывается при движении мышки
        {
            base.OnMouseMove(e);// Обращаемся в базовму методу
            cursor = new Vector2((float)(e.Position.X / deltaX), (float)ortoHeight - (float)(e.Position.Y / deltaY)); //тут мы считаем координаты курсора с учетом коэфициентов, а ось Y отражаем, потому что экранная ось У смотри сверху вниз, а игровая снизу вверх
        }
        protected override void OnMouseDown(MouseButtonEventArgs e) // Вызывается когда нажимаешь клавишу мыши
        {
            base.OnMouseDown(e);// Обращаемся в базовму методу
            foreach (var but in buttons) // Проходимся по всем кнопкам в  массиве кнопок. var - копмлпиятор сам выберет какой там тип данных. В этом случае равносильно Button but in buttons. 
            {
                if(but.IsPointIn(cursor.X,cursor.Y)) // Проверяем для кнопки нажится ли курсор на ней ( заметь что мы тпо уже кликнули мышкокй, тут типо проверяем а попали ли мы на кнопку)
                {
                    but.OnMouseDown?.Invoke(); // Если попали то тут мы говорим делегату выполнить все методы которые к нему прибавили. А если методов нет т.е. делегат равен null то ничего не делать. за это отвечает знак "?" 
                }
            }
            if (gameOver) // если игра проиграна то проверяем кнопку проигрыша аналогично тому как выше
                if (gameOverButton.IsPointIn(cursor.X, cursor.Y))
                    gameOverButton.OnMouseDown?.Invoke();
            if (gameWin)// если игра выиграна то проверяем кнопку выигрыша аналогично тому как выше
                if (gameOverButton.IsPointIn(cursor.X, cursor.Y))
                    gameOverButton.OnMouseDown?.Invoke();
        }
        private void CheackAllBlocks() // функция проверяющая поподание по блоку
        {
            foreach (var block in blocks) // проходимя по всем блоками
            {
                for (int i = 0; i < ball.Cords.Length; i += 2) // по всем координатам, причем из массива берём по двойкам
                {
                    if (block.IsPointIn(ball.Cords[i], ball.Cords[i + 1])) // Проверяем находится ли данная точка шарика в блоке и если да то....
                    {
                        block.Status = false; // Переключаем стату блока на фолс - что будет означать что блок нужно удалить из массива 
                        ChangeBallVector(block); // меняем направление шарика в зависимости от того в какую часть блока попали
                    }
                }
            }
            foreach (var block in uberBlocks) // проходимя по всем  нелоаемым блоками
            {
                for (int i = 0; i < ball.Cords.Length; i += 2) // по всем координатам, причем из массива берём по двойкам
                {
                    if (block.IsPointIn(ball.Cords[i], ball.Cords[i + 1])) // Проверяем находится ли данная точка шарика в блоке и если да то....
                    {
                        ChangeBallVector(block);// меняем направление шарика в зависимости от того в какую часть блока попали и не помечаем блоки для удаления.
                    }
                }
            }
        }
        private void ChangeBallVector(Blok currentBlok) // функция которая меняет вектора шарик в зависимости куда он врезался в блок
        {
            float centerBlokX = currentBlok.X + currentBlok.Width / 2; float CenterBlockY = currentBlok.Y + currentBlok.Height / 2; // центр вписанной окружности в блок
            float centerBallX = ball.X + ball.Width / 2; float centerBallY = ball.Y + ball.Height / 2; // центр вписанной окружности в снаряд
            float x = Math.Abs(centerBlokX - centerBallX); float y = Math.Abs(CenterBlockY - centerBallY); // расчёт расстояния между центрами
            if (x > y) // определение плоскости столкновения в данном случае если true то это либо право либо лево. Можешь вот просто самостоятельн в на бумаге два прямоугольника нарисовать и пробежаться по этому алгоритму чтобы понять как он работает
            {
                if (centerBallX > centerBlokX) // справа или слева
                    dx = Math.Abs(dx); // справа - скорость по оси Х делаем положительной чтобы шарик в другую сторону полетел
                else
                    dx = -Math.Abs(dx); // слева - скорость по оси Х нужно сменить на другую чтобы шарик в другую сторону полетел 
            }
            else
            {
                if (centerBallY > CenterBlockY) // низ или верх
                    dy = Math.Abs(dy);// низ вертикальную скорость направляем вверх 
                else // вверх
                    dy = -Math.Abs(dy); // вверх - вертикальную меняем на противоположную.
            }
        }
        private void BallCollisionOnWallsAndPaddleChecker() // Проверяем столкновение со стенками и отражалкой внизу 
        {
            if (ball.Cords[4] >= ortoWidth) // столкновение с границей справа
                dx = -Math.Abs(dx); // отправляем шарик влево
            if (ball.Cords[0] <= 0) //столкновение с границей слева
                dx = Math.Abs(dx); // отправляем шарик вправо
            if (ball.Cords[3] >= ortoHeight) // сверху
                dy = -Math.Abs(dy); // отправляем шарик вниз
            if (ball.Cords[1] <= 0) // что делаем если шарик упал
            {
                gameOver = true; // делаем флаг проигрыша игры на true
            }

            for (int i = 0; i < ball.Cords.Length; i += 2) // проходимся по координатм фигуры причем берем по две 
            {
                if (paddle.IsPointIn(ball.Cords[i], ball.Cords[i + 1])) // проверяем попала ли точка на отражалку
                {
                    float ballCenterX = ball.X + ball.Width / 2; // находим центр шара, но только координату Х центра
                    if (ballCenterX > paddle.X && ballCenterX < paddle.X + paddle.Width / 3) // проверяем находится ли центр шара правее самой левой точки отражалки и левее трети отражалки
                    {
                        Debug.WriteLine("Первая треть"); dy = 1; dx = -3; // если да то отправляем шарик по диагонали влево
                    }
                    else if (ballCenterX > paddle.X + paddle.Width / 3 && ballCenterX < paddle.X + 2 * paddle.Width / 3)// проверяем находится ли центр шара правее  трети отражалки и левее двух третей
                    {
                        Debug.WriteLine("Вторая треть"); dy = 1; dx = 0; // отправляем шарик наверх
                    }
                    else if (ballCenterX > paddle.X + 2 * paddle.Width / 3 && ballCenterX < paddle.X + paddle.Width) // проверяем находится ли центр шара правее  двух третей отражалки и левее самой правой точки отражалки
                    {
                        Debug.WriteLine("Третья треть"); dy = +1; dx = +1;// если да то отправляем шарик по диагонали вправо
                    }
                    else if (ball.X > paddle.X + 2 * paddle.Width / 3 && ball.X < paddle.X + paddle.Width) // проверяем находится самая левая точка шара правее  двух третей отражалки и левее самой правой точки отражалки. На тот случай если центр шара не попал на плаформу, но сам шарик же шире чем центр, поэтому будем еще проверять крйние точки шара
                    {
                        Debug.WriteLine("Третья треть"); dy = +1; dx = +1; // если да то отправляем шарик по диагонали вправо
                    }
                    else if (ball.X + ball.Width > paddle.X && ball.X < paddle.X + paddle.Width / 3) // проверяем находится самая правая точка шара правее самой левой точки платформы и левее трети отражалки.
                    {
                        Debug.WriteLine("Первая треть"); dy = 1; dx = -3; // если да то отправляем шарик по диагонали влево
                    }

                }
            }

        }
        private void ButtonRender() // Метод для отрисовки всех кнопок
        {
            foreach (var but in buttons)
            {
                but.Draw();
            }
        }
        private void GameMaster() // Метод, хранящий всю игровую логику.
        {
            if (!isPaused) // если игра не на паузе
            {
                if (!gameOver && !gameWin) // если игра не проиграна или не выиграна
                {
                    Blok delBlock = null; // Создаём блок для удаления но не назначаем ему никакой блок
                    foreach (var blok in blocks) // Проходимся по всем блокам
                    {
                        if (blok.Status == false) // если блок помечен как для удаления 
                        {
                            delBlock = blok; // то наш блок для удаления становится этим блоком
                             
                        }
                    }
                    if (delBlock != null)// соответственно если нашли блок для удаления
                    {
                        delBlock.Delete(); // очищаем буферы видеокарты блока
                        blocks.Remove(delBlock); // удаляем блок из массива
                        scores += 10; // добавляем очки
                        Title = "Acranoid           Scores: " + scores.ToString(); // отображаем очки в названии формы
                        delBlock = null; // блок для удаления снова пуст. Почему не удалять блок сразу где мы его находим? Потому что мы там используем цикл foreach а если мы вызовем у листа, который находится в foreach метод delete, но размер листа изменится, что ломает работу foreach, поэтому он там жалуется в духе мол коллекция была изменения я дальше работать не буду. И из-за этой особености foreach мы удаляем блок уже вне цикла. Да и вообще лучше изменять массивы( имею в виду удалять из них инфу) когда вышел из цикла работы с массивом, чтобы ничего не сломать другого.
                    }
#if DEBUG // следущий блок кода будет работать только если установлен режим debug (сверху левее хеленой стрелочки запуска проекта, левее Any CPU
                    if (KeyboardState.IsKeyDown(Keys.Right)) // если нажата стрелка вправо - сдвинуть мяч вправо
                        ball.Move(new Vector2(2f, 0));
                    if (KeyboardState.IsKeyDown(Keys.Left))
                        ball.Move(new Vector2(-2f, 0));
                    if (KeyboardState.IsKeyDown(Keys.Down))
                        ball.Move(new Vector2(0, -2f));
                    if (KeyboardState.IsKeyDown(Keys.Up))
                        ball.Move(new Vector2(0, 2f));
                    if (KeyboardState.IsKeyDown(Keys.E)) // если нажата кнопка е - сменить вектор по оси ох на случайный от -3 до 2 включительно
                        dx = rnd.Next(-3, 3);
#endif
                    ball.Move(new Vector2(dx / 2f, dy / 2f)); // сдвинуть мяч по координатам который мы считали раньше. делим на 2 потому что если просто dх то там скрость чет высокая на мой взгляд

                    CheackAllBlocks(); // метод для проверки столкновения с блоками
                    BallCollisionOnWallsAndPaddleChecker(); // метод для проверки столкновения со стенками и отражалкой
                    if (blocks.Count == 0) // если количество блоков в массиве равно 0 значит мы всё уничтожили и значит мы победиди
                    {
                        gameWin = true;
                    }

                }
            }
        }
        private void GameMasterRender() // функция для рендера всех блоков и финальных кнопок победы или проигрыша
        {
            foreach (var blok in blocks) //рисуем все блоки
                blok.Draw();
            foreach (var blok in uberBlocks) // рисуем все неразрушаемые блоки
                blok.Draw();
            ball.Draw(); //рисуем мяч
            paddle.Draw(); // рисуем отражалку
            if (gameOver) // если игра проиграна рисуем кнопку проигрыша
            {
                gameOverButton.Draw();
            }
            if (gameWin) // если игра выиграна рисуем кнопку проигрыша
            {
                gameWinButton.Draw();
            }
        }
        private void StartScreen() // функция которая создаёт две кнопки и закидывает их в массив кнопок. Эти кнопки показываются при самом первом запуске игра - кнопки старты и close 
        {
            buttons.Add(new Button(230, 500, 100, 100,  StartGameID));// добавляем в массив кнопку старт. Причем кнопку создаём прямо внутри, не записывая её в отедльную переменную по типу Button b = new Button(...); buttins.Add(b). Просто на нафиг не нужна эта b если можем через массив обращаться к b
            buttons.Add(new Button(230, 300, 100, 100,  CloseGameID)); // добавляем в массив кнопку выход
            buttons[1].OnMouseDown += CloseGame; // подключаем ко второй кнопку метод закрытия игры
            buttons[0].OnMouseDown += Start; // подключаем к первой кнопке метод по переходу к менюшке выбора уровеней
        }
        private void CloseGame() // метод для закрытия игры
        {
            OnUnload();// На всякий случай вручную вызываю метод OnUnload - описанный выше
            Close(); // закрываю игру меотдом родительского класса ( он по идеи вызывает наш onUnload, но я чет не уверен что оно так работает, Cloae же метод GameWindow - он не может иметь доступа к нашему методу OnUnload из класса Game. Он имеет доступ OnUnload от класса GameWindow... Крч сточка кода выше просто перестраховка
        }
        private void Start() // метод для перехода к менюшкая выбора лвл
        {
            levelChoose = true; // переключаем флаг который говорит о том что мы теперь выбираем уровни
            clearButtons = true; // говорим что в onUpdateFrame нужно удет очистить старые кнопки из массива
            foreach (var but in buttons) // удаляем следы кнопок на видеокарте - vbo и vao от них
                but.Delete();
        }
        private void Level1() // функция которая подготавливает первый уровень
        {
            level = true; // отвечает за то, чтобы дать понять onUpdateFrame что мы выбрали уровень
            scores = 0; // устанвливаем очки пользователя раными 0
            foreach (var but in buttons) // удаляем в видеопамяти следы кнопок
                but.Delete();
            clearButtons = true; // // говорим что в onUpdateFrame нужно будет очистить старые кнопки из массива
            ball = new Ball(300, 30, 10, 10, ballTextureID); // создаём мяч
            paddle = new Paddle(250, 10, 100, 10, brickTextureID, ortoWidth); // создаём отражалкую Передавая в неё параметр ortoWidth. Он нужен для метода Move чтобы отражалка не могла за пределы экрана улететь

            dx = rnd.Next(-3, 3); // создаем случано направление по оси х
            dy = 1; // по оси у наверх идём
            for (int i = 0; i < 6; i++) // создаём 6 рядов
            {
                for (int j = 0; j < 8; j++)// по 8 блоков
                {
                    blocks.Add(new Blok(50 + 60 * j, 350 + 60 * i, 50, 50, brickTextureID)); // создаём блоки с шагом в 10 между ними (60-50=10)
                }
            }
        }
        private void Level2()// функция которая подготавливает второй уровень
        {
            level = true;
            scores = 0;
            foreach (var but in buttons)
                but.Delete();
            clearButtons = true;
            ball = new Ball(300, 30, 10, 10, ballTextureID);
            paddle = new Paddle(250, 10, 100, 10, brickTextureID, ortoWidth);
            dx = rnd.Next(-3, 3);
            dy = 1;
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (i == 0 && j == 0 || i == 0 && j == 7 || i == 5 && j == 0 || i == 5 && j == 7) // по краям у нас блоки стоять не будем, поэтому мы эти места пропускаем
                        continue; // если break выгоняет из цикла то continue переходит к следующей итерации цикла, т.е. j++ делает и в начало кидает
                    blocks.Add(new Blok(50 + 60 * j, 350 + 60 * i, 50, 50, brickTextureID));
                }
            }
            uberBlocks.Add(new Blok(50 + 60 * 0, 350 + 60 * 0, 50, 50, uberBrickTextureID)); // добавляем по тем пробущеным местам неразрушаемые блоки
            uberBlocks.Add(new Blok(50 + 60 * 7, 350 + 60 * 0, 50, 50, uberBrickTextureID));
            uberBlocks.Add(new Blok(50 + 60 * 7, 350 + 60 * 5, 50, 50, uberBrickTextureID));
            uberBlocks.Add(new Blok(50 + 60 * 0, 350 + 60 * 5, 50, 50, uberBrickTextureID));
            for(int i=0;i<10;i+=2) // создаем внизу линию неразрушаемых блоков для сложности
            {
                uberBlocks.Add(new Blok(50 + 60 * i, 250 , 50, 50, uberBrickTextureID));
            }
        }
        private void ReturnToMainMenu() // функция которая возвращает пользователя на главный экран где у нас только кнопка стар и выхода
        {
            level = false;   // устанавливаем наши значения всех флагов как было в самомо начале в OnLoad
            levelChoose = false;
            clearButtons = false;
            foreach (var but in buttons) // очищаем буфер кнопок на всякий случай но по идеи их там нет. Но если я вдруг где-то тупанул но видеопамять всё равно будет в порядке.
                but.Delete();
            gameOver = false;
            gameWin = false;
            Title = "Acranoid"; // убираем из названия окна количество очков

            blocks.Clear();// очищаем массив блоков и неразрушаемых блоков
            uberBlocks.Clear();

            StartScreen(); // вызываем функцию подготовки начального окна - читай выше где-то
        }

    }
    public class Blok
    {
        float x, y, height, width; // по умолчанию если у поля не стоит модификатор жосутпа то он private
        protected int bufferCordsID, bufferTextureID, vaoID, textureID; // тут protected чтобы классы наследники могла к этим полям обращаться. Тут перечислены переменных которых хранят айдишники разных массивов в видеопамяти. Сделал их протектет чтобы в классах наследниках можно было метод move написать итд. 
        bool status = true; // отвечает за тот факт попал ли по блоку мяч или нет
        protected float[] cords, textCords; // аналогично айдишникам сделал protected 
        public bool Status { get => status; set => status = value; } // свойство для доступа к состоянию блока используется при проверка на удаление блока
        public virtual float X { get => x; set => x = value; } // получаю левую нижнюю координату блока  Помечен как virtual чтобы можно было переопределить в классах наследниках
        public virtual float Y { get => y; set => y = value; } // получаю левую нижнюю координату блока  Помечен как virtual чтобы можно было переопределить в классах наследниках
        public float Height { get => height; set => height = value; } // получаю высоту блока
        public float Width { get => width; set => width = value; } // его ширину
        public Blok() { } // конструктор по умполчанию. Не нужен. 
        public Blok(float x, float y, float width, float height, int textureID)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            
            this.textureID = textureID;
            cords = new float[] { x, y, x, y + height, x + width, y + height, x + width, y }; // создаю массив координат блока
            textCords = new float[] { 0f, 1f, 0f, 0f, 1f, 0f, 1f, 1f }; // массив координат текстуры для сопоставления с блоком
            bufferCordsID = GL.GenBuffer();// генерация индефикатора
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferCordsID);//тип буфера, индефикатор - подключаюсь к буферу
            GL.BufferData(BufferTarget.ArrayBuffer, cords.Length * sizeof(float), cords, BufferUsageHint.StaticDraw);//тип, байты, данны, тип доступа отпарвлю инфу в видеокарту по данному буферу
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0); // отключаюсь от буфера

            bufferTextureID = GL.GenBuffer(); // генерация идефикатора для текстурного массива
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferTextureID);//тип буфера, индефикатор - подключаюсь к буферу
            GL.BufferData(BufferTarget.ArrayBuffer, textCords.Length * sizeof(float), textCords, BufferUsageHint.StaticDraw);//тип, байты, данны, тип доступа отпарвлю инфу в видеокарту по данному буферу
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0); // отключаюсь от буфера

            vaoID = GL.GenVertexArray(); // генерирую айдишник массима из vboшек
            GL.BindVertexArray(vaoID); // подключаюсь к нему
            GL.EnableClientState(ArrayCap.VertexArray); // разрешаю использовать массив координат объекта
            GL.EnableClientState(ArrayCap.TextureCoordArray); // разрешаю использовать массив координат текстуры

            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferTextureID); // подключаюсь к буферу координат текстуры
            GL.TexCoordPointer(2, TexCoordPointerType.Float,0, 0); // говорю о том как читать координаты текстуры: брать по две штуки, тип данных флоат, через сколько элементов массива лежит следующая пара координат, с какого элемента начать
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferCordsID);// подключаюсь к буферу координат блока
            GL.VertexPointer(2, VertexPointerType.Float, 0, 0); // говорю о том как читать координаты блока: брать по две штуки, тип данных флоат, через сколько элементов массива лежит следующая пара координат, с какого элемента начать
            // таким образом я скопировал ссылки на два vbo в vao
            GL.BindVertexArray(0); // отключаюсь от vao
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0); // отключаюсь от vbo

            GL.DisableClientState(ArrayCap.VertexArray); // забираю разрешения обратно
            GL.DisableClientState(ArrayCap.TextureCoordArray); // забираю разрешения обратно

        }
        public void Draw()
        {

            GL.Enable(EnableCap.Texture2D); // разрешаю использование текстур
            GL.Enable(EnableCap.Blend);// Подключаем режим отображения текстур для работы с прозрачностью
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha); // делаем так чтобы прозрачность была прозрачной а не просто белой 
            GL.EnableClientState(ArrayCap.VertexArray); // разрешаю использовать массив координат объекта
            GL.EnableClientState(ArrayCap.TextureCoordArray); // разрешаю использовать массив координат текстуры

            GL.BindTexture(TextureTarget.Texture2D, textureID); // подключаюсь к текстуре

            GL.BindVertexArray(vaoID); // подключаюсь к vao

            GL.DrawArrays(PrimitiveType.Polygon, 0, 4); // рисую фигуру и накладываю текстуру сразу. одним действием. Если бы не vao нужно было бы два раза вызывать для vbo 
            GL.BindVertexArray(0); // отключаюсь от vao

            GL.BindTexture(TextureTarget.Texture2D, 0);// отключаюсь от текстуры

            GL.Disable(EnableCap.Blend); // забираю все разрешения
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.TextureCoordArray);
            GL.Disable(EnableCap.Texture2D);

        }
        public virtual void Delete() // метод для очистки видеопамяти от себя
        {
            GL.DeleteBuffer(bufferCordsID); //удаляю vbo блока
            GL.DeleteBuffer(bufferTextureID); //удаляю vbo текстуры
            GL.DeleteVertexArray(vaoID); //удаляю vao. Тк вао хранит только ссылки на вбо - удалив только вао я не удалю вбо.
        }
        public virtual bool IsPointIn(float pointX, float pointY)// функция которая проверяет находится ли данная точка в фигуре. Помечен как virtual чтобы можно было переопределить в классах наследниках
        {
            if (pointX < x) return false;// если наша точка левее левой координаты то мимо
            if (pointY < y) return false;// если наша точка ниже нижней координаты то мимо
            if (pointX > x + width) return false;// если наша точка правее правой координаты то мимо
            if (pointY > y + height) return false; // если наша точка выше высшей координаты то мимо
            return true; 
        }
    }
    public class Ball : Blok // наследуем все от блока 
    {
        public float[] Cords { get => cords; set => cords = value; } // делаем доступ к координатам через свойство
        public override float X { get => cords[0]; set => cords[0] = value; } // ТУТ ЗАМЕТЬ ВЫ ВОЗВРАЩАЕМ НЕ Х А КООРДИНАТУ ИЗ МАССИВА! Ведь мячик у нас может двигаться, а движение эмулируется за счет смены всех координат в массиве на dx и dy. Поэтому вернух просто Х мы вернем его левую СТАРТОВУЮ координату
        public override float Y { get => cords[1]; set => cords[1] = value; } // аналогично
        public Ball(float x, float y, float width, float height, int textureID) : base(x, y, width, height, textureID) // обращаемся к конструктору базового класса
        {
        }
        public void Move(Vector2 vector) // метод для сдвига шара на вектор
        {
            for (int i = 0; i < cords.Length; i += 2) // проходимся по всем координатам шара по двойкам
            {
                cords[i] += vector.X; // перую координату меняем на Х составляющую вектора
                cords[i + 1] += vector.Y; // вторую на У
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferCordsID);//тип буфера, индефикатор Снова подключаемся у буферу ( поэтому там делал protected, чтобы idшник видеть)--- и отправлю новые координаты
            GL.BufferData(BufferTarget.ArrayBuffer, cords.Length * sizeof(float), cords, BufferUsageHint.StaticDraw);//тип, байты, данны, тип доступа 
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
    }
    public class Paddle : Blok
    {
        public override float X { get => cords[0]; set => cords[0] = value; }// ТУТ ЗАМЕТЬ ВЫ ВОЗВРАЩАЕМ НЕ Х А КООРДИНАТУ ИЗ МАССИВА! Ведь мячик у нас может двигаться, а движение эмулируется за счет смены всех координат в массиве на dx и dy. Поэтому вернух просто Х мы вернем его левую СТАРТОВУЮ координату
        public override float Y { get => cords[1]; set => cords[1] = value; } // аналогично
        int ortoWidth; // переменная которая будет харанить ширину экрана
        public Paddle(float x, float y, float width, float height, int textureID, int ortoWidth) : base(x, y, width, height, textureID)
        {
            this.ortoWidth = ortoWidth; // сохраняем переданную ширину
        }
        public void Move(Vector2 vector)
        {
            if (cords[0] + vector.X>= 0 && cords[0]+Width + vector.X <= ortoWidth) // а вот тут проверяем. Типо если к текущей левой и правой координате прибаивть наше смещение то выйдем ли мы за правую и левую границу? Если выйдем то ничего делать не будем, т.е. прибавлять не будем, а если не выйдем то прибавим.
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
        public override bool IsPointIn(float pointX, float pointY) // тут. т.к. у нас панелька умеет двигаться то нужно и изменить метод, чтобы он не по Х измерял а по первой координате в массиве
        {
            if (pointX < cords[0]) return false;
            if (pointY < cords[1]) return false;
            if (pointX > cords[0] + Width) return false;
            if (pointY > cords[1] + Height) return false;
            return true;
        }
    }
    public class ContentPipe // класс который нужен для загрузки текстуры
    {
        static public int LoadTexture(string path) // метод для загрузки текстуры. Статический. Значит не нужно создавать экземпляр класса чтобы метод использовать. Это как Console.Write где Console это класс а Write это статический метод. Метод принимает путь до картинки с текстурой
        {
            if (!File.Exists(path)) // проверяем существует ли файл и если нет выбрасываем испключение. Чисто для разраба если он тупанул
            {
                throw new FileNotFoundException($"File not found at '{path}'");
            }

            int id = GL.GenTexture(); // создаём айдишник текстуры его и вернем как закончим настройку текстуры
            GL.BindTexture(TextureTarget.Texture2D, id); // покдлючаемся к айдишнику 

            Bitmap bmp = new Bitmap(path); //Превращаем картинку в двоисные данные с которыми уже можно работать 
            BitmapData data = bmp.LockBits( // блокируем двоичные и создаём прямоугольник с размерами. как у картинки
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly, // Ставим режим картинки только чтение, 
                System.Drawing.Imaging.PixelFormat.Format32bppArgb); // а также даём понять какой битности картинка - 32ю Даже если глубина цвета меньше все равно будет работать. А если больше то часть цветов потеряем

            GL.TexImage2D(TextureTarget.Texture2D,0, PixelInternalFormat.Rgba, // выбираем формати пикселя или его цветовые компаненты, там где стоит 0 в конструкторе класса было сказано сделать 0. Пиксель формат указывает в каком формате будет работать OptnGL - red green blue alpha
                data.Width, data.Height,0, // указываем размеры исходя из картинки выше
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, // говорим в каком формате сама картинка - blue green red alpha
                PixelType.UnsignedByte, // говорим что под цвет выделяет беззнаковый байт то есть 0-255
                data.Scan0); // передаём информацию о картинке - сами биты 

            bmp.UnlockBits(data); // разблокируем двоичные биты 

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp); // указываем что делать если текстуры не хватило на объект. В данном случае прижимаеи текстуру к посленему пикчелю на границе.по разным осям
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear); //Функция уменьшения текстуры при масштабировании
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear); //Функция увеличения текстуры при масштабировании. на основе 5 точек рядом с рассматрвиаемой точкой. Про эти четыре функции очень хорошо написано на сайте: https://opentk.net/learn/chapter1/5-textures.html?tabs=load-texture-opentk3 в подразделе Texture Wrapping и Texture Filtering. Там хоть и на английском но по картинкам посмотри и станет понятно +-

            GL.BindTexture(TextureTarget.Texture2D, 0); // отключаемся от текстуры

            return id;
        }

    }
    delegate void MouseDelegate(); // создали делегат для функцияй которы void и не принимающих аргументов. Изпользуется в классе кнопки
    class Button: Blok // класс чисто чтобы отличать блоки от кнопок. Имеет делегат к которому будем присоединять фукнции которые будут как бы реакцией программы на нажатие по этой кнопке.
    {
        public MouseDelegate OnMouseDown;
        public Button(float x, float y, float width, float height, int textureID) : base(x, y, width, height, textureID)
        {
        }
    }
}