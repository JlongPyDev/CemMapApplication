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
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.LocalServices;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System.Drawing;
using Esri.ArcGISRuntime.Tasks;

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
            InitializeComponent();
            Initialize();

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
                                  set name_LAST = upper(@Lname), name_FIRST = upper(@Fname), name_LAST2 = upper(@Lname2),
                                  name_FIRST2 = upper(@Fname2), Lot = @Lot, Plot = @Plot, Plot_Status = @Status,
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




            

        private void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Get the user-tapped location
            MapPoint mapLocation = e.Location;

            // Project the user-tapped map point location to a geometry
            Esri.ArcGISRuntime.Geometry.Geometry myGeometry = GeometryEngine.Project(mapLocation, SpatialReferences.Wgs84);

            // Convert to geometry to a traditional Lat/Long map point
            MapPoint projectedLocation = (MapPoint)myGeometry;

            // Format the display callout string based upon the projected map point (example: "Lat: 100.123, Long: 100.234")
            string mapLocationDescription = string.Format("Lat: {0:F3} Long:{1:F3}", projectedLocation.Y, projectedLocation.X);

            // Create a new callout definition using the formatted string
            CalloutDefinition myCalloutDefinition = new CalloutDefinition("Location:", mapLocationDescription);

            // Display the callout
            MyMapView.ShowCalloutAt(mapLocation, myCalloutDefinition);
        }
        



        private void Initialize()
        {
            //Map myMap = new Map(Basemap.CreateTerrainWithLabels());
            Map myMap = new Map(BasemapType.ImageryWithLabelsVector, 35.728990, -78.856027, 18);
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        //LegendInfo MapLegend = new LegendInfo {Name, Symbol };

        //var uri = new Uri("C:\\Users\\Jlong\\source\\repos\\CemMapApp\\CemMapApp\\Legend.JPG");
        //legend.Source = new BitmapImage(uri);


        // TEST GDB SERVICE VS MAP SERVICE
        // http://apexgis:6080/arcgis/rest/services/CemeteryHost/CEMTESTSERV/GeoDataServer/versions/dbo.DEFAULT
        // WFS serv http://apexgis:6080/arcgis/services/C+emeteryHost/CEMTESTSERV/GeoDataServer/WFSServer


        // LEGEND ILayerContent

        //var CemLot = new Uri("http://apexgis.ci.apex.nc.us:6080/arcgis/services/CemeteryHost/CemAppData/GeoDataServer");
        var CemLot = new Uri("http://apexgis:6080/arcgis/rest/services/CemeteryHost/CEMTESTSERV/FeatureServer/0");
            FeatureLayer LotLayer = new FeatureLayer(CemLot);


            SimpleLineSymbol LotLineSymb = new SimpleLineSymbol(
                SimpleLineSymbolStyle.Solid, System.Drawing.Color.Black, 1);

            SimpleFillSymbol lotSymbol = new SimpleFillSymbol()
            {
                Color = System.Drawing.Color.Transparent,
                Outline = LotLineSymb,

            };

            SimpleRenderer LotRenderer = new SimpleRenderer(lotSymbol);

                // Lot label info
            StringBuilder LotStringLabelBuild = new StringBuilder();

            LotStringLabelBuild.AppendLine("{");

            LotStringLabelBuild.AppendLine("\"labelExpressionInfo\": {");
            LotStringLabelBuild.AppendLine("\"expression\": \"return $feature.LOT_ID;\"},");
            LotStringLabelBuild.AppendLine("\"maxScale\": 50,");
            LotStringLabelBuild.AppendLine("\"minScale\": 1000,");
            //     Align labels above the center of each point
            LotStringLabelBuild.AppendLine("\"labelPlacement\": \"esriServerPolygonPlacementAlwaysHorizontal\",");
            //     Use a white bold text symbol
            LotStringLabelBuild.AppendLine("\"symbol\": {");
            // Color ARGB  [R  ALPHA  B  G]
            LotStringLabelBuild.AppendLine("\"color\": [13,38,68,255],");
            LotStringLabelBuild.AppendLine("\"font\": {\"size\": 9, \"weight\": \"bold\"},");
            LotStringLabelBuild.AppendLine("\"type\": \"esriTS\"}");
            
            LotStringLabelBuild.AppendLine("}");

            var LotLabelsJson = LotStringLabelBuild.ToString();

            LabelDefinition LotLabelsDef = LabelDefinition.FromJson(LotLabelsJson);
         
            LotLayer.LabelDefinitions.Clear();

            LotLayer.LabelDefinitions.Add(LotLabelsDef);
            LotLayer.LabelsEnabled = true;

                // CemPlot Properties
            var CemPlot = new Uri("http://apexgis:6080/arcgis/rest/services/CemeteryHost/CEMTESTSERV/FeatureServer/1");
            
            FeatureLayer PlotLayer = new FeatureLayer(CemPlot);

            var CemPlot2 = new Uri("http://apexgis:6080/arcgis/rest/services/CemeteryHost/CEMTESTSERV/FeatureServer/1");

            FeatureLayer PlotLayer2 = new FeatureLayer(CemPlot);


                // Plot Label Properties

            StringBuilder PlotStringLabelBuild = new StringBuilder();

            PlotStringLabelBuild.AppendLine("{");

            PlotStringLabelBuild.AppendLine("\"labelExpressionInfo\": {");
            PlotStringLabelBuild.AppendLine("\"expression\": \"return $feature.plot;\"},");
            PlotStringLabelBuild.AppendLine("\"maxScale\": 10,");
            PlotStringLabelBuild.AppendLine("\"minScale\": 250,");
                //     Align labels above the center of each point
            PlotStringLabelBuild.AppendLine("\"labelPlacement\": \"esriServerPolygonPlacementAlwaysHorizontal\",");
                //     Use a white bold text symbol
            PlotStringLabelBuild.AppendLine("\"symbol\": {");
                // Color ARGB  [R  ALPHA  B  G]
            PlotStringLabelBuild.AppendLine("\"color\": [13,38,68,255],");
            PlotStringLabelBuild.AppendLine("\"font\": {\"size\": 8, \"weight\": \"italic\"},");
            PlotStringLabelBuild.AppendLine("\"type\": \"esriTS\"}");

            PlotStringLabelBuild.AppendLine("}");

            var PlotLabelsJson = PlotStringLabelBuild.ToString();

            LabelDefinition PlotLabelsDef = LabelDefinition.FromJson(PlotLabelsJson);

            PlotLayer.LabelDefinitions.Clear();

            PlotLayer.LabelDefinitions.Add(PlotLabelsDef);
            PlotLayer.LabelsEnabled = true;

                // Plot Symbology 
            
            var plotRenderer2 = new UniqueValueRenderer();

            plotRenderer2.FieldNames.Add("name_LAST");

            var plotOutline = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.White, 1);
                     
            var town_plot = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.Color.HotPink, plotOutline);
            plotRenderer2.UniqueValues.Add(new UniqueValue("Town Own", "Town Own", town_plot, "APEX"));
            plotRenderer2.UniqueValues.Add(new UniqueValue("Town Own", "Town Own", town_plot, "TOWN OF APEX"));


            var defaultFillSymbol2 = new SimpleFillSymbol(SimpleFillSymbolStyle.Null, System.Drawing.Color.Transparent, plotOutline);
            plotRenderer2.DefaultSymbol = defaultFillSymbol2;

            //plotRenderer.SceneProperties
            var plotRenderer = new UniqueValueRenderer();

            plotRenderer.FieldNames.Add("Plot_status");
            

            var validPlot = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.Color.GreenYellow, plotOutline);
            var unvalidPlot = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.Color.Yellow, plotOutline);
            var notsaleable = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.Color.Red, plotOutline);
                                
            plotRenderer.UniqueValues.Add(new UniqueValue("Valid Plot","Valid Plot", validPlot, "Deeded"));
            plotRenderer.UniqueValues.Add(new UniqueValue("Unvalid", "Unvalid Plot", unvalidPlot, "Unknown"));
            plotRenderer.UniqueValues.Add(new UniqueValue("No Sale", "No Sell", notsaleable, "Not Saleable"));
            
            var defaultFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Null, System.Drawing.Color.Transparent, plotOutline);
          
            plotRenderer.DefaultSymbol = defaultFillSymbol;
            plotRenderer.DefaultLabel = "Other";

            // CemBoundary Graphics

            var CemBound = new Uri("http://apexgis:6080/arcgis/rest/services/CemeteryHost/CEMTESTSERV/FeatureServer/2");
            FeatureLayer BoundLayer = new FeatureLayer(CemBound);

            SimpleLineSymbol BoundLineSymb = new SimpleLineSymbol(
                SimpleLineSymbolStyle.Solid, System.Drawing.Color.IndianRed, 3);

            SimpleFillSymbol symbol = new SimpleFillSymbol()
            {                
                Outline = BoundLineSymb,              
            };



            SimpleRenderer renderer = new SimpleRenderer(symbol);
           
            // Main GIS Operation Calls 
                   
            BoundLayer.Renderer = renderer;          
            PlotLayer.Renderer = plotRenderer;
            LotLayer.Renderer = LotRenderer;
            PlotLayer2.Renderer = plotRenderer2;

                    
            myMap.OperationalLayers.Add(BoundLayer);           
            myMap.OperationalLayers.Add(PlotLayer);
            myMap.OperationalLayers.Add(PlotLayer2);
            myMap.OperationalLayers.Add(LotLayer);

            // Assign the map to the MapView
            myMap.MinScale = 2250;
            myMap.MaxScale =30;


            //MyMapView.GeoViewTapped += async (s, e) =>
            //{
            //  System.Windows.Point tapScreenPoint = e.Position;

            //};
            
            MyMapView.Map = myMap;

        }

        // ... code here to show popup ...
    }
}

    
    

