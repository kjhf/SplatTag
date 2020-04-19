using SplatTagCore;
using SplatTagDatabase;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace SplatTagUI
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    internal static readonly SplatTagController splatTagController;
    private static readonly SplatTagJsonDatabase splatTagDatabase;
    private static readonly string jsonFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SplatTag");

    /// <summary>
    /// Version string to display.
    /// </summary>
    public string Version => "Version 0.0.3";

    /// <summary>
    /// Version tooltip string to display.
    /// </summary>
    public string VersionToolTip => "Added support for season X. Added expander for teams.";
    
    static MainWindow()
    {
      splatTagDatabase = new SplatTagJsonDatabase(jsonFile);
      splatTagController = new SplatTagController(splatTagDatabase);
      splatTagController.Initialise(new string[0]);
    }

    public MainWindow()
    {
      InitializeComponent();

      // Now we've initialised, hook up the check changed.
      ignoreCaseCheckbox.Checked += CheckedChanged;
      ignoreCaseCheckbox.Unchecked += CheckedChanged;
      nearMatchCheckbox.Checked += CheckedChanged;
      nearMatchCheckbox.Unchecked += CheckedChanged;
      regexCheckbox.Checked += CheckedChanged;
      regexCheckbox.Unchecked += CheckedChanged;

      // Focus the input text box so we can start typing as soon as the program starts.
      searchInput.Focus();
    }

    private void FetchButton_Click(object sender, RoutedEventArgs e)
    {
      InputWindow inputWindow = new InputWindow
      {
        HintText = "File or site to import?"
      };
      bool? dialog = inputWindow.ShowDialog();
      if (dialog == true)
      {
        string errorMessage = splatTagController.TryImport(inputWindow.Input);
        if (!string.IsNullOrWhiteSpace(errorMessage))
        {
          MessageBox.Show("ERROR: " + errorMessage);
        }
        else
        {
          MessageBox.Show("Successfully imported. Note that the database has not been saved yet.");
        }
      }
    }

    private void LoadButton_Click(object sender, RoutedEventArgs e)
    {
      var result = MessageBox.Show("Are you sure? This will overwrite any imported database changes.", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
      if (result == MessageBoxResult.Yes)
      {
        splatTagController.LoadDatabase();
        MessageBox.Show("Loaded successfully.");
      }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
      splatTagController.SaveDatabase();
      MessageBox.Show("Saved successfully.");
    }

    private void SearchInput_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
      Search();
    }

    private void Search()
    {
      if (allowEmptySearchCheckbox.IsChecked == false && searchInput.Text.Length < 1)
      {
        return;
      }

      playersListBox.ItemsSource =
       splatTagController.MatchPlayer(searchInput.Text,
         new MatchOptions
         {
           IgnoreCase = ignoreCaseCheckbox.IsChecked == true,
           NearCharacterRecognition = nearMatchCheckbox.IsChecked == true,
           QueryIsRegex = regexCheckbox.IsChecked == true
         }
       );

      teamsListBox.ItemsSource =
      splatTagController.MatchTeam(searchInput.Text,
        new MatchOptions
        {
          IgnoreCase = ignoreCaseCheckbox.IsChecked == true,
          NearCharacterRecognition = nearMatchCheckbox.IsChecked == true,
          QueryIsRegex = regexCheckbox.IsChecked == true
        }
      );
    }

    private void CheckedChanged(object sender, RoutedEventArgs e)
    {
      Search();
    }
  }

  public class GetTeamPlayersConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is Team t)
      {
        return MainWindow.splatTagController.GetCurrentPlayersForTeam(t).Select(p => p.Name).ToArray();
      }
      return new string[] { "(Unknown Players)" };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}