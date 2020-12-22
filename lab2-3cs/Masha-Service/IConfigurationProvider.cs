namespace M_Service
{
    public interface IConfigurationProvider
    {
        T Parse<T>() where T : new();
    }
}
