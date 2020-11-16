using MM_IdealGas.Annotations;
using MM_IdealGas.PhysicalComponents;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Input;
using OxyPlot;
using Timer = System.Timers.Timer;
using OxyPlot.Series;
using System;

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
		private PlotModel plotModel;
		public PlotModel PlotModels
		{
			get { return plotModel; }
			set { plotModel = value; OnPropertyChanged("PlotModel"); }
		}


		private readonly Timer _timer;
		private int _timerTick;

		private Physical _physical;

		private bool startOrStop = true;

		public string StopOrStartName => startOrStop ? "Запустить" : "Остановить";
		private string _countSteps;
		public string CountSteps
		{
			get => _countSteps;
			set
			{

				_countSteps = value;
				OnPropertyChanged();
			}
		}

		private double _sizeCell;
		public double SizeCell
		{
			get => _sizeCell;
			set
			{
				_sizeCell = value;
				OnPropertyChanged();
			}
		}

		private double _windowHeight;
		public double WindowHeight
		{
			get => _windowHeight;
			set
			{
				_windowHeight = value;
				OnPropertyChanged();
			}
		}
		private double _windowWidth;
		public double WindowWidth
		{
			get => _windowWidth;
			set
			{
				_windowWidth = value;
				OnPropertyChanged();
			}
		}
		public int ParticleNumber { get; set; } = 50;
		public double MarginInit { get; set; } = 0.9;
		public double U0MaxInit { get; set; } = 100;
		public double CoeffR1 { get; set; } = 1.1;
		public double CoeffR2 { get; set; } = 1.8;
		public double TimeDelta { get; set; } = 2e-14;
		public int TimeCounts { get; set; } = 500;


		public ICommand Generate { get; set; }
		public ICommand Start { get; set; }
		public ICommand Transform { get; set; }

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
			PlotModels = new PlotModel { Title = "Example 1" };
			PlotModels.Series.Add(new FunctionSeries(Math.Cos, 0, 10, 0.1, "cos(x)"));
			_windowHeight = 800;
			_windowWidth = 1000;
			_timer = new Timer(1); //TODO: подобрать правильный шаг!
			_timer.Elapsed += OnTimedEvent;
			_timerTick = 0;
			CountSteps = $"Количество шагов: {_timerTick} ";

			_physical = new Physical();
			_physical.InitAll(ParticleNumber, MarginInit, U0MaxInit, TimeDelta, TimeCounts, CoeffR1, CoeffR2);
			_physical.GenerateInitState();

			SizeCell = Physical.GetCellSize() + Physical.GetParticleSize(); // потому что нету нормального "половинного" деления частицы


			Particles = _physical.GetParticlesCollection(_timerTick++);

			Generate = new RelayCommand(o =>
			{
				//сброс таймера
				_timerTick = 0;
				CountSteps = $"Количество шагов: {_timerTick} ";
				if (!startOrStop) SetTimer();
				_physical.InitAll(ParticleNumber, MarginInit, U0MaxInit, TimeDelta, TimeCounts, CoeffR1, CoeffR2);
				_physical.GenerateInitState();
				Particles = _physical.GetParticlesCollection(0);
			});

			Start = new RelayCommand(o =>
			{
				SetTimer();
			});
			Transform = new RelayCommand(o =>
			{
				TransformPanel();
			});
		}
		/// <summary>
		/// Функция вызываемая при изменении размеров окна
		/// </summary>
		private void TransformPanel()
		{
			//MachineMultiplier= чему-то там, по идее  она констнта, её хуй поменяешь, но надо , или заново мир генерировать
			_timer.Enabled = false;
			_timer.Enabled = true;
		}

		private void SetTimer()
		{
			if (startOrStop)
			{
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
			Particles = _physical.GetParticlesCollection();
			CountSteps = $"Количество шагов: {_timerTick} ";
		}


	}
}