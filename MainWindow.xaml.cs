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
        string strConn;
        string sql1, sql2;
        string sqllot, cId;


        public MainWindow()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization 
            Initialize();

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


               
        private void Initialize()
        {
            //Map myMap = new Map(Basemap.CreateTerrainWithLabels());
            Map myMap = new Map(BasemapType.ImageryWithLabelsVector, 35.728990, -78.856027, 18);

            //LegendInfo MapLegend = new LegendInfo {Name, Symbol };

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
                     
            var town_plot = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.Color.Pink, plotOutline);
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
            
            MyMapView.Map = myMap;

        } 

    } 
    
}
