using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;
using Esri.ArcGISRuntime.Mapping;
using System.IO;
using System.Runtime.CompilerServices;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.UI;
using System.ComponentModel;

namespace CemMapApp
{
    public partial class MainWindow : Window

    {
        SqlConnection cnn;
        string strConn;
        string sql1, sql2;
        string sqllot, cId;


        public MainWindow()
        {
            //InitializeComponent();

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                strConn = "Data Source=ApexGIS;Initial catalog=ApexCEM;User Id=" + Id_TextEntry.Text + ";Password=" + Pw_Box.Password + ";";


                cnn = new SqlConnection(strConn);
                cnn.Open();
                Logon_btn.IsEnabled = true;
                Logoff_btn.IsEnabled = true;
                Id_TextEntry.IsEnabled = false;
                Pw_Box.IsEnabled = false;
                this.Title = "Logged In";

            }
            catch
            {
                MessageBox.Show("Logon failure.  Check Logon Id and Password.");
            }
        }




        private void LogOffClose_Click(object sender, EventArgs e)
        {
            try
            {
                cnn.Close();
                cnn.Dispose();
                Logoff_btn.IsEnabled = false;
                SearchBtn.IsEnabled = true;
                Logon_btn.IsEnabled = true;
                Id_TextEntry.IsEnabled = true;
                Pw_Box.IsEnabled = true;
                Id_TextEntry.Clear();
                Pw_Box.Clear();
                LastNameSearch.Clear();
                PlotSearch.Clear();
                // this.Close();
                this.Title = "Logged Out";
            }
            catch { this.Close(); }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {//populate the grid based on lot

            //clearData();
            SearchBtn.IsEnabled = true;
            sqllot = this.PlotSearch.Text;

            sql1 = @"select OBJECTID as 'Id', Lot, Plot, [name_LAST] as 'Last Name', [name_FIRST] as 'First Name'
                        from CemeteryPlotsVets where Lot = " + sqllot + " order by [name_LAST];";
            // MessageBox.Show(sql1);
            SqlDataAdapter da1 = new SqlDataAdapter(sql1, cnn);
            da1.SelectCommand.CommandTimeout = 0;
            DataSet ds1 = new DataSet();
            da1.Fill(ds1);

            SrcGrdList.DataContext = ds1.Tables[0];
            SrcGrdList.SelectionMode = DataGridSelectionMode.Single;

            da1.Dispose();
            ds1.Dispose();

        }

        public class MapViewModel : INotifyPropertyChanged
        {
            public MapViewModel()
            {
                

            }

            private Map _map = new Map(Basemap.CreateStreets());

            /// <summary>
            /// Gets or sets the map
            /// </summary>
            public Map Map
            {
                get { return _map; }
                set { _map = value; OnPropertyChanged(); }
            }

            /// <summary>
            /// Raises the <see cref="MapViewModel.PropertyChanged" /> event
            /// </summary>
            /// <param name="propertyName">The name of the property that has changed</param>
            protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                var propertyChangedHandler = PropertyChanged;
                if (propertyChangedHandler != null)
                    propertyChangedHandler(this, new PropertyChangedEventArgs(propertyName));
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

    }
   
}
