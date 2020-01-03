using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms.Markers;

using System.Runtime.InteropServices;

namespace mmhis_data_2_vs2012
{
	public partial class Map : the_sub_window
	{
		public GMap.NET.WindowsForms.GMapControl gMapControl_map;
		public System.Windows.Forms.Label label_current_gps_coordinates;
		public System.Drawing.Color map_line_color_0 = System.Drawing.Color.FromArgb(255, 0, 0);
		public int map_line_width_0 = 1;
		public System.Drawing.Color map_line_color_1 = System.Drawing.Color.FromArgb(0, 255, 0);
		public int map_line_width_1 = 2;
		public bool map_center_current_location;

		private bool disable_map_center_current_location_temperarily = false;
		// For mouse click on the map we don't we want to go to the location but NOT centering
		// it (because obviously it is visible -- the user just clicked the point). This only
		// applies to the window that received the mouse click so other map windows will still
		// center the point, a behavior expected by the user.

		public Map()
		{
			InitializeComponent();
		}

		private void Map_Load(object sender, EventArgs e)
		{
		}

		public void initialize_gMap_control()
		{
			GMaps.Instance.Mode = AccessMode.ServerOnly;
			gMapControl_map.ShowCenter = false;
			gMapControl_map.Bearing = 0F;
			gMapControl_map.CanDragMap = true;
			gMapControl_map.Dock = System.Windows.Forms.DockStyle.Fill;
			gMapControl_map.EmptyTileColor = System.Drawing.Color.Navy;
			gMapControl_map.GrayScaleMode = false;
			gMapControl_map.HelperLineOption = GMap.NET.WindowsForms.HelperLineOptions.DontShow;
			gMapControl_map.LevelsKeepInMemmory = 5;
			gMapControl_map.Location = new System.Drawing.Point(200, 200);
			gMapControl_map.MarkersEnabled = false;
			gMapControl_map.MouseWheelZoomEnabled = true;
			gMapControl_map.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionWithoutCenter;
			gMapControl_map.Name = "gmap";
			gMapControl_map.NegativeMode = false;
			gMapControl_map.PolygonsEnabled = false;
			gMapControl_map.RetryLoadTile = 0;
			gMapControl_map.RoutesEnabled = false;
			gMapControl_map.ScaleMode = GMap.NET.WindowsForms.ScaleModes.Fractional;
			gMapControl_map.SelectedAreaFillColor = System.Drawing.Color.FromArgb(
				((int)(((byte)(255)))),
				((int)(((byte)(000)))),
				((int)(((byte)(000)))),
				((int)(((byte)(225))))
			);
			gMapControl_map.ShowTileGridLines = false;
			gMapControl_map.Size = new System.Drawing.Size(400, 400);
			gMapControl_map.TabIndex = 1;
			//gMapControl_map.SetPositionByKeywords("Little Rock");
			gMapControl_map.DisableFocusOnMouseEnter = true;
			gMapControl_map.MapScaleInfoEnabled = true;
			gMapControl_map.DragButton = System.Windows.Forms.MouseButtons.Left;

			gMapControl_map.Overlays.Add(new GMapOverlay("main_layer"));
			gMapControl_map.Overlays.Add(new GMapOverlay("second_layer"));
		}

		public void set_map_center(PointLatLng point)
		{
			gMapControl_map.Position = point;
		}

		public void set_zoom(double zoom)
		{
			gMapControl_map.Zoom = zoom;
		}

		public void set_max_zoom(int max_zoom)
		{
			gMapControl_map.MaxZoom = max_zoom;
		}

		public void set_min_zoom(int min_zoom)
		{
			gMapControl_map.MinZoom = min_zoom;
		}

