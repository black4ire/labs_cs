using System;
using System.IO.Compression;
using System.IO;
using System.Configuration;

namespace M_Service
{
    public class Archivator
    {
        
        readonly object Obect = new object(); 
        string ErrorLogPath { get; } = ConfigurationManager.AppSettings["ErrorLogPath"];
        public Archivator()
        {
            try
            {
                ErrorLogPath = (new Manager()).GetOptions<Logs>().ErrorLog;
            }
            catch (Exception ex)
            {
                lock (Obect)
                {
                    string errorLogPath = ErrorLogPath;
                    if (File.Exists(errorLogPath))
                    {
                        File.AppendAllText(errorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
                    }
                }
            }
        }
        string PathEncryptedFile(string fileName, string targetDir)
        {
            fileName = fileName.Replace(Path.GetDirectoryName(fileName), targetDir);
            return fileName.Replace(Path.GetFileName(fileName), Path.GetFileNameWithoutExtension(fileName) + 
                                    "_encrypted" + Path.GetExtension(fileName));
        }
        string PathDecryptedFile(string fileNameRecording, string targetDir)
        {
            fileNameRecording = Path.Combine(targetDir, fileNameRecording);
            string strname = Path.GetFileNameWithoutExtension(fileNameRecording);
            strname = strname.Replace("_encrypted", "_decrypted");
            fileNameRecording = fileNameRecording.Replace(Path.GetFileNameWithoutExtension(fileNameRecording), strname);

            string tmpname = Path.Combine(Path.GetDirectoryName(fileNameRecording), Path.GetFileNameWithoutExtension(fileNameRecording));
            string ext = Path.GetExtension(fileNameRecording);
            string newName = tmpname + ext;
            int i = 1;
            while (File.Exists(newName))
            {
                tmpname += $" ({i++})" + ext;
                newName = tmpname;
                tmpname = Path.Combine(Path.GetDirectoryName(fileNameRecording), Path.GetFileNameWithoutExtension(fileNameRecording));
            }
            return newName;
        }
        public void Archivate(string fileName, string targetDir)
        {
            FileStream fileToEncrypt = null;
            try
            {
                string fileNameEncrypted = PathEncryptedFile(fileName, targetDir);
                using (var memory = new MemoryStream())//хранение zip архива
                {
                    while (File.Exists(fileName)) 
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
                        throw new Exception("\nНевозможно получить доступ к удалённому файлу!");
                    }

                    using (var zip = new ZipArchive(memory, ZipArchiveMode.Create, true))
                    {
                        //перемещение защифрованного в поток памяти
                        ZipArchiveEntry newEntry = zip.CreateEntry(Path.GetFileName(fileNameEncrypted));
                        using (Stream entryStream = newEntry.Open())
                        {
                            (new Encryptor()).Encrypt(fileToEncrypt, entryStream);
                        }
                    }

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

                    using (var encryptedFS = new FileStream(Path.Combine(newname), FileMode.Create))
                    {
                        memory.Seek(0, SeekOrigin.Begin);
                        memory.CopyTo(encryptedFS);
                    }
                }
            }
            catch (Exception ex)
            {
                lock (Obect)
                {
                    string errorLogPath = ErrorLogPath;
                    if (File.Exists(errorLogPath))
                    {
                        File.AppendAllText(errorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
                    }
                }
            }
            finally
            {
                fileToEncrypt?.Dispose();
            }
        }
        public void Dearchivate(string fileName, string targetDir)
        {
            ZipArchive zipObj = null;
            try
            {
                while (File.Exists(fileName)) 
                {
                    try
                    {
                        zipObj = ZipFile.OpenRead(fileName);
                    }
                    catch (IOException)
                    {
                        continue;
                    }
                    break;
                }
                if (zipObj == null)
                {
                    throw new Exception("\nНевозможно получить доступ к удалённому файлу!");
                }

                ZipArchiveEntry fileInZip = zipObj.Entries[0];
                string fileNameDecrypted = PathDecryptedFile(fileInZip.Name, targetDir);

                using (var targetStream = new FileStream(fileNameDecrypted, FileMode.OpenOrCreate, FileAccess.Write))
                using (var entryZipStream = fileInZip.Open())
                    (new Encryptor()).Decrypt(entryZipStream, targetStream);
            }
            catch (Exception ex)
            {
                lock (Obect)
                {
                    string errorLogPath = ErrorLogPath;
                    if (File.Exists(errorLogPath))
                    {
                        File.AppendAllText(errorLogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
                    }
                }
            }
            finally
            { 
                zipObj?.Dispose(); 
            }
        }
    }
}
