
namespace CSharpCraft;

public class ConcurrentString
{
    private string value;
    private readonly Lock stringLock = new();

    public ConcurrentString(string initialValue = "")
    {
        value = initialValue;
    }

    public string Value
    {
        get
        {
            lock (stringLock)
            {
                return value;
            }
        }
        set
        {
            lock (stringLock)
            {
                this.value = value;
            }
        }
    }
}
