using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Сalculating_Pi_ClientServer
{
    public class ViewModel
    {
        public ObservableCollection<Point> Points { get; set; }
        public Colors StatusColor { get; set; }
        public string Pi { get; set; }

        public ViewModel()
        {
            Points = new ObservableCollection<Point>();
        }

        RelayCommand m_connectCommand;

        public RelayCommand ConnectCommand
        {
            get
            {
                return m_connectCommand ??
                    (m_connectCommand = new RelayCommand(obj =>
                    {
                        Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            ConnectToServer();
                        });
                    }));
            }
        }

        private void ConnectToServer()
        {

        }
    }
}
