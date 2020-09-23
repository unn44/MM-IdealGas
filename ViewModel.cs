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

        private World _world;
        public int Particles { get; set; } = 40;
        public ICommand Generate { get; set; }
        public ViewModel()
        {
            Generate = new RelayCommand(o =>
            {
                _world = new World(Particles, MaxU0);
                _world.SetMargin(Margin);
                _world.GenerateInitState();
                OnPropertyChanged(nameof(Frame));
            });
        }

        public BitmapSource Frame => _world.GetStaticFrame();

        public double Margin { get; set; } = 0.9;

        public double MaxU0 { get; set; } = 1.0;

    }
}