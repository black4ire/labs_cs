using System.IO;
using System.Threading.Tasks;

namespace LoggerToTXT
{
    public class LoggerToTxt
    {
        public LoggerToTxt() { }
        FileStream GetFileReady(string filename)
        {
            FileStream fs;
            if (!File.Exists(filename))
                throw new System.Exception($"Файл {filename} не найден.");
            while (true)
            {
                try
                {
                    fs = new FileStream(filename, FileMode.Open, FileAccess.Write, FileShare.None);
                    break;
                }
                catch
                {
                    continue;
                }
            }
            return fs;
        }
        public async void LogToAsync(string filename, string text)
        {
            FileStream fs = GetFileReady(filename);
            try
            {
                fs.Seek(fs.Length, SeekOrigin.Begin);
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    await Task.Run(() => writer.WriteLine(text));
                }
            }
            finally
            {
                fs.Close();
            }

        }
    }
}
