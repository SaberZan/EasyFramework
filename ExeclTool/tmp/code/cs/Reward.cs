/**
 * Reward
 */
	[System.Serializable]
	public class Reward
 	{
		[Newtonsoft.Json.JsonProperty]
 		private int itemId;    //itemId

		[Newtonsoft.Json.JsonIgnore]
 		public int ItemId => itemId;

		[Newtonsoft.Json.JsonProperty]
 		private int itemNum;    //itemNum

		[Newtonsoft.Json.JsonIgnore]
 		public int ItemNum => itemNum;

	} 
