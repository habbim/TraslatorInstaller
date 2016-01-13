using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GenerateBin
{
    class Program
    {
        public static int totalsize = 1;
        static void Main(string[] args)
        {
            if (Directory.Exists(@"./generatebin"))
            {
                Console.WriteLine("Generando el archivo data.bin ten paciencia ....");
                CreateTarGZ("data.bin", @"./generatebin");
                //EncryptFile("datau.bin", "data.bin", "UefJLJh2MBNysS8BRCiM", new byte[] { 0x07, 0x09, 0x03, 0x08, 0x10, 0x22, 0x19, 0x30 }, iterations);
                List();
                MessageBox.Show("Archivo data.bin generado correctamente Total Files: " + totalsize, "Tarea Completada", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                var sha1 = new SHA256Managed().ComputeHash(new FileStream(@"data.bin", FileMode.Open, FileAccess.Read));
                string convertSHA256 = BitConverter.ToString(sha1).Replace("-", String.Empty);
                Console.WriteLine("Total Files: " + totalsize+" Hash: "+convertSHA256);
                Console.ReadLine();
            }
            else
            {
                MessageBox.Show("No se ha encontrado la carpeta generatebin", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public static void List()
        {
            TarArchive ta = TarArchive.CreateInputTarArchive(new GZipStream(new FileStream(@"data.bin", FileMode.Open, FileAccess.Read), CompressionMode.Decompress));
            ta.ProgressMessageEvent += MyLister;
            ta.ListContents();
            ta.Close();
        }

        public static void MyLister(TarArchive ta, TarEntry te, string msg)
        {
            if (te.Size > 0)
            {
                Console.WriteLine(te.Name + " " + te.Size);
                totalsize = totalsize + 1;
            }
        }
        public const int iterations = 1042; // Recommendation is >= 1000.
        /// <summary>Encrypt a file.</summary>
        /// <param name="sourceFilename">The full path and name of the file to be encrypted.</param>
        /// <param name="destinationFilename">The full path and name of the file to be output.</param>
        /// <param name="password">The password for the encryption.</param>
        /// <param name="salt">The salt to be applied to the password.</param>
        /// <param name="iterations">The number of iterations Rfc2898DeriveBytes should use before generating the key and initialization vector for the decryption.</param>
        public static void EncryptFile(string sourceFilename, string destinationFilename, string password, byte[] salt, int iterations)
        {
            AesManaged aes = new AesManaged();
            aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
            aes.KeySize = aes.LegalKeySizes[0].MaxSize;
            // NB: Rfc2898DeriveBytes initialization and subsequent calls to   GetBytes   must be eactly the same, including order, on both the encryption and decryption sides.
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, salt, iterations);
            aes.Key = key.GetBytes(aes.KeySize / 8);
            aes.IV = key.GetBytes(aes.BlockSize / 8);
            aes.Mode = CipherMode.CBC;
            ICryptoTransform transform = aes.CreateEncryptor(aes.Key, aes.IV);

            using (FileStream destination = new FileStream(destinationFilename, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                using (CryptoStream cryptoStream = new CryptoStream(destination, transform, CryptoStreamMode.Write))
                {
                    using (FileStream source = new FileStream(sourceFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        source.CopyTo(cryptoStream);
                    }
                }
            }
        }
        static private void CreateTarGZ(string tgzFilename, string sourceDirectory)
        {
            Stream outStream = File.Create(tgzFilename);
            Stream gzoStream = new GZipOutputStream(outStream);
            TarArchive tarArchive = TarArchive.CreateOutputTarArchive(gzoStream);
            tarArchive.RootPath = sourceDirectory.Replace('\\', '/');
            if (tarArchive.RootPath.EndsWith("/"))
                tarArchive.RootPath = tarArchive.RootPath.Remove(tarArchive.RootPath.Length - 1);

            AddDirectoryFilesToTar(tarArchive, sourceDirectory, true);
            tarArchive.Close();
        }
        static private void AddDirectoryFilesToTar(TarArchive tarArchive, string sourceDirectory, bool recurse)
        {
            TarEntry tarEntry = TarEntry.CreateEntryFromFile(sourceDirectory);
            tarArchive.WriteEntry(tarEntry, false);
            string[] filenames = Directory.GetFiles(sourceDirectory);
            foreach (string filename in filenames)
            {
                tarEntry = TarEntry.CreateEntryFromFile(filename);
                tarArchive.WriteEntry(tarEntry, true);
            }

            if (recurse)
            {
                string[] directories = Directory.GetDirectories(sourceDirectory);
                foreach (string directory in directories)
                    AddDirectoryFilesToTar(tarArchive, directory, recurse);
            }
        }
    }
}
