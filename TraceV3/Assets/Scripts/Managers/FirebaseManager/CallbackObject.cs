public class CallbackObject
{
    public CallbackEnum callbackEnum;
    public object ReturnValue;
    public string message;
    public CallbackObject()
    {
        callbackEnum = CallbackEnum.FAILED;
        message = "";
    }
}

public enum CallbackEnum
{
    SUCCESS,
    CONNECTIONERROR,
    FAILED
}