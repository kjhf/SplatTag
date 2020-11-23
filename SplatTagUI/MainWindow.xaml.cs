using Newtonsoft.Json;
using SplatTagCore;
using SplatTagDatabase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
    private const int MIN_CHARACTERS_FOR_TIMER_SKIP = 6;

    private readonly string titleLead;

    internal static readonly SplatTagController splatTagController;
    private static readonly GenericFilesImporter sourcesImporter;
    private readonly Timer smoothSearchDelayTimer;
    private readonly SynchronizationContext context;

    /// <summary>
    /// Version string to display.
    /// </summary>
    public string Version => "Version 0.0.22";

    /// <summary>
    /// Version tooltip string to display.
    /// </summary>
    public string VersionToolTip =>
      "v0.0.22: More merging bug fixes. UI now has Battlefy buttons if the slug(s) are known. DSB compatibility.\n" +
      "v0.0.21: Stability, merging, and other bug fixes.\n" +
      "v0.0.20: Reworked matching to show higher relevance results first.\n" +
      "v0.0.19: Stat.ink compatibility.\n" +
      "v0.0.18: Better context menus and info now copies.\n" +
      "v0.0.17: Twitter buttons for Players\n" +
      "v0.0.16: TSV loading to include early LUTI\n" +
      "v0.0.15: Sendou details \n" +
      "v0.0.14: Now works off of snapshots to drastically improve load time. \n" +
      "v0.0.13: Better div support. Better sameness detection.\n" +
      "v0.0.12: Added command-line. \n" +
      "v0.0.11: Added Twitter handle capability. \n" +
      "v0.0.10: Supports InkTV. Pulls friend codes from names. Corrected bugs around incomplete tournament data. Added a browse button to the file loader.";

    static MainWindow()
    {
      (splatTagController, sourcesImporter) = SplatTagControllerFactory.CreateController();
    }

    public MainWindow()
    {
      InitializeComponent();
      titleLead = $"Slapp - {splatTagController.MatchPlayer(null).Length} Players and {splatTagController.MatchTeam(null).Length} Teams loaded! - ";
      Title = titleLead;

      // Initialise the delay timer if we have a UI context, otherwise don't use the timer.
      context = SynchronizationContext.Current;
      if (context != null)
      {
        smoothSearchDelayTimer = new Timer(TimerExpired);
      }

      // If we've loaded from a snapshot, then hide the setup button(s).
      if (sourcesImporter == null)
      {
        otherFunctionsGrid.Visibility = Visibility.Collapsed;
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
          // Increase the smooth delay for low n characters cause we'll be lagging if they didn't mean it...
          float multiplier;
          switch (searchInput.Text.Length)
          {
            case 1: multiplier = 2; break;
            case 2: multiplier = 1.6f; break;
            case 3: multiplier = 1.3f; break;
            case 4: multiplier = 1.1f; break;
            default: multiplier = 1; break;
          }
          int time = (int)(delaySlider.Value * multiplier);
          smoothSearchDelayTimer.Change(time, Timeout.Infinite);
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
      string query = searchInput.Text;
      if (allowEmptySearchCheckbox.IsChecked == false && query.Length < 1)
      {
        return;
      }

      var options = new MatchOptions
      {
        IgnoreCase = ignoreCaseCheckbox.IsChecked == true,
        NearCharacterRecognition = nearMatchCheckbox.IsChecked == true,
        QueryIsRegex = regexCheckbox.IsChecked == true
      };

      var playersMatched = splatTagController.MatchPlayer(query, options);
      var teamsMatched = splatTagController.MatchTeam(query, options);
      Title = titleLead + $"Matched {playersMatched.Length} players and {teamsMatched.Length} teams!";
      playersListBox.ItemsSource = playersMatched;
      teamsListBox.ItemsSource = teamsMatched;
    }

    private void CheckedChanged(object sender, RoutedEventArgs e)
    {
      SearchWithTimerDelay();
    }

    private void DataBaseButton_Click(object sender, RoutedEventArgs e)
    {
      if (sourcesImporter == null) return;

      DatabaseEditor databaseEditor = new DatabaseEditor(splatTagController, sourcesImporter.Sources);
      bool? result = databaseEditor.ShowDialog();
      if (result == true)
      {
        // Editor accepted, save the sources list.
        sourcesImporter.SaveSources(databaseEditor.DatabaseSources);

        // And load the new database.
        splatTagController.LoadDatabase();
      }
    }

    private void TwitterButton_Click(object sender, RoutedEventArgs e)
    {
      FrameworkElement b = (FrameworkElement)sender;
      if (b?.DataContext == null)
      {
        throw new ArgumentException("Unknown Twitter Button DataContext binding, it is not bound (null).");
      }
      else if (b.DataContext is Team t)
      {
        splatTagController.TryLaunchTwitter(t);
      }
      else if (b.DataContext is Player p)
      {
        splatTagController.TryLaunchTwitter(p);
      }
      else
      {
        throw new ArgumentException("Unknown Twitter Button DataContext binding: " + b.DataContext);
      }
    }

    private void BattlefyButton_Click(object sender, RoutedEventArgs e)
    {
      FrameworkElement b = (FrameworkElement)sender;
      if (b?.DataContext == null)
      {
        throw new ArgumentException("Unknown Twitter Button DataContext binding, it is not bound (null).");
      }
      else if (b.DataContext is string slug)
      {
        splatTagController.TryLaunchAddress("https://battlefy.com/users/" + slug);
      }
      else
      {
        throw new ArgumentException("Unknown Twitter Button DataContext binding: " + b.DataContext);
      }
    }

    private void MenuItemCopyOnClick(object sender, RoutedEventArgs e)
    {
      MenuItem item = ((MenuItem)sender);
      TextBlock textBlock = (TextBlock)item.Header;
      Clipboard.SetData(DataFormats.UnicodeText, textBlock.Text);
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

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new InvalidOperationException();
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

      string separator = parameter?.ToString() ?? ", ";
      return string.Join(separator, sources);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new InvalidOperationException();
  }

  public class PlayerOldTeamsToStringConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null) return "(not set)";

      IEnumerable<Team> oldTeams;
      if (value is Player p)
      {
        oldTeams = p.OldTeams.Select(id => MainWindow.splatTagController?.GetTeamById(id) ?? Team.UnlinkedTeam);
      }
      else if (value is IEnumerable<Team> t)
      {
        oldTeams = t;
      }
      else
      {
        throw new InvalidDataException("Unknown type to convert: " + value.GetType());
      }

      string separator = parameter?.ToString() ?? ", ";
      return string.Join(separator, oldTeams);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new InvalidOperationException();
  }

  public class GetTeamBestPlayerDivConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is Team t)
      {
      }
      else if (value is Guid teamId)
      {
        t = MainWindow.splatTagController.GetTeamById(teamId);
      }
      else
      {
        throw new InvalidDataException("Unknown type to convert: " + value.GetType());
      }

      return t.GetBestTeamPlayerDivString(MainWindow.splatTagController);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new InvalidOperationException();
  }

  public class Top500ToString : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      bool top500;
      if (value is Player p)
      {
        top500 = p.Top500;
      }
      else if (value is bool x)
      {
        top500 = x;
      }
      else
      {
        throw new InvalidDataException("Unknown type to convert: " + value.GetType());
      }
      return top500 ? "👑" : "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new InvalidOperationException();
  }

  public class TeamIdToString : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is Team t)
      {
        return t;
      }
      else if (value is Guid teamId)
      {
        return MainWindow.splatTagController.GetTeamById(teamId);
      }
      else
      {
        throw new InvalidDataException("Unknown type to convert: " + value.GetType());
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new InvalidOperationException();
  }

  public class JoinStringsConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      string separator = ", ";
      if (parameter != null)
      {
        separator = parameter.ToString();
      }

      if (value is string[] values1)
      {
        return string.Join(separator, values1);
      }
      else if (value is IEnumerable<string> values2)
      {
        return string.Join(separator, values2);
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

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new InvalidOperationException();
  }

  public class ValidStringToVisibleConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      bool isValid;

      if (value == null)
      {
        isValid = false;
      }
      else if (value is IEnumerable<string> collection)
      {
        isValid = collection.Any(s => !string.IsNullOrWhiteSpace(s?.ToString()));
      }
      else
      {
        isValid = !string.IsNullOrWhiteSpace(value?.ToString());
      }
      return new BooleanToVisibilityConverter().Convert(isValid, targetType, parameter, culture);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new InvalidOperationException();
  }

  public class PlayerContextMenuConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      const int MAX_ELEMENTS_UNTIL_LINE_BREAKS = 2;
      if (value is Player p)
      {
        var tuples = new List<Tuple<string, string>>();
        foreach (PropertyInfo prop in typeof(Player).GetProperties())
        {
          var fieldName = prop.Name;
          var fieldVal = prop.GetValue(p, null);
          if (fieldVal is null || fieldName == nameof(Player.OldTeams) || fieldName == nameof(Player.Name) || fieldName == nameof(Player.CurrentTeam))
          {
            // Don't bother listing values that are not set (or provided by other values)
            continue;
          }
          else if (fieldVal is bool b)
          {
            fieldVal = b ? "✔" : "❌";
          }
          else if (fieldVal is IEnumerable<Guid> ids)
          {
            int count = ids.Count();
            if (count == 0)
            {
              continue;
            }
            string separator = (count > MAX_ELEMENTS_UNTIL_LINE_BREAKS) ? "\n" : ", ";

            var oldTeams = ids.Select(id => MainWindow.splatTagController?.GetTeamById(id) ?? Team.UnlinkedTeam);
            fieldVal = string.Join(separator, oldTeams);
          }
          else if (fieldVal is IEnumerable<long> longs)
          {
            int count = longs.Count();
            if (count == 0)
            {
              continue;
            }
            string separator = (count > MAX_ELEMENTS_UNTIL_LINE_BREAKS) ? "\n" : ", ";
            fieldVal = string.Join(separator, longs);
          }
          else if (fieldVal is IEnumerable<string> strings)
          {
            int count = strings.Count();
            if (count == 0)
            {
              continue;
            }
            string separator = (count > MAX_ELEMENTS_UNTIL_LINE_BREAKS) ? "\n" : ", ";
            fieldVal = string.Join(separator, strings);
          }
          else if (fieldVal is IEnumerable<object> objects)
          {
            int count = objects.Count();
            if (count == 0)
            {
              continue;
            }
            string separator = (count > MAX_ELEMENTS_UNTIL_LINE_BREAKS) ? "\n" : ", ";
            fieldVal = string.Join(separator, objects);
          }
          tuples.Add(new Tuple<string, string>(fieldName, fieldVal.ToString()));
        }
        return tuples;
      }
      return new Tuple<string, string>[0];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new InvalidOperationException();
  }
}