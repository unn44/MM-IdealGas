using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using MM_IdealGas.Components;
using MM_IdealGas.PhysicalComponents;

namespace MM_IdealGas
{
    public class World
    {
        /// <summary>
        /// Равновесное расстояние между центрами атомов (нм).
        /// </summary>
        private const double A = 0.382;
        /// <summary>
        /// Модуль потенц. энергии взаимодействия между атомами при равновесии (эВ). 
        /// </summary>
        private const double D = 0.0103;
        /// <summary>
        /// Реальная ширина ячейки (без учета А и шага)
        /// </summary>
        private const int CellW = 30;
        /// <summary>
        /// Реальная высота ячейки (без учета А и шага)
        /// </summary>
        private const int CellH = 30;
        /// <summary>
        /// Шаг по координатам (т.е. сколько точек в реальной единице)
        /// </summary>
        private const int CoordStep = 50; // не мало ли? но если больше, возникают проблемы с отображением
        
        /// <summary>
        /// Общее количество точек мира
        /// </summary>
        private int _worldW, _worldH;
        /// <summary>
        /// Последние точки мира
        /// </summary>
        private int _lastInW, _lastInH;
        /// <summary>
        /// Равновесное расстояние в мировых координатах (т.е. с учетом шага)
        /// </summary>
        private readonly int _worldA;
        /// <summary>
        /// Радиус одной частицы в мире (т.е. с учетом шага)
        /// </summary>
        private readonly int _particleRadius;
        /// <summary>
        /// Текущий кадр мира
        /// </summary>
        private double[,] _wFrame;
        /// <summary>
        /// Количество частиц (задается пользователем с окна)
        /// </summary>
        private readonly int _particlesQuantity;
        /// <summary>
        /// Отступ для равновесного расстояния между частицами при начальной генерации
        /// (может быть изменено пользователем в диапазоне от 0,85 до 0,9)
        /// </summary>
        private double _margin = 0.9;
        /// <summary>
        /// Максимальная начальная скорость частицы.
        /// </summary>
        private double _maxU0;
        /// <summary>
        /// Координаты центров всех существующих точек в мире.
        /// </summary>
        private List<Particle> _particles;

        public World(int particlesQuantity, double maxU0)
        {
            _particlesQuantity = particlesQuantity;
            _maxU0 = maxU0;
            _worldA = (int) (A * CoordStep);
            _particleRadius = (int) (A / 2.0 * CoordStep);
            _wFrame = CreateWorldFrame();
        }

        public void SetMargin(double margin) => _margin = margin;

        private double[,] CreateWorldFrame()
        {
            _worldW = (int)Math.Ceiling(A * CellW * CoordStep);
            _worldH = (int)Math.Ceiling(A * CellH * CoordStep);
            _lastInW = _worldW - 1;
            _lastInH = _worldH - 1;
            var arr = new double[_worldW,_worldH];
            for (var x = 0; x < _worldW; x ++) arr[x, 0] = arr[x, _lastInH] = 0.5;
            for (var y = 0; y < _worldH; y ++) arr[0, y] = arr[_lastInW, y] = 0.5;
            return arr;
        }

        private static int SqrtDistance(int x1, int y1, int x2, int y2)
        {
            return (int) Math.Ceiling(Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2)));
        }

        private double GenerateInitVel()
        {
            var rnd = new Random();
            // надо [0,1], но NextDouble() дает [0,1), но да ладно :)
            return (-1 + 2 * rnd.NextDouble()) * _maxU0;
        }

        public void GenerateInitState()
        {
            _particles = new List<Particle>();
            var rnd = new Random();
            var particlesNow = 0; // количество сгенерированных частиц
            
            // минимальное допустимое расстояние между частицами:
            var dist = (int) Math.Ceiling(_margin * _worldA + _particleRadius*2);
            // + _particleRadius*2 - так как расчёт между центрами, то необходимо учесть радиус у обеих частиц
            
            // минимальное допустимое расстояние от границ:
            var distBoard = (int) Math.Ceiling(_margin * _worldA / 2.0 + _particleRadius); 
            // (_margin * _worldA) / 2.0 - для того, чтобы шары у противоположных границ тоже оказывались на таком расстоянии
            // + _particleRadius - так как расчёт между центром и границей, то достаточно учесть радиус только у одной частицы
            
            while (true)
            {
                var shit = false; // если true, то точку на выброс
                var x = rnd.Next(distBoard, _worldW - distBoard);
                var y = rnd.Next(distBoard, _worldH - distBoard);
                foreach (var unused in _particles.Where(par => SqrtDistance(par.X, par.Y, x, y) < dist))
                {
                    shit = true;
                }
                if (shit) continue;
                _particles.Add(new Particle(x, y, GenerateInitVel(), GenerateInitVel())); // запомни
                _wFrame = Figures.AddCircle(x, y, _particleRadius, _wFrame); // построй
                if (++particlesNow == _particlesQuantity) break;
            }
        }

        public BitmapSource GetStaticFrame()
        {
            var arrToBitmap = new ArrToBitmap(_wFrame);
            return arrToBitmap.getBitmapInverted();
        }
    }
}