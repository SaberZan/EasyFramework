/**
 * StructTest
 */
	[System.Serializable]
	public class StructTest
 	{
		[Newtonsoft.Json.JsonProperty]
 		private string value1;    //value1

		[Newtonsoft.Json.JsonIgnore]
 		public string Value1 => value1;

		[Newtonsoft.Json.JsonProperty]
 		private int value2;    //value2

		[Newtonsoft.Json.JsonIgnore]
 		public int Value2 => value2;

		[Newtonsoft.Json.JsonProperty]
 		private float value3;    //value3

		[Newtonsoft.Json.JsonIgnore]
 		public float Value3 => value3;

	} 
