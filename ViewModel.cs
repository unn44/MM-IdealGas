using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MM_IdealGas.Annotations;

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

        public ICommand Generate { get; set; }
        public ViewModel()
        {
            Generate = new RelayCommand(o =>
            {
                world = new World(Particles);
                OnPropertyChanged(nameof(Frame));
            });
        }
        
        World world = new World(30);

        public BitmapSource Frame => world.GetStaticFrame();

        public int Particles { get; set; }
    }
}