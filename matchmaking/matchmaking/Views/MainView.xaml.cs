using matchmaking.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using matchmaking.Domain;
using matchmaking.Services;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;

namespace matchmaking.Views
{
    internal sealed partial class MainView : Page
    {
        internal MainViewModel? ViewModel { get; private set; }
        private NotificationService? _notificationService;
        private int _userId;
        private List<int> _knownNotificationIds = new List<int>();
        private DispatcherQueueTimer? _timer;

        public MainView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel = e.Parameter as MainViewModel;

            if (NavView.MenuItems.Count > 0)
            {
                NavView.SelectedItem = NavView.MenuItems[0];

                if (ViewModel != null)
                {
                    _notificationService = ViewModel.NotificationService;
                    _userId = ViewModel.UserId;

                    var existing = _notificationService.FindByRecipientId(_userId);
                    _knownNotificationIds = existing.Select(n => n.NotificationId).ToList();

                    UpdateBadge();
                    StartPolling();
                }
            }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                string tag = item.Tag?.ToString() ?? string.Empty;
                NavigateToTab(tag);
            }
        }

        private void NavigateToTab(string tag)
        {
            if (ViewModel == null) return;

            switch (tag)
            {
                case "Discover":
                    ViewModel.DiscoverViewModel.LoadCandidates();
                    ContentFrame.Navigate(typeof(DiscoverView));
                    if (ContentFrame.Content is DiscoverView discoverView)
                    {
                        discoverView.SetViewModel(ViewModel.DiscoverViewModel);
                    }
                    break;
                case "Notifications":
                    ContentFrame.Navigate(typeof(NotificationsView));
                    if (ContentFrame.Content is NotificationsView notificationsView)
                    {
                        notificationsView.SetViewModel(ViewModel.NotificationsViewModel);
                    }
                    UpdateBadge(); 
                    break;

                case "HotSeat":
                    ContentFrame.Navigate(typeof(HotSeatView), ViewModel.HotSeatViewModel);
                    break;

                case "EditProfile":
                    ContentFrame.Navigate(typeof(EditProfileView), ViewModel.EditProfileViewModel);
                    break;

            }
        }
        private void StartPolling()
        {
            _timer = DispatcherQueue.GetForCurrentThread().CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(10);
            _timer.Tick += (s, e) => CheckForNewNotifications();
            _timer.Start();
        }

        private void CheckForNewNotifications()
        {
            if (_notificationService == null) return;

            var notifications = _notificationService.FindByRecipientId(_userId);
            var newNotifications = notifications
                .Where(n => !_knownNotificationIds.Contains(n.NotificationId))
                .ToList();

            foreach (Notification notification in newNotifications)
            {
                _knownNotificationIds.Add(notification.NotificationId);
                ShowPopup(notification);
            }

            UpdateBadge();
        }

        private void UpdateBadge()
        {
            if (_notificationService == null) return;

            var notifications = _notificationService.FindByRecipientId(_userId);
            int unreadCount = notifications.Count(n => !n.IsRead);

            if (unreadCount > 0)
            {
                NotificationBadge.Visibility = Visibility.Visible;
                NotificationBadgeText.Text = unreadCount.ToString();
            }
            else
            {
                NotificationBadge.Visibility = Visibility.Collapsed;
            }
        }

        private async void ShowPopup(Notification notification)
        {
            PopupTitle.Text = notification.Title;
            PopupDescription.Text = notification.Description;
            NotificationPopup.Visibility = Visibility.Visible;

            await System.Threading.Tasks.Task.Delay(4000);
            NotificationPopup.Visibility = Visibility.Collapsed;
        }
    }

}
