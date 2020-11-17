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
		#endregion
		
		public ViewModel()
		{

			_timer = new Timer(1);
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

			Generate = new RelayCommand(o =>
			{
				//сброс таймера
				_timerTick = 0;
				CountSteps = $"Количество шагов: {_timerTick} ";
				if (!startOrStop) SetTimer();
				_physical.InitAll(ParticleNumber, MarginInit, U0MaxInit, TimeDelta, TimeCounts, CoeffR1, CoeffR2);
				_physical.GenerateInitState();
				Particles = _physical.GetParticlesCollection(0);
				
				InvalidateFlag = 0;
				PointsKinetic.Clear();
				PointsPotential.Clear();
				PointsEnergy.Clear();
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
			InvalidateFlag++; // для OxyPlot
			_timerTick++;
			
			Particles = _physical.GetParticlesCollection();

			var kinetic = CalсKinetic();
			var potential = CalcPotential();
			var energy = kinetic + potential;
			PointsKinetic.Add(new DataPoint(_timerTick, kinetic));
			PointsPotential.Add(new DataPoint(_timerTick, potential));
			PointsEnergy.Add(new DataPoint(_timerTick, energy));
			
			if (PointsKinetic.Count > 1000) PointsKinetic.RemoveRange(0, 1);
			if (PointsPotential.Count > 1000) PointsPotential.RemoveRange(0, 1);
			if (PointsEnergy.Count > 1000) PointsEnergy.RemoveRange(0, 1);
			
			CountSteps = $"Количество шагов: {_timerTick} ";
		}

		private double CalсKinetic()
		{
			double avgUx = 0.0, avgUy = 0.0;
			foreach (var par in Particles)
			{
				avgUx += par.Ux;
				avgUy += par.Uy;
			}

			avgUx /= ParticleNumber;
			avgUy /= ParticleNumber;

			var avgU = Math.Sqrt(avgUx * avgUx + avgUy * avgUy);
			const double Mass = 39.948 * 1.66054e-27; // константа массы частицы
			return Mass * avgU * avgU / 2.0;
		}

		private double CalcPotential()
		{
			double sumUx = 0.0, sumUy = 0.0;
			foreach (var par in Particles)
			{
				sumUx += par.Fx *-1; // бред? возможно...
				sumUy += par.Fy *-1;
			}
			
			return Math.Sqrt(sumUx * sumUx + sumUy * sumUy);
		}

	}
}