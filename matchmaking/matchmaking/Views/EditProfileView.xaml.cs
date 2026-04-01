using matchmaking.Domain;
using matchmaking.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;


namespace matchmaking.Views
{

    public sealed partial class EditProfileView : Page
    {
        internal EditProfileViewModel? ViewModel { get; private set; }
        private bool _isRenderingPhotos = false;

        public EditProfileView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel = e.Parameter as EditProfileViewModel;
            this.DataContext = ViewModel;

            PhotosListView.Items.VectorChanged -= PhotoItems_VectorChanged;
            PhotosListView.Items.VectorChanged += PhotoItems_VectorChanged;

            BioBox.TextChanged -= BioBox_TextChanged;
            BioBox.TextChanged += BioBox_TextChanged;

            BioBox.BeforeTextChanging -= BioBox_BeforeTextChanging;
            BioBox.BeforeTextChanging += BioBox_BeforeTextChanging;

            DistanceSlider.ValueChanged -= DistanceSlider_ValueChanged;
            DistanceSlider.ValueChanged += DistanceSlider_ValueChanged;

            RefreshAllUI();
        }

        private Grid CreatePhotoSlot(Photo photo)
        {
            Grid slot = new Grid() { Width = 120, Height = 120, Margin = new Thickness(0, 0, 8, 0), Tag = photo.PhotoId };
            Image img = new Image() { Width = 120, Height = 120, Stretch = Stretch.UniformToFill, Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri("file:///" + photo.Location.Replace("\\", "/"))) };
            Button deleteBtn = CreateDeletePhotoButton(photo.PhotoId);
            slot.Children.Add(img);
            slot.Children.Add(deleteBtn);
            return slot;
        }

        private Button CreateDeletePhotoButton(int photoId)
        {
            Button deleteBtn = new Button()
            {
                Content = "✕",
                Width = 28,
                Height = 28,
                Padding = new Thickness(0),
                CornerRadius = new CornerRadius(14),
                Background = new SolidColorBrush(Microsoft.UI.Colors.Red),
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.White),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, -8, -8, 0),
                IsEnabled = ViewModel.CanRemovePhoto(),
                CanDrag = false,
                AllowDrop = false
            };
            deleteBtn.Click += (s, e) => { if (ViewModel.CanRemovePhoto()) { ViewModel.RemovePhoto(photoId); RenderPhotos(); } };
            return deleteBtn;
        }

        private void RefreshAllUI()
        {
            RenderPhotos();
            RenderInterests();
            RenderPreferredGenders();
            UpdateInterestsHeader();
            UpdateBioHeader();
            UpdateDistanceHeader();
            UpdateArchivedBanner();
            UpdateQuestionnaireButton();
        }

        private void RenderPhotos()
        {
            _isRenderingPhotos = true;

            PhotosListView.Items.Clear();

            foreach (Photo photo in ViewModel.Photos)
            {
                Grid slot = CreatePhotoSlot(photo);
                PhotosListView.Items.Add(slot);
            }

            if (ViewModel.Photos.Count < 6)
            {
                Border addSlot = new Border();
                addSlot.Width = 120;
                addSlot.Height = 120;
                addSlot.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.LightGray);
                addSlot.BorderThickness = new Thickness(1);
                addSlot.CornerRadius = new CornerRadius(8);
                addSlot.Margin = new Thickness(0, 0, 8, 0);

                Button addBtn = new Button();
                addBtn.Content = "+";
                addBtn.FontSize = 24;
                addBtn.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray);
                addBtn.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
                addBtn.HorizontalAlignment = HorizontalAlignment.Center;
                addBtn.VerticalAlignment = VerticalAlignment.Center;
                addBtn.Click += AddPhoto_Click;

                addSlot.Child = addBtn;
                PhotosListView.Items.Add(addSlot);
            }

            int emptySlots = ViewModel.Photos.Count < 6
                ? 6 - ViewModel.Photos.Count - 1
                : 0;

            for (int i = 0; i < emptySlots; i++)
            {
                Border emptySlot = new Border();
                emptySlot.Width = 120;
                emptySlot.Height = 120;
                emptySlot.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.LightGray);
                emptySlot.BorderThickness = new Thickness(1);
                emptySlot.CornerRadius = new CornerRadius(8);
                emptySlot.Margin = new Thickness(0, 0, 8, 0);
                PhotosListView.Items.Add(emptySlot);
            }

            _isRenderingPhotos = false;
        }

        private void PhotoItems_VectorChanged(Windows.Foundation.Collections.IObservableVector<object> sender, Windows.Foundation.Collections.IVectorChangedEventArgs e)
        {
            if (_isRenderingPhotos || ViewModel == null)
                return;

            List<int> newOrder = PhotosListView.Items
                .OfType<Grid>()
                .Where(g => g.Tag is int)
                .Select(g => (int)g.Tag)
                .ToList();

            if (newOrder.Count == ViewModel.Photos.Count)
            {
                ViewModel.ReorderPhotos(newOrder);
            }
        }

        private async void AddPhoto_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(
                (Application.Current as App)._window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                Photo newPhoto = new Photo(ViewModel._userId, file.Path, ViewModel.Photos.Count);
                ViewModel.AddPhoto(newPhoto);
                RenderPhotos();
            }
        }

        private void RenderPreferredGenders()
        {
            PreferredGendersPanel.Children.Clear();
            Gender[] genderValues = { Gender.MALE, Gender.FEMALE, Gender.NON_BINARY, Gender.OTHER };
            List<string> genders = ViewModel.GenderOptions;

            for (int i = 0; i < genders.Count; i++)
            {
                Gender g = genderValues[i];
                bool isSelected = ViewModel.PreferredGenders.Contains(g);

                Button btn = new Button();
                btn.Content = genders[i];
                btn.Style = isSelected
                    ? (Style)Resources["SelectedInterestButtonStyle"]
                    : (Style)Resources["UnselectedInterestButtonStyle"];

                btn.Click += (s, e) =>
                {
                    if (ViewModel.PreferredGenders.Contains(g))
                        ViewModel.PreferredGenders.Remove(g);
                    else
                        ViewModel.PreferredGenders.Add(g);
                    RenderPreferredGenders();
                };

                PreferredGendersPanel.Children.Add(btn);
            }
        }

        private void RenderInterests()
        {
            if (ViewModel == null) return;

            InterestsPanel.Children.Clear();

            foreach (string interest in ViewModel.AllInterests)
            {
                bool isSelected = ViewModel.Interests
                    .Any(i => i.Trim().Equals(interest.Trim(), StringComparison.OrdinalIgnoreCase));

                Button btn = new Button();
                btn.Content = interest;
                btn.IsEnabled = true;
                btn.Style = isSelected
                    ? (Style)Resources["SelectedInterestButtonStyle"]
                    : (Style)Resources["UnselectedInterestButtonStyle"];

                string interestCopy = interest;
                btn.Click += (s, e) =>
                {
                    string? existingInterest = ViewModel.Interests
                        .FirstOrDefault(i => i.Trim().Equals(interestCopy.Trim(), StringComparison.OrdinalIgnoreCase));

                    if (existingInterest != null)
                    {
                        if (ViewModel.CanRemoveInterest(existingInterest))
                            ViewModel.RemoveInterest(existingInterest);
                    }
                    else
                    {
                        if (ViewModel.CanAddInterest())
                            ViewModel.AddInterest(interestCopy);
                    }

                    RenderInterests();
                    UpdateInterestsHeader();
                };

                InterestsPanel.Children.Add(btn);
            }
        }

        private void UpdateInterestsHeader()
        {
            InterestsHeader.Text = $"Interests ({ViewModel.Interests.Count}/15)";
        }

        private void UpdateBioHeader()
        {
            BioHeader.Text = $"Bio ({BioBox.Text.Length}/250)";
        }

        private void UpdateDistanceHeader()
        {
            if (ViewModel != null)
            {
                DistanceHeader.Text = $"Maximum Distance ({ViewModel.MaxDistance} km)";
            }
        }

        private void DistanceSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            UpdateDistanceHeader();
        }

        private void BioBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (sender.Text.Length >= 250)
            {
                args.Cancel = true;
            }
        }

        private void BioBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateBioHeader();
        }

        private async void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Save Changes";
            dialog.Content = "Are you sure you want to save your changes?";
            dialog.PrimaryButtonText = "Save";
            dialog.CloseButtonText = "Cancel";
            dialog.XamlRoot = this.XamlRoot;

            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                ViewModel.SaveChangesCommand.Execute(null);
            }
        }

        private async void DiscardChanges_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Discard Changes";
            dialog.Content = "Are you sure you want to discard all changes? This cannot be undone.";
            dialog.PrimaryButtonText = "Discard";
            dialog.CloseButtonText = "Cancel";
            dialog.XamlRoot = this.XamlRoot;

            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                ViewModel.DiscardChangesCommand.Execute(null);
                RefreshAllUI();
            }
        }

        private async void ArchiveProfile_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Archive Profile";
            dialog.Content = "Are you sure you want to archive your profile? It will not be visible to other users.";
            dialog.PrimaryButtonText = "Archive";
            dialog.CloseButtonText = "Cancel";
            dialog.XamlRoot = this.XamlRoot;

            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                ViewModel.ArchiveProfileCommand.Execute(null);
                UpdateArchivedBanner();
            }
        }

        private async void UnarchiveProfile_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Unarchive Profile";
            dialog.Content = "Are you sure you want to unarchive your profile? It will be visible to other users again.";
            dialog.PrimaryButtonText = "Unarchive";
            dialog.CloseButtonText = "Cancel";
            dialog.XamlRoot = this.XamlRoot;

            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                ViewModel.UnarchiveProfileCommand.Execute(null);
                UpdateArchivedBanner();
            }
        }

        private async void DeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Delete Profile";
            dialog.Content = "Are you sure you want to delete your profile? This cannot be undone.";
            dialog.PrimaryButtonText = "Delete";
            dialog.CloseButtonText = "Cancel";
            dialog.XamlRoot = this.XamlRoot;

            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                ViewModel.DeleteProfileCommand.Execute(null);
            }
        }

        private void UpdateArchivedBanner()
        {
            ArchivedBanner.Visibility = ViewModel.IsArchived
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void QuestionnaireButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(QuestionnaireView), ViewModel);
        }

        private void UpdateQuestionnaireButton()
        {
            QuestionnaireButton.Visibility = ViewModel.HasLoverType
                ? Visibility.Collapsed
                : Visibility.Visible;
        }
    }
}