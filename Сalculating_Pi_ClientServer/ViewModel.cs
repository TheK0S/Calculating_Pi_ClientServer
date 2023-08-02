using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using static Сalculating_Pi_ClientServer.Connections;

namespace Сalculating_Pi_ClientServer
{
    public class ViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Point> Points { get; set; }       
        
        Random random = new Random();

        double m_coordinateX;
        double m_coordinateY;
        string m_statusText;
        SolidColorBrush m_statusColor;
        double m_pi;

        RelayCommand m_connectCommand;
        RelayCommand m_disconnectCommand;
        RelayCommand m_addPointToListCommand;
        RelayCommand m_addRandomPointToListCommand;
        RelayCommand m_clearPointListCommand;
        RelayCommand m_sendPointListToServerCommand;

        public double CoordinateX
        {
            get => m_coordinateX;
            set
            {
                m_coordinateX = value;
                OnPropertyChanged("CoordinateX");                
            }
        }

        public double CoordinateY
        {
            get => m_coordinateY;
            set
            {
                m_coordinateY = value;
                OnPropertyChanged("CoordinateY");
            }
        }

        public SolidColorBrush StatusColor 
        {
            get => m_statusColor;
            set
            {
                m_statusColor = value;
                OnPropertyChanged("StatusColor");
            }
        }

        public string StatusText
        {
            get => m_statusText;
            set
            {
                m_statusText = value;
                OnPropertyChanged("StatusText");
            }
        }

        public double Pi 
        {
            get => m_pi;
            set
            {
                m_pi = value;
                OnPropertyChanged("Pi");
            }
        }

        public ViewModel()
        {
            Points = new ObservableCollection<Point>();
            StatusColor = new SolidColorBrush(Colors.Red);
            StatusText = "Offline";            
        }        

        public RelayCommand ConnectCommand
        {
            get
            {
                return m_connectCommand ??
                    (m_connectCommand = new RelayCommand(obj =>
                    {
                        Application.Current.Dispatcher.InvokeAsync(async () =>
                        {
                            await ConnectToServer();
                            await ReceivePiAsync();
                        });
                        StatusColor = new SolidColorBrush(Colors.Green);
                        StatusText = "Online";
                    }));
            }
        }

        public RelayCommand DisconnectCommand
        {
            get
            {
                return m_disconnectCommand ??
                    (m_disconnectCommand = new RelayCommand(obj =>
                    {
                        Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            DisconnectFromServer();

                            StatusColor = new SolidColorBrush(Colors.Red);
                            StatusText = "Ofline";
                        });
                    }));
            }
        }

        public RelayCommand AddPointToList
        {
            get
            {
                return m_addPointToListCommand ??
                    (m_addPointToListCommand = new RelayCommand(obj =>
                    {
                        Point point = new Point(CoordinateX, CoordinateY);

                        if (point.X < 0 || point.X > 1) { MessageBox.Show("Значение X не в диапазоне от 0 до 1"); return; }
                        if (point.Y < 0 || point.Y > 1) { MessageBox.Show("Значение Y не в диапазоне от 0 до 1"); return; }
                        if (Points.Count > 10) { MessageBox.Show("Количество точек за за один запрос не более 10"); return; }
                        if (Points.Contains(point)) { MessageBox.Show("Такая точка уже есть в списке"); return; }

                        Points.Add(new Point(CoordinateX, CoordinateY));
                    }));
            }
        }

        public RelayCommand AddRandomPointToList
        {
            get
            {
                return m_addRandomPointToListCommand ??
                    (m_addRandomPointToListCommand = new RelayCommand(obj =>
                    {
                        Point point = new Point(random.NextDouble(), random.NextDouble());

                        if (Points.Count > 10) { MessageBox.Show("Количество точек за за один запрос не более 10"); return; }
                        if (Points.Contains(point)) { MessageBox.Show("Такая точка уже есть в списке"); return; }

                        Points.Add(point);
                    }));
            }
        }

        public RelayCommand ClearPointList
        {
            get
            {
                return m_clearPointListCommand ??
                    (m_clearPointListCommand = new RelayCommand(obj =>
                    {
                        Points.Clear();
                    }));
            }
        }

        public RelayCommand SendPointListToServer
        {
            get
            {
                return m_sendPointListToServerCommand ??
                    (m_sendPointListToServerCommand = new RelayCommand(async obj =>
                    {
                        await Task.Run(async () =>
                        {
                            await SendPointListAsync(Points);
                        });
                    }));
            }
        }
               

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
