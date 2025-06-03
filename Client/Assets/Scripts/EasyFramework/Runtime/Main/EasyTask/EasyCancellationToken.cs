
using System;

public class EasyCancelException : Exception
{
}

public class EasyCancellationToken
{
    public bool IsCanceled { get; set; }
    public void Cancel()
    {
        IsCanceled = true;
    }
}