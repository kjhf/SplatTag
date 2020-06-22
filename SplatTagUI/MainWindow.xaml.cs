using SplatTagCore;
using SplatTagDatabase;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;

namespace SplatTagUI
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    /// <summary>
    /// Timer delay in milliseconds for smooth searching for small searches
    /// </summary>
    private const int TIMER_DELAY_MILLIS = 300;

    /// <summary>
    /// Number of characters minimum before skipping the smooth searching timer delay
    /// </summary>
    private const int MIN_CHARACTERS_FOR_TIMER_SKIP = 4;

    internal static readonly SplatTagController splatTagController;
    private static readonly SplatTagJsonDatabase jsonDatabase;
    private static readonly MultiDatabase splatTagDatabase;
    private static readonly GenericFilesImporter multiSourceImporter;
    private static readonly string saveFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SplatTag");
    private readonly Timer smoothSearchDelayTimer;
    private readonly SynchronizationContext context;

    /// <summary>
    /// Version string to display.
    /// </summary>
    public string Version => "Version 0.0.6";

    /// <summary>
    /// Version tooltip string to display.
    /// </summary>
    public string VersionToolTip => "v0.0.06: Reduced lag for small searches. \nv0.0.05: Added multiple database GUI and merging of players and teams. \n  v0.0.05.1: Small improvement to handling of blank search. \n  v0.0.05.2: Added icon.";

    static MainWindow()
    {
      jsonDatabase = new SplatTagJsonDatabase(saveFolder);
      multiSourceImporter = new GenericFilesImporter(saveFolder);
      splatTagDatabase = new MultiDatabase(saveFolder, jsonDatabase, multiSourceImporter);
      splatTagController = new SplatTagController(splatTagDatabase);
      splatTagController.Initialise(new string[0]);
    }

    public MainWindow()
    {
      InitializeComponent();

      // Initialise the delay timer if we have a UI context, otherwise don't use the timer.
      context = SynchronizationContext.Current;
      if (context != null)
      {
        smoothSearchDelayTimer = new Timer(TimerExpired);
      }

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

    private void TimerExpired(object state)
    {
      this.context.Post((_) => Search(), this);
    }

    private void SearchWithTimerDelay()
    {
      if (smoothSearchDelayTimer != null)
      {
        // Apply a filter if the length of the search is less than n characters
        if (searchInput.Text.Length < MIN_CHARACTERS_FOR_TIMER_SKIP)
        {
          smoothSearchDelayTimer.Change(TIMER_DELAY_MILLIS, Timeout.Infinite);
        }
        else
        {
          smoothSearchDelayTimer.Change(1, Timeout.Infinite);
        }
      }
      else
      {
        Search();
      }
    }

    private void SearchInput_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
      SearchWithTimerDelay();
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
      SearchWithTimerDelay();
    }

    private void DataBaseButton_Click(object sender, RoutedEventArgs e)
    {
      DatabaseEditor databaseEditor = new DatabaseEditor(splatTagController, multiSourceImporter.Sources);
      bool? result = databaseEditor.ShowDialog();
      if (result == true)
      {
        // Editor accepted, save the sources list.
        multiSourceImporter.SaveSources(databaseEditor.DatabaseSources);

        // And load the new database.
        splatTagController.LoadDatabase();
      }
    }
  }

  public class GetTeamPlayersConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is Team t)
      {
        return MainWindow.splatTagController.GetPlayersForTeam(t).Select(tuple => tuple.Item1.Name + " " + (tuple.Item2 ? "(Current)" : "(Ex)")).ToArray();
      }
      return new string[] { "(Unknown Players)" };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  public class PlayerOldTeamsToStringConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      IEnumerable<Team> teams;
      if (value is Player p)
      {
        teams = p.Teams;
      }
      else if (value is IEnumerable<Team> t)
      {
        teams = t;
      }
      else
      {
        throw new InvalidDataException("Unknown type to convert: " + value.GetType());
      }

      StringBuilder sb = new StringBuilder();
      teams = teams.Skip(1);
      if (teams.Any())
      {
        sb.Append("(Old teams: ");
        sb.Append(string.Join(", ", teams.Select(t => t.Tag + " " + t.Name)));
        sb.Append(")");
      }

      return sb.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}