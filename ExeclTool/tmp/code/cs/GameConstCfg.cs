/**
 * GameConstCfg
 */
	[Easy.Config("GameConstCfg")]
	[System.Serializable]
	public class GameConstCfg : System.Collections.Generic.Dictionary<string, GameConstCfg>
 	{
		[Newtonsoft.Json.JsonProperty]
 		private int _id;    //id

		[Newtonsoft.Json.JsonIgnore]
 		public int id => _id;

		[Newtonsoft.Json.JsonProperty]
 		private string paramString;    //paramString

		[Newtonsoft.Json.JsonIgnore]
 		public string ParamString => paramString;

		[Newtonsoft.Json.JsonProperty]
 		private int paramInt;    //paramInt

		[Newtonsoft.Json.JsonIgnore]
 		public int ParamInt => paramInt;

		[Newtonsoft.Json.JsonProperty]
 		private int[] paramIntArray;    //paramIntArray

		[Newtonsoft.Json.JsonIgnore]
 		public int[] ParamIntArray => paramIntArray;

		[Newtonsoft.Json.JsonProperty]
 		private int test;    //test

		[Newtonsoft.Json.JsonIgnore]
 		public int Test => test;

	} 
