using System;
using System.Collections.Generic;

namespace MM_IdealGas.PhysicalComponents
{
    public class PhysFuncs : World
    {
        private readonly List<double> _fXk, _fYk;

        public PhysFuncs()
        {
            _fXk = new List<double>();
            _fYk = new List<double>();
        }
        
        public void DoStep()
        {
            var i = 0;
            foreach (var particle in _particles)
            {
                particle.X = BorderControl(NextCoord(i, 1));
                particle.Y = BorderControl(NextCoord(i++, 2));
            }

            i = 0;
            foreach (var particle in _particles)
            {
                particle.Ux = NextVel(i, 1);
                particle.Uy = NextVel(i++, 2);
            }
            _fXk.Clear();
            _fYk.Clear();
        }
        
        public void RunDynamic()
        {
            for (var i = 0; i < _tCount; i++) DoStep();
        }

        /// <summary>
        /// Расчёт координаты частицы для следующего шага по времени.
        /// </summary>
        /// <param name="index">Индекс частицы, для которой будет производиться расчёт, из общего списка частиц.</param>
        /// <param name="direction">Направление в плоскости: 1 = X, 2 = Y.</param>
        /// <returns>Координата частицы на следующем шаге по времени.</returns>
        private int NextCoord(int index, int direction)
        {
            // координата и скорость на текущем шаге
            int curCoord;
            double curVel;

            double force;
            
            if (direction == 1)
            {
                curCoord = _particles[index].X;
                curVel = _particles[index].Ux;
                force = CalcAllForce(index, 1);
                _fXk.Add(force);
            }
            else
            {
                curCoord = _particles[index].Y;
                curVel = _particles[index].Uy;
                force = CalcAllForce(index, 2);
                _fYk.Add(force);
            }
            
            var res = curCoord + curVel * _deltaT + force * _deltaT * _deltaT / (2*_m);
            
            return (int) Math.Round(res);
        }
        /// <summary>
        /// Расчёт скорости частицы для следующего шага по времени.
        /// </summary>
        /// <param name="index">Индекс частицы, для которой будет производиться расчёт, из общего списка частиц.</param>
        /// <param name="direction">Направление в плоскости: 1 = X, 2 = Y.</param>
        /// <returns>Скорость частицы на следующем шаге по времени.</returns>
        private double NextVel(int index, int direction)
        {
            // скорость и сила на текущем шаге
            double curVel, curForce;
            
            if (direction == 1)
            {
                curVel = _particles[index].Ux;
                curForce = _fXk[index];
            }
            else
            {
                curVel = _particles[index].Uy;
                curForce = _fYk[index];
            }

            return curVel + (CalcAllForce(index, direction) + curForce) * _deltaT / (2 * _m);
        }
        
        private double CalcAllForce(int index, int direction)
        {
            int mainX = _particles[index].X, mainY = _particles[index].Y;
            double sum = 0; //итоговая сумма
            
            var i = 0;
            foreach (var par in _particles)
            {
                if (i == index) {i++; continue;}
                var sqrtR = SqrtDistanceDouble(mainX, mainY, par.X, par.Y);
                var sqrtCheck = SqrtDistance(mainX, mainY, par.X, par.Y);

                double kMulty; //множитель K
                if (_worldR1 <= sqrtCheck && sqrtCheck <= _worldR2)
                {
                    kMulty = Math.Pow(1.0 - Math.Pow((sqrtR - _r1) / (_r1 - _r2), 2), 2);
                }
                else if (sqrtCheck < _worldR1) kMulty = 1;
                else
                {
                    //проверяем с измененными из-за границы точками
                    var sqrtR2 = SqrtBoardDistance(mainX, mainY, par.X, par.Y,0);
                    var sqrtCheck2 = SqrtBoardDistanceCheck(mainX, mainY, par.X, par.Y,0);
                    if (_worldR1 <= sqrtCheck2 && sqrtCheck2 <= _worldR2)
                    {
                        kMulty = Math.Pow(1.0 - Math.Pow((sqrtR2 - _r1) / (_r1 - _r2), 2), 2);
                    }
                    else if (sqrtCheck2 < _worldR1) kMulty = 1;
                    else
                    {
                        sqrtR2 = SqrtBoardDistance(mainX, mainY, par.X, par.Y,1);
                        sqrtCheck2 = SqrtBoardDistanceCheck(mainX, mainY, par.X, par.Y,1);
                        if (_worldR1 <= sqrtCheck2 && sqrtCheck2 <= _worldR2)
                        {
                            kMulty = Math.Pow(1.0 - Math.Pow((sqrtR2 - _r1) / (_r1 - _r2), 2), 2);
                        }
                        else if (sqrtCheck2 < _worldR1) kMulty = 1;
                        else
                        {
                            sqrtR2 = SqrtBoardDistance(mainX, mainY, par.X, par.Y, 2);
                            sqrtCheck2 = SqrtBoardDistanceCheck(mainX, mainY, par.X, par.Y, 2);
                            if (_worldR1 <= sqrtCheck2 && sqrtCheck2 <= _worldR2)
                            {
                                kMulty = Math.Pow(1.0 - Math.Pow((sqrtR2 - _r1) / (_r1 - _r2), 2), 2);
                            }
                            else if (sqrtCheck2 < _worldR1) kMulty = 1;
                            else
                            {
                                i++;
                                continue;
                            } // k = 0, упрощение вычислений для процессора
                        }
                    }
                }

                int n1, n2;
            
                if (direction == 1)
                {
                    n1 = mainX;
                    n2 = par.X;
                }
                else
                {
                    n1 = mainY;
                    n2 = par.Y;
                }
                
                sum += kMulty * -12 * D * Math.Pow(A, 6) * (Math.Pow(A / sqrtR, 6) - 1) * (n1 - n2) / Math.Pow(sqrtR, 8);
                
                i++;
            }

            return -sum;
        }
        
        private static double SqrtDistanceDouble(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }

        private double SqrtBoardDistance(int x1, int y1, int x2, int y2, int mode)
        {
            var newX2 = _worldSize - x2;
            var newY2 = _worldSize - y2;
            switch (mode)
            {
                case 0:
                    return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - newY2) * (y1 - newY2));
                case 1:
                    return Math.Sqrt((x1 - newX2) * (x1 - newX2) + (y1 - y2) * (y1 - y2));
                default:
                    return Math.Sqrt((x1 - newX2) * (x1 - newX2) + (y1 - newY2) * (y1 - newY2));
            }
        }
        
        private int SqrtBoardDistanceCheck(int x1, int y1, int x2, int y2, int mode)
        {
            return (int) Math.Ceiling(SqrtBoardDistance(x1, y1, x2, y2, mode));
        }

        /// <summary>
        /// Обеспечивает постоянное нахождение центра частицы в ячейке.
        /// </summary>
        /// <param name="x">Одна из координат центра.</param>
        /// <returns></returns>
        private int BorderControl(int x)
        {
            if (x >= 0 && x <= _lastIndex) return x;
            if (x<0) return x + _worldSize;
            return x - _worldSize; // (x > _lastIndex)
        }
    }
}