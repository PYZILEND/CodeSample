using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VatsCallsMonitoring.Models;

namespace VatsCallsMonitoring.ViewModels
{
    class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            Initialize();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<Group> groups;
        public ObservableCollection<Group> Groups
        {
            get { return groups; }
            set
            {
                groups = value;
                OnPropertyChanged();
            }
        }
        
        private void StartDataRefresher()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Run(() =>
                    {
                        Groups = new ObservableCollection<Group>(DataProvider.RefreshCallsData(Groups.ToList()));                       
                    });
                    await Task.Delay(120000);
                }
            });
        }

        private async void Initialize()
        {
            await Task.Run(() =>
            {
                Groups = new ObservableCollection<Group>(DataProvider.GetGroups());
            });
            StartDataRefresher();
        }
    }
}
