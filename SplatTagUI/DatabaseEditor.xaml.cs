using SplatTagCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace SplatTagUI
{
  /// <summary>
  /// Interaction logic for DatabaseEditor.xaml
  /// </summary>
  public partial class DatabaseEditor : Window
  {
    private readonly ObservableCollection<string> filesSource = new ObservableCollection<string>();
    private readonly SplatTagController splatTagController;

    /// <summary>Take a copy of the file sources selected by the user that should make up the database.</summary>
    public string[] DatabaseSources => filesSource.ToArray();

    private DatabaseEditor()
    {
      InitializeComponent();
    }

    public DatabaseEditor(SplatTagController splatTagController, IEnumerable<string> originalFilesSource = null)
    {
      InitializeComponent();
      if (originalFilesSource != null)
      {
        filesSource = new ObservableCollection<string>(originalFilesSource);
      }
      databaseOrderListBox.ItemsSource = filesSource;
      this.splatTagController = splatTagController ?? throw new ArgumentNullException(nameof(splatTagController));
    }

    private void acceptButton_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
      Close();
    }

    private void ReloadButton_Click(object sender, RoutedEventArgs e)
    {
      var result = MessageBox.Show("Are you sure? This will overwrite any imported database changes.", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
      if (result == MessageBoxResult.Yes)
      {
        splatTagController.LoadDatabase();
        MessageBox.Show("Reloaded successfully.");
      }
    }

    private void LoadButton_Click(object sender, RoutedEventArgs e)
    {
      InputWindow inputWindow = new InputWindow
      {
        HintText = "File or site to import?"
      };
      bool? dialog = inputWindow.ShowDialog();
      if (dialog == true)
      {
        filesSource.Add(inputWindow.Input);
      }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
      splatTagController.SaveDatabase();
      MessageBox.Show("Saved successfully.");
    }

    private void RemoveButton_Click(object sender, RoutedEventArgs e)
    {
      if (databaseOrderListBox.SelectedIndex >= 0)
      {
        filesSource.RemoveAt(databaseOrderListBox.SelectedIndex);
      }
    }

    private void OrderUpButton_Click(object sender, RoutedEventArgs e)
    {
      if (databaseOrderListBox.SelectedIndex >= 0)
      {
        int newIndex = (databaseOrderListBox.SelectedIndex - 1) % filesSource.Count;
        filesSource.Move(databaseOrderListBox.SelectedIndex, newIndex);
        databaseOrderListBox.SelectedIndex = newIndex;
      }
    }

    private void OrderDownButton_Click(object sender, RoutedEventArgs e)
    {
      if (databaseOrderListBox.SelectedIndex >= 0)
      {
        int newIndex = (databaseOrderListBox.SelectedIndex + 1) % filesSource.Count;
        filesSource.Move(databaseOrderListBox.SelectedIndex, newIndex);
        databaseOrderListBox.SelectedIndex = newIndex;
      }
    }
  }

  public class ObjectNullConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return !Equals(value, null);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}