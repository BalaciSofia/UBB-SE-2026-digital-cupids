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
    public sealed partial class AdminView : Page
    {
        internal AdminViewModel ViewModel { get; private set; }

        public AdminView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel = (AdminViewModel)e.Parameter;
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Search();
        }

        private void Ticket_Expanding(Expander sender, ExpanderExpandingEventArgs args)
        {
            var ticket = (SupportTicket)sender.Tag;
            ViewModel.SelectTicket(ticket);
            NoTicketText.Visibility = Visibility.Collapsed;

            if (ticket.PartnerPhotoPath != null)
            {
                SelectedPartnerPhoto.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(
                    new Uri(ticket.PartnerPhotoPath));
            }
            else
            {
                SelectedPartnerPhoto.Source = null;
            }
        }

        private void Ticket_Collapsed(Expander sender, ExpanderCollapsedEventArgs args)
        {
            ViewModel.SelectTicket(null);
            SelectedPartnerPhoto.Source = null;
            SelectedPartnerName.Text = string.Empty;
            NoTicketText.Visibility = Visibility.Visible;
        }

        private void Found_Click(object sender, RoutedEventArgs e)
        {
            var ticket = (SupportTicket)((Button)sender).Tag;
            ViewModel.SelectTicket(ticket);
            ViewModel.ResolveFound();
            SelectedPartnerPhoto.Source = null;
            SelectedPartnerName.Text = string.Empty;
            NoTicketText.Visibility = Visibility.Visible;
        }

        private void NotFound_Click(object sender, RoutedEventArgs e)
        {
            var ticket = (SupportTicket)((Button)sender).Tag;
            ViewModel.SelectTicket(ticket);
            ViewModel.ResolveNotFound();
            SelectedPartnerPhoto.Source = null;
            SelectedPartnerName.Text = string.Empty;
            NoTicketText.Visibility = Visibility.Visible;
        }

        private void UpdateSelectedPhoto()
        {
            if (ViewModel.SelectedTicket?.PartnerPhotoPath != null)
            {
                SelectedPartnerPhoto.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(
                    new System.Uri(ViewModel.SelectedTicket.PartnerPhotoPath));
            }
            else
            {
                SelectedPartnerPhoto.Source = null;
            }
        }

        private void SearchResultsList_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.Item is DatingProfile profile)
            {
                var container = args.ItemContainer.ContentTemplateRoot as StackPanel;
                if (container == null) return;

                var image = container.Children[0] as Image;
                if (image == null) return;

                var firstPhoto = profile.Photos?.OrderBy(p => p.ProfileOrderIndex).FirstOrDefault();
                if (firstPhoto?.Location != null)
                {
                    image.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(
                        new Uri(firstPhoto.Location));
                }
            }
        }
    }
}