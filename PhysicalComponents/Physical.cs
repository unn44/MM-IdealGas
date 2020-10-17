using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MM_IdealGas.PhysicalComponents
{
    /// <summary>
    /// Основная физическая логика приложения.
    /// </summary>
    public class Physical
    {
        #region Поля класса, константы и логика инициализации
        /// <summary>
        /// Равновесное расстояние между центрами атомов (м).
        /// </summary>
        private const double A = 0.382e-9;
        /// <summary>
        /// Модуль потенц. энергии взаимодействия между атомами при равновесии (Дж). 
        /// </summary>
        private const double D = 0.0103 * 1.6e-19; // 0.0103 эВ переведены в Дж
        /// <summary>
        /// Ширина / высота квадрата (ед.).
        /// </summary>
        private const int SquareSize = 30;
        /// <summary>
        /// Ширина / высота исследуемой ячейки (м).
        /// </summary>
        private const double CellSize = SquareSize * A;
        /// <summary>
        /// Радиус одной частицы (м).
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
        public double U0MaxInit { get; set; } = 1e-9;
        /// <summary>
        /// Масса частицы (кг).
        /// </summary>
        public double Mass { get; set; } = 39.948 * 1.66054e-27; // 39.948 а.е.м. переведены в кг
        /// <summary>
        /// Шаг по времени (с).
        /// </summary>
        public double TimeDelta { get; set; } = 2e-14;
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
        /// Радиус обрезания R1 (м).
        /// </summary>
        private double _r1 = CoeffR1 * A;
        /// <summary>
        /// Радиус обрезания R2 (м).
        /// </summary>
        private double _r2 = CoeffR2 * A;
        /// <summary>
        /// Равновесное расстояние между частицами при начальной генерации (м).
        /// </summary>
        private double _marginInit = MarginInit * A;
        
        /// <summary>
        /// Коллекция всех частиц в исследуемой ячейке.
        /// Содержит: координаты X,Y центра частицы, текущую скорость частицы (Ux, Uy).
        /// </summary>
        private ObservableCollection<Particle> _particles;
        /// <summary>
        /// Коллекция, содержащая коллекцию координат и скоростей всех частиц в исследуемой ячейке на всех временных шагах.
        /// </summary>
        private ObservableCollection<ObservableCollection<Particle>> _allParticles;
        
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
        #region Вспомогательные функции
        /// <summary>
        /// Рассчитать квадрат расстояния между центрами двух частиц. 
        /// </summary>
        /// <param name="x1">Координата X первой частицы.</param>
        /// <param name="y1">Координата Y первой частицы.</param>
        /// <param name="x2">Координата X второй частицы.</param>
        /// <param name="y2">Координата Y второй частицы.</param>
        /// <returns>Квадрат расстояния между центрами двух частиц (м).</returns>
        private static double SqrtDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }
        /// <summary>
        /// Сгенерировать начальную скорость по одной проекции для одной частицы.
        /// </summary>
        /// <returns>Случайная скорость частицы в [-U0max,U0max) (м/с).</returns>
        private double RandomParticleU0()
        {
            // надо [0,1], но NextDouble() дает [0,1), но и ладно :)
            return (-1 + 2 * _rnd.NextDouble()) * U0MaxInit;
        }
        /// <summary>
        /// Сгенерировать начальную координату по одной проекции для одной частицы.
        /// </summary>
        /// <param name="marginBorder">Минимальное расстояние от границ.</param>
        /// <returns>Случайная координата частицы (м).</returns>
        private double RandomParticleXY0(double marginBorder=0.0)
        {
            return marginBorder + _rnd.NextDouble() * (CellSize - 2*marginBorder); //2*marginBorder из-за минимума и отступа от конца CellSize
        }
        /// <summary>
        /// Обеспечить нахождение координаты по одной проекции в границах ячейки.
        /// </summary>
        /// <param name="coord">Координата частицы по одной проекции.</param>
        /// <returns>Координата частицы, точно находящаяся в границах ячейки (м).</returns>
        private double BorderChecker(double coord)
        {
            if (coord < 0) return coord + CellSize;
            if (coord > CellSize) return coord - CellSize;
            return coord;
        }
        /// <summary>
        /// Рассчитать dx (dy) с учётом находения частиц с разных сторон границы.
        /// </summary>
        /// <param name="x1">Координата X(Y) первой частицы.</param>
        /// <param name="x2">Координата X(Y) второй частицы.</param>
        /// <returns>Разность координат (м).</returns>
        private double DxyThroughBorder(double x1, double x2)
        {
            var dx = x1 - x2;
            var signX = dx > 0 ? 1 : -1;
            return Math.Abs(dx) > CellSize / 2.0 ? dx - signX * CellSize : dx;
        }
        /// <summary>
        /// Рассчитать квадрат расстояния между центрами двух частиц (с учётом периодических границ). 
        /// </summary>
        /// <param name="x1">Координата X первой частицы.</param>
        /// <param name="y1">Координата Y первой частицы.</param>
        /// <param name="x2">Координата X второй частицы.</param>
        /// <param name="y2">Координата Y второй частицы.</param>
        /// <returns>Квадрат расстояния между центрами двух частиц (м).</returns>
        private double SmartDistance(double x1, double y1, double x2, double y2)
        {
            var dx = DxyThroughBorder(x1, x2);
            var dy = DxyThroughBorder(y1, y2);
            return Math.Sqrt(dx*dx + dy*dy);
        }
        /// <summary>
        /// Посчитать значение К функции.
        /// </summary>
        /// <param name="r">Расстояние между центрами взаимодействующих частиц.</param>
        /// <returns>Результат выполнения К функции (м).</returns>
        private double FuncK(double r)
        {
            if (r < _r1) return 1;
            if (r > _r2) return 0;
            return Math.Pow(1.0 - Math.Pow((r - _r1) / (_r1 - _r2), 2), 2);
        }
        #endregion
        #region Функции, необходимые для расчёта след. шага
        /// <summary>
        /// Рассчитать силу, действующую на частицу, на текущем шаге по времени.
        /// </summary>
        /// <param name="index">Индекс частицы, для которой будет производиться расчёт, из общего списка частиц.</param>
        /// <param name="direction">Направление в плоскости (проекция): 1 = X, 2 = Y.</param>
        /// <returns>Сила, действующая на частицу, на текущем шаге по времени (Дж).</returns>
        private double ForceForOneParticle(int index, int direction)
        {
            double curX = _particles[index].X, curY = _particles[index].Y;
            var sum = 0.0; //итоговая сумма

            for (var i = 0; i < _particles.Count; i++)
            {
                if (i==index) continue;
                double othX = _particles[i].X, othY = _particles[i].Y;
                var dist = SmartDistance(curX, curY, othX, othY); //Rik
                var k = FuncK(dist);
                if (k==0.0) continue;
                var dxdy = direction == 1 ? DxyThroughBorder(curX, othX) : DxyThroughBorder(curY, othY); //dx or dy 
                sum += (Math.Pow(A / dist, 6) - 1) * dxdy / Math.Pow(dist, 8) * k;
            }

            sum *= -12 * D * Math.Pow(A,6); //домножим на коэффициенты
            sum *= -1; //потенциальную энергию в силу
            return sum;
        }
        /// <summary>
        /// Рассчитать координату по одной проекции частицы на следующем шаге по времени.
        /// </summary>
        /// <param name="index">Индекс частицы, для которой будет производиться расчёт, из общего списка частиц.</param>
        /// <param name="direction">Направление в плоскости (проекция): 1 = X, 2 = Y.</param>
        /// <param name="forceK">Переменная для записи силы на текущем шаге по времени (для расчёта скорости).</param>
        /// <returns>Координата по одной проекции частицы на следующем шаге по времени (м/с).</returns>
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
        /// Сделать один шаг по времени (сместить частицы по времени на TimeDelta).
        /// </summary>
        private void DoTimeStep()
        {
            var i = 0;
            foreach (var particle in _particles)
            {
                double forceX = 0.0, forceY = 0.0;
                particle.X = BorderChecker(CoordinateNext(i, 1, ref forceX));
                particle.Y = BorderChecker(CoordinateNext(i, 2, ref forceY));
                particle.Ux = VelocityNext(i, 1, forceX);
                particle.Uy = VelocityNext(i++, 2, forceY);
            }
        }
        /// <summary>
        /// Рассчитать все шаги по времени и сохранить их в коллекцию коллекций.
        /// </summary>
        public void CalcAllTimeSteps()
        {
            _allParticles = new ObservableCollection<ObservableCollection<Particle>> {_particles};
            for (var i = 0; i < TimeCounts; i++)
            {
                DoTimeStep();
                _allParticles.Add(_particles);
            }
        }
        /// <summary>
        /// Получить коллекцию, содержащую коллекцию координат и скоростей всех частиц в исследуемой ячейке на всех временных шагах.
        /// Может быть использована для отображения результатов расчётов вне класса (например, построение в GUI).
        /// </summary>
        /// <returns>Коллекция, содержащая коллекцию координат и скоростей всех частиц в исследуемой ячейке на всех временных шагах.
        /// Координаты - м; Скорости - м/с.</returns>
        public ObservableCollection<ObservableCollection<Particle>> GetParticlesCollection() => _allParticles;
    }
}