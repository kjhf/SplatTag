using Android.App;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Widget;
using Java.Lang;
using Java.Net;
using SplatTagCore;
using SplatTagDatabase;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SplatTagAndroid
{
  [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
  public class MainActivity : AppCompatActivity
  {
    private static readonly string splatTagFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "SplatTag");
    private static SplatTagController splatTagController;
    private static SplatTagJsonSnapshotDatabase snapshotDatabase;

    private TextView playersResults;
    private TextView teamsResults;

    protected override void OnCreate(Bundle savedInstanceState)
    {
      base.OnCreate(savedInstanceState);
      Xamarin.Essentials.Platform.Init(this, savedInstanceState);

      // Set our view from the "main" layout resource
      SetContentView(Resource.Layout.activity_main);

      // Get our UI controls from the loaded layout
      EditText enteredQuery = FindViewById<EditText>(Resource.Id.enteredQuery);
      playersResults = FindViewById<TextView>(Resource.Id.playersResults);
      teamsResults = FindViewById<TextView>(Resource.Id.teamsResults);
      Button goButton = FindViewById<Button>(Resource.Id.goButton);

      // Get the asset files
      List<string> sources = new List<string>();
      try
      {
        Directory.CreateDirectory(splatTagFolder);

        // Unpack the assets
        AssetManager assetManager = this.ApplicationContext.Assets;
        foreach (var file in from string file in assetManager.List("")
                             where Path.GetExtension(file) == ".json" && !File.Exists(Path.Combine(splatTagFolder, file))
                             select file)
        {
          // Read the contents of our asset
          string content;
          using (StreamReader sr = new StreamReader(assetManager.Open(file)))
          {
            content = sr.ReadToEnd();
          }
          string fileSource = Path.Combine(splatTagFolder, file);
          sources.Add(fileSource);
          File.WriteAllText(fileSource, content);
        }
      }
      catch (Exception ex)
      {
        teamsResults.Text = ex.ToString();
        Toast.MakeText(this, "Exception: " + ex.ToString(), ToastLength.Long).Show();
      }

      try
      {
        // Construct the database
        snapshotDatabase = new SplatTagJsonSnapshotDatabase(splatTagFolder);
        splatTagController = new SplatTagController(snapshotDatabase);

        // Load the database
        splatTagController.Initialise();

        // Hook up events
        enteredQuery.AfterTextChanged += (sender, e) => Search(enteredQuery.Text);
        goButton.Click += (sender, e) => Search(enteredQuery.Text);
      }
      catch (Exception ex)
      {
        playersResults.Text = ex.ToString();
        Toast.MakeText(this, "Exception: " + ex.ToString(), ToastLength.Long).Show();
      }
    }

    private void Search(string query, bool toast = false)
    {
      int playersFound = 0;
      int teamsFound = 0;
      if (query.Length > 0)
      {
        {
          IEnumerable<Player> players = splatTagController.MatchPlayer(query,
             new MatchOptions
             {
               IgnoreCase = /* ignoreCaseCheckbox.IsChecked == true, */ true,
               NearCharacterRecognition = /* nearMatchCheckbox.IsChecked == true, */ true,
               QueryIsRegex = /* regexCheckbox.IsChecked == true */ false
             }
           );

          playersFound = players.Count();
          if (playersFound != 0)
          {
            StringBuilder playerStrings = new StringBuilder();
            foreach (var p in players)
            {
              // Use URLEncoder to unmangle any special characters
              string currentTeam = splatTagController.GetTeamById(p.CurrentTeam).ToString();
              string oldTeams = string.Join(", ", p.OldTeams.Select(t => splatTagController.GetTeamById(t)));
              playerStrings
                .Append(p.Name.Value)
                .Append(" (Plays for ")
                .Append(currentTeam)
                .Append(") Old Teams: ")
                .Append(oldTeams)
                .Append("\n\n");
            }

            playersResults.Text = playerStrings.ToString();
          }
        }

        {
          IEnumerable<Team> teams = splatTagController.MatchTeam(query,
            new MatchOptions
            {
              IgnoreCase = /* ignoreCaseCheckbox.IsChecked == true, */
            true,
              NearCharacterRecognition = /* nearMatchCheckbox.IsChecked == true, */ true,
              QueryIsRegex = /* regexCheckbox.IsChecked == true */ false
            }
          );

          teamsFound = teams.Count();
          if (teamsFound != 0)
          {
            StringBuilder teamStrings = new StringBuilder();
            foreach (var t in teams)
            {
              string div = t.Div.Name;
              string bestPlayer = t.GetBestTeamPlayerDivString(splatTagController);
              string[] teamPlayers = t.GetTeamPlayersStrings(splatTagController);
              teamStrings
                .Append(t.Tag?.Value ?? "")
                .Append(" ")
                .Append(t.Name.Value)
                .Append(" (")
                .Append(div)
                .Append("). ")
                .Append(bestPlayer)
                .Append("\nPlayers:\n")
                .Append(string.Join(", ", teamPlayers))
                ;
            }

            teamsResults.Text = teamStrings.ToString();
          }
        }
      }

      if (teamsFound == 0)
      {
        teamsResults.LayoutParameters = new LinearLayout.LayoutParams(0, Android.Views.ViewGroup.LayoutParams.MatchParent, 0.0f);
      }
      else
      {
        if (playersFound == 0)
        {
          teamsResults.LayoutParameters = new LinearLayout.LayoutParams(0, Android.Views.ViewGroup.LayoutParams.MatchParent, 2.0f);
        }
        else
        {
          teamsResults.LayoutParameters = new LinearLayout.LayoutParams(0, Android.Views.ViewGroup.LayoutParams.MatchParent, 1.0f);
        }
      }

      if (playersFound == 0)
      {
        if (teamsFound == 0)
        {
          playersResults.Text = query.Length > 0 ? "No results!" : "Nothing to search!";
          playersResults.LayoutParameters = new LinearLayout.LayoutParams(0, Android.Views.ViewGroup.LayoutParams.MatchParent, 2.0f);
        }
        else
        {
          playersResults.LayoutParameters = new LinearLayout.LayoutParams(0, Android.Views.ViewGroup.LayoutParams.MatchParent, 0.0f);
        }
      }
      else
      {
        if (teamsFound == 0)
        {
          playersResults.LayoutParameters = new LinearLayout.LayoutParams(0, Android.Views.ViewGroup.LayoutParams.MatchParent, 2.0f);
        }
        else
        {
          playersResults.LayoutParameters = new LinearLayout.LayoutParams(0, Android.Views.ViewGroup.LayoutParams.MatchParent, 1.0f);
        }
      }

      if (toast)
      {
        Toast.MakeText(this, $"{playersFound} players found\n{teamsFound} teams found", ToastLength.Long).Show();
      }
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
    {
      Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

      base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }
  }
}