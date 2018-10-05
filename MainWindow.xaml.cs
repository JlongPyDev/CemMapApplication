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
using Esri.ArcGISRuntime.Ogc;
using System.ComponentModel;
using Esri.ArcGISRuntime.Security;


namespace CemMapApp
{
    

    public partial class MainWindow : Window

    {
        SqlConnection cnn;
        string sql1, sql2;
        string strConn, xId;
        private FeatureLayer LotLayer;
        private FeatureLayer PlotLayer;
        

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

          
        public async void Initialize()
        {

            string licenseKey = "runtimelite,1000,rud3766972362,none,MJJC7XLS1HSB003AD015";
            Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.SetLicense(licenseKey);

            //Map myMap = new Map(Basemap.CreateTerrainWithLabels());
            Map myMap = new Map(BasemapType.LightGrayCanvasVector, 35.728990, -78.856027, 18);

            // TEST GDB SERVICE VS MAP SERVICE
            // http://apexgis:6080/arcgis/rest/services/CemeteryHost/CEMTESTSERV/GeoDataServer/versions/dbo.DEFAULT
            // WFS serv http://apexgis:6080/arcgis/services/C+emeteryHost/CEMTESTSERV/GeoDataServer/WFSServer


            // LEGEND ILayerContent

            //var CemLot = new Uri("http://apexgis:6080/arcgis/rest/services/CemeteryHost/CEMTESTSERV/FeatureServer/0");
            var CemLot = new Uri("http://apexgis:6080/arcgis/rest/services/CemeteryHost/CEMPRODSERV/FeatureServer/0");
            FeatureLayer LotLayer = new FeatureLayer(CemLot);
            await LotLayer.LoadAsync();

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

            //var CemPlot = new Uri("http://apexgis:6080/arcgis/rest/services/CemeteryHost/CEMTESTSERV/FeatureServer/1");
            var CemPlot = new Uri("http://apexgis:6080/arcgis/rest/services/CemeteryHost/CEMPRODSERV/FeatureServer/1");
            
            FeatureLayer PlotLayer = new FeatureLayer(CemPlot);

            await PlotLayer.LoadAsync();

            var CemPlot2 = new Uri("http://apexgis:6080/arcgis/rest/services/CemeteryHost/CEMPRODSERV/FeatureServer/1");

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
                     
            var town_plot = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.ColorTranslator.FromHtml("#DB87F2"), plotOutline);
            plotRenderer2.UniqueValues.Add(new UniqueValue("Town Own", "Town Own", town_plot, "APEX"));
            plotRenderer2.UniqueValues.Add(new UniqueValue("Town Own", "Town Own", town_plot, "TOWN OF APEX"));


            var defaultFillSymbol2 = new SimpleFillSymbol(SimpleFillSymbolStyle.Null, System.Drawing.Color.Transparent, plotOutline);
            plotRenderer2.DefaultSymbol = defaultFillSymbol2;

            //plotRenderer.SceneProperties
            var plotRenderer = new UniqueValueRenderer();

            plotRenderer.FieldNames.Add("Plot_status");
            
            
            var validPlot = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.ColorTranslator.FromHtml("#8DC055"), plotOutline);
            var unvalidPlot = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.ColorTranslator.FromHtml("#F9F07F"), plotOutline);
            var notsaleable = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.ColorTranslator.FromHtml("#F5503B"), plotOutline);
                                
            plotRenderer.UniqueValues.Add(new UniqueValue("Valid Plot","Valid Plot", validPlot, "Deeded"));
            plotRenderer.UniqueValues.Add(new UniqueValue("Unvalid", "Unvalid Plot", unvalidPlot, "Unknown"));
            plotRenderer.UniqueValues.Add(new UniqueValue("No Sale", "No Sell", notsaleable, "Not Saleable"));
            
            var defaultFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Null, System.Drawing.Color.Transparent, plotOutline);
          
            plotRenderer.DefaultSymbol = defaultFillSymbol;
            plotRenderer.DefaultLabel = "Other";

            // CemBoundary Graphics

            //var CemBound = new Uri("http://apexgis:6080/arcgis/rest/services/CemeteryHost/CEMTESTSERV/FeatureServer/2");

            var CemBound = new Uri("http://apexgis:6080/arcgis/rest/services/CemeteryHost/CEMPRODSERV/FeatureServer/2");
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

            

            MyMapView.Map = myMap;
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
          
        }


        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {


            //var CemLot = new Uri("http://apexgis:6080/arcgis/rest/services/CemeteryHost/CEMTESTSERV/FeatureServer/0");
            // FeatureLayer LotLayer = new FeatureLayer(CemLot);

            //await LotLayer.LoadAsync();

            // NEED TO ACCESS LAYERS AND ADD TO OPERATION MAPPP!!!!!!!!!


            // Perform the identify operation
            MapPoint tapScreenPoint = e.Location;

            var layer = MyMapView.Map.OperationalLayers[1];
            var pixelTolerance = 10;
            var returnPopupsOnly = false;
            var maxResults = 200;
            MyMapView.DismissCallout();
            //IdentifyLayerResult myIdentifyResult = await MyMapView.IdentifyLayerAsync(layer, e.Position, pixelTolerance, returnPopupsOnly, maxResults);

            IdentifyLayerResult myIdentifyResult = await MyMapView.IdentifyLayerAsync(layer, e.Position, pixelTolerance, returnPopupsOnly, maxResults);

            //IReadOnlyList<IdentifyLayerResult> myIdentifyResult = await MyMapView.IdentifyLayersAsync(e.Position, pixelTolerance, returnPopupsOnly, maxResults);

            // Return if there's nothing to show
            if (myIdentifyResult.GeoElements.Count() < 1)
            {
                return;
            }

            FeatureLayer idLayer = myIdentifyResult.LayerContent as FeatureLayer;

            // Retrieve the identified feature, which is always a WmsFeature for WMS layers

            Feature idFeature = (Feature)myIdentifyResult.GeoElements[0];
            //foreach (GeoElement idElement in myIdentifyResult.GeoElements)
            // {
            // cast the result GeoElement to Feature
            //   Feature idFeature = idElement as Feature;

            try
            {
                string content = string.Format("{0}   {1}    {2}    {3}  {4}", idFeature.Attributes["name_FIRST"].ToString(), idFeature.Attributes["name_LAST"].ToString(), idFeature.Attributes["plot"].ToString(), idFeature.Attributes["lot"].ToString(), idFeature.Attributes["PLOT_ID"].ToString());



                CalloutDefinition myCalloutDefinition = new CalloutDefinition("Plot Attributes",content);

                MyMapView.ShowCalloutAt(tapScreenPoint, myCalloutDefinition);
            }

            catch
            {
                MessageBox.Show("Not an Identifeable Layer");
            }

        }

    }
}

    
    

