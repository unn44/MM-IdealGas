using MM_IdealGas.Annotations;
using MM_IdealGas.PhysicalComponents;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Input;
using Timer = System.Timers.Timer;

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

		private readonly Timer _timer;
		private int _timerTick;
		
		private Physical _physical;

		private bool startOrStop = true;
		private string stop;
		public string StopOrStartName
		{
			get
			{
			return	(startOrStop ? "Запустить" : "Остановить");
			}
			set
			{
				
				stop = value;
				OnPropertyChanged();
			}
		}
		private string countSteps;
		public string CountSteps
		{
			get
			{
				return countSteps;
			}
			set
			{

				countSteps = value;
				OnPropertyChanged();
			}
		}
		public int ParticleNumber { get; set; } = 50;
		public int SizeCell { get; set; } = 415;
		public double MarginInit { get; set; } = 0.9;
		public double U0MaxInit { get; set; } = 1e-9;
		public double CoeffR1 { get; set; } = 1.1;
		public double CoeffR2 { get; set; } = 1.8;
		public double TimeDelta { get; set; } = 1e-13;
		public int TimeCounts { get; set; } = 500;

		
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
			_timer = new Timer(5); //TODO: подобрать правильный шаг!
			_timerTick = 0;
			
			_physical = new Physical();
			_physical.InitAll(ParticleNumber, MarginInit, U0MaxInit, TimeDelta, TimeCounts, CoeffR1, CoeffR2);
			_physical.GenerateInitState();
			Particles = _physical.GetParticlesCollection(_timerTick++);
		
			Generate = new RelayCommand(o =>
			{
				_physical.InitAll(ParticleNumber, MarginInit, U0MaxInit, TimeDelta, TimeCounts, CoeffR1, CoeffR2);
				_physical.GenerateInitState();
				Particles = _physical.GetParticlesCollection(0);
			});
			
			Start = new RelayCommand(o =>
			{
					SetTimer();
			});
		}

		private void SetTimer()
		{
			if (startOrStop)
			{
				_timer.Elapsed += OnTimedEvent;
				_timer.AutoReset = true;
				_timer.Enabled = true;
				startOrStop = false;
				OnPropertyChanged(nameof(StopOrStartName));
			}
			else
			{
				_timer.Enabled = false;
				startOrStop = true;
				OnPropertyChanged(nameof(StopOrStartName));
			}
		}

		private void OnTimedEvent(object source, ElapsedEventArgs e)
		{
			_timerTick++;
	     Particles	=	_physical.GetParticlesCollection();
			CountSteps = string.Format("Количество шагов: {0} ", _timerTick);
		}


	}
}