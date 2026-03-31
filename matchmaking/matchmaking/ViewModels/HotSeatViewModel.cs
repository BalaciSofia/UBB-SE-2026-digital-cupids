using matchmaking.Domain;
using matchmaking.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace matchmaking.ViewModels
{
    internal class HotSeatViewModel : INotifyPropertyChanged
    {
        private ProfileService _profileService;
        private BidService _bidService;
        private RegisterInteractionUseCase _registerInteractionUseCase;

        private int _userId;
        private DatingProfile _hotSeatProfile;
        private int _highestBid;
        private string _errorMessage;
        private int _bidInput;
        private int _currentPhotoIndex;


        public bool ShowInteractionButtons =>
        HotSeatProfile != null && _userId!=_hotSeatProfile.UserId;

        public string? CurrentPhoto =>
        HotSeatProfile?.Photos != null && HotSeatProfile.Photos.Count > 0
        ? HotSeatProfile.Photos[CurrentPhotoIndex].Location
        : null;

        public bool HasHotSeatProfile => HotSeatProfile != null;
        public bool HasNoBid => HighestBid == 0;
        public bool HasBid => HighestBid > 0;
        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public DatingProfile HotSeatProfile
        {
            get => _hotSeatProfile;
            private set { _hotSeatProfile = value; OnPropertyChanged();
                OnPropertyChanged(nameof(ShowInteractionButtons));
                OnPropertyChanged(nameof(CurrentPhoto));
                OnPropertyChanged(nameof(HasHotSeatProfile));
            }
        }

        public int HighestBid
        {
            get => _highestBid;
            private set { _highestBid = value; OnPropertyChanged();
                OnPropertyChanged(nameof(HasNoBid));
                OnPropertyChanged(nameof(HasBid));
            }
        }
        public string ErrorMessage
        {
            get => _errorMessage;
            private set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasErrorMessage)); }
        }

        public int BidInput
        {
            get => _bidInput;
            set { _bidInput = value; OnPropertyChanged(); }
        }

        public int CurrentPhotoIndex
        {
            get => _currentPhotoIndex;
            private set { _currentPhotoIndex = value; OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentPhoto));
            }
        }

        public ICommand LoadHotSeatCommand { get; }
        public ICommand PlaceBidCommand { get; }
        public ICommand BoostProfileCommand { get; }
        public ICommand LikeHotSeatCommand { get; }
        public ICommand SuperLikeHotSeatCommand { get; }
        public ICommand NextPhotoCommand { get; }
        public ICommand PreviousPhotoCommand { get; }

        public HotSeatViewModel(int userId, ProfileService profileService, BidService bidService, RegisterInteractionUseCase registerInteractionUseCase)
        {
            _userId = userId;
            _profileService = profileService;
            _bidService = bidService;
            _registerInteractionUseCase = registerInteractionUseCase;

            LoadHotSeatCommand = new RelayCommand(LoadHotSeat);
            PlaceBidCommand = new RelayCommand(PlaceBid);
            BoostProfileCommand = new RelayCommand(BoostProfile);
            LikeHotSeatCommand = new RelayCommand(LikeHotSeat, () => HotSeatProfile != null && HotSeatProfile.UserId != _userId);
            SuperLikeHotSeatCommand = new RelayCommand(SuperLikeHotSeat, () => HotSeatProfile != null && HotSeatProfile.UserId != _userId);
            NextPhotoCommand = new RelayCommand(NextPhoto);
            PreviousPhotoCommand = new RelayCommand(PreviousPhoto);
        }

        public void LoadHotSeat()
        {
            ErrorMessage = string.Empty;
            CurrentPhotoIndex = 0;

            var allProfiles = _profileService.GetAllProfiles();

            // DEBUG: Print all profiles to console
            Console.WriteLine("=== ALL PROFILES IN DATABASE ===");
            foreach (var profile in allProfiles)
            {
                Console.WriteLine($"UserId: {profile.UserId}, Name: {profile.Name}, IsHotSeat: {profile.IsHotSeat}, IsArchived: {profile.IsArchived}, PhotoCount: {profile.Photos.Count}");
            }
            Console.WriteLine("=== END OF PROFILES ===");

            // HARDCODED FOR TESTING - Remove this after testing
            HotSeatProfile = allProfiles.FirstOrDefault(p => p.IsHotSeat && !p.IsArchived);
            // Normal behavior: HotSeatProfile = allProfiles.FirstOrDefault(p => p.IsHotSeat && !p.IsArchived);

            if (HotSeatProfile != null)
            {
                Console.WriteLine($"=== LOADED HOT SEAT PROFILE ===");
                Console.WriteLine($"UserId: {HotSeatProfile.UserId}, Name: {HotSeatProfile.Name}, PhotoCount: {HotSeatProfile.Photos.Count}");
                Console.WriteLine($"Bio: {HotSeatProfile.Bio}");
                Console.WriteLine($"=== END HOT SEAT PROFILE ===");
            }
            else
            {
                Console.WriteLine("NO HOT SEAT PROFILE FOUND!");
            }

            HighestBid = _bidService.getHighestBid();
        }

        public void PlaceBid()
        {
            ErrorMessage = string.Empty;
            try
            {
                Bid newBid = new Bid(_userId, (int)BidInput);
                _bidService.AddBid(newBid);
                HighestBid = _bidService.getHighestBid();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        public void BoostProfile()
        {
            ErrorMessage = string.Empty;
            DatingProfile profile = _profileService.GetProfileById(_userId);

            if (profile.IsBoosted && profile.BoostDay == DateTime.Today.Day)
            {
                ErrorMessage = "Your profile is already boosted for today.";    
                return;
            }

            profile.IsBoosted = true;
            profile.BoostDay = DateTime.Today.Day;
            _profileService.UpdateBoost(_userId);
        }

        public void LikeHotSeat()
        {
            if (_userId == HotSeatProfile.UserId)
            {
                throw new Exception("You cant like your own profile");
            }

            Interaction inter = new Interaction(_userId, HotSeatProfile.UserId, InteractionType.LIKE);
            _registerInteractionUseCase.RegisterInteraction(inter);
        }

        public void SuperLikeHotSeat()
        {
            if (_userId == HotSeatProfile.UserId)
            {
                throw new Exception("You cant like your own profile");
            }
            Interaction inter = new Interaction(_userId, HotSeatProfile.UserId, InteractionType.SUPER_LIKE);
            _registerInteractionUseCase.RegisterInteraction(inter);
        }

        public void NextPhoto()
        {
            if (HotSeatProfile.Photos == null || HotSeatProfile.Photos.Count == 0) return;
            CurrentPhotoIndex = (CurrentPhotoIndex + 1 + HotSeatProfile.Photos.Count) % HotSeatProfile.Photos.Count;
        }

        public void PreviousPhoto()
        {
            if (HotSeatProfile?.Photos == null || HotSeatProfile.Photos.Count == 0) return;
            CurrentPhotoIndex = (CurrentPhotoIndex - 1) % HotSeatProfile.Photos.Count;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}