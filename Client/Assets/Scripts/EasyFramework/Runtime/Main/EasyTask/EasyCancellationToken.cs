
using System;

public class EasyCancelException : Exception
{
    public override string Message { get => "EasyTask Canceled"; }
}

public class EasyCancellationToken
{
    public bool IsCanceled { get; set; }
    public void Cancel()
    {
        IsCanceled = true;
    }
}