public abstract class Singleton<T> where T : class, new()
{
    private static object _obj = new object();
    private static T _instance;

    protected Singleton()
    {
    }

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_obj)
                {
                    if (_instance == null)
                    {
                        _instance = new T();
                    }
                }
            }

            return _instance;
        }
    }
}