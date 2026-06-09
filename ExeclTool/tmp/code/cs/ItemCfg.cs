/**
 * ItemCfg
 */
	[Easy.Config("ItemCfg")]
	[System.Serializable]
	public class ItemCfg : System.Collections.Generic.Dictionary<int, ItemCfg>
 	{
/**
 * ItemCfgAttr
 */
	[System.Serializable]
	public class ItemCfgAttr
 	{
		[Newtonsoft.Json.JsonProperty]
 		private string name;    //name

		[Newtonsoft.Json.JsonIgnore]
 		public string Name => name;

		[Newtonsoft.Json.JsonProperty]
 		private int value;    //value

		[Newtonsoft.Json.JsonIgnore]
 		public int Value => value;

		[Newtonsoft.Json.JsonProperty]
 		private float coeff;    //coeff

		[Newtonsoft.Json.JsonIgnore]
 		public float Coeff => coeff;

	} 
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
 		private ItemCfgAttr attr;    //attr

		[Newtonsoft.Json.JsonIgnore]
 		public ItemCfgAttr Attr => attr;

	} 
