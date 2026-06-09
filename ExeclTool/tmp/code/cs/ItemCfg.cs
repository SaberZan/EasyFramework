/**
 * ItemCfg
 */
	[Easy.Config("ItemCfg")]
	public class ItemCfg : System.Collections.Generic.Dictionary<int, ItemCfg>
 	{
		[Newtonsoft.Json.JsonProperty]
 		private int _id;    //id

		[Newtonsoft.Json.JsonIgnore]
 		public int id => _id;

		[Newtonsoft.Json.JsonProperty]
 		private string name;    //name

		[Newtonsoft.Json.JsonIgnore]
 		public string Name => name;

		[Newtonsoft.Json.JsonProperty]
 		private int price;    //price

		[Newtonsoft.Json.JsonIgnore]
 		public int Price => price;

		[Newtonsoft.Json.JsonProperty]
 		private Attr attr;    //attr

		[Newtonsoft.Json.JsonIgnore]
 		public Attr Attr => attr;

	} 
