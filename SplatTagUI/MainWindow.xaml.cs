using SplatTagCore;
using SplatTagDatabase;
using System;
using System.IO;
using System.Windows;

namespace SplatTagUI
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private readonly SplatTagController splatTagController;
    private readonly SplatTagJsonDatabase splatTagDatabase;
    private static readonly string jsonFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SplatTag");

    /// <summary>
    /// Version string to display.
    /// </summary>
    public string Version => "Version 0.0.2";

    /// <summary>
    /// Version tooltip string to display.
    /// </summary>
    public string VersionToolTip => "Added empty search option, added version, added automatic scroll bars to outputs.";

    public MainWindow()
    {
      splatTagDatabase = new SplatTagJsonDatabase(jsonFile);
      splatTagController = new SplatTagController(splatTagDatabase);
      splatTagController.Initialise(new string[0]);
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
}