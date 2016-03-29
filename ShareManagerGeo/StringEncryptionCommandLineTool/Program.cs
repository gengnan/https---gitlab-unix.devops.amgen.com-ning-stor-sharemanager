using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace StringEncryptionCommandLineTool
{
    class Program
    {
        static int Main(string[] args)
        {
            string filepath = "";
            string paramstring = "";
            string returnstring = "";
            bool done = false;
            int param = 0;

            if (args.Length == 0)
            {
                PrintHelp();
                return 1;
            }

            if (args[0].ToLower() == "-file")
            {
                filepath = args[1];
                param = 1;
            }
            else if (args[0].ToLower() == "-decrypt" && !string.IsNullOrEmpty(args[1]))
            {
                returnstring = DecryptString(args[1]);
                done = true;
            }

            if (!done && args[param + 1].ToLower() == "-encrypt")
            {
                paramstring = args[param + 2];
                returnstring = EncryptString(paramstring, filepath);
                done = true;
            }

            if (done)
            {
                Console.WriteLine("{0}", returnstring);
                return 0;
            }
            else
            {
                Console.WriteLine("Unknown Error");
                return 0;
            }

            
        }

        private static string EncryptString(string plainText, string certFile)
        {
            Console.WriteLine("Beginning Encryption for {0}", plainText);
            EnvelopedCms cms = new EnvelopedCms(new ContentInfo(Encoding.UTF8.GetBytes(plainText)));
            cms.Encrypt(new CmsRecipient(new X509Certificate2(certFile)));
            Console.WriteLine("Encryption Complete");
            return Convert.ToBase64String(cms.Encode());

        }

        private static string DecryptString(string encryptedText)
        {
            EnvelopedCms cms = new EnvelopedCms();
            cms.Decode(Convert.FromBase64String(encryptedText));
            cms.Decrypt();
            return Encoding.UTF8.GetString(cms.ContentInfo.Content);
        }

        private static void PrintHelp()
        {
            Console.WriteLine("NAME\n\tStringEncrypt.exe\n");
            Console.WriteLine("SYNOPSIS\n\tEncrypts a string using a public key file (*.cer) or Decrypts a string using a Private key in the Windows Certstore.\n");
            Console.WriteLine("--------- ENCRYPTION EXAMPLE ---------");
            Console.WriteLine("C:\\PS>StringEncrypt.exe -file <path to .cer> -encrypt '<string>' > outputfile.txt\n\n");
            Console.WriteLine("--------- DECRYPTION EXAMPLE ---------");
            Console.WriteLine("C:\\PS>StringEncrypt.exe -decrypt '<string>' > outputfile.txt\n\n");
        }
    }
}
