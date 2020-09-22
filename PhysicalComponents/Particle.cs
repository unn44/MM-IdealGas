namespace MM_IdealGas.PhysicalComponents
{
    public struct Particle
    {
        /// <summary>
        /// Координаты центра частицы в мире.
        /// </summary>
        public int X, Y;

        public Particle(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}