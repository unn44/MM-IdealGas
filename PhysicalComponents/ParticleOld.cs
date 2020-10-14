
using MM_IdealGas.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MM_IdealGas.PhysicalComponents
{

    public class ParticleOld : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		private int x=0, y=0;
		public int X
		{
			get
			{
				return x;
			}
			set
			{
				x = value;
				OnPropertyChanged();
			}
		}
		public int Y
		{
			get
			{
				return y;
			}
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

		public ParticleOld(int x, int y, double ux, double uy)
        {
            X = x;
            Y = y;
            Ux = ux;
            Uy = uy;
        }
    }
}