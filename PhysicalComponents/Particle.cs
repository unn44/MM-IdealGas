namespace MM_IdealGas.PhysicalComponents
{
    public struct Particle
    {
        /// <summary>
        /// Координаты центра частицы в мире.
        /// </summary>
        public int X, Y;
        /// <summary>
        /// Скорости частицы.
        /// </summary>
        public double Ux, Uy;

        public Particle(int x, int y, double ux, double uy)
        {
            X = x;
            Y = y;
            Ux = ux;
            Uy = uy;
        }
    }
}