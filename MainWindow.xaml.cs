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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;
using Esri.ArcGISRuntime.Mapping;

namespace CemMapApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SqlConnection cnn;
        string strConn;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                //PROD
                strConn = "Data Source=ApexGIS;Initial catalog=ApexCEM;User Id=" + Id_TextEntry.Text + ";Password=" + Pw_Box.Password + ";";

                //TEST
                //strConn = "Data Source=ApexGIS;Initial catalog=Sandbox;User Id=" + txtId.Text + ";Password=" + txtPW.Text + ";";
                cnn = new SqlConnection(strConn);
                cnn.Open();
                SearchBtn.IsEnabled = true;
                Logoff_btn.IsEnabled = true;
                Logon_btn.IsEnabled = false;
                Id_TextEntry.IsEnabled = false;
                Pw_Box.IsEnabled = false;
            }
            catch
            {
                MessageBox.Show("Logon failure.  Check Logon Id and Password.");
            }
        }


    }

}

