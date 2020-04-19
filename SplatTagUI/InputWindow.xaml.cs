using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SplatTagUI
{
  /// <summary>
  /// Interaction logic for InputWindow.xaml
  /// </summary>
  public partial class InputWindow : Window
  {
    public static readonly DependencyProperty InputProperty = DependencyProperty.Register("Input", typeof(string), typeof(InputWindow), new PropertyMetadata(""));

    public static readonly DependencyProperty HintTextProperty = DependencyProperty.Register("HintText", typeof(string), typeof(InputWindow), new PropertyMetadata(""));

    public string Input
    {
      get { return (string)this.GetValue(InputProperty); }
      set { this.SetValue(InputProperty, value); }
    }
    public string HintText
    {
      get { return (string)this.GetValue(HintTextProperty); }
      set { this.SetValue(HintTextProperty, value); }
    }

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

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = false;
      Close();
    }
  }
}
