/**
 * Attr
 */
	[System.Serializable]
	public class Attr
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
