namespace Config_Provider
{
    public interface IConfigurationProvider
    {
        T Parse<T>() where T : new();
    }
}
