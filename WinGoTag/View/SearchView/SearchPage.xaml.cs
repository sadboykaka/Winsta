﻿using InstaSharper.Classes.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using WinGoTag.View.UserViews;

// Il modello di elemento Pagina vuota è documentato all'indirizzo https://go.microsoft.com/fwlink/?LinkId=234238

namespace WinGoTag.View.SearchView
{
    /// <summary>
    /// Pagina vuota che può essere usata autonomamente oppure per l'esplorazione all'interno di un frame.
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        public static Grid GridAuto;
        public static SearchPage Current;
        public SearchPage()
        {
            this.InitializeComponent();
            Current = this;
            EditFr.Navigate(typeof(Page));
            GridAuto = GridAutoSuggest;
            Loaded += SearchPage_Loaded;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e != null && e.Parameter != null && e.Parameter is string str)
            {
                PushSearch(str);
            }
        }
        public void PushSearch(string query)
        {
            if (!string.IsNullOrEmpty(query))
                SearchBox.Text = query;
            SearchBox.Focus(FocusState.Pointer);
        }
        private async void SearchPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var recent = await AppCore.InstaApi.DiscoverProcessor.GetRecentSearchsAsync();
                RecentList.ItemsSource = recent.Value.Recent;
            }
            catch { }
        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {

        }

        private async void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var query = SearchBox.Text;
            if (query.StartsWith("@"))
            {
                var People = await AppCore.InstaApi.DiscoverProcessor.SearchPeopleAsync(query);
                PeopleList.ItemsSource = People.Value.Users;
                PivotSearch.SelectedIndex = 1;
            }
            else if (query.StartsWith("#"))
            {
                var ForTag = await AppCore.InstaApi.SearchHashtag(query.Replace("#", ""));
                TagsList.ItemsSource = ForTag.Value;
                PivotSearch.SelectedIndex = 2;
            }
            else
            {
                if (string.IsNullOrEmpty(query) || string.IsNullOrWhiteSpace(query))
                    return;
                if (PivotSearch.SelectedIndex == 3)
                {
                    var ForLocation = await AppCore.InstaApi.SearchLocation(0, 0, query);
                    PlacesList.ItemsSource = ForLocation.Value;
                }
                else
                {
                    var People = await AppCore.InstaApi.DiscoverProcessor.SearchPeopleAsync(query);
                    PeopleList.ItemsSource = People.Value.Users;
                    PivotSearch.SelectedIndex = 1;
                }

            }
        }


        private void CancelBT_Click(object sender, RoutedEventArgs e)
        {
            AnimationGrid(GridSearch, 1, 0, Visibility.Collapsed);
            //GridSearch.Visibility = Visibility.Collapsed;
            CancelBT.Visibility = Visibility.Collapsed;
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if(CancelBT.Visibility != Visibility.Visible)
            {
                AnimationGrid(GridSearch, 0, 1, Visibility.Visible);
                //GridSearch.Visibility = Visibility.Visible;
                CancelBT.Visibility = Visibility.Visible;
            }
        }

        private void AnimationGrid(Grid sender, double From, double To, Visibility visibility)
        {
            DoubleAnimation fade = new DoubleAnimation()
            {
                From = From,
                To = To,
                Duration = TimeSpan.FromSeconds(0.4),
                EnableDependentAnimation = true
            };
            Storyboard.SetTarget(fade, sender);
            Storyboard.SetTargetProperty(fade, "Opacity");
            Storyboard openpane = new Storyboard();
            openpane.Children.Add(fade);
            openpane.Begin();
            Task.Delay(04);
            GridSearch.Visibility = visibility;
        }



        private void ExploreFr_Loaded(object sender, RoutedEventArgs e)
        {
            ExploreFr.Navigate(typeof(ExploreView));
        }

        private void PeopleList_ItemClick(object sender, ItemClickEventArgs e)
        {
            EditFr.Navigate(typeof(UserProfileView), e.ClickedItem);
        }

        private void PivotSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SearchBox_TextChanged(SearchBox, null);
            }
            catch { }
        }
    }
}
