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

namespace CARD
{
    /// <summary>
    /// GameWay.xaml 的交互逻辑
    /// </summary>
    public partial class GameWay : Window
    {
        int chooseId;
        public GameWay()
        {
            InitializeComponent();
        }
        public int getChooseId()
        {
            return chooseId;
        }
        private void RadioButton1_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radio = sender as RadioButton;
            if (radio.IsChecked == true)
            {
                chooseId = 1;
            }
        }
        private void RadioButton2_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radio = sender as RadioButton;
            if (radio.IsChecked == true)
            {
                chooseId = 2;
            }
        }
        private void ButtonOKClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
