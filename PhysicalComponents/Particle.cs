
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

		private double x, y;
		private double xCnvs, yCnvs;

		/// <summary>
		/// Координата X центра частицы.
		/// </summary>
		public double X
		{
			get => x;
			set
			{
				x = value;
				xCnvs = x - Diameter / 2.0;
				OnPropertyChanged(nameof(XCnvs));
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
				yCnvs = y - Diameter / 2.0;
				OnPropertyChanged(nameof(YCnvs));
				OnPropertyChanged();
			}
		}

		public double XCnvs
		{
			get => xCnvs;
			set => xCnvs = value;
		}

		public double YCnvs
		{
			get => yCnvs;
			set => yCnvs = value;
		}


		/// <summary>
		/// Диаметр (размер) частицы. Одинаковый для всех частиц.
		/// </summary>
		public static double Diameter { get; set; }

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
            
            XCnvs = X - Diameter / 2.0;
            YCnvs = Y - Diameter / 2.0;
        }
    }
}