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
    private static GenericFilesImporter importer;
    private static MultiDatabase database;

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
      // TODO - for now just set this to the asset files.
      string[] sources = new[]
      {
        "LUTI-Season-7.json",
        "LUTI-Season-8.json",
        "LUTI-Season-9.json",
        "LUTI-Season-X.json"
      };

      try
      {
        Directory.CreateDirectory(splatTagFolder);

        // Unpack the assets
        foreach (string file in sources)
        {
          if (!File.Exists(Path.Combine(splatTagFolder, file)))
          {
            // Read the contents of our asset
            string content;
            AssetManager assets = this.Assets;
            using (StreamReader sr = new StreamReader(assets.Open(file)))
            {
              content = sr.ReadToEnd();
            }
            File.WriteAllText(Path.Combine(splatTagFolder, file), content);
          }
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
        importer = new GenericFilesImporter(sources.Select(file => Path.Combine(splatTagFolder, file)).ToArray());
        database = new MultiDatabase(splatTagFolder, importer);

        // Construct the controller.
        splatTagController = new SplatTagController(database);

        // Load the database
        splatTagController.LoadDatabase();
      }
      catch (Exception ex)
      {
        playersResults.Text = ex.ToString();
        Toast.MakeText(this, "Exception: " + ex.ToString(), ToastLength.Long).Show();
      }

      // Hook up events
      enteredQuery.AfterTextChanged += (sender, e) => Search(enteredQuery.Text);

      goButton.Click += (sender, e) => Search(enteredQuery.Text);
    }

    private void Search(string query, bool toast = false)
    {
      int playersFound = 0;
      int teamsFound = 0;
      if (query.Length > 0)
      {
        {
          IEnumerable<string> playerStrings = splatTagController.MatchPlayer(query,
             new MatchOptions
             {
               IgnoreCase = /* ignoreCaseCheckbox.IsChecked == true, */ true,
               NearCharacterRecognition = /* nearMatchCheckbox.IsChecked == true, */ true,
               QueryIsRegex = /* regexCheckbox.IsChecked == true */ false
             }
           ).Select(p =>
             // Use URLEncoder to unmangle any special characters
             URLDecoder.Decode(URLEncoder.Encode($"{p.Name} (Plays for {splatTagController.GetTeamById(p.CurrentTeam).Name}) {GetOldTeamsAsString(p)}", "UTF-8"), "UTF-8"));

          playersFound = playerStrings.Count();
          if (playersFound != 0)
          {
            playersResults.Text = string.Join("\n\n", playerStrings);
          }
        }

        {
          IEnumerable<string> teamStrings = splatTagController.MatchTeam(query,
            new MatchOptions
            {
              IgnoreCase = /* ignoreCaseCheckbox.IsChecked == true, */ true,
              NearCharacterRecognition = /* nearMatchCheckbox.IsChecked == true, */ true,
              QueryIsRegex = /* regexCheckbox.IsChecked == true */ false
            }
          ).Select(t =>
            // Use URLEncoder to unmangle any special characters
            URLDecoder.Decode(URLEncoder.Encode(t.ToString(), "UTF-8"), "UTF-8"));

          teamsFound = teamStrings.Count();
          if (teamsFound != 0)
          {
            teamsResults.Text = string.Join("\n\n", teamStrings);
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

    public string GetOldTeamsAsString(Player p)
    {
      StringBuilder sb = new StringBuilder();
      IEnumerable<long> teamIds = p.Teams.Skip(1);
      if (teamIds.Any())
      {
        sb.Append("(Old teams: ");
        sb.Append(string.Join(", ", teamIds.Select(id =>
        {
          Team t = splatTagController.GetTeamById(id);
          return t.Tag + " " + t.Name;
        })));
        sb.Append(")");
      }

      return sb.ToString();
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
    {
      Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

      base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }
  }
}