using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Timers;
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


		public int CountParticles { get; set; } = 50;
		public ICommand Generate { get; set; }
		public ICommand Start { get; set; }
		private ObservableCollection<Particle> _particles;
		public ObservableCollection<Particle> Particles
		{
			get => _particles;

			set
			{
			_particles=value;
			OnPropertyChanged(nameof(Particles));
			}
		}
		public ViewModel()
		{
			_world = new PhysFuncs();
			_world.SetMargin(Margin);
			_world.SetParticlesQuantity(CountParticles);
			_world.SetMaxInitVel(MaxU0);
			_world.SetR1R2(R1,R2);
			_world.SetMass(Mass);
			_world.SetTimeParams(DeltaT, TCounts);
			_world.GenerateInitState();
			Particles = _world.GetParticles();
		
			Generate = new RelayCommand(o =>
			{
				_world = new PhysFuncs();
				_world.SetMargin(Margin);
				_world.SetParticlesQuantity(CountParticles);
				_world.SetMaxInitVel(MaxU0);
				_world.SetR1R2(R1,R2);
				_world.SetMass(Mass);
				_world.SetTimeParams(DeltaT, TCounts);
				_world.GenerateInitState();
				Particles = _world.GetParticles();
			});
			Start = new RelayCommand(o =>
			{
				/*for (var i = 0; i < TCounts; i++)
				{
					_world.DoStep();
					Particles = _world.GetParticles();

				}*/
				SetTimer();
			});
		}

		private void SetTimer()
		{
			// Create a timer with a two second interval.
			var aTimer = new System.Timers.Timer(100);
			// Hook up the Elapsed event for the timer. 
			aTimer.Elapsed += OnTimedEvent;
			aTimer.AutoReset = true;
			aTimer.Enabled = true;
		}

		private void OnTimedEvent(Object source, ElapsedEventArgs e)
		{
			_world.DoStep();
			Particles = _world.GetParticles();
		}
		
		public double Margin { get; set; } = 0.9;

		public double MaxU0 { get; set; } = 1.0;
		public double R1 { get; set; } = 1.1;
		public double R2 { get; set; } = 1.8;
		public double Mass { get; set; } = 39.948;
		public double DeltaT { get; set; } = 1.0;
		public int TCounts { get; set; } = 200;
	}
}