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
		private string _countSteps, _maxVelText;
		public string CountSteps
		{
			get => _countSteps;
			set
			{

				_countSteps = value;
				OnPropertyChanged();
			}
		}
		
		public string MaxVelText
		{
			get => _maxVelText;
			set
			{
				_maxVelText = value;
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

		private double _maxVel, _deltaVel; //MAXWELL
		private double[] _maxwell = new double[50];

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
		private int _invalidateFlag, _invalidateFlagMaxwell;
		public int InvalidateFlag
		{
			get => _invalidateFlag;
			set
			{
				_invalidateFlag = value;
				OnPropertyChanged();
			}
		}
		
		public int InvalidateFlagMaxwell
		{
			get => _invalidateFlagMaxwell;
			set
			{
				_invalidateFlagMaxwell = value;
				OnPropertyChanged();
			}
		}
		public List<DataPoint> PointsKinetic { get; set; }
		public List<DataPoint> PointsPotential { get; set; }
		public List<DataPoint> PointsEnergy { get; set; }
		public List<DataPoint> PointsTemperature { get; set; }
		
		public List<DataPoint> PointsMaxwell1 { get; set; }
		public List<DataPoint> PointsMaxwell2 { get; set; }
		public List<DataPoint> PointsMaxwell3 { get; set; }
		public List<DataPoint> PointsMaxwell4 { get; set; }
		public List<DataPoint> PointsMaxwell5 { get; set; }

		double kinetic, potential, energy, temperature;
		double kineticTemp, potentialTemp, energyTemp, temperatureTemp;

		#endregion

		public ViewModel()
		{

			_timer = new Timer(5); //??
			_timer.Elapsed += OnTimedEvent;
			_timerTick = 0;
			_maxVel = 0.0;
			CountSteps = $"Количество шагов: {_timerTick} ";
			MaxVelText = "";

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
			
			PointsMaxwell1 = new List<DataPoint>();
			PointsMaxwell2 = new List<DataPoint>();
			PointsMaxwell3 = new List<DataPoint>();
			PointsMaxwell4 = new List<DataPoint>();
			PointsMaxwell5 = new List<DataPoint>();

			kinetic = 0;
			potential = 0;
			energy = 0;
			temperature = 0;
			kineticTemp = 0;
			potentialTemp = 0;
			energyTemp = 0;
			temperatureTemp = 0;
			
			Generate = new RelayCommand(o =>
			{
				OnPropertyChanged(nameof(MaxwellMode));
				Generation();
			});

			Start = new RelayCommand(o =>
			{
				OnPropertyChanged(nameof(MaxwellMode));
				SetTimer();
			});
		}

		private void Generation()
		{
			//сброс таймера
			_timerTick = 0;
			_maxVel = 0.0;
			CountSteps = $"Количество шагов: {_timerTick} ";
			MaxVelText = "";
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

			for (var i = 0; i < 50; i++) _maxwell[i] = 0.0;
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
				var current = Particles.Max(par => Math.Sqrt(par.Ux * par.Ux + par.Uy * par.Uy));
				if (current > _maxVel) _maxVel = current;
				MaxVelText = $"Макс. скорость: {_maxVel} ";
			}

			if (_timerTick == 500)
			{
				_deltaVel = 2 * _maxVel / 50.0;
			}

			if (_timerTick > 500 && _timerTick <= MaxwellSteps +500)
			{
				//main processor.
				foreach (var par in Particles)
				{
					var velocity = Math.Sqrt(par.Ux * par.Ux + par.Uy * par.Uy);
					for (var i = 0; i < 50; i++)
					{
						if (!(_deltaVel * i <= velocity) || !(velocity < _deltaVel * (i + 1))) continue;
						_maxwell[i] += 1.0;
						break;
					}
				}
			}

			//draw temperature chart.
			InvalidateFlag++; // для OxyPlot
			temperatureTemp += CalcTemperature(CalсKinetic());
			if (_timerTick % 10 == 0)
			{
				const double step = 10.0;
				temperature = temperatureTemp / step;
				temperatureTemp = 0;

				PointsTemperature.Add(new DataPoint(_timerTick, temperature));
			}

			if (PointsTemperature.Count > 100) PointsTemperature.RemoveRange(0, 1);
			
			if (_timerTick == MaxwellSteps +500)
			{
				//draw (create new series and add points to it)

				List<DataPoint> chart;
				if (PointsMaxwell1.Count == 0) chart = PointsMaxwell1;
				else if (PointsMaxwell2.Count == 0) chart = PointsMaxwell2;
				else if (PointsMaxwell3.Count == 0) chart = PointsMaxwell3;
				else if (PointsMaxwell4.Count == 0) chart = PointsMaxwell4;
				else chart = PointsMaxwell5;
				
				for (var i = 0; i < 50; i++)
				{
					InvalidateFlagMaxwell++;
					_maxwell[i] /= ParticleNumber;
					chart.Add(new DataPoint(i, _maxwell[i]));
				}
				
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
		
		private double CalcTemperature(double kinetic)
		{
			const double k = 1.38 * 1e-23; //Дж/К

			return kinetic * 2.0 / 3.0 / k;
		}

	}
}