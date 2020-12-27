using System;
using System.IO.Compression;
using System.IO;
using System.Configuration;

using Config_Provider;
using LoggerToTXT;
namespace STW_Service
{
    public class Archivator
    {
        LoggerToTxt logger = new LoggerToTxt();
        readonly object obj = new object(); // just a mutex
        string ErrorLogPath { get; } = ConfigurationManager.AppSettings["ErrorLogPath"];
        string ConfigsPath { get; } = ConfigurationManager.AppSettings["PathToConfig"];
        public Archivator()
        {
            try
            {
                ErrorLogPath = (new ConfigManager(ConfigsPath)).GetOptions<Logs>().ErrorLog;
            }
            catch (Exception ex)
            {
                logger.LogToAsync(ErrorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
            }
        }
        string TargetEncryptedFilePath(string fileName, string targetDir)
        {
            fileName = fileName.Replace(Path.GetDirectoryName(fileName), targetDir);
            return fileName.Replace(Path.GetFileName(fileName), Path.GetFileNameWithoutExtension(fileName) +
                                    "_encrypted" + Path.GetExtension(fileName));
        }
        string TargetDecryptedFilePath(string entryFileName, string targetDir)
        {
            entryFileName = Path.Combine(targetDir, entryFileName);
            string sname = Path.GetFileNameWithoutExtension(entryFileName);
            sname = sname.Replace("_encrypted", "_decrypted");
            entryFileName = entryFileName.Replace(Path.GetFileNameWithoutExtension(entryFileName), sname);

            string tmpname = Path.Combine(Path.GetDirectoryName(entryFileName), Path.GetFileNameWithoutExtension(entryFileName));
            string ext = Path.GetExtension(entryFileName);
            string newname = tmpname + ext;
            int i = 1;
            while (File.Exists(newname))
            {
                tmpname += $" ({i++})" + ext;
                newname = tmpname;
                tmpname = Path.Combine(Path.GetDirectoryName(entryFileName), Path.GetFileNameWithoutExtension(entryFileName));
            }
            return newname;
        }
        public void Archivate(string fileName, string targetDir)
        {
            FileStream fileToEncrypt = null;
            try
            {
                string encryptedFileName = TargetEncryptedFilePath(fileName, targetDir);
                using (var memory = new MemoryStream())//here we can store our new zip archive for some time
                {
                    while (File.Exists(fileName)) //catching file from other thread
                    {
                        try
                        {
                            fileToEncrypt = new FileStream(fileName, FileMode.Open);
                        }
                        catch (IOException)
                        {
                            continue;
                        }
                        break;
                    }
                    if (fileToEncrypt == null)
                    {
                        throw new Exception("\nНевозможно получить доступ к удалённому файлу и закончить шифрование!");
                    }

                    using (var zip = new ZipArchive(memory, ZipArchiveMode.Create, true))
                    {
                        //move the whole encrypted thing to Memory Stream first
                        ZipArchiveEntry newEntry = zip.CreateEntry(Path.GetFileName(encryptedFileName));
                        using (Stream entryStream = newEntry.Open())
                        {
                            (new Encryptor()).Encrypt(fileToEncrypt, entryStream);
                        }
                    }

                    //checking for name availability
                    string tmpname = Path.Combine(targetDir, Path.GetFileNameWithoutExtension(fileName));
                    string ext = ".zip";
                    string newname = tmpname + ext;
                    int i = 1;
                    while (File.Exists(newname))
                    {
                        tmpname += $" ({i++})" + ext;
                        newname = tmpname;
                        tmpname = Path.Combine(targetDir, Path.GetFileNameWithoutExtension(fileName));
                    }
                    //recreating the new zip archive bringing it back from RAM
                    //by pushing all the bytes to an example in File System
                    using (var encryptedFS = new FileStream(Path.Combine(newname), FileMode.Create))
                    {
                        memory.Seek(0, SeekOrigin.Begin);
                        memory.CopyTo(encryptedFS);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogToAsync(ErrorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
            }
            finally
            {
                fileToEncrypt?.Dispose();
            }
        }
        public void Dearchivate(string fileName, string targetDir)
        {
            ZipArchive zip = null;
            try
            {
                while (File.Exists(fileName)) //catching file from other thread
                {
                    try
                    {
                        zip = ZipFile.OpenRead(fileName);
                    }
                    catch (IOException)
                    {
                        continue;
                    }
                    break;
                }
                if (zip == null)
                {
                    throw new Exception("\nНевозможно получить доступ к удалённому файлу и закончить шифрование!");
                }

                ZipArchiveEntry fileInZip = zip.Entries[0];
                string decryptedFileName = TargetDecryptedFilePath(fileInZip.Name, targetDir);

                using (var targetStream = new FileStream(decryptedFileName, FileMode.OpenOrCreate, FileAccess.Write))
                using (var zipEntryStream = fileInZip.Open())
                    (new Encryptor()).Decrypt(zipEntryStream, targetStream);
            }
            catch (Exception ex)
            {
                logger.LogToAsync(ErrorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
            }
            finally
            {
                zip?.Dispose();
            }
        }
    }
}
