/**
 * EquipmentCfg
 */
	[Easy.Config("EquipmentCfg")]
	[System.Serializable]
	public class EquipmentCfg : System.Collections.Generic.Dictionary<int, EquipmentCfg>
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
 		private StructTest[] structTest;    //StructTest

		[Newtonsoft.Json.JsonIgnore]
 		public StructTest[] StructTest => structTest;

	} 