		public void set_map_provider(string provider)
		{
			if (provider == "ArcGIS_DarbAE_Q2_2011_NAVTQ_Eng_V5_MapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.ArcGIS_DarbAE_Q2_2011_NAVTQ_Eng_V5_MapProvider.Instance;
			}
			else if (provider == "ArcGIS_Imagery_World_2D_MapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.ArcGIS_Imagery_World_2D_MapProvider.Instance;
			}
			else if (provider == "ArcGIS_ShadedRelief_World_2D_MapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.ArcGIS_ShadedRelief_World_2D_MapProvider.Instance;
			}
			else if (provider == "ArcGIS_StreetMap_World_2D_MapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.ArcGIS_StreetMap_World_2D_MapProvider.Instance;
			}
			else if (provider == "ArcGIS_Topo_US_2D_MapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.ArcGIS_Topo_US_2D_MapProvider.Instance;
			}
			else if (provider == "ArcGIS_World_Physical_MapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.ArcGIS_World_Physical_MapProvider.Instance;
			}
			else if (provider == "ArcGIS_World_Shaded_Relief_MapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.ArcGIS_World_Shaded_Relief_MapProvider.Instance;
			}
			else if (provider == "ArcGIS_World_Street_MapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.ArcGIS_World_Street_MapProvider.Instance;
			}
			else if (provider == "ArcGIS_World_Terrain_Base_MapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.ArcGIS_World_Terrain_Base_MapProvider.Instance;
			}
			else if (provider == "ArcGIS_World_Topo_MapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.ArcGIS_World_Topo_MapProvider.Instance;
			}
			else if (provider == "BingHybridMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.BingHybridMapProvider.Instance;
			}
			else if (provider == "BingMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.BingMapProvider.Instance;
			}
			else if (provider == "BingSatelliteMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.BingSatelliteMapProvider.Instance;
			}
			else if (provider == "CloudMadeMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.CloudMadeMapProvider.Instance;
			}
			else if (provider == "EmptyProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.EmptyProvider.Instance;
			}
			else if (provider == "GoogleHybridMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.GoogleHybridMapProvider.Instance;
			}
			else if (provider == "GoogleMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
			}
			else if (provider == "GoogleSatelliteMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.GoogleSatelliteMapProvider.Instance;
			}
			else if (provider == "GoogleTerrainMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.GoogleTerrainMapProvider.Instance;
			}
			else if (provider == "MapBenderWMSProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.MapBenderWMSProvider.Instance;
			}
			else if (provider == "NearHybridMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.NearHybridMapProvider.Instance;
			}
			else if (provider == "NearMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.NearMapProvider.Instance;
			}
			else if (provider == "NearSatelliteMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.NearSatelliteMapProvider.Instance;
			}
			else if (provider == "OpenCycleLandscapeMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.OpenCycleLandscapeMapProvider.Instance;
			}
			else if (provider == "OpenCycleMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.OpenCycleMapProvider.Instance;
			}
			else if (provider == "OpenCycleTransportMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.OpenCycleTransportMapProvider.Instance;
			}
			else if (provider == "OpenStreet4UMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.OpenStreet4UMapProvider.Instance;
			}
			else if (provider == "OpenStreetMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance;
			}
			else if (provider == "OpenStreetMapQuestHybridProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.OpenStreetMapQuestHybridProvider.Instance;
			}
			else if (provider == "OpenStreetMapQuestProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.OpenStreetMapQuestProvider.Instance;
			}
			else if (provider == "OpenStreetMapQuestSatteliteProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.OpenStreetMapQuestSatteliteProvider.Instance;
			}
			else if (provider == "OviHybridMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.OviHybridMapProvider.Instance;
			}
			else if (provider == "OviMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.OviMapProvider.Instance;
			}
			else if (provider == "OviSatelliteMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.OviSatelliteMapProvider.Instance;
			}
			else if (provider == "OviTerrainMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.OviTerrainMapProvider.Instance;
			}
			else if (provider == "WikiMapiaMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.WikiMapiaMapProvider.Instance;
			}
			else if (provider == "YahooHybridMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.YahooHybridMapProvider.Instance;
			}
			else if (provider == "YahooMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.YahooMapProvider.Instance;
			}
			else if (provider == "YahooSatelliteMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.YahooSatelliteMapProvider.Instance;
			}
			else if (provider == "YandexHybridMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.YandexHybridMapProvider.Instance;
			}
			else if (provider == "YandexMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.YandexMapProvider.Instance;
			}
			else if (provider == "YandexSatelliteMapProvider")
			{
				gMapControl_map.MapProvider = GMap.NET.MapProviders.YandexSatelliteMapProvider.Instance;
			}
			else
			{
				// default to google road
				gMapControl_map.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
			}
			gMapControl_map.Refresh();
		}

		public void show_line(List<PointLatLng> points)
		{
			gMapControl_map.Overlays[0].Routes.Clear();
			if (points != null)
			{
				GMapRoute my_line = new GMapRoute(points, "my_line");
				my_line.Stroke = new Pen(map_line_color_0, map_line_width_0);

				gMapControl_map.Overlays[0].Routes.Add(my_line);
				gMapControl_map.RoutesEnabled = true;
			}
		}

		public void show_line_of_interest(List<PointLatLng> points)
		{
			gMapControl_map.Overlays[1].Routes.Clear();
			if (points != null)
			{
				GMapRoute my_line = new GMapRoute(points, "my_line");
				my_line.Stroke = new Pen(map_line_color_1, map_line_width_1);

				gMapControl_map.Overlays[1].Routes.Add(my_line);
				gMapControl_map.RoutesEnabled = true;
			}
		}

		public void show_point(PointLatLng point)
		{
			gMapControl_map.Overlays[0].Markers.Clear();
			if (point != new PointLatLng(0, 0))
			{
				gMapControl_map.Overlays[0].Markers.Add(
					new GMap.NET.WindowsForms.Markers.GMarkerGoogle(
						point,
						GMap.NET.WindowsForms.Markers.GMarkerGoogleType.yellow_small
					)
				);
				if (map_center_current_location && !disable_map_center_current_location_temperarily)
				{
					set_map_center(point);
				}

				disable_map_center_current_location_temperarily = false;
				// This controls the centering behavior only once each time the map is clicked

				gMapControl_map.MarkersEnabled = true;
			}
		}

		public void show_point_of_interest(PointLatLng point)
		{
			gMapControl_map.Overlays[1].Markers.Clear();
			if (point != new PointLatLng(0, 0))
			{
				gMapControl_map.Overlays[1].Markers.Add(
					new GMap.NET.WindowsForms.Markers.GMarkerGoogle(
						point,
						GMap.NET.WindowsForms.Markers.GMarkerGoogleType.arrow
					)
				);
				if (map_center_current_location)
				{
					set_map_center(point);
				}
				gMapControl_map.MarkersEnabled = true;
			}
		}

		public void gMapControl_map_MouseUp(object sender, MouseEventArgs e)
		{
			var mouse_click_location = gMapControl_map.FromLocalToLatLng(e.X, e.Y);
			double latitude = mouse_click_location.Lat;
			double longitude = mouse_click_location.Lng;
			if (dlee_win32.win32.GetAsyncKeyState(17) < 0) // ctrl key
			{
				if (e.Button == System.Windows.Forms.MouseButtons.Left)
				{
					the_parent_form.textBox_preview_query_data_gps.Text = latitude.ToString("F13") + ", " + longitude.ToString("F13");
					disable_map_center_current_location_temperarily = true;
					the_parent_form.go_to_gps(latitude, longitude);
				}
			}
		}

		public void label_current_gps_coordinates_TextChanged(object sender, EventArgs e)
		{
			show_current_gps_location();
		}

		public void show_current_gps_location()
		{
			if (!Visible)
			{
				return;
			}
			if (label_current_gps_coordinates.Text.Trim().Length == 0)
			{
				show_point(new PointLatLng(0, 0));
			}
			else
			{
				try
				{
					string[] coordinates = label_current_gps_coordinates.Text.Split(new char[] { ',' }, 2);
					show_point(
						new PointLatLng(
							Convert.ToDouble(coordinates[0]),
							Convert.ToDouble(coordinates[1])
						)
					);
				}
				catch
				{
					show_point(new PointLatLng(0, 0));
				}
			}
		}
	}
}
