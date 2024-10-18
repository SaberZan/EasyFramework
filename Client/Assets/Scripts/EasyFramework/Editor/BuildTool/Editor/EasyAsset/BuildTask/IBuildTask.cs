using System;
using System.Threading.Tasks;

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
        
        Task<BuildResult> Run(GenerateContext context);
    }
}