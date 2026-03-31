using matchmaking.Domain;
using matchmaking.Services;
using matchmaking.Utils;
using matchmaking.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.Storage.Pickers;
namespace matchmaking.Views
{
    internal sealed partial class CreateProfileView : Page
    {
        internal CreateProfileViewModel? ViewModel { get; private set; }

        public CreateProfileView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is CreateProfileViewModel viewModel)
            {
                ViewModel = viewModel;
                ViewModel.LoadUserData(ViewModel.UserId);
                UsernameText.Text = viewModel.ProfileData != null ? GetUsername() : "";
                AgeText.Text = viewModel.ProfileData != null ? GetAge().ToString() : "";
                LoadLocations();
                LoadInterests();
                UpdatePhotoSlots();
                UpdateNextButton();
            }
        }

        private string GetUsername()
        {
            var userUtil = new MockUserUtil();
            return userUtil.GetUserData(ViewModel!.UserId).Username;
        }

        private int GetAge()
        {
            var userUtil = new MockUserUtil();
            var birthDate = userUtil.GetUserData(ViewModel!.UserId).Birthdate;
            int age = DateTime.Now.Year - birthDate.Year;
            if (DateTime.Now < birthDate.AddYears(age)) age--;
            return age;
        }

        // ── Step navigation ───────────────────────────────────────────────────

        private void HandleNextStepClick(object sender, RoutedEventArgs e)
        {
            SyncUItoViewModel();
            ViewModel!.NextStep();
            UpdateStepUI();
        }

        private void HandlePreviousStepClick(object sender, RoutedEventArgs e)
        {
            SyncUItoViewModel();
            ViewModel!.PreviousStep();
            UpdateStepUI();
        }

        private void SyncUItoViewModel()
        {
            if (ViewModel?.ProfileData == null) return;
            int step = ViewModel.CurrentStep;

            if (step == 1)
            {
                ViewModel.ProfileData.Location = LocationComboBox.SelectedItem as string ?? string.Empty;
                ViewModel.ProfileData.Nationality = NationalityTextBox.Text;
                ViewModel.ProfileData.Gender = GenderComboBox.SelectedIndex switch
                {
                    0 => Gender.MALE,
                    1 => Gender.FEMALE,
                    2 => Gender.NON_BINARY,
                    _ => Gender.OTHER
                };
                ViewModel.ProfileData.PreferredGenders = new List<Gender>();
                if (PrefMaleCheckBox.IsChecked == true) ViewModel.ProfileData.PreferredGenders.Add(Gender.MALE);
                if (PrefFemaleCheckBox.IsChecked == true) ViewModel.ProfileData.PreferredGenders.Add(Gender.FEMALE);
                if (PrefNonBinaryCheckBox.IsChecked == true) ViewModel.ProfileData.PreferredGenders.Add(Gender.NON_BINARY);
                if (PrefOtherCheckBox.IsChecked == true) ViewModel.ProfileData.PreferredGenders.Add(Gender.OTHER);
                ViewModel.ProfileData.MaxDistance = (int)MaxDistanceSlider.Value;
                ViewModel.ProfileData.MinPreferredAge = (int)MinAgeSlider.Value;
                ViewModel.ProfileData.MaxPreferredAge = (int)MaxAgeSlider.Value;
                ViewModel.ProfileData.DisplayStarSign = DisplayStarSignToggle.IsOn;
            }
            else if (step == 3)
            {
                ViewModel.ProfileData.Bio = BioTextBox.Text;
            }
        }

        private void SyncViewModeltoUI()
        {
            if (ViewModel?.ProfileData == null) return;
            int step = ViewModel.CurrentStep;

            if (step == 1)
            {
                LocationComboBox.SelectedItem = ViewModel.ProfileData.Location;
                NationalityTextBox.Text = ViewModel.ProfileData.Nationality;
                GenderComboBox.SelectedIndex = ViewModel.ProfileData.Gender switch
                {
                    Gender.MALE => 0,
                    Gender.FEMALE => 1,
                    Gender.NON_BINARY => 2,
                    _ => 3
                };
                PrefMaleCheckBox.IsChecked = ViewModel.ProfileData.PreferredGenders.Contains(Gender.MALE);
                PrefFemaleCheckBox.IsChecked = ViewModel.ProfileData.PreferredGenders.Contains(Gender.FEMALE);
                PrefNonBinaryCheckBox.IsChecked = ViewModel.ProfileData.PreferredGenders.Contains(Gender.NON_BINARY);
                PrefOtherCheckBox.IsChecked = ViewModel.ProfileData.PreferredGenders.Contains(Gender.OTHER);
                MaxDistanceSlider.Value = ViewModel.ProfileData.MaxDistance;
                MinAgeSlider.Value = ViewModel.ProfileData.MinPreferredAge;
                MaxAgeSlider.Value = ViewModel.ProfileData.MaxPreferredAge;
                MaxDistanceValueText.Text = ViewModel.ProfileData.MaxDistance.ToString();
                MinAgeValueText.Text = ViewModel.ProfileData.MinPreferredAge.ToString();
                MaxAgeValueText.Text = ViewModel.ProfileData.MaxPreferredAge.ToString();
                DisplayStarSignToggle.IsOn = ViewModel.ProfileData.DisplayStarSign;
            }
            else if (step == 3)
            {
                BioTextBox.Text = ViewModel.ProfileData.Bio;
                BioLengthText.Text = ViewModel.ProfileData.Bio.Length.ToString();
            }
        }

        private void UpdateStepUI()
        {
            int step = ViewModel!.CurrentStep;

            Step1Panel.Visibility = step == 1 ? Visibility.Visible : Visibility.Collapsed;
            Step2Panel.Visibility = step == 2 ? Visibility.Visible : Visibility.Collapsed;
            Step3Panel.Visibility = step == 3 ? Visibility.Visible : Visibility.Collapsed;
            Step4Panel.Visibility = step == 4 ? Visibility.Visible : Visibility.Collapsed;

            PreviousStepButton.IsEnabled = step != 1;
            NextStepButton.Visibility = step == 4 ? Visibility.Collapsed : Visibility.Visible;
            CreateProfileButton.Visibility = step == 4 ? Visibility.Visible : Visibility.Collapsed;

            Step1Dot.Fill = step == 1 ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White)
                                      : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(128, 255, 255, 255));
            Step2Dot.Fill = step == 2 ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White)
                                      : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(128, 255, 255, 255));
            Step3Dot.Fill = step == 3 ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White)
                                      : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(128, 255, 255, 255));
            Step4Dot.Fill = step == 4 ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White)
                                      : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(128, 255, 255, 255));

            SyncViewModeltoUI();

            if (step == 2)
                UpdatePhotoSlots();

            if (step == 4)
                UpdatePreview();

            UpdateNextButton();
        }

        // ── Validation ────────────────────────────────────────────────────────

        private void UpdateNextButton()
        {
            if (ViewModel?.ProfileData == null) return;
            int step = ViewModel.CurrentStep;

            NextStepButton.IsEnabled = step switch
            {
                1 => IsStep1Valid(),
                2 => IsStep2Valid(),
                3 => IsStep3Valid(),
                _ => true
            };
        }

        private bool IsStep1Valid()
        {
            return !string.IsNullOrWhiteSpace(ViewModel!.ProfileData!.Location) &&
                   !string.IsNullOrWhiteSpace(ViewModel.ProfileData.Nationality) &&
                   GenderComboBox.SelectedIndex >= 0 &&
                   (PrefMaleCheckBox.IsChecked == true ||
                    PrefFemaleCheckBox.IsChecked == true ||
                    PrefNonBinaryCheckBox.IsChecked == true ||
                    PrefOtherCheckBox.IsChecked == true);
        }

        private bool IsStep2Valid()
        {
            return ViewModel!.ProfileData!.Photos.Count >= 2;
        }

        private bool IsStep3Valid()
        {
            return ViewModel!.ProfileData!.Bio.Length >= 20 &&
                   ViewModel.ProfileData.Bio.Length <= 250 &&
                   ViewModel.ProfileData.Interests.Count >= 3;
        }

        // ── Step 1 handlers ───────────────────────────────────────────────────

        private void LoadLocations()
        {
            var locationUtil = new LocationUtil();
            var locations = locationUtil.GetAllLocations();
            foreach (string location in locations)
                LocationComboBox.Items.Add(location);
        }

        private void HandleLocationChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel?.ProfileData == null) return;
            ViewModel.ProfileData.Location = LocationComboBox.SelectedItem as string ?? string.Empty;
            UpdateNextButton();
        }

        private void HandleNationalityChanged(object sender, TextChangedEventArgs e)
        {
            if (ViewModel?.ProfileData == null) return;
            ViewModel.ProfileData.Nationality = NationalityTextBox.Text;
            UpdateNextButton();
        }

        private void HandleGenderChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel?.ProfileData == null) return;
            ViewModel.ProfileData.Gender = GenderComboBox.SelectedIndex switch
            {
                0 => Gender.MALE,
                1 => Gender.FEMALE,
                2 => Gender.NON_BINARY,
                3 => Gender.OTHER,
                _ => Gender.OTHER
            };
            UpdateNextButton();
        }

        private void HandlePreferredGenderChanged(object sender, RoutedEventArgs e)
        {
            if (ViewModel?.ProfileData == null) return;
            ViewModel.ProfileData.PreferredGenders = new List<Gender>();
            if (PrefMaleCheckBox.IsChecked == true) ViewModel.ProfileData.PreferredGenders.Add(Gender.MALE);
            if (PrefFemaleCheckBox.IsChecked == true) ViewModel.ProfileData.PreferredGenders.Add(Gender.FEMALE);
            if (PrefNonBinaryCheckBox.IsChecked == true) ViewModel.ProfileData.PreferredGenders.Add(Gender.NON_BINARY);
            if (PrefOtherCheckBox.IsChecked == true) ViewModel.ProfileData.PreferredGenders.Add(Gender.OTHER);
            UpdateNextButton();
        }

        private void HandleMaxDistanceChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (ViewModel?.ProfileData == null) return;
            ViewModel.ProfileData.MaxDistance = (int)MaxDistanceSlider.Value;
            MaxDistanceValueText.Text = ((int)MaxDistanceSlider.Value).ToString();
        }

        private void HandleMinAgeChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (ViewModel?.ProfileData == null) return;
            ViewModel.ProfileData.MinPreferredAge = (int)MinAgeSlider.Value;
            MinAgeValueText.Text = ((int)MinAgeSlider.Value).ToString();
        }

        private void HandleMaxAgeChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (ViewModel?.ProfileData == null) return;
            ViewModel.ProfileData.MaxPreferredAge = (int)MaxAgeSlider.Value;
            MaxAgeValueText.Text = ((int)MaxAgeSlider.Value).ToString();
        }

        // ── Step 2 handlers ───────────────────────────────────────────────────

        private async void HandleAddPhotoClick(object sender, RoutedEventArgs e)
        {
            int slotIndex = int.Parse((sender as Button)!.Tag.ToString()!);

            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(
                (Application.Current as App)!._window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            StorageFile file = await picker.PickSingleFileAsync();
            if (file == null) return;

            var properties = await file.GetBasicPropertiesAsync();
            if (properties.Size > 10 * 1024 * 1024)
            {
                var sizeDialog = new ContentDialog
                {
                    Title = "File Too Large",
                    Content = "The selected file exceeds the 10MB limit.",
                    CloseButtonText = "OK",
                    XamlRoot = XamlRoot
                };
                await sizeDialog.ShowAsync();
                return;
            }

            Photo photo = new Photo(ViewModel!.UserId, file.Path, slotIndex);

            try
            {
                ViewModel.AddPhoto(photo);
                UpdatePhotoSlots();
                UpdateNextButton();
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = ex.Message,
                    CloseButtonText = "OK",
                    XamlRoot = XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private async void HandleRemovePhotoClick(object sender, RoutedEventArgs e)
        {
            int photoId = (int)(sender as Button)!.Tag;
            try
            {
                ViewModel!.RemovePhoto(photoId);
                UpdatePhotoSlots();
                UpdateNextButton();
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = ex.Message,
                    CloseButtonText = "OK",
                    XamlRoot = XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private void UpdatePhotoSlots()
        {
            var slots = new[] { PhotoSlot0, PhotoSlot1, PhotoSlot2, PhotoSlot3, PhotoSlot4, PhotoSlot5 };
            var photos = ViewModel!.ProfileData!.Photos;
            bool canRemove = photos.Count > 2;
            bool canAdd = photos.Count < 6;

            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].Child = null;

                if (i < photos.Count)
                {
                    var grid = new Grid();
                    var img = new Image
                    {
                        Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(photos[i].Location!)),
                        Stretch = Microsoft.UI.Xaml.Media.Stretch.UniformToFill
                    };

                    var removeBtn = new Button
                    {
                        Content = new FontIcon { Glyph = "\uE711", FontSize = 16 },
                        Tag = photos[i].PhotoId,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top,
                        Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                            Microsoft.UI.ColorHelper.FromArgb(180, 0, 0, 0)),
                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White),
                        Width = 32,
                        Height = 32,
                        CornerRadius = new CornerRadius(16),
                        Margin = new Thickness(0, 4, 4, 0),
                        IsEnabled = canRemove
                    };
                    removeBtn.Click += HandleRemovePhotoClick;
                    grid.Children.Add(img);
                    grid.Children.Add(removeBtn);
                    slots[i].Child = grid;
                }
                else
                {
                    var addBtn = new Button
                    {
                        Tag = i.ToString(),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent),
                        BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                            Microsoft.UI.ColorHelper.FromArgb(255, 200, 200, 200)),
                        BorderThickness = new Thickness(2),
                        Content = new FontIcon
                        {
                            Glyph = "\uE710",
                            FontSize = 32,
                            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                                Microsoft.UI.ColorHelper.FromArgb(255, 180, 180, 180))
                        },
                        IsEnabled = canAdd
                    };
                    addBtn.Click += HandleAddPhotoClick;
                    slots[i].Child = addBtn;
                }
            }
        }

        // ── Step 3 handlers ───────────────────────────────────────────────────

        private void HandleBioTextChanged(object sender, TextChangedEventArgs e)
        {
            if (ViewModel?.ProfileData == null) return;
            ViewModel.ProfileData.Bio = BioTextBox.Text;
            BioLengthText.Text = BioTextBox.Text.Length.ToString();
            UpdateNextButton();
        }

        private void LoadInterests()
        {
            var interestUtil = new InterestUtil();
            var allInterests = interestUtil.GetAll();
            var buttons = new List<Button>();

            foreach (string interest in allInterests)
            {
                var btn = new Button
                {
                    Content = interest,
                    Tag = interest,
                    CornerRadius = new CornerRadius(20),
                    Padding = new Thickness(16, 8, 16, 8),
                    BorderThickness = new Thickness(2),
                    BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 235, 59, 89)),
                    Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 255, 255, 255)),
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 235, 59, 89)),
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
                };
                btn.Click += HandleInterestClick;
                buttons.Add(btn);
            }

            InterestsRepeater.ItemsSource = buttons;
        }

        private async void HandleInterestClick(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            string interest = (string)btn.Tag;
            bool isSelected = btn.Background is Microsoft.UI.Xaml.Media.SolidColorBrush brush
                              && brush.Color == Microsoft.UI.ColorHelper.FromArgb(255, 235, 59, 89);

            try
            {
                if (isSelected)
                {
                    ViewModel!.RemoveInterest(interest);
                    btn.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 255, 255, 255));
                    btn.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 235, 59, 89));
                    btn.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 235, 59, 89));
                }
                else
                {
                    ViewModel!.AddInterest(interest);
                    btn.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 235, 59, 89));
                    btn.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White);
                    btn.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 235, 59, 89));
                }

                InterestCountText.Text = ViewModel!.ProfileData!.Interests.Count.ToString();
                UpdateNextButton();
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = ex.Message,
                    CloseButtonText = "OK",
                    XamlRoot = XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        // ── Step 4 handlers ───────────────────────────────────────────────────

        private void UpdatePreview()
        {
            DatingProfile preview = ViewModel!.GetPreviewProfile();

            PreviewNameAge.Text = $"{preview.Name}, {preview.Age}";
            PreviewAge.Text = preview.Age.ToString();
            PreviewGender.Text = preview.Gender.ToString();
            PreviewLocation.Text = preview.Location;
            PreviewNationality.Text = preview.Nationality;
            PreviewStarSignPanel.Visibility = preview.DisplayStarSign ? Visibility.Visible : Visibility.Collapsed;
            if (preview.DisplayStarSign)
                PreviewStarSign.Text = preview.GetStarSign().ToString();
            PreviewBio.Text = preview.Bio;
            PreviewInterestsControl.ItemsSource = preview.Interests;

            if (preview.Photos.Count > 0)
            {
                PreviewPhotoImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(
                    new Uri(preview.Photos[0].Location!));
            }
        }

        private void HandleNextPhotoClick(object sender, RoutedEventArgs e)
        {
            ViewModel!.NextPhoto();
            var photos = ViewModel.ProfileData!.Photos;
            if (photos.Count > 0)
            {
                PreviewPhotoImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(
                    new Uri(photos[ViewModel.CurrentPhotoIndex].Location!));
            }
        }

        private void HandlePreviousPhotoClick(object sender, RoutedEventArgs e)
        {
            ViewModel!.PreviousPhoto();
            var photos = ViewModel.ProfileData!.Photos;
            if (photos.Count > 0)
            {
                PreviewPhotoImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(
                    new Uri(photos[ViewModel.CurrentPhotoIndex].Location!));
            }
        }

        private void HandleTermsConditionsChecked(object sender, RoutedEventArgs e)
        {
            ViewModel!.TermsAccepted = TermsCheckBox.IsChecked == true;
            CreateProfileButton.IsEnabled = ViewModel.TermsAccepted;
        }

        private async void HandleCreateDatingProfileClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel!.CreateDatingProfile();
                Frame.Navigate(typeof(DiscoverView));
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = ex.Message,
                    CloseButtonText = "OK",
                    XamlRoot = XamlRoot
                };
                await dialog.ShowAsync();
            }
        }
    }
}
