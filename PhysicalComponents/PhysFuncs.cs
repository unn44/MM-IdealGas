using System;
using System.Collections.Generic;

namespace MM_IdealGas.PhysicalComponents
{
    public class PhysFuncs : World
    {
        private readonly List<double> _fXk, _fYk;
        private readonly int _worldR1, _worldR2;

        public PhysFuncs()
        {
            _fXk = new List<double>();
            _fYk = new List<double>();
            _worldR1 = (int) (_r1 * CoordStep);
            _worldR2 = (int) (_r2 * CoordStep);
        }
        
        public void DoStep()
        {
            var i = 0;
            foreach (var particle in _particles)
            {
                //TODO: переход через границу
                particle.X = NextCoord(i, 1);
                particle.Y = NextCoord(i++, 2);
            }

            i = 0;
            foreach (var particle in _particles)
            {
                particle.Ux = NextVel(i, 1);
                particle.Uy = NextVel(i++, 2);
            }
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
            double kMulty; //множитель K
            double sum = 0; //итоговая сумма
            
            var i = 0;
            foreach (var par in _particles)
            {
                if (i == index) {i++; continue;}
                var sqrtR = SqrtDistanceDouble(mainX, mainY, par.X, par.Y);
                var sqrtCheck = SqrtDistance(mainX, mainY, par.X, par.Y);
                if (sqrtCheck > _worldR2)
                {
                    //TODO: проверить точку с другой стороны (т.е. переход через границу)
                    continue; // если частицы далеки друг от друга (в том числе и через границу)
                }
                if (_worldR1 <= sqrtCheck && sqrtCheck <= _worldR2)
                {
                    kMulty = Math.Pow(1.0 - Math.Pow((sqrtR - _r1) / (_r1 - _r2), 2), 2);
                }
                else kMulty = 1;

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

                //TODO: проверить коэффициенты (размерность) и сумму
                sum += kMulty * -12 * D * Math.Pow(A, 6) * (Math.Pow(A / sqrtR, 6) - 1 /*сумма??*/) * (n1 - n2) / Math.Pow(sqrtR, 8);
                
                i++;
            }

            return -sum;
        }
        
        private static double SqrtDistanceDouble(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }
    }
}