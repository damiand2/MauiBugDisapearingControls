using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MauiBugDisapearingControls
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        
        bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }

        string title = string.Empty;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        private double displayWidth;

        public double DisplayWidth
        {
            get => displayWidth;
            set => SetProperty(ref displayWidth, value);
        }

        protected int CalculateDisplayWidth(double ratio = 0.9)
        {
            return (int)(DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density * ratio);
        }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
    

    public class MainPageViewModel : BaseViewModel
    {       
        public bool doNotHandleFilterChangeEvent = false;
        
        public HashSet<string> SelectedFacilityIds = new HashSet<string>();
        private bool pendingFilterChangedEvent;
        private bool pendingCountryChangedEvent;
        private bool viewVisible;
        private bool isListFullyLoaded;

        private const int amountOfFacilitiesThatCanBeTaken = 10;

        public MainPageViewModel()
        {
            FacilityList = new ObservableRangeCollection<PlaceModel>();
            InitialFacilityList();
           
            

            LoadMorePlacesCommand = new Command(async () =>
            {
                await LoadMorePlaces();
            });

        }

       

        private bool isLoaderVisible;
        public bool IsLoaderVisible
        {
            get { return isLoaderVisible; }
            set { SetProperty(ref isLoaderVisible, value); }
        }

        private bool isListLoaderVisible;
        public bool IsListLoaderVisible
        {
            get { return isListLoaderVisible; }
            set { SetProperty(ref isListLoaderVisible, value); }
        }

        private int listLoaderRowHeight;
        public int ListLoaderRowHeight
        {
            get { return listLoaderRowHeight; }
            set { SetProperty(ref listLoaderRowHeight, value); }
        }

        private bool exploreSectionShow;
        public bool ExploreSectionShow
        {
            get { return exploreSectionShow; }
            set { SetProperty(ref exploreSectionShow, value); }
        }

        private string searchText = null;
        public string SearchText
        {
            get => searchText;
            set => SetProperty(ref searchText, value);
        }
           

        
        public ICommand LoadMorePlacesCommand { get; set; }       
       
        public ObservableRangeCollection<PlaceModel> FacilityList { get; set; }

        
        private void InitCountryData()
        {
            IsLoaderVisible = true;
            
            Task.Run(async () =>
            {               
                FacilityListModel response = new FacilityListModel { Facilities = new List<FacilityModel>() };
                for (int i = 0; i < 10; i++)
                {
                    var facility = new FacilityModel
                    {
                        ActivityGroups = new List<ActivityGroupModel>(),
                        Address = $"address {i}",
                        Cards = new List<CardModel>(),
                        City = $"city {i}",
                        Distance = i,
                        FacilityId = i.ToString(),
                        Name = "name {i}"
                    };
                    response.Facilities.Add(facility);
                    for (int j = 0; j<= i; j++)
                    {
                        facility.ActivityGroups.Add(new ActivityGroupModel
                        {
                            ActivitiesCount = j,
                            Name = $"activity group {j}",
                            ActivityGroupId = j.ToString()
                        });
                        facility.Cards.Add(new CardModel
                        {
                            CardId = j.ToString(),
                            Name = $"card {j}",
                            ThumbnailURL = "dotnet_bot.svg"
                        });
                    }
                }
               

                MainThread.BeginInvokeOnMainThread(async () =>
                {                   
                    await ShowFacilityList(response, true);                  
                   
                    IsLoaderVisible = false;
                });
            });
        }

        

       

        private void InitialFacilityList()
        {
            IsLoaderVisible = true;
            _ = Task.Run(async () =>
            {
                FacilityListModel response = new FacilityListModel { Facilities = new List<FacilityModel>() };
                for (int i = 0; i < 10; i++)
                {
                    var facility = new FacilityModel
                    {
                        ActivityGroups = new List<ActivityGroupModel>(),
                        Address = $"address {i}",
                        Cards = new List<CardModel>(),
                        City = $"city {i}",
                        Distance = i,
                        FacilityId = i.ToString(),
                        Name = "name {i}"
                    };
                    response.Facilities.Add(facility);
                    for (int j = 0; j<= i; j++)
                    {
                        facility.ActivityGroups.Add(new ActivityGroupModel
                        {
                            ActivitiesCount = j,
                            Name = $"activity group {j}",
                            ActivityGroupId = j.ToString()
                        });
                        facility.Cards.Add(new CardModel
                        {
                            CardId = j.ToString(),
                            Name = $"card {j}",
                            ThumbnailURL = "dotnet_bot.svg"
                        });
                    }
                }

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    IsLoaderVisible = false;
                    await ShowFacilityList(response, true);
                });
            });
        }

       

       

        public async Task ShowFacilityList(FacilityListModel response, bool clearList)
        {
           

            var places = new List<PlaceModel>();
            if (response.Facilities != null)
            {
                foreach (var item in response.Facilities)
                {
                    places.Add(new PlaceModel(this, item));
                }
            }

            if (clearList)
            {
                FacilityList = new ObservableRangeCollection<PlaceModel>(places);
                OnPropertyChanged(nameof(FacilityList));
                isListFullyLoaded = false;
            }
            else
                FacilityList.AddRange(places);

            if (response.Facilities == null
                || response.Facilities.Count() < amountOfFacilitiesThatCanBeTaken)
            {
                isListFullyLoaded = true;
            }
        }        

        public async Task LoadMorePlaces()
        {
            if (isListFullyLoaded)
                return;

            IsListLoaderVisible = true;
            ListLoaderRowHeight = 30;
            await Task.Run(async () =>
            {
                FacilityListModel response = new FacilityListModel { Facilities = new List<FacilityModel>() };
                for (int i = 0; i < 10; i++)
                {
                    var facility = new FacilityModel
                    {
                        ActivityGroups = new List<ActivityGroupModel>(),
                        Address = $"address {i}",
                        Cards = new List<CardModel>(),
                        City = $"city {i}",
                        Distance = i,
                        FacilityId = i.ToString(),
                        Name = "name {i}"
                    };
                    response.Facilities.Add(facility);
                    for (int j = 0; j<= i; j++)
                    {
                        facility.ActivityGroups.Add(new ActivityGroupModel
                        {
                            ActivitiesCount = j,
                            Name = $"activity group {j}",
                            ActivityGroupId = j.ToString()
                        });
                        facility.Cards.Add(new CardModel
                        {
                            CardId = j.ToString(),
                            Name = $"card {j}",
                            ThumbnailURL = "dotnet_bot.svg"
                        });
                    }
                }
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                   
                    if (response.Facilities.Count() < amountOfFacilitiesThatCanBeTaken)
                    {
                        isListFullyLoaded = true;
                    }

                    var places = new List<PlaceModel>();
                    if (response.Facilities != null)
                    {
                        foreach (var item in response.Facilities)
                        {
                            places.Add(new PlaceModel(this, item));
                        }
                    }
                    FacilityList.AddRange(places);
                    ListLoaderRowHeight = 0;
                    IsListLoaderVisible = false;
                });
            });

        }

        
    }

   
    public class PlaceModel : BaseViewModel
    {
        public FacilityModel Facility { get; private set; }
        public List<ActivityGroupModel> VisibleActivities { get => Facility.ActivityGroups; }
        
        public bool HasActivities { get => Facility.ActivityGroups?.Any() == true; }
        public bool CardsVisible { get => Facility.Cards?.Any() == true; }
        

        private bool isLocationEnabled;
        public bool IsLocationEnabled
        {
            get { return isLocationEnabled; }
            set { SetProperty(ref isLocationEnabled, value); }
        }

        public PlaceModel(MainPageViewModel FacilityListViewModel, FacilityModel facility)
        {
            Facility = facility;
            IsLocationEnabled = true;
            FacilityDetailsClicked = new Command<string>(async facilityId =>
            {
                                
            });            
        }

        public ICommand FacilityDetailsClicked { get; set; }
        
    }
}
