#nullable enable

using Microsoft.Win32;
using System.Windows;

namespace SplatTagUI
{
  /// <summary>
  /// Interaction logic for InputWindow.xaml
  /// </summary>
  public partial class InputWindow : Window
  {
    public static readonly DependencyProperty InputProperty = DependencyProperty.Register("Input", typeof(string), typeof(InputWindow), new PropertyMetadata(""));

    public static readonly DependencyProperty HintTextProperty = DependencyProperty.Register("HintText", typeof(string), typeof(InputWindow), new PropertyMetadata(""));

    /// <summary>
    /// The raw text that was input. Safer to user <see cref="InputPaths"/>.
    /// </summary>
    public string Input
    {
      get { return (string)this.GetValue(InputProperty); }
      set { this.SetValue(InputProperty, value); }
    }

    /// <summary>
    /// The hint text that goes over the top of the input text
    /// </summary>
    public string HintText
    {
      get { return (string)this.GetValue(HintTextProperty); }
      set { this.SetValue(HintTextProperty, value); }
    }

    /// <summary>
    /// The text that was input split into an array of paths.
    /// </summary>
    /// <remarks>
    /// The > symbol has been chosen as it is an invalid path character in URIs and Windows.</remarks>
    public string[] InputPaths => Input.Split('>');

    public InputWindow()
    {
      InitializeComponent();

      // Focus the text box for easy copy-paste
      input.Focus();
    }

    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
      Close();
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog ofd = new OpenFileDialog
      {
        CheckFileExists = true,
        CheckPathExists = true,
        Title = "Please select one or more files to import!",
        Multiselect = true,
        ValidateNames = true
      };

      if (ofd.ShowDialog() == true)
      {
        DialogResult = true;
        Input = string.Join(">", ofd.FileNames);
        Close();
      }
    }
  }
}