using SplatTagCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
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

    private void AcceptButton_Click(object sender, RoutedEventArgs e)
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
        HintText = "File, directory, or website to import? (You can paste into here!)"
      };
      bool? dialog = inputWindow.ShowDialog();
      if (dialog == true)
      {
        foreach (string path in inputWindow.Input.Split('>').Where(s => !string.IsNullOrWhiteSpace(s)))
        {
          if (IsDirectory(path) == true)
          {
            foreach (var file in Directory.EnumerateFiles(path))
            {
              if (!filesSource.Contains(file) && !file.Contains(SplatTagDatabase.GenericFilesImporter.SourcesFileName))
              {
                filesSource.Add(file);
              }
            }
          }
          else
          {
            filesSource.Add(path);
          }
        }
      }
    }

    /// <summary>
    /// Get if the path is a directory (true), file (false), or doesn't exist (null).
    /// </summary>
    private bool? IsDirectory(string path)
    {
      if (File.Exists(path)) return false;
      if (Directory.Exists(path)) return true;
      return null;
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
      if (databaseOrderListBox.SelectedIndex >= 1)
      {
        int newIndex = (databaseOrderListBox.SelectedIndex - 1) % filesSource.Count;
        filesSource.Move(databaseOrderListBox.SelectedIndex, newIndex);
        databaseOrderListBox.SelectedIndex = newIndex;
      }
      else if (databaseOrderListBox.SelectedIndex == 0)
      {
        int newIndex = filesSource.Count - 1;
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