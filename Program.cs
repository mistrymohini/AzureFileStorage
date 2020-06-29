using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AzureFileStorage
{
    class Program
    {
        static void Main(string[] args)
        {
            //file name stored on azure
            string filename ="Azureupload.pdf" ;

            // give share name always in lowercase
            string share_name = "xyz";

            //path of folder where need to store files(could be single or nested,if nested use '/' delimeter)
            string filepath = "My Document/PDF";

            //get files from root directory
            string[] Documents = System.IO.Directory.GetFiles("../../PDF/");
            Console.Write("Select option 1 for upload file and 2 for download:");
            int input = (int)Convert.ToInt32(Console.ReadLine());
            switch(input)
            {
                case 1:
                    //read all file from directory
                    foreach (string path in Documents)
                    {
                        //get extension of file
                        string Extension = System.IO.Path.GetExtension(path).ToLower();
                        if (Extension == ".pdf")
                        {
                            //create stream object to get file and store file into this object
                            var fileStream = new FileStream(path, FileMode.Open);
                            //pass all parameter to store file on Azure FileStorage
                            UploadFile(fileStream, filename, filepath, share_name);
                        }
                    }
                    break;
                case 2:
                    DownloadFile(share_name + "/" + filepath + "/" + filename);
                    break;
                default:
                    Console.WriteLine("Choose proper option from 1 or 2.");
                    break;
            }
           
            
        }
        public static void UploadFile(Stream streamfilename, string filename, string FolderPath, string ShareName)
        {
            
            //read connection string to store data on azure from config file with account name and key
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudFileClient cloudFileClient = cloudStorageAccount.CreateCloudFileClient();

            //GetShareReference() take reference from cloud
            CloudFileShare cloudFileShare = cloudFileClient.GetShareReference(ShareName);
            //Create share name if not exist
            cloudFileShare.CreateIfNotExists();

            //get all directory reference located in share name
            CloudFileDirectory cloudFileDirectory = cloudFileShare.GetRootDirectoryReference();

            //Specify the nested folder
            var nestedFolderStructure = FolderPath;
            var delimiter = new char[] { '/' };
            //split all nested folder by delimeter
            var nestedFolderArray = nestedFolderStructure.Split(delimiter);
            for (var i = 0; i < nestedFolderArray.Length; i++)
            {
                //check directory avilability if not exist then create directory
                cloudFileDirectory = cloudFileDirectory.GetDirectoryReference(nestedFolderArray[i]);
                cloudFileDirectory.CreateIfNotExists();
            }
            ////create object of file reference and get all files within directory
            CloudFile cloudFile = cloudFileDirectory.GetFileReference(filename);

            //upload file on azure
            cloudFile.UploadFromStream(streamfilename);

            Console.WriteLine("File uploaded sucessfully");
            Console.ReadLine();
        }

        public static void DownloadFile(string path)
        {
            var value = path;
            if (value != null && value != "")
            {
                string[] fname = value.Split('/');
                string foldername = "";
                int count = 0;
                for (int i = fname.Length - 2; i <= (fname.Length - 2); i--)
                {
                    if (i != 0)
                    {
                        count++;
                        foldername += fname[count] + '/';
                    }
                    else
                        break;
                }
                //get share name
                string ShareName = fname[0].ToLower();
                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudFileClient cloudFileClient = cloudStorageAccount.CreateCloudFileClient();
                CloudFileShare cloudFileShare = cloudFileClient.GetShareReference(ShareName);
                CloudFileDirectory root = cloudFileShare.GetRootDirectoryReference();
                CloudFileDirectory directoryToUse = root.GetDirectoryReference(foldername);
                CloudFile cloudFile = directoryToUse.GetFileReference(fname.Last());
                //checking for file exist on directory or not
                if (directoryToUse.Exists())
                {
                    //if yes store it to local path of your project with given file name
                    var memStream = new MemoryStream();
                    using (var fileStream = System.IO.File.OpenWrite(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Download.pdf")))
                    {
                        cloudFile.DownloadToStream(memStream);
                    }
                    Console.WriteLine("File saved in {0}", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Download.pdf"));
                    Console.ReadLine();
                }
                else
                {
                    Console.WriteLine("File not exist on Azure.");
                }
            }
        }
    }
}
