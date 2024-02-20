/**
 * GameConstCfg
 */
	[Easy.Config("GameConstCfg")]
	public class GameConstCfgDictionary : System.Collections.Generic.Dictionary<string, CfgSpace.GameConstCfg>
	{
		public static GameConstCfgDictionary CreateConfig(Google.FlatBuffers.ByteBuffer byteBuffer)
 		{
			GameConstCfgDictionary cfgs = new GameConstCfgDictionary();
			CfgSpace.GameConstCfgs configData = CfgSpace.GameConstCfgs.GetRootAsGameConstCfgs(byteBuffer);
			for(int i = 0; i < configData.GameConstCfgArrayLength; ++i)
			{
				CfgSpace.GameConstCfg cfg = (CfgSpace.GameConstCfg)configData.GameConstCfgArray(i);
				cfgs.Add(cfg.Id, cfg);
			}
			return cfgs;
		}
	}
