using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MM_IdealGas.PhysicalComponents
{
    public class Physical
    {
        #region Class fields, constants and init logic
        /// <summary>
        /// Равновесное расстояние между центрами атомов (нм).
        /// </summary>
        private const double A = 0.382;
        /// <summary>
        /// Модуль потенц. энергии взаимодействия между атомами при равновесии (эВ). 
        /// </summary>
        private const double D = 0.0103;
        /// <summary>
        /// Ширина / высота квадрата (ед.).
        /// </summary>
        private const int SquareSize = 30;
        /// <summary>
        /// Ширина / высота исследуемой ячейки (нм).
        /// </summary>
        private const double CellSize = SquareSize * A;
        /// <summary>
        /// Радиус одной частицы (нм).
        /// </summary>
        private const double ParticleRadius = A / 2.0;
        
        /// <summary>
        /// Общее количество частиц.
        /// </summary>
        public int ParticleNumber { get; set; } = 50;
        /// <summary>
        /// Отступ для равновесного расстояния между частицами при начальной генерации (ед.).
        /// (диапазон = 0,85 - 0,9)
        /// </summary>
        public static double MarginInit { get; set; } = 0.9;
        /// <summary>
        /// Максимальная начальная скорость частицы при начальной генерации (м/с).
        /// </summary>
        public double U0MaxInit { get; set; } = 1.0;
        /// <summary>
        /// Масса частицы (а. ед. м.).
        /// </summary>
        public double Mass { get; set; } = 39.948;
        /// <summary>
        /// Шаг по времени (с).
        /// </summary>
        public double TimeDelta { get; set; } = 10; //??????
        /// <summary>
        /// Количество шагов по времени.
        /// </summary>
        public int TimeCounts { get; set; } = 500;
        /// <summary>
        /// Коэффициент для радиуса обрезания R1 (ед.).
        /// (диапазон = 1,1 - 1,2)
        /// </summary>
        public static double CoeffR1 { get; set; } = 1.1;
        /// <summary>
        /// Коэффициент для радиуса обрезания R2 (ед.).
        /// (диапазон = 1,7 - 1,8)
        /// </summary>
        public static double CoeffR2 { get; set; } = 1.8;
        /// <summary>
        /// Радиус обрезания R1 (нм).
        /// </summary>
        private double _r1 = CoeffR1 * A;
        /// <summary>
        /// Радиус обрезания R2 (нм).
        /// </summary>
        private double _r2 = CoeffR2 * A;
        /// <summary>
        /// Равновесное расстояние между частицами при начальной генерации (нм).
        /// </summary>
        private double _marginInit = MarginInit * A;
        
        /// <summary>
        /// Коллекция всех частиц в исследуемой ячейке.
        /// Содержит: координаты X,Y центра частицы, текущую скорость частицы (Ux, Uy).
        /// </summary>
        private ObservableCollection<Particle> _particles; //мб тоже public сделать и убрать функцию get-типа?
        /// <summary>
        /// Получить коллекцию всех частиц в исследуемой ячейке.
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<Particle> GetParticles() => _particles;

        /// <summary>
        /// Внутренний рандом для класса Physical.
        /// </summary>
        private readonly Random _rnd;

        /// <summary>
        /// Конструктор: инициализация коллекции частиц и рандома. 
        /// </summary>
        public Physical()
        {
            _rnd = new Random();
            _particles = new ObservableCollection<Particle>();
        }
        #endregion
        #region Auxiliary functions
        /// <summary>
        /// Расчёт квадрата расстояния между центрами двух частиц. 
        /// </summary>
        /// <param name="x1">Координата X первой частицы.</param>
        /// <param name="y1">Координата Y первой частицы.</param>
        /// <param name="x2">Координата X второй частицы.</param>
        /// <param name="y2">Координата Y второй частицы.</param>
        /// <returns>Квадрат расстояния между центрами двух частиц (нм)</returns>
        private static double SqrtDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }
        /// <summary>
        /// Сгенерировать начальную скорость по одной проекции для одной частицы.
        /// </summary>
        /// <returns>Случайная скорость частицы в [-1,1) (м/с).</returns>
        private double RandomParticleU0()
        {
            // надо [0,1], но NextDouble() дает [0,1), но и ладно :)
            return (-1 + 2 * _rnd.NextDouble()) * U0MaxInit;
        }
        /// <summary>
        /// Сгенерировать начальную координату по одной проекции для одной частицы.
        /// </summary>
        /// <param name="marginBorder">минимальное расстояние от границ</param>
        /// <returns>Случайная координата частицы (нм).</returns>
        private double RandomParticleXY0(double marginBorder=0.0)
        {
            return marginBorder + _rnd.NextDouble() * (CellSize - 2*marginBorder); //2*marginBorder из-за минимума и отступа от CellSize
        }
        #endregion

        /// <summary>
        /// Расположить частицы в ячейку и сохранить их положения и начальные скорости в коллекцию. 
        /// </summary>
        public void GenerateInitState()
        {
            _particles.Clear();
            var particlesNow = 0; // текущее количество сгенерированных частиц.
            const double doubleRadius = ParticleRadius * 2.0; //расчёт между центрами, поэтому необходимо учесть радиус у обеих частиц.

            var betweenParticles = _marginInit + doubleRadius; //минимальное допустимое расстояние между частицами.
            var marginBorder = _marginInit / 2.0 + ParticleRadius; //минимальное допустимое расстояние от границ.
            
            while (true)
            {
                var trashPnt = false; //true -> точку на выброс.
                var x = RandomParticleXY0(marginBorder);
                var y = RandomParticleXY0(marginBorder);
                foreach (var unused in _particles.Where(par => SqrtDistance(par.X, par.Y, x, y) < betweenParticles))
                {
                    trashPnt = true;
                }
                if (trashPnt) continue;
                _particles.Add(new Particle(x, y, RandomParticleU0(), RandomParticleU0()));
                if (++particlesNow == ParticleNumber) break;
            }
        }

        /// <summary>
        /// Сделать один шаг по времени (сместить частицы по времени TimeDelta).
        /// </summary>
        private void DoTimeStep()
        {
            var i = 0;
            foreach (var particle in _particles)
            {
                double forceX = 0.0, forceY = 0.0;
                particle.X = CoordinateNext(i, 1, ref forceX); //пока нету перехода через границу!!
                particle.Y = CoordinateNext(i, 2, ref forceY);
                particle.Ux = VelocityNext(i, 1, forceX);
                particle.Uy = VelocityNext(i++, 2, forceY);
            }
        }
        /// <summary>
        /// Рассчитать все шаги по времени и сохранить их в коллекцию коллекций.
        /// </summary>
        private void CalcAllTimeSteps()
        {
            //TODO: гл. коллекция для записи коллекций частиц на каждом временном шаге.
            //TODO: положить в гл. коллекцию начальное положение.
            for (var i = 0; i < TimeCounts; i++)
            {
                DoTimeStep();
                //TODO: положить в гл. коллекцию текущее положение.
            }
        }
        
        #region "Next" functions
        /// <summary>
        /// Рассчитать координату по одной проекции частицы на следующем шаге по времени.
        /// </summary>
        /// <param name="index">Индекс частицы, для которой будет производиться расчёт, из общего списка частиц.</param>
        /// <param name="direction">Направление в плоскости (проекция): 1 = X, 2 = Y.</param>
        /// <param name="forceK">Переменная для записи силы на текущем шаге по времени (для расчёта скорости).</param>
        /// <returns>Координата по одной проекции частицы на следующем шаге по времени (??=конфликт:нм^м/с).</returns>
        private double CoordinateNext(int index, int direction, ref double forceK)
        {
            var coordK = direction==1 ? _particles[index].X : _particles[index].Y; //координата на текущем шаге.
            var velK = direction==1 ? _particles[index].Ux : _particles[index].Uy; //скорость на текущем шаге.
            forceK = ForceForOneParticle(index, direction);
            
            return coordK + velK * TimeDelta + forceK * TimeDelta * TimeDelta / (2*Mass);
        }
        /// <summary>
        /// Рассчитать скорость по одной проекции частицы на следующем шаге по времени.
        /// </summary>
        /// <param name="index">Индекс частицы, для которой будет производиться расчёт, из общего списка частиц.</param>
        /// <param name="direction">Направление в плоскости (проекция): 1 = X, 2 = Y.</param>
        /// <param name="forceK">Сила, действующая на частицу, на текущем шаге по времени.</param>
        /// <returns>Скорость по одной проекции частицы на следующем шаге по времени (м/с).</returns>
        private double VelocityNext(int index, int direction, double forceK)
        {
            var velK = direction==1 ? _particles[index].Ux : _particles[index].Uy; //скорость на текущем шаге.
            var forceK1 = ForceForOneParticle(index, direction); //сила на следующем временном шаге.
            
            return velK + (forceK1 + forceK) * TimeDelta / (2*Mass);
        }
        #endregion

        private double ForceForOneParticle(int index, int direction)
        {
            //TODO
            return -1;
        }
    }
}