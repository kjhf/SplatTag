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
using System.Windows.Controls;
using System.Windows.Data;

namespace SplatTagUI
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
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
    public string Version => "Version 0.0.13";

    /// <summary>
    /// Version tooltip string to display.
    /// </summary>
    public string VersionToolTip => "v0.0.13: Better div support. Better sameness detection.\nv0.0.12: Added command-line. \nv0.0.11: Added Twitter handle capability. \nv0.0.10: Supports InkTV. Pulls friend codes from names. Corrected bugs around incomplete tournament data. Added a browse button to the file loader. \nv0.0.09: Now searches FCs and Discord tags. Teams now have a highest div'd label. Stability improvements.";

    static MainWindow()
    {
      jsonDatabase = new SplatTagJsonDatabase(saveFolder);
      multiSourceImporter = new GenericFilesImporter(saveFolder);
      splatTagDatabase = new MultiDatabase(saveFolder, jsonDatabase, multiSourceImporter);
      splatTagController = new SplatTagController(splatTagDatabase);
      splatTagController.Initialise();
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
          smoothSearchDelayTimer.Change((int)delaySlider.Value, Timeout.Infinite);
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

    private void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
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

    private void TwitterButton_Click(object sender, RoutedEventArgs e)
    {
      FrameworkElement b = (FrameworkElement)sender;
      Team t = (Team)b.DataContext;
      splatTagController.TryLaunchTwitter(t);
    }
  }

  public class GetTeamPlayersConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is Team t)
      {
        return t.GetTeamPlayersStrings(MainWindow.splatTagController);
      }
      return new string[] { "(Unknown Players)" };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
  }

  public class SourcesToStringConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      IEnumerable<string> sources;
      if (value is IEnumerable<string> s)
      {
        sources = s;
      }
      else if (value is Player p)
      {
        sources = p.Sources;
      }
      else if (value is Team t)
      {
        sources = t.Sources;
      }
      else
      {
        throw new InvalidDataException("Unknown type to convert: " + value.GetType());
      }

      return string.Join(", ", sources);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
  }

  public class PlayerOldTeamsToStringConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      IEnumerable<Team> teams;
      if (value is Player p)
      {
        teams = p.Teams.Select(id => MainWindow.splatTagController.GetTeamById(id));
      }
      else if (value is IEnumerable<Team> t)
      {
        teams = t;
      }
      else
      {
        throw new InvalidDataException("Unknown type to convert: " + value.GetType());
      }

      return teams.GetOldTeamsStrings();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
  }

  public class GetTeamBestPlayerDivConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is Team t)
      {
      }
      else if (value is long teamId)
      {
        t = MainWindow.splatTagController.GetTeamById(teamId);
      }
      else
      {
        throw new InvalidDataException("Unknown type to convert: " + value.GetType());
      }

      return t.GetBestTeamPlayerDivString(MainWindow.splatTagController);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
  }

  public class TeamIdToString : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is Team t)
      {
        return t;
      }
      else if (value is long teamId)
      {
        return MainWindow.splatTagController.GetTeamById(teamId);
      }
      else
      {
        throw new InvalidDataException("Unknown type to convert: " + value.GetType());
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
  }

  public class JoinStringsConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is string[] values1)
      {
        return string.Join(", ", values1);
      }
      else if (value is IEnumerable<string> values2)
      {
        return string.Join(", ", values2);
      }
      else if (value is string values3)
      {
        return values3;
      }
      else
      {
        throw new InvalidDataException("Unknown type to convert: " + value.GetType());
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
  }

  public class ValidStringToVisibleConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      bool isValid;
      if (value is string str)
      {
        isValid = !string.IsNullOrWhiteSpace(str);
      }
      else
      {
        isValid = false;
      }

      return new BooleanToVisibilityConverter().Convert(isValid, targetType, parameter, culture);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
  }
}