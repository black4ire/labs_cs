namespace ServiceLayer
{
    public interface IDALService
    {
        void MakeJob(string pathToFTP);
        void MakeJobAsync(string pathToFTP);
    }
}
