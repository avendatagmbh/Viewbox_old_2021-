using DbAccess.Attributes;

namespace Viewbox.Structures
{
	[DbTable("co_kostenstellengruppe_anzeigen")]
	public class KostenstellengruppeAnzeigen
	{
		[DbColumn("MANDT")]
		public string MANDT { get; set; }

		[DbColumn("Kostenstellengruppe")]
		public string Kostenstellengruppe { get; set; }

		[DbColumn("Kostenrechnungskreis")]
		public string Kostenrechnungskreis { get; set; }

		[DbColumn("KOSTL")]
		public string KOSTL { get; set; }

		[DbColumn("KOSTL_Bez")]
		public string KOSTL_Bez { get; set; }

		[DbColumn("LEAF")]
		public string LEAF { get; set; }

		[DbColumn("LEAF_Bez")]
		public string LEAF_Bez { get; set; }

		[DbColumn("KOSTL1")]
		public string KOSTL1 { get; set; }

		[DbColumn("KOSTL1_Bez")]
		public string KOSTL1_Bez { get; set; }

		[DbColumn("LEAF1")]
		public string LEAF1 { get; set; }

		[DbColumn("LEAF1_Bez")]
		public string LEAF1_Bez { get; set; }

		[DbColumn("KOSTL2")]
		public string KOSTL2 { get; set; }

		[DbColumn("KOSTL2_Bez")]
		public string KOSTL2_Bez { get; set; }

		[DbColumn("LEAF2")]
		public string LEAF2 { get; set; }

		[DbColumn("LEAF2_Bez")]
		public string LEAF2_Bez { get; set; }

		[DbColumn("KOSTL3")]
		public string KOSTL3 { get; set; }

		[DbColumn("KOSTL3_Bez")]
		public string KOSTL3_Bez { get; set; }

		[DbColumn("LEAF3")]
		public string LEAF3 { get; set; }

		[DbColumn("LEAF3_Bez")]
		public string LEAF3_Bez { get; set; }

		[DbColumn("KOSTL4")]
		public string KOSTL4 { get; set; }

		[DbColumn("KOSTL4_Bez")]
		public string KOSTL4_Bez { get; set; }

		[DbColumn("LEAF4")]
		public string LEAF4 { get; set; }

		[DbColumn("LEAF4_Bez")]
		public string LEAF4_Bez { get; set; }

		[DbColumn("KOSTL5")]
		public string KOSTL5 { get; set; }

		[DbColumn("KOSTL5_Bez")]
		public string KOSTL5_Bez { get; set; }

		[DbColumn("LEAF5")]
		public string LEAF5 { get; set; }

		[DbColumn("LEAF5_Bez")]
		public string LEAF5_Bez { get; set; }

		[DbColumn("KOSTL6")]
		public string KOSTL6 { get; set; }

		[DbColumn("KOSTL6_Bez")]
		public string KOSTL6_Bez { get; set; }

		[DbColumn("LEAF6")]
		public string LEAF6 { get; set; }

		[DbColumn("LEAF6_Bez")]
		public string LEAF6_Bez { get; set; }

		[DbColumn("KOSTL7")]
		public string KOSTL7 { get; set; }

		[DbColumn("KOSTL7_Bez")]
		public string KOSTL7_Bez { get; set; }

		[DbColumn("LEAF7")]
		public string LEAF7 { get; set; }

		[DbColumn("LEAF7_Bez")]
		public string LEAF7_Bez { get; set; }

		[DbColumn("KOSTL8")]
		public string KOSTL8 { get; set; }

		[DbColumn("KOSTL8_Bez")]
		public string KOSTL8_Bez { get; set; }

		[DbColumn("LEAF8")]
		public string LEAF8 { get; set; }

		[DbColumn("LEAF8_Bez")]
		public string LEAF8_Bez { get; set; }

		[DbColumn("KOSTL9")]
		public string KOSTL9 { get; set; }

		[DbColumn("KOSTL9_Bez")]
		public string KOSTL9_Bez { get; set; }

		[DbColumn("LEAF9")]
		public string LEAF9 { get; set; }

		[DbColumn("LEAF9_Bez")]
		public string LEAF9_Bez { get; set; }

		[DbColumn("KOSTL10")]
		public string KOSTL10 { get; set; }

		[DbColumn("KOSTL10_Bez")]
		public string KOSTL10_Bez { get; set; }

		[DbColumn("LEAF10")]
		public string LEAF10 { get; set; }

		[DbColumn("LEAF10_Bez")]
		public string LEAF10_Bez { get; set; }

		public string GetData(string name)
		{
			return name.ToLower() switch
			{
				"mandt" => MANDT, 
				"kostenstellengruppe" => Kostenstellengruppe, 
				"kostenrechnungskreis" => Kostenrechnungskreis, 
				"kostl" => KOSTL, 
				"kostl_bez" => KOSTL_Bez, 
				"leaf" => LEAF, 
				"leaf_bez" => LEAF_Bez, 
				"kostl1" => KOSTL1, 
				"kostl1_bez" => KOSTL1_Bez, 
				"leaf1" => LEAF1, 
				"leaf1_bez" => LEAF1_Bez, 
				"kostl2" => KOSTL2, 
				"kostl2_bez" => KOSTL2_Bez, 
				"leaf2" => LEAF2, 
				"leaf2_bez" => LEAF2_Bez, 
				"kostl3" => KOSTL3, 
				"kostl3_bez" => KOSTL3_Bez, 
				"leaf3" => LEAF3, 
				"leaf3_bez" => LEAF3_Bez, 
				"kostl4" => KOSTL4, 
				"kostl4_bez" => KOSTL4_Bez, 
				"leaf4" => LEAF4, 
				"leaf4_bez" => LEAF4_Bez, 
				"kostl5" => KOSTL5, 
				"kostl5_bez" => KOSTL5_Bez, 
				"leaf5" => LEAF5, 
				"leaf5_bez" => LEAF5_Bez, 
				"kostl6" => KOSTL6, 
				"kostl6_bez" => KOSTL6_Bez, 
				"leaf6" => LEAF6, 
				"leaf6_bez" => LEAF6_Bez, 
				"kostl7" => KOSTL7, 
				"kostl7_bez" => KOSTL7_Bez, 
				"leaf7" => LEAF7, 
				"leaf7_bez" => LEAF7_Bez, 
				"kostl8" => KOSTL8, 
				"kostl8_bez" => KOSTL8_Bez, 
				"leaf8" => LEAF8, 
				"leaf8_bez" => LEAF8_Bez, 
				"kostl9" => KOSTL9, 
				"kostl9_bez" => KOSTL9_Bez, 
				"leaf9" => LEAF9, 
				"leaf9_bez" => LEAF9_Bez, 
				"kostl10" => KOSTL10, 
				"kostl10_bez" => KOSTL10_Bez, 
				"leaf10" => LEAF10, 
				"leaf10_bez" => LEAF10_Bez, 
				_ => "", 
			};
		}
	}
}
