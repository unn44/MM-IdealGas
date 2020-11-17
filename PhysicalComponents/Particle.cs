
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
		private double _margin;

		/// <summary>
		/// Координата X центра частицы.
		/// </summary>
		public double X
		{
			get => x;
			set
			{
				x = value;
				Xcanvas = x - _margin;
				OnPropertyChanged(nameof(Xcanvas));
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
				Ycanvas = y - _margin;
				OnPropertyChanged(nameof(Ycanvas));
				OnPropertyChanged();
			}
		}
		/// <summary>
		/// Отцентрированная координата X центра частицы для построения на канвасе.
		/// </summary>
		public double Xcanvas { get; set; }
		/// <summary>
		/// Отцентрированная координата Y центра частицы для построения на канвасе.
		/// </summary>
		public double Ycanvas { get; set; }



		/// <summary>
		/// Диаметр (размер) частицы. Одинаковый для всех частиц.
		/// </summary>
		public static double Diameter { get; set; }

		/// <summary>
		/// Скорость частицы.
		/// </summary>
		public double Ux, Uy;
		
		public double Fx, Fy;

		/// <summary>
		/// Конструктор: явная инициализация координат и скоростей частицы.
		/// </summary>
		public Particle(double x, double y, double ux, double uy)
        {
            X = x;
            Y = y;
            Ux = ux;
            Uy = uy;
            
            _margin = Diameter / 2.0;
            
            Xcanvas = X - _margin;
            Ycanvas = Y - _margin;
        }
    }
}