using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MM_IdealGas.Annotations;
using MM_IdealGas.PhysicalComponents;

namespace MM_IdealGas
{
	public class ViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private PhysFuncs _world;


		public int CountParticles { get; set; } = 90;
		public ICommand Generate { get; set; }
		private ObservableCollection<Particle> particles;
		public ObservableCollection<Particle> Particles
		{
			get
			{
				return particles;
			}

			set
			{
			particles=value;
				OnPropertyChanged();
			}
		}
		public ViewModel()
		{
			_world = new PhysFuncs();
			_world.SetMargin(Margin);
			_world.SetParticlesQuantity(CountParticles);
			_world.SetMaxInitVel(MaxU0);
			_world.GenerateInitState();
			Particles = _world.GetParticles();
		
			Generate = new RelayCommand(o =>
			{
				_world.DoStep();
				Particles = _world.GetParticles();
				OnPropertyChanged();
			});
		}

		public double Margin { get; set; } = 0.9;

		public double MaxU0 { get; set; } = 1.0;

	}
}