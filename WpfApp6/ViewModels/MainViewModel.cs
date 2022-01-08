using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WpfApp6.Command;
using WpfApp6.Model;
using WpfApp6.Views;

namespace WpfApp6.ViewModels
{
    public class MainViewModel
    {
        Stopwatch timer = new Stopwatch();
        public ObservableCollection<Car> Cars { get; set; } = new ObservableCollection<Car>();
        MainWindow mainWindow;

        public RelayCommand StartCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }

        CancellationTokenSource cts;
        Thread thread;

        public MainViewModel(MainWindow _mainWindow)
        {
            mainWindow = _mainWindow;

            mainWindow.DataContext = this;
            mainWindow.txb_timer.Text = "00:00:00";

            StartCommand = new RelayCommand
               (
                act =>
                {
                    StartButtonClick();
                },
                pre => true
              );
            CancelCommand = new RelayCommand
              (
                act =>
                {
                    if (mainWindow.toggl_btn.IsChecked == false) thread.Abort();
                    else if(mainWindow.toggl_btn.IsChecked==true) cts.Cancel();
                },
                pre => true
              );

        }

        private void ThreadProcess()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + @"\Json");

            foreach (var item in directoryInfo.GetFiles())
            {
                timer.Start();
                var filedirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + @"\Json\";
                filedirectory += item.Name;

                var car = File.ReadAllText(filedirectory);
                ObservableCollection<Car> tempCars = JsonSerializer.Deserialize<ObservableCollection<Car>>(car);

                for (int i = 0; i < tempCars.Count; i++)
                {
                    Thread.Sleep(800);
                    mainWindow.Dispatcher.Invoke(new Action(() =>
                     {
                         mainWindow.txb_timer.Text = timer.Elapsed.ToString(@"hh\:mm\:ss");
                         Cars.Add(tempCars[i]);
                     }));
                }
            }
         
        }

        private void StartButtonClick()
        {
            timer.Reset();
            mainWindow.txb_timer.Text = "00:00:00";
            if (mainWindow.listbox_image.Items.Count != 0) Cars.Clear();

            if (mainWindow.toggl_btn.IsChecked == false)
            {
                thread = new Thread(ThreadProcess);
                thread.Start();
            }
            else if (mainWindow.toggl_btn.IsChecked == true)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + @"\Json");

                foreach (var item in directoryInfo.GetFiles())
                {
                    ThreadPool.QueueUserWorkItem((o) =>
                    {
                        cts = new CancellationTokenSource();
                        timer.Start();
                        var filedirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + @"\Json\";
                        filedirectory += item.Name;

                        var car = File.ReadAllText(filedirectory);
                        ObservableCollection<Car> tempCars = JsonSerializer.Deserialize<ObservableCollection<Car>>(car);

                        for (int i = 0; i < tempCars.Count; i++)
                        {
                            if (cts.Token.IsCancellationRequested) break;
                            Thread.Sleep(800);
                            mainWindow.Dispatcher.Invoke(new Action(() =>
                            {
                                mainWindow.txb_timer.Text = timer.Elapsed.ToString(@"hh\:mm\:ss");
                                Cars.Add(tempCars[i]);
                            }));
                        }
                    });

                }
            }
        }
    }










}
