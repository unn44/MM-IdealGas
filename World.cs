using System;
using System.Windows.Media.Imaging;
using MM_IdealGas.Components;

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
        /// Радиус одной частицы в мире (т.е. с учетом шага)
        /// </summary>
        private int _particleRadius;
        /// <summary>
        /// Текущий кадр мира
        /// </summary>
        private double[,] _wFrame;
        /// <summary>
        /// Количество частиц (задается пользователем с окна)
        /// </summary>
        private int _particles;

        public World(int particles)
        {
            _particles = particles;
            _particleRadius = (int) (A / 2.0 * CoordStep);
            _wFrame = CreateWorldFrame();
        }

        private double[,] CreateWorldFrame()
        {
            _worldW = (int)Math.Ceiling(A * CellW * CoordStep);
            _worldH = (int)Math.Ceiling(A * CellH * CoordStep);
            _lastInW = _worldW - 1;
            _lastInH = _worldH - 1;
            return new double[_worldW,_worldH];
        }

        public BitmapSource GetStaticFrame()
        {
            /* Обводка границы кадра -> Вынести в отдельную функцию CreateFrame? */
            for (var x = 0; x < _worldW; x ++) _wFrame[x, 0] = _wFrame[x, _lastInH] = 0.5;
            for (var y = 0; y < _worldH; y ++) _wFrame[0, y] = _wFrame[_lastInW, y] = 0.5;
            /* */
            
            /* !! Это должно считаться в отдельной функции !! Здесь добавление границ? и чисто отрисовка */
            _wFrame = Figures.AddCircle(_particleRadius, _particleRadius, _particleRadius, _wFrame);
            _wFrame = Figures.AddCircle(_lastInW - _particleRadius, _lastInH - _particleRadius, _particleRadius, _wFrame);
            /* */
            
            var arrToBitmap = new ArrToBitmap(_wFrame);
            return arrToBitmap.getBitmapInverted();
        }
    }
}