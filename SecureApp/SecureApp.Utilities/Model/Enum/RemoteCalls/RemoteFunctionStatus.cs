namespace SecureApp.Utilities.Model.Enum.RemoteCalls
{
    public enum RemoteFunctionStatus : byte
    {
        Success,
        PermissionDenied,
        ExceptionThrown,
        DoesNotExist,
        InvalidParameters
    }
}