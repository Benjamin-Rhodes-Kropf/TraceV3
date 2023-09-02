public class CallbackObject
{
    public LoginStatus LoginStatus;
    public object ReturnValue;
    public string message;
    public CallbackObject()
    {
        LoginStatus = LoginStatus.Failed;
        message = "";
    }
}

public enum LoginStatus
{
    Success,
    ConnectionError,
    Failed,
    UnFinishedRegistration,
}