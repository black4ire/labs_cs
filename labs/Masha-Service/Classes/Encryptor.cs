using System.Text;
using System.Security.Cryptography;
using System.IO;
using System;
using System.Configuration;

using Config_Provider;
using LoggerToTXT;
namespace STW_Service
{
    public class Encryptor
    {
        LoggerToTxt logger = new LoggerToTxt();
        readonly DESCryptoServiceProvider crypto;
        readonly object obj = new object(); // just a mutex
        string ErrorLogPath { get; } = ConfigurationManager.AppSettings["ErrorLogPath"];
        readonly string configPath = ConfigurationManager.AppSettings["PathToConfig"];
        public Encryptor()
        {
            CryptoOptions options = (new ConfigManager(configPath)).GetOptions<CryptoOptions>();
            crypto = new DESCryptoServiceProvider
            {
                Key = Encoding.ASCII.GetBytes(options.Key),
                IV = Encoding.ASCII.GetBytes(options.IV)
            };

            try
            {
                ErrorLogPath = (new ConfigManager(configPath)).GetOptions<Logs>().ErrorLog;
            }
            catch (Exception ex)
            {
                logger.LogToAsync(ErrorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
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
                logger.LogToAsync(ErrorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
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
                logger.LogToAsync(ErrorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}
