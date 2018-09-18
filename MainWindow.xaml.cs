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
        string sql1, sql2;
        string strConn, xId; 
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
                Logoff_btn.IsEnabled = true;
                Id_TextEntry.IsEnabled = false;
                Pw_Box.IsEnabled = false;
                this.Title = "Logged In";
                Logon_btn.IsEnabled = false;
            }
            catch
            {
                cnn.Dispose();
                Logon_btn.IsEnabled = true;
                Id_TextEntry.IsEnabled = true;
                Pw_Box.IsEnabled = true;
                Logoff_btn.IsEnabled = false;
                MessageBox.Show("Logon failure.  Check Logon Id and Password.");
            }
        }
        public void LastNameSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearData();
            PlotSearch.TextChanged -= PlotSearch_TextChanged;
            PlotSearch.Clear();
            PlotSearch.TextChanged += PlotSearch_TextChanged;

            grdList.ItemsSource = null;

            if (LastNameSearch.Text != "")
            {
                sql1 = @"select OBJECTID as 'Id',  Lot, Plot, [name_LAST] as 'LastName', [name_FIRST] as 'FirstName'
                         from CemeteryPlotsVets where 
                        name_LAST like '" + LastNameSearch.Text.Replace("'", "''") + "%' order by name_LAST;";

                PopGrid();

                //btnSearch.Enabled = true;
            }
        }
       
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //populate the grid based on lot

            sql1 = @"select OBJECTID as 'Id', Lot, Plot, [name_LAST] as 'LastName', [name_FIRST] as 'FirstName'
                            from CemeteryPlotsVets where Lot = " + PlotSearch.Text.ToString() + " order by [name_LAST];";

            PopGrid();
        }

        private void PopGrid()
        {

            SqlCommand cmd = new SqlCommand(sql1, cnn);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            sda.Fill(ds);

            if (ds.Tables[0].Rows.Count > 0)
            {
                grdList.ItemsSource = ds.Tables[0].DefaultView;
            }

            cmd.Dispose();
            sda.Dispose();
            ds.Dispose();
        }
        private void PlotSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearData();

            LastNameSearch.TextChanged -= LastNameSearch_TextChanged;
            LastNameSearch.Clear();
            LastNameSearch.TextChanged += LastNameSearch_TextChanged;

            SearchBtn.IsEnabled = true;
            grdList.ItemsSource = null;
        }

        private void LogOffClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                cnn.Close();
                cnn.Dispose();
                Logoff_btn.IsEnabled = false;                
                SaveBtn.IsEnabled = false;
                Logon_btn.IsEnabled = true;
                Id_TextEntry.IsEnabled = true;
                Pw_Box.IsEnabled = true;
                Id_TextEntry.Clear();
                Pw_Box.Clear();
                LastNameSearch.Clear();
                PlotSearch.Clear();
                Logon_btn.IsEnabled = true;
                SearchBtn.IsEnabled = false;
                this.Title = "Logged Out";
            }
            catch { this.Close(); }            
        }
        private void ClearData()
        {
            txtLot.Clear();
            txtPlot.Clear();
            txtLName.Clear();
            txtFName.Clear();
            txtLName2.Clear();
            txtFName2.Clear();
            cmbStatus.SelectedIndex = -1;
            txtPurchase.Clear();
            txtCost.Clear();
            txtTransfer.Clear();
            txtOcc.Clear();
            cbVet.IsChecked = false;
            cbRec.IsChecked = false;
            txtNotes.Clear();

        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            //confirmation window here
            System.Windows.MessageBoxResult dialogResult = MessageBox.Show("Do you want to save your changes?", "Save Changes", MessageBoxButton.YesNo);

            if (dialogResult == System.Windows.MessageBoxResult.Yes)
            {
                string cbCheck, cbVets;                

                if (cbRec.IsChecked == true)
                {
                    cbCheck = "Yes";
                }
                else
                {
                    cbCheck = "No";
                }
                if (cbVet.IsChecked == true)
                {
                    cbVets = "Yes";
                }
                else
                {
                    cbVets = "No";
                }
                try
                {
                    using (SqlCommand cmd = cnn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE CemeteryPlotsVets
                                  set name_LAST = @Lname, name_FIRST = @Fname, name_LAST2 = @Lname2,
                                  name_FIRST2 = @Fname2, Lot = @Lot, Plot = @Plot, Plot_Status = @Status,
                                  Deed_date = @Date, Cost = @Cost, transfer = @Transfer, Occupant = @Occupant,
                                  GOBR = @cbCheck, veterans = @cbVets, Notes = @Notes
                                  where OBJECTID = '" + xId + "';";

                        //replaces a blank with a NULL
                        //prikaz.Parameters.AddWithValue("@odjezd", cb_odjezd.Text == string.Empty ? DBNull.value : cb_odjezd.Text);
                        cmd.Parameters.AddWithValue("@Lname", txtLName.Text == string.Empty ? DBNull.Value : (object)txtLName.Text.ToString());
                        cmd.Parameters.AddWithValue("@Fname", txtFName.Text == string.Empty ? DBNull.Value : (object)txtFName.Text.ToString());
                        cmd.Parameters.AddWithValue("@Lname2", txtLName2.Text == string.Empty ? DBNull.Value : (object)txtLName2.Text.ToString());
                        cmd.Parameters.AddWithValue("@Fname2", txtFName2.Text == string.Empty ? DBNull.Value : (object)txtFName2.Text.ToString());
                        cmd.Parameters.AddWithValue("@Lot", txtLot.Text == string.Empty ? DBNull.Value : (object)txtLot.Text.ToString());
                        cmd.Parameters.AddWithValue("@Plot", txtPlot.Text == string.Empty ? DBNull.Value : (object)txtPlot.Text.ToString());
                        cmd.Parameters.AddWithValue("@Status", cmbStatus.Text == string.Empty ? DBNull.Value : (object)cmbStatus.Text.ToString());
                        cmd.Parameters.AddWithValue("@Date", txtPurchase.Text == string.Empty ? DBNull.Value : (object)txtPurchase.Text.ToString());
                        cmd.Parameters.AddWithValue("@Cost", txtCost.Text == string.Empty ? DBNull.Value : (object)txtCost.Text.ToString());
                        cmd.Parameters.AddWithValue("@Transfer", txtTransfer.Text == string.Empty ? DBNull.Value : (object)txtTransfer.Text.ToString());
                        cmd.Parameters.AddWithValue("@Occupant", txtOcc.Text == string.Empty ? DBNull.Value : (object)txtOcc.Text.ToString());
                        cmd.Parameters.AddWithValue("@cbCheck", cbCheck);
                        cmd.Parameters.AddWithValue("@cbVets", cbVets);
                        cmd.Parameters.AddWithValue("@Notes", txtNotes.Text == string.Empty ? DBNull.Value : (object)txtNotes.Text.ToString());

                        cmd.ExecuteNonQuery();
                    }
                }
                catch { MessageBox.Show("Unable to update record."); }
            }
            else if (dialogResult == System.Windows.MessageBoxResult.No)
            {
                //do something else
                return;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                cnn.Close();
                cnn.Dispose();
                this.Close();
            }
            catch { this.Close(); }
        }

        private void grdList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                int i = grdList.Items.IndexOf(grdList.CurrentItem);
                xId = (grdList.Items[i] as DataRowView).Row.ItemArray[0].ToString();
                PopDetails();
                SaveBtn.IsEnabled = true;
            }
            catch
            {
                MessageBox.Show("Unable to show details.");
            }

        }

        private void PopDetails()
        {
            cbRec.IsChecked = false;
            //populate dataset from single record in grid

            sql2 = @"select name_LAST, name_FIRST, name_LAST2, name_FIRST2, Lot, Plot,
                    Plot_Status, Deed_date, Cost, [Transfer], Occupant, GOBR, veterans, Notes
                    from CemeteryPlotsVets where OBJECTID = '" + xId + "';";
            
            try
            {
                cbRec.IsChecked = false;
                SqlDataAdapter da2 = new SqlDataAdapter(sql2, cnn);
                da2.SelectCommand.CommandTimeout = 0;
                DataSet ds2 = new DataSet();
                da2.Fill(ds2);

                //populate text boxes from the data set
                txtLName.Text = ds2.Tables[0].Rows[0]["name_LAST"].ToString();
                txtFName.Text = ds2.Tables[0].Rows[0]["name_FIRST"].ToString();
                txtLName2.Text = ds2.Tables[0].Rows[0]["name_LAST2"].ToString();
                txtFName2.Text = ds2.Tables[0].Rows[0]["name_FIRST2"].ToString();
                txtLot.Text = ds2.Tables[0].Rows[0]["Lot"].ToString();
                txtPlot.Text = ds2.Tables[0].Rows[0]["Plot"].ToString();
                cmbStatus.Text = ds2.Tables[0].Rows[0]["Plot_Status"].ToString();
                txtPurchase.Text = ds2.Tables[0].Rows[0]["Deed_date"].ToString();
                txtCost.Text = ds2.Tables[0].Rows[0]["Cost"].ToString();
                txtTransfer.Text = ds2.Tables[0].Rows[0]["Transfer"].ToString();
                txtOcc.Text = ds2.Tables[0].Rows[0]["Occupant"].ToString();
                if (ds2.Tables[0].Rows[0]["GOBR"].ToString() == "yes")
                {
                    cbRec.IsChecked = true;
                }
                if (ds2.Tables[0].Rows[0]["veterans"].ToString() == "yes")
                {
                    cbVet.IsChecked = true;
                }
                txtNotes.Text = ds2.Tables[0].Rows[0]["Notes"].ToString();

                da2.Dispose();
                ds2.Dispose();
            }

            catch
            {                
                MessageBox.Show("Double click on an ID in the grid.");
            }

        }

        //    public class MapViewModel : INotifyPropertyChanged
        //    {
        //        public MapViewModel()
        //        {


        //        }

        //        private Map _map = new Map(Basemap.CreateStreets());

        //        /// <summary>
        //        /// Gets or sets the map
        //        /// </summary>
        //        public Map Map
        //        {
        //            get { return _map; }
        //            set { _map = value; OnPropertyChanged(); }
        //        }

        //        /// <summary>
        //        /// Raises the <see cref="MapViewModel.PropertyChanged" /> event
        //        /// </summary>
        //        /// <param name="propertyName">The name of the property that has changed</param>
        //        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //        {
        //            var propertyChangedHandler = PropertyChanged;
        //            if (propertyChangedHandler != null)
        //                propertyChangedHandler(this, new PropertyChangedEventArgs(propertyName));
        //        }

        //        public event PropertyChangedEventHandler PropertyChanged;
    }

    }