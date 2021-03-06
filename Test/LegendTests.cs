﻿using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using BruTile.Web;
using GeoAPI.Geometries;
using NetTopologySuite;
using NUnit.Framework;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Layers.Symbolizer;
using SharpMap.Rendering.Decoration.Legend.Factories;
using System.Drawing;
using SharpMap.Rendering.Symbolizer;
using SharpMap.Utilities.Wfs;

namespace SharpMap.Rendering.Decoration.Legend
{
	public class LegendTests
	{
		[TestFixtureSetUp]
		public void SetUp()
		{
			GeoAPI.GeometryServiceProvider.Instance = NtsGeometryServices.Instance;
			SharpMap.Rendering.VectorRenderer.SizeOfString = SizeOfString;
		}
		
		private static SizeF SizeOfString(Graphics g, string text, Font font) {
			return g.MeasureString(text, font);
			//return new SizeF(s.Width*0.9f, s.Height * 0.9f);
			
		}

		[Test]
		public void TestStandardLayers()
		{
			using (var map = new Map())
			{
				map.Layers.Add(
					new TileLayer(new OsmTileSource(new OsmRequest(KnownOsmTileServers.Mapnik)),
						"OSM - Mapnik"));


                //var wfs = new SharpMap.Web.Wfs.Client(@"http://www2.dmsolutions.ca/cgi-bin/mswfs_gmap?version=1.0.0&request=getcapabilities&service=wfs");
                //var p = new SharpMap.Data.Providers.WFS(
                //    new WfsFeatureTypeInfo { SRID = "EPSG:42304", Geometry = new WfsFeatureTypeInfo.GeometryInfo{}});
                
                map.Layers.Add(
					new VectorLayer("Lineal VectorLayer",
					                new SharpMap.Data.Providers.GeometryFeatureProvider(
					                	NtsGeometryServices.Instance.CreateGeometryFactory(4326).CreateLineString(
					                		new [] { new GeoAPI.Geometries.Coordinate(0, 0), new GeoAPI.Geometries.Coordinate(10, 0)}))));
				var l = map.Layers[1] as VectorLayer;
				l.Style = SharpMap.Styles.VectorStyle.CreateRandomLinealStyle();
				
				map.Layers.Add(
					new VectorLayer("Puntal VectorLayer",
					                new SharpMap.Data.Providers.GeometryFeatureProvider(
					                	NtsGeometryServices.Instance.CreateGeometryFactory(4326).CreatePoint(
					                		new GeoAPI.Geometries.Coordinate(5, 5)))));
				l = map.Layers[2] as VectorLayer;
				l.Style = SharpMap.Styles.VectorStyle.CreateRandomPuntalStyle();
				map.Layers.Add(
					new VectorLayer("Polygonal VectorLayer",
					                new SharpMap.Data.Providers.GeometryFeatureProvider(
					                	NtsGeometryServices.Instance.CreateGeometryFactory(4326).CreatePolygon(null, null))));
				l = map.Layers[3] as VectorLayer;
				l.Style = SharpMap.Styles.VectorStyle.CreateRandomPolygonalStyle();

				map.Layers.Add(
					new VectorLayer("Geometry VectorLayer",
					                new SharpMap.Data.Providers.GeometryFeatureProvider(
					                	NtsGeometryServices.Instance.CreateGeometryFactory(4326).CreatePoint(
					                		new GeoAPI.Geometries.Coordinate(5, 5)))));
				l = map.Layers[4] as VectorLayer;
				l.Style = SharpMap.Styles.VectorStyle.CreateRandomStyle();

				map.Layers.Add(
					new VectorLayer("Geometry VectorLayer with theme",
					                new SharpMap.Data.Providers.GeometryFeatureProvider(
					                	NtsGeometryServices.Instance.CreateGeometryFactory(4326).CreatePoint(
					                		new GeoAPI.Geometries.Coordinate(5, 5)))));
				l = map.Layers[5] as VectorLayer;

				var dict = new Dictionary<int, SharpMap.Styles.IStyle>();
				dict.Add(1, SharpMap.Styles.VectorStyle.CreateRandomLinealStyle());
				dict.Add(2, SharpMap.Styles.VectorStyle.CreateRandomLinealStyle());
				dict.Add(3, SharpMap.Styles.VectorStyle.CreateRandomLinealStyle());
				dict.Add(4, SharpMap.Styles.VectorStyle.CreateRandomLinealStyle());
				l.Theme = new SharpMap.Rendering.Thematics.UniqueValuesTheme<int>("test", dict,
				                                                                  SharpMap.Styles.VectorStyle.CreateRandomLinealStyle());

				map.Layers.Add(
					new VectorLayer("Geometry VectorLayer with gradient theme",
					                new SharpMap.Data.Providers.GeometryFeatureProvider(
					                	NtsGeometryServices.Instance.CreateGeometryFactory(4326).CreatePoint(
					                		new GeoAPI.Geometries.Coordinate(5, 5)))));
				l = map.Layers[6] as VectorLayer;
				l.Theme = new SharpMap.Rendering.Thematics.GradientTheme("test", -100, 100,
				                                                         SharpMap.Styles.VectorStyle.CreateRandomPolygonalStyle(),
				                                                         SharpMap.Styles.VectorStyle.CreateRandomPolygonalStyle());
				((SharpMap.Rendering.Thematics.GradientTheme)l.Theme).FillColorBlend = SharpMap.Rendering.Thematics.ColorBlend.RainbowMOHID;

				var ct = new SharpMap.Rendering.Thematics.CategoryTheme<int>();
				ct.ColumnName = "test";
				ct.Default = new SharpMap.Rendering.Thematics.CategoryThemeValuesItem<int> { Title = "standard", Style =
						SharpMap.Styles.VectorStyle.CreateRandomPolygonalStyle()};
				ct.Add(new SharpMap.Rendering.Thematics.CategoryThemeValuesItem<int> { Title = "Line", Style =
				       		SharpMap.Styles.VectorStyle.CreateRandomLinealStyle()});
				ct.Add(new SharpMap.Rendering.Thematics.CategoryThemeValuesItem<int> { Title = "Point", Style =
				       		SharpMap.Styles.VectorStyle.CreateRandomPuntalStyle()});
				
				map.Layers.Add(
					new VectorLayer("Geometry VectorLayer with category theme",
					                new SharpMap.Data.Providers.GeometryFeatureProvider(
					                	NtsGeometryServices.Instance.CreateGeometryFactory(4326).CreatePoint(
					                		new GeoAPI.Geometries.Coordinate(5, 5)))));
				l = map.Layers[7] as VectorLayer;
				l.Theme = ct;
				
				try
				{

					map.ZoomToExtents();
					var factory = new LegendFactory();
					((LegendSettings)factory.LegendSettings).SymbolSize = new Size(16, 16);
                    ((LegendSettings)factory.LegendSettings).HeaderFont = new Font("Times New Roman", 8, FontStyle.Italic);
					var legend = factory.Create(map, null);
					((MapDecoration)legend).BackgroundColor = System.Drawing.Color.White;
					legend.Root.Label = "This is a super long legend title";
					using (var img = legend.GetLegendImage())
					{
						img.Save("legendimage.png");
						System.Diagnostics.Process.Start("legendimage.png");
					}}
				catch(Exception e)
				{
					throw e;
				}
			}

		}

	    [Test]
	    public void TestSymbolizerLayers()
	    {
            using (var map = new Map())
            {
                map.SRID = 4326;

                map.Layers.Add(new PuntalVectorLayer("Puntal with CharacterPointSymbolizer",
                    new GeometryProvider(map.Factory.CreatePoint(new Coordinate(0, 0))),
                    new CharacterPointSymbolizer
                    {
                        Font = new Font("Arial", 12, FontStyle.Bold, GraphicsUnit.Pixel),
                        CharacterIndex = 65,
                        Foreground = new SolidBrush(Color.Violet),
                        Halo = 2,
                        HaloBrush = new SolidBrush(Color.Silver)
                    }));

                map.Layers.Add(new PuntalVectorLayer("Puntal with PathPointSymbolizer",
                    new GeometryProvider(map.Factory.CreatePoint(new Coordinate(0, 0))),
                    PathPointSymbolizer.CreateCircle(Pens.Aquamarine, Brushes.BurlyWood, 24)));

                try
                {

                    map.ZoomToExtents();
                    var factory = new LegendFactory();
                    ((LegendSettings)factory.LegendSettings).SymbolSize = new Size(24, 24);
                    ((LegendSettings)factory.LegendSettings).HeaderFont = new Font("Times New Roman", 14, FontStyle.Italic, GraphicsUnit.Pixel);
                    var legend = factory.Create(map, null);
                    ((MapDecoration)legend).BackgroundColor = System.Drawing.Color.White;
                    legend.Root.Label = "This is a super long legend title";
                    using (var img = legend.GetLegendImage(300))
                    {
                        img.Save("legendimage2.png");
                        System.Diagnostics.Process.Start("legendimage2.png");
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
	}
}
