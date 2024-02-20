using System;

namespace Easy.EasyAsset
{
    public enum BuildResult
    {
        Success,
        Fail,
    }
    
    public interface IBuildTask
    {
        string BuildName();
        
        BuildResult Run(GenerateContext context);
    }
}