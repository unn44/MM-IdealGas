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
using System.Collections.Generic;
using System.Linq;

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

		public bool StartOrStop { get; set; } = true;

		public string StopOrStartName => StartOrStop ? "Запустить" : "Остановить";
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
		
		public double CanvasBorderThickness { get; set; } = 2;
		public double CanvasSize { get; set; }
		public double CanvasZoom { get; set; } = 8e10; // коэффициент масштабирования канваса
		
		
		public int ParticleNumber { get; set; } = 50;
		public double MarginInit { get; set; } = 0.9;
		public double U0MaxInit { get; set; } = 100;
		public double CoeffR1 { get; set; } = 1.1;
		public double CoeffR2 { get; set; } = 1.8;
		public double TimeDelta { get; set; } = 2e-14;
		public int TimeCounts { get; set; } = 500;
		
		public int MaxwellSteps { get; set; } = 15000;
		
		public bool MaxwellMode { get; set; } = false;

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

		#region Charts
		private int _invalidateFlag;
		public int InvalidateFlag
		{
			get => _invalidateFlag;
			set
			{
				_invalidateFlag = value;
				OnPropertyChanged();
			}
		}
		public List<DataPoint> PointsKinetic { get; set; }
		public List<DataPoint> PointsPotential { get; set; }
		public List<DataPoint> PointsEnergy { get; set; }
		public List<DataPoint> PointsTemperature { get; set; }
		
		double kinetic, potential, energy, temperature;
		double kineticTemp, potentialTemp, energyTemp, temperatureTemp;

		#endregion

		public ViewModel()
		{

			_timer = new Timer(5); //??
			_timer.Elapsed += OnTimedEvent;
			_timerTick = 0;
			CountSteps = $"Количество шагов: {_timerTick} ";

			_physical = new Physical();
			_physical.InitAll(ParticleNumber, MarginInit, U0MaxInit, TimeDelta, TimeCounts, CoeffR1, CoeffR2);
			_physical.GenerateInitState();

			SizeCell = Physical.GetCellSize(); // потому что нету нормального "половинного" деления частицы
			CanvasSize = SizeCell * CanvasZoom + CanvasBorderThickness* 2 /* т.к. границы с двух сторон*/;

			Particles = _physical.GetParticlesCollection(_timerTick++);
			
			/*var kinetic = CalсKinetic();
			var potential = CalcPotential();
			var energy = kinetic + potential;*/
			InvalidateFlag = 0;
			PointsKinetic = new List<DataPoint>();
			PointsPotential = new List<DataPoint>();
			PointsEnergy = new List<DataPoint>();
			PointsTemperature = new List<DataPoint>();

			kinetic = 0;
			potential = 0;
			energy = 0;
			temperature = 0;
			kineticTemp = 0;
			potentialTemp = 0;
			energyTemp = 0;
			temperatureTemp = 0;
			
			Generate = new RelayCommand(o => { Generation(); });

			Start = new RelayCommand(o =>
			{
				SetTimer();
			});
		}

		private void Generation()
		{
			//сброс таймера
			_timerTick = 0;
			CountSteps = $"Количество шагов: {_timerTick} ";
			if (!StartOrStop) SetTimer();
			_physical.InitAll(ParticleNumber, MarginInit, U0MaxInit, TimeDelta, TimeCounts, CoeffR1, CoeffR2);
			_physical.GenerateInitState();
			Particles = _physical.GetParticlesCollection(0);

			InvalidateFlag = 0;
			PointsKinetic.Clear();
			PointsPotential.Clear();
			PointsEnergy.Clear();
			PointsTemperature.Clear();

			kinetic = 0;
			potential = 0;
			energy = 0;
			temperature = 0;
			kineticTemp = 0;
			potentialTemp = 0;
			energyTemp = 0;
			temperatureTemp = 0;
		}

		private void SetTimer()
		{
			if (StartOrStop)
			{
				_timer.AutoReset = true;
				_timer.Enabled = true;
				StartOrStop = false;
				OnPropertyChanged(nameof(StopOrStartName));
				OnPropertyChanged(nameof(StartOrStop));
			}
			else
			{
				_timer.Enabled = false;
				StartOrStop = true;
				OnPropertyChanged(nameof(StopOrStartName));
				OnPropertyChanged(nameof(StartOrStop));
			}
		}

		private void OnTimedEvent(object source, ElapsedEventArgs e)
		{
			if (!MaxwellMode) NormalProcessor();
			else MaxwellProcessor();
		}

		private void NormalProcessor()
		{
			InvalidateFlag++; // для OxyPlot
			_timerTick++;

			Particles = _physical.GetParticlesCollection();

			var kineticNow = CalсKinetic();
			var potentialNow = _physical.GetPotential();
			
			kineticTemp += kineticNow;
			potentialTemp += potentialNow;
			energyTemp += kineticNow * 1e10 + potentialNow * 1e10;
			//energyTemp /= 1e10;
			temperatureTemp += CalcTemperature(kineticNow);

			if (_timerTick % 10 == 0)
			{
				const double step = 10.0;
				kinetic = kineticTemp / step;
				potential = potentialTemp / step;
				energy = energyTemp / step / 1e10;
				temperature = temperatureTemp / step;

				kineticTemp = 0;
				potentialTemp = 0;
				energyTemp = 0;
				temperatureTemp = 0;


				PointsKinetic.Add(new DataPoint(_timerTick, kinetic));
				PointsPotential.Add(new DataPoint(_timerTick, potential));
				PointsEnergy.Add(new DataPoint(_timerTick, energy));
				PointsTemperature.Add(new DataPoint(_timerTick, temperature));
			}

			if (PointsKinetic.Count > 100) PointsKinetic.RemoveRange(0, 1);
			if (PointsPotential.Count > 100) PointsPotential.RemoveRange(0, 1);
			if (PointsEnergy.Count > 100) PointsEnergy.RemoveRange(0, 1);
			if (PointsTemperature.Count > 100) PointsTemperature.RemoveRange(0, 1);
			

			CountSteps = $"Количество шагов: {_timerTick} ";
		}

		private void MaxwellProcessor()
		{
			_timerTick++;
			CountSteps = $"Количество шагов: {_timerTick} ";

			Particles = _physical.GetParticlesCollection();
			
			if (_timerTick < 500)
			{
				//TODO: find max speed.
			}

			if (_timerTick == 500)
			{
				//TODO: process the results of 500 steps.
			}

			if (_timerTick > 500)
			{
				//TODO: main processor.
			}

			//draw temperature chart.
			InvalidateFlag++; // для OxyPlot
			temperatureTemp += CalcTemperature(CalсKinetic());
			if (_timerTick % 10 == 0)
			{
				const double step = 10.0;
				temperature = temperatureTemp / step;
				temperatureTemp = 0;
			}
			PointsTemperature.Add(new DataPoint(_timerTick, temperature));
			if (PointsTemperature.Count > 500) PointsTemperature.RemoveRange(0, 1);
			
			if (_timerTick == MaxwellSteps)
			{
				//TODO: draw (create new series and add points to it)
				
				//turn off the timer
				_timer.Enabled = false;
				StartOrStop = true;
				OnPropertyChanged(nameof(StopOrStartName));
				OnPropertyChanged(nameof(StartOrStop));
			}
		}
		
		private double CalсKinetic()
		{
			var avgU2 = Particles.Sum(par => par.Ux * par.Ux + par.Uy * par.Uy);
			avgU2 /= ParticleNumber;

			const double mass = 39.948 * 1.66054e-27; // константа массы частицы
			return mass * avgU2 / 2.0;
		}

		private double CalcPotential()
		{
			//return Particles.Sum(par => Math.Sqrt(par.Fx * par.Fx + par.Fy * par.Fy));
			return -1;
		}

		private double FindMaxVelocityOnStep()
		{
			return Particles.Max(par => par.Ux * par.Ux + par.Uy * par.Uy);
		}
		
		private double CalcTemperature(double kinetic)
		{
			const double k = 1.38 * 1e-23; //Дж/К

			return kinetic * 2.0 / 3.0 / k;
		}

	}
}