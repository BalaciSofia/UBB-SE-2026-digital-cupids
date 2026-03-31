using matchmaking.Domain;
using matchmaking.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace matchmaking.ViewModels
{
    internal class AdminViewModel : INotifyPropertyChanged
    {
        private readonly SupportTicketService _supportTicketService;
        private readonly ProfileService _profileService;

        private List<SupportTicket> _unresolvedTickets = new();
        private SupportTicket _selectedTicket;
        private List<DatingProfile> _searchResults = new();
        private string _searchQuery = string.Empty;
        private string _errorMessage = string.Empty;

        public List<SupportTicket> UnresolvedTickets
        {
            get => _unresolvedTickets;
            private set { if (_unresolvedTickets != value) { _unresolvedTickets = value; OnPropertyChanged(); } }
        }

        public SupportTicket SelectedTicket
        {
            get => _selectedTicket;
            set { if (_selectedTicket != value) { _selectedTicket = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasSelectedTicket)); } }
        }

        public bool HasSelectedTicket => SelectedTicket != null;

        public List<DatingProfile> SearchResults
        {
            get => _searchResults;
            private set { if (_searchResults != value) { _searchResults = value; OnPropertyChanged(); } }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set { if (_searchQuery != value) { _searchQuery = value; OnPropertyChanged(); } }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            private set { if (_errorMessage != value) { _errorMessage = value; OnPropertyChanged(); } }
        }

        public AdminViewModel(SupportTicketService supportTicketService, ProfileService profileService)
        {
            _supportTicketService = supportTicketService;
            _profileService = profileService;

            LoadUnresolvedTickets();
        }

        public void LoadUnresolvedTickets()
        {
            try
            {
                UnresolvedTickets = _supportTicketService.GetAllUnresolved();
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        public void SelectTicket(SupportTicket ticket)
        {
            SelectedTicket = ticket;
        }

        public void Search()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchQuery))
                {
                    SearchResults = new List<DatingProfile>();
                    return;
                }

                SearchResults = _profileService.SearchByName(SearchQuery);
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        public void ResolveFound()
        {
            if (SelectedTicket == null) return;
            try
            {
                _supportTicketService.ResolveTicket(SelectedTicket.Email, true);
                SelectedTicket = null;
                LoadUnresolvedTickets();
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        public void ResolveNotFound()
        {
            if (SelectedTicket == null) return;
            try
            {
                _supportTicketService.ResolveTicket(SelectedTicket.Email, false);
                SelectedTicket = null;
                LoadUnresolvedTickets();
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        public bool CanResolve() => SelectedTicket != null;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}