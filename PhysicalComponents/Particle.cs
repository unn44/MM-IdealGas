
using MM_IdealGas.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MM_IdealGas.PhysicalComponents
{
	/// <summary>
	/// Информация об одной частице: координаты центров и скорости.
	/// </summary>
	public class Particle : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private double x = 0, y = 0, radius =11;
		/// <summary>
		/// Координата X центра частицы.
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
		/// Координата Y центра частицы.
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
		public double Radius
		{
			get => radius;
			set
			{
				radius = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Скорость частицы.
		/// </summary>
		public double Ux, Uy;

		/// <summary>
		/// Конструктор: явная инициализация координат и скоростей частицы.
		/// </summary>
		public Particle(double x, double y, double ux, double uy)
        {
            X = x;
            Y = y;
            Ux = ux;
            Uy = uy;
        }
    }
}