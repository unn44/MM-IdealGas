
using MM_IdealGas.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MM_IdealGas.PhysicalComponents
{
	public class Particle : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		private double x=0, y=0;
		/// <summary>
		/// Координата X частицы.
		/// </summary>
		public double X
		{
			get => x;
			set
			{
				x = value;
				OnPropertyChanged();
			}
		}
		/// <summary>
		/// Координата Y частицы.
		/// </summary>
		public double Y
		{
			get => y;
			set
			{
				y = value;
				OnPropertyChanged();
			}
		}
		/// <summary>
		/// Скорости частицы.
		/// </summary>
        public double Ux, Uy;

		public Particle(double x, double y, double ux, double uy)
        {
            X = x;
            Y = y;
            Ux = ux;
            Uy = uy;
        }
    }
}