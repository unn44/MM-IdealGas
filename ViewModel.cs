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


		private double deltaT = 1.0;

		private double margin = 0.9;

		private double maxU0 = 1.0;
		private double r1 = 1.1;
		private double r2 = 1.8;
		private double mass = 39.948;
		private int countParticles = 50;
		public ICommand Generate { get; set; }
		public ICommand Start { get; set; }
		private ObservableCollection<Particle> _particles;
		public ObservableCollection<Particle> Particles
		{
			get => _particles;

			set
			{
				_particles = value;
				OnPropertyChanged(nameof(Particles));
			}
		}
		public ViewModel()
		{
			_world = new PhysFuncs();
			_world.SetMargin(Margin);
			_world.SetParticlesQuantity(CountParticles);
			_world.SetMaxInitVel(MaxU0);
			_world.SetR1R2(R1, R2);
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
				_world.SetR1R2(R1, R2);
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

		public double Margin
		{
			get => margin;
			set
			{
				margin = value;
				OnPropertyChanged();
			}
		}

		public double MaxU0
		{
			get => maxU0;
			set
			{
				maxU0 = value;
				OnPropertyChanged();
			}
		}
		public double R1
		{
			get => r1;
			set
			{
				r1 = value;
				OnPropertyChanged();
			}
		}
		public double R2
		{
			get => r2;
			set
			{
				r2 = value;
				OnPropertyChanged();
			}
		}
		public double Mass
		{
			get => mass;
			set
			{
				mass = value;
				OnPropertyChanged();
			}
		}
		public int CountParticles
		{
			get => countParticles;
			set
			{
				countParticles = value;
				OnPropertyChanged();
			}
		}
		public double DeltaT
		{
			get => deltaT;

			set
			{
				deltaT = value;
				OnPropertyChanged(nameof(DeltaT));
			}
		}
		public int TCounts { get; set; } = 200;
	}
}