using System.Text;
using System.Security.Cryptography;
using System.IO;
using System;
using System.Configuration;
using System.ServiceProcess;

namespace M_Service
{
    public class Encryptor
    {
        readonly DESCryptoServiceProvider crypto;
        readonly object Obect = new object();
        string ErrorLogPath { get; } = ConfigurationManager.AppSettings["ErrorLogPath"];
        public Encryptor()
        {
            CryptoOptions options = (new Manager()).GetOptions<CryptoOptions>();
            crypto = new DESCryptoServiceProvider
            {
                Key = Encoding.ASCII.GetBytes(options.Key),
                IV = Encoding.ASCII.GetBytes(options.IV)
            };

            try
            {
                ErrorLogPath = (new Manager()).GetOptions<Logs>().ErrorLog;
            }
            catch (Exception ex)
            {
                lock (Obect)
                {
                    using (var errorStream = new StreamWriter(new FileStream(ErrorLogPath, FileMode.OpenOrCreate)))
                    {
                        errorStream.WriteLine(ex.Message + "\n" + ex.StackTrace);
                    }
                    var service = new ServiceController("STW-Service");
                    service.Stop();
                }
            }
        }

        public void Encrypt(Stream sourceStream, Stream targetEncryptedStream)
        {
            try
            {
                using (var ecStream = new CryptoStream(targetEncryptedStream, crypto.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    sourceStream.CopyTo(ecStream);
                }
            }
            catch (Exception ex)
            {
                lock (Obect)
                {
                    using (var errorStream = new StreamWriter(new FileStream(ErrorLogPath, FileMode.OpenOrCreate)))
                    {
                        errorStream.WriteLine(ex.Message + "\n" + ex.StackTrace);
                    }
                    var service = new ServiceController("STW-Service");
                    service.Stop();
                }
            }
        }

        public void Decrypt(Stream sourceEncryptedStream, Stream targetDecryptedStream)
        {
            try
            {
                using (var dcStream = new CryptoStream(sourceEncryptedStream, crypto.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    dcStream.CopyTo(targetDecryptedStream);
                }
            }
            catch (Exception ex)
            {
                lock (Obect)
                {
                    using (StreamWriter errorStream = new StreamWriter(new FileStream(ErrorLogPath, FileMode.OpenOrCreate)))
                    {
                        errorStream.WriteLine(ex.Message + "\n" + ex.StackTrace);
                    }
                    var service = new ServiceController("STW-Service");
                    service.Stop();
                }
            }
        }
    }
}
