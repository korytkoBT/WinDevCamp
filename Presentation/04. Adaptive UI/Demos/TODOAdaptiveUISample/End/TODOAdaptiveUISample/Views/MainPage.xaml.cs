﻿using System;
using TODOAdaptiveUISample.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace TODOAdaptiveUISample.Views
{
    public sealed partial class MainPage : Page
    {
        ViewModels.MainPageViewModel ViewModel { get; set; }
        private TodoItemViewModel PreviousSelectedItem;

        public MainPage()
        {
            this.InitializeComponent();
            this.ViewModel = this.DataContext as ViewModels.MainPageViewModel;
            var viewTitleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;


            viewTitleBar.BackgroundColor = Windows.UI.Color.FromArgb(0xCC, 0xC9, 0xC9, 0xC9);
            viewTitleBar.ButtonBackgroundColor = Windows.UI.Color.FromArgb(0xCC, 0xC9, 0xC9, 0xC9);

            if (ApiInformation.IsTypePresent(typeof(StatusBar).ToString()))
                StatusBar.GetForCurrentView().HideAsync();

            this.Loaded += MainPage_Loaded;
            this.SizeChanged += MainPage_SizeChanged;
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 600 && e.PreviousSize.Width <= 600)
            {
                ToDoListView.SelectedItem = PreviousSelectedItem;
            }
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (Window.Current.Bounds.Width > 600)
            {
                try
                {
                    ToDoListView.SelectedIndex = 0;
                }
                catch
                {
                    //nop
                }
            }
        }


        // using a tapped event so we can have hitable areas inside the listviewitem without
        // actualy selecting the item
        private void TodoItem_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            // If the inline panel is not showing, navigate to the separate editing page
            if ((sender as Border).DataContext != null)
            {
                ToDoListView.SelectedItem = ((TodoItemViewModel)(sender as Border).DataContext);
            }
        }

        private void ToDoListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InlineViewerEditor.DataContext != null && InlineViewerEditor.DataContext is TodoItemViewModel)
            {
                (InlineViewerEditor.DataContext as TodoItemViewModel).UpdateItemCommand.Execute(null);
            }

            if (ToDoListView.SelectedItem == null)
            {
                splitView.IsPaneOpen = false;
                InlineViewerEditor.DataContext = null;
            }
            else
            {
                InlineViewerEditor.DataContext = ToDoListView.SelectedItem;
                OpenPane();
            }
            
        }

        private void OpenPane()
        {
            splitView.IsPaneOpen = true;
            if (Window.Current.Bounds.Width <= 600)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            }
        }

        private TextBox NewToDoItemNameTextBox = null;

        private AppBarButton AddNewItemConfirmButton = null;

        private void AddNewItemConfirmButton_Loaded(object sender, RoutedEventArgs e)
        {
            // This button is in a data template, so we can use the Loaded event to get a reference to it
            // You can't get at controls in Data Templates in Item Templates using their name
            AddNewItemConfirmButton = sender as AppBarButton;
        }

        private void TextBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            NewToDoItemNameTextBox = textBox;

            if (!string.IsNullOrEmpty(textBox.Text)
                && textBox.Text.Length > 1)
            {
                if (AddNewItemConfirmButton != null)
                    AddNewItemConfirmButton.IsEnabled = true;

                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    // Handle 'Enter' key for keyboard users
                    e.Handled = true;
                    CreateNewToDoItem(textBox);
                }
            }
            else
            {
                if (AddNewItemConfirmButton != null)
                    AddNewItemConfirmButton.IsEnabled = false;
            }
        }

        private void CreateNewToDoItem(TextBox textBox)
        {
            var vm = textBox.DataContext as ViewModels.MainPageViewModel;
            vm.AddItemCommand.Execute(textBox.Text);
            textBox.Text = string.Empty;
            textBox.Focus(Windows.UI.Xaml.FocusState.Programmatic);

            if (AddNewItemConfirmButton != null)
                AddNewItemConfirmButton.IsEnabled = false;
        }

        private void AddNewItemConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewToDoItemNameTextBox != null)
            {
                CreateNewToDoItem(NewToDoItemNameTextBox);
            }
        }

        private void splitView_PaneClosed(SplitView sender, object args)
        {
            if (Window.Current.Bounds.Width > 600)
                try
                {
                    ToDoListView.SelectedIndex = 0;
                }
                catch { }
            else
            {
                PreviousSelectedItem = ToDoListView.SelectedItem as TodoItemViewModel;
                ToDoListView.SelectedItem = null;
            }

            if (!this.Frame.CanGoBack)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            }

        }

        private void InlineViewerEditor_DeleteItemClicked(object sender, EventArgs e)
        {
            if (InlineViewerEditor.DataContext != null && InlineViewerEditor.DataContext is TodoItemViewModel)
            {
                var vm = this.DataContext as MainPageViewModel;
                vm.RemoveItemCommand.Execute(null);
            }
            
        }

        private void InlineViewerEditor_CommandCompleted(object sender, CommandCompletedEventArgs e)
        {
            if (Window.Current.Bounds.Width <= 600)
            {
                ToDoListView.SelectedItem = e.ViewModel;
            }
        }
    }
}
