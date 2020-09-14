using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.IO.Compression;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

using Microsoft.Win32;

namespace FTPDownload4
{
    class Program
    {
        public static string resourceDirectory = System.IO.Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"resourceFiles");
        //public const string baseFolder = @"C:\Users\Public\Documents\YouCam\TestDirectory";
        public static string baseFolder = System.IO.Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Downloads");
        public static string baseFolderChangeable;
        public static string downloadBatchFilePath = resourceDirectory + "\\" + "downloadBatch.bat";
        public static string uploadBatchFilePath = resourceDirectory + "\\" + "uploadBatch.bat";
        public static string uploadDirectoryBatchFilePath = resourceDirectory + "\\" + "uploadDirectoryBatch.bat";
        public static string fileExtensionsPath = resourceDirectory + "\\" + "file_extensions_sorted.txt";
        public static string configPath = resourceDirectory + "\\" + "config.txt";
        public static string sourceFolderPath = resourceDirectory + "\\" + "ftpSourceFolders.txt";
        public static string notificationDirectory = System.IO.Path.Combine(resourceDirectory, @"Notification");
        public static string[] fileExtensions; //{ "doc", "txt", "rar", "html", "jpg", "png", "exe", "gif", "js", "docx", "pdf", "ppt", "pptx"};
        public static string[] newFileExtensions = new string[500];
        public static string[] sourceFolders;
        public static int dictionaryFolderCounter;
        public static bool falseFolderDetected = false;
        public static int falseFolderCounter = 0;
        public static bool writeNewExtensions = false;
        public static bool connectionStatus;
        public static bool directoryRootRescue = false;
        public static bool directoryRootRescueExecuted = false;
        public static string ftpAddress;
        public static string ftpOnlyAddress;
        public static bool verboseOutput;
        public static bool runningStatus = true;
        public static bool infiniteFalseFolderDetected = false;
        public static string username, password;

        public static Dictionary<string, List<string>>[] fileFolderDictionaryArray = null;

        //Console window properties and functionalities - start

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int cmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        //Console window properties and functionalities - end

        //Time Interval

        static int interval = 0, startTime, elapsedTime;
        static long secondCounter;
        
        static void Main(string[] args)
        {
            checkUpdate();

            showNotification();
            
            System.IO.FileInfo fInfo1 = new System.IO.FileInfo(downloadBatchFilePath);
            System.IO.FileInfo fInfo2 = new System.IO.FileInfo(uploadBatchFilePath);
            System.IO.FileInfo fInfo3 = new System.IO.FileInfo(uploadDirectoryBatchFilePath);
            System.IO.FileInfo fInfo4 = new System.IO.FileInfo(fileExtensionsPath);
            System.IO.FileInfo fInfo5 = new System.IO.FileInfo(configPath);
            System.IO.FileInfo fInfo6 = new System.IO.FileInfo(sourceFolderPath);

            try
            {
                do
                {
                    try
                    {
                        fInfo1.IsReadOnly = false;
                        fInfo2.IsReadOnly = false;
                        fInfo3.IsReadOnly = false;
                        fInfo4.IsReadOnly = false;
                        fInfo5.IsReadOnly = false;
                        fInfo6.IsReadOnly = false;

                        secondCounter = 0;

                        readConfig();

                        if (runningStatus == true)
                        {
                            readConfig();

                            do
                            {
                                readConfig();

                                connectionStatus = checkConnection(ftpOnlyAddress, 10000);
                                Thread.Sleep(3000);

                                if (connectionStatus == false)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("No Connection\n");
                                    Console.ForegroundColor = ConsoleColor.Gray;
                                }
                            }
                            while (connectionStatus == false && runningStatus == true);
                            
                            if (connectionStatus == true && runningStatus == true)
                            {
                                int i;

                                fileExtensions = System.IO.File.ReadAllLines(fileExtensionsPath);
                                sourceFolders = System.IO.File.ReadAllLines(sourceFolderPath);

                                for (i = 0; i < fileExtensions.Length; i++)
                                {
                                    newFileExtensions[i] = fileExtensions[i];
                                }

                                fileFolderDictionaryArray = new Dictionary<string, List<string>>[1000];
                                initDictionary(fileFolderDictionaryArray);

                                string sourceFolderName;

                                foreach (string source in sourceFolders)
                                {
                                    readConfig();

                                    if (runningStatus == true)
                                    {
                                        directoryRootRescue = true;
                                        directoryRootRescueExecuted = false;

                                        dictionaryFolderCounter = 0;

                                        sourceFolderName = source;

                                        /*Console.Write("Source Folder Name: ");
                                        sourceFolderName = Console.ReadLine();*/

                                        //Console.Write("Destination Folder Name: ");

                                        baseFolderChangeable = baseFolder;

                                        if (sourceFolderName != null)
                                        {
                                            getFilesFolders(sourceFolderName, sourceFolderName, fileFolderDictionaryArray);
                                        }


                                        updateExtensionsRepository();


                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine("\n\nDownload Completed. Press any key to continue");
                                        Console.ForegroundColor = ConsoleColor.Gray;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }

                            fInfo1.IsReadOnly = true;
                            fInfo2.IsReadOnly = true;
                            fInfo3.IsReadOnly = true;
                            fInfo4.IsReadOnly = true;
                            fInfo5.IsReadOnly = true;
                            fInfo6.IsReadOnly = true;

                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("\nWaiting for the interval: " + interval.ToString() + " minutes\n");
                            Console.ForegroundColor = ConsoleColor.Gray;

                            startTime = DateTime.Now.Minute;

                            do
                            {
                                readConfig();

                                if (runningStatus == false)
                                {
                                    break;
                                }

                                if (DateTime.Now.Minute < startTime)
                                {
                                    elapsedTime = DateTime.Now.Minute + (60 - startTime);
                                }
                                else
                                {
                                    elapsedTime = DateTime.Now.Minute - startTime;
                                }

                                Thread.Sleep(5000);
                                secondCounter += 5;

                                if (secondCounter % 60 == 0)
                                {
                                    elapsedTime += (int)secondCounter / 60;
                                }
                            }
                            while (elapsedTime < interval);
                        }

                        readConfig();

                        if (runningStatus == false)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Stopped");
                            Console.ForegroundColor = ConsoleColor.Gray;

                            do
                            {
                                readConfig();
                                Thread.Sleep(1000);

                                if (runningStatus == true)
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("Resumed");
                                    Console.ForegroundColor = ConsoleColor.Gray;

                                    break;
                                }
                            }
                            while (true);
                        }
                    }
                    catch(Exception e)
                    {
                        //MessageBox.Show(e.Message,"FTPAutoSync");
                    }
                }
                while (true);
            }
            catch(Exception e)
            {
                //MessageBox.Show(e.Message + "\nFTP Auto Sync is shutting down. Something went bananas! Check your ftp connection!","FTPAutoSync");
                
                string restartPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/FTPAutoSync.exe";
                Process.Start(restartPath);
                Environment.Exit(1);
            }
            finally
            {
                
            }
        }

        public static void checkUpdate()
        {
            try
            {
                int i;
                string temp = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                char[] tempCharacterArray = new char[temp.Length - 25];

                for (i = 0; i < temp.Length - 25; i++)
                {
                    tempCharacterArray[i] = temp[i];
                }

                String path = new String(tempCharacterArray);
                string updatePath = path + "\\UpdateAgent\\UpdateAgent.exe";

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Path to Update file: " + updatePath);
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Gray;

                ProcessStartInfo pInfo = new ProcessStartInfo();
                pInfo.FileName = updatePath;
                pInfo.WindowStyle = ProcessWindowStyle.Hidden;

                Process p = new Process();
                p.StartInfo = pInfo;
                p.Start();
            }
            catch(Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Update files are missing. Please restore the update files to receive updates");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
            }
            
        }

        public static void showNotification()
        {
            ProcessStartInfo pInfo = new ProcessStartInfo();
            pInfo.FileName = notificationDirectory + @"\runNotification.bat";
            pInfo.Arguments = '"' + notificationDirectory + '"'; 
            pInfo.WindowStyle = ProcessWindowStyle.Hidden;

            Process p = new Process();
            p.StartInfo = pInfo;
            p.Start();
        }

        public static void readConfig()
        {
            var handle = GetConsoleWindow();

            int i;
            string[] temp = new string[5];

            temp = System.IO.File.ReadAllLines(configPath);

            ftpAddress = temp[0];

            char[] tempCharArray = new char[ftpAddress.Length - 7];

            for (i = 0; i < ftpAddress.Length - 1; i++)
            {
                if (i > 5)
                {
                    tempCharArray[i - 6] = ftpAddress[i];
                }
            }

            string tempString = new string(tempCharArray);
            ftpOnlyAddress = tempString;

            if (temp[1] == "False")
            {
                verboseOutput = false;
                ShowWindow(handle, SW_HIDE);
            }
            else
            {
                verboseOutput = true;
                ShowWindow(handle, SW_SHOW);
            }

            Int32.TryParse(temp[2], out interval);

            username = temp[3];
            password = temp[4];
        }   

        public static bool checkConnection(String address,int timeout)
        {
            try
            {
                Ping pingSender = new Ping();

                PingReply reply = pingSender.Send(address, timeout);

                Console.WriteLine(reply.Status.ToString() + "\n");

                if (reply.Status == IPStatus.Success)
                {
                    Console.ForegroundColor = ConsoleColor.Green;

                    Console.WriteLine("Address: {0}", reply.Address.ToString());

                    Console.WriteLine("RoundTripTime: {0}", reply.RoundtripTime);
                    Console.WriteLine("Time to live: {0}", reply.Options.Ttl);
                    Console.WriteLine("Don't Fragment: {0}", reply.Options.DontFragment);
                    Console.WriteLine("Buffer Size: {0}\n", reply.Buffer.Length);

                    Console.ForegroundColor = ConsoleColor.Gray;

                    return true;
                }
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            return false;
        }

        public static void initDictionary(Dictionary<string, List<string>>[] fileFolderDictionaryArray)
        {
            int i;

            for(i=0;i<fileFolderDictionaryArray.Length;i++)
            {
                fileFolderDictionaryArray[i] = new Dictionary<string, List<string>>();
            }
        }

        public static void getFilesFolders(string folderName, string parentFolder, Dictionary<string,List<string>>[] fileFolderDictionaryArray)
        {
            string destinationPath = null;

            readConfig();

            if(runningStatus == true)
            {
                Thread.Sleep(500);

                FtpWebRequest request = null;
                FtpWebResponse response = null;
                Stream stream = null;
                StreamReader reader = null;

                try
                {
                    string fileFolderName = null;
                    bool checkFolder;

                    string destinationDirectory = System.IO.Path.Combine(baseFolderChangeable, folderName);

                    try
                    {
                        //check if folder already exists at the destination directory
                        if (Directory.Exists(destinationDirectory) == true)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine(folderName + " already exists. Checking inside the folder.");
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                        else
                        {
                            createFolder(baseFolderChangeable, folderName);
                            Console.WriteLine(folderName + " folder has been created");
                        }
                        //end folder existence check
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("\nInside Folder Existence Check: Exception - " + ex.Message);

                        //Possibly files with "" extension - Invalid files are misclassified as folders
                        if (ex.Message.Contains("because a file or directory with the same name already exists"))
                        {
                            if(dictionaryFolderCounter>0)
                            {
                                dictionaryFolderCounter--;
                            }

                            return;
                        }

                        if(ex.Message.Contains("The specified path, file name, or both are too long."))
                        {
                            int i,j;
                            String tempFileName = null;
                            bool infiniteFolderFound = true;

                            if(dictionaryFolderCounter>0)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;

                                for(i=0;i<=3;i++)
                                {
                                    Dictionary<String, List<String>> tempDictionary = fileFolderDictionaryArray[dictionaryFolderCounter - i];

                                    foreach (var file in tempDictionary)
                                    {
                                        Console.WriteLine(i + " " + file.Key);

                                        if(tempFileName == null)
                                        {
                                            tempFileName = file.Key;
                                        }
                                        else
                                        {
                                            if(tempFileName != file.Key)
                                            {
                                                infiniteFolderFound = false;

                                                Console.WriteLine("\nInfinite Folder: False Alarm\n");

                                                infiniteFalseFolderDetected = true;

                                                Console.ForegroundColor = ConsoleColor.Gray;

                                                return;
                                            }
                                        }
                                    }
                                }

                                if (infiniteFolderFound == true)
                                {
                                    bool rootFound = false;
                                    String deletionPath = null;
                                    
                                    Console.WriteLine("\nInfinite Folder Found!\n");

                                    dictionaryFolderCounter--;

                                    for(i=0;(dictionaryFolderCounter-i)>=0;i++)
                                    {
                                        Dictionary<String, List<String>> tempDictionary = fileFolderDictionaryArray[dictionaryFolderCounter-i];

                                        foreach(var file in tempDictionary)
                                        {
                                            Console.WriteLine(i + " " + file.Key);

                                            if(tempFileName != file.Key)
                                            {
                                                //Console.WriteLine("Iteration over: " + file.Key);

                                                rootFound = true;
                                                break;
                                            }

                                            List<String>.Enumerator enumString = file.Value.GetEnumerator();
                                            enumString.MoveNext();
                                            deletionPath = enumString.Current + "\\" + tempFileName;
                                        }

                                        if(rootFound == true)
                                        {
                                            break;
                                        }
                                    }

                                    Console.WriteLine("\nDeletion Path: " + deletionPath + "\n");

                                    Directory.Delete(baseFolderChangeable + "\\" + deletionPath, true);

                                    Console.WriteLine("\nInfinite Folder Deleted\n");

                                    i-=2;

                                    //clean-up
                                    for (j = 0; j < i;j++)
                                    {
                                        fileFolderDictionaryArray[dictionaryFolderCounter--].Remove(tempFileName);
                                    }

                                    infiniteFalseFolderDetected = true;

                                    String ext = extractExtension(tempFileName);
                                    addExtension(ext);

                                    Console.WriteLine("\nAI Sequence Initiated \nAdding extension- " + "'" + ext + "'" + " to extension repository");

                                    Console.ForegroundColor = ConsoleColor.Gray;

                                    return;
                                }
                                
                                Thread.Sleep(10000);
                            }
                        }
                    }

                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("Opening: " + parentFolder);
                    Console.ForegroundColor = ConsoleColor.Gray;

                    try
                    {
                        reader = createConnection(ref request, ref response, ref stream, folderName);

                        directoryRootRescue = false;

                        while (reader != null && !reader.EndOfStream && directoryRootRescueExecuted == false && runningStatus == true)
                        {
                            readConfig();

                            fileFolderName = reader.ReadLine();

                            Thread.Sleep(100);

                            bool fileFolderExists = searchFileFolderVisited(fileFolderName, parentFolder, fileFolderDictionaryArray[dictionaryFolderCounter]);

                            if (fileFolderExists == false)
                            {
                                //Console.WriteLine(fileFolderName);

                                checkFolder = checkFolderInFTP(fileFolderName);

                                if (checkFolder == true)
                                {
                                    reader.Close();
                                    response.Close();

                                    List<string> parentFolderInList = new List<string>();

                                    addToDictionary(fileFolderDictionaryArray[dictionaryFolderCounter], fileFolderName, parentFolder);
                                    //fileFolderDictionaryArray[dictionaryFolderCounter].Add(fileFolderName, parentFolder);

                                    Console.WriteLine("FileFolder name: " + fileFolderName + " Parent Folder: " + parentFolder);

                                    dictionaryFolderCounter++;

                                    fileFolderName = folderName + "/" + fileFolderName;

                                    getFilesFolders(fileFolderName, fileFolderName, fileFolderDictionaryArray);

                                    reader = createConnection(ref request, ref response, ref stream, folderName);

                                    if (falseFolderDetected == true || infiniteFalseFolderDetected == true)
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("FileFolder name: " + fileFolderName + " Parent Folder: " + parentFolder);

                                    destinationPath = baseFolder + "\\" + folderName;
                                    string sourcePath = ftpAddress + folderName;

                                    string destinationFileFolderPath = System.IO.Path.Combine(destinationPath, fileFolderName);
                                    string sourceFileFolderPath = sourcePath + "/" + fileFolderName;

                                    //check existence and update 

                                    if (File.Exists(destinationFileFolderPath) == true)
                                    {
                                        //Update check - start

                                        try
                                        {
                                            Console.ForegroundColor = ConsoleColor.Yellow;
                                            Console.WriteLine(fileFolderName + " already exists at: " + destinationPath + "\nChecking date of modification");

                                            DateTime dtSource = checkFTPFileModificationDate(sourceFileFolderPath);
                                            DateTime dtDestination = File.GetLastWriteTime(destinationFileFolderPath);
                                            Console.WriteLine("\nSource: " + dtSource + "\nDestination: " + dtDestination);

                                            if (dtDestination <= dtSource)
                                            {
                                                Console.WriteLine("\nFile is obsolete. Starting update.");
                                                createFileUsingBatchFile(folderName, fileFolderName, destinationPath);
                                            }
                                            else
                                            {
                                                Console.WriteLine("\nFile is up to date.");
                                            }

                                            Console.ForegroundColor = ConsoleColor.Gray;
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine("Existing File Update Exception - " + e.Message);
                                        }

                                        //Update check - end
                                    }
                                    else if (File.Exists(destinationFileFolderPath) == false)
                                    {
                                        createFileUsingBatchFile(folderName, fileFolderName, destinationPath);
                                    }

                                    addToDictionary(fileFolderDictionaryArray[dictionaryFolderCounter], fileFolderName, parentFolder);
                                }
                            }
                        }

                        if (reader != null && directoryRootRescueExecuted == false)
                        {
                            reader.Close();
                            response.Close();

                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("Closing: " + parentFolder);
                            Console.ForegroundColor = ConsoleColor.Gray;

                            if (falseFolderDetected == false)
                            {
                                uploadNonExistentFiles(parentFolder);

                                dictionaryFolderCounter--;           //Going back to the previous level    
                            }

                            else if (falseFolderDetected == true)
                            {
                                deleteFalseLocalDirectory(folderName);
                                Thread.Sleep(100);

                                String falseFileName = extractFileName(folderName);

                                //Clearing up false dictionary entries - start

                                dictionaryFolderCounter--;
                                fileFolderDictionaryArray[dictionaryFolderCounter].Remove(falseFileName);
                                dictionaryFolderCounter--;
                                fileFolderDictionaryArray[dictionaryFolderCounter].Remove(falseFileName);

                                //Clearing up false dictionary entries - end

                                Console.WriteLine("\n\n" + "False File Name: " + falseFileName + "\n\n");

                                falseFolderDetected = false;         //Resetting
                            }

                            infiniteFalseFolderDetected = false; //Resetting
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Inside getFilesFolders - Exception 1: " + e.Message);

                        Console.WriteLine();
                        Console.WriteLine(e.StackTrace);
                        Console.WriteLine();

                        if(e.Message.Contains("The system cannot find the file specified"))
                        {
                            /*bool successfulDownload = false;

                            do
                            {
                                try
                                {
                                    Console.ForegroundColor = ConsoleColor.White;
                                    Console.WriteLine("Attempting again\n");
                                    Console.ForegroundColor = ConsoleColor.Gray;

                                    createFileUsingBatchFile(folderName, fileFolderName, destinationPath);

                                    successfulDownload = true;
                                }
                                catch(Exception localException)
                                {
                                    successfulDownload = false;
                                }
                                
                            }
                            while(successfulDownload == false);*/
                            
                            if(dictionaryFolderCounter > 0)
                            {
                                dictionaryFolderCounter--;
                            }
                        }

                        if (reader != null)
                        {
                            reader.Close();
                            response.Close();
                        }

                        if (e.Message == "The underlying connection was closed: An unexpected error occurred on a receive.")
                        {
                            Console.WriteLine("Anomaly has been taken care of. This may be device specific.");

                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("Closing: " + parentFolder);
                            Console.ForegroundColor = ConsoleColor.Gray;

                            if (dictionaryFolderCounter > 0)
                            {
                                dictionaryFolderCounter--;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("\nInside getFilesFolders - Exception 2: " + e.Message);
                }
            }
        }

        public static void addToDictionary(Dictionary<string, List<string>> fileFolderDictionary, string fileFolderName, string parentName)
        {
            bool fileFolderFound = false;
            List<string> tempList = new List<string>();

            foreach(KeyValuePair<string,List<string>> kvp in fileFolderDictionary)
            {
                if(kvp.Key == fileFolderName)
                {
                    kvp.Value.Add(parentName);
                    fileFolderFound = true;
                    break;
                }
            }

            if(fileFolderFound==false)
            {
                tempList.Add(parentName);
                fileFolderDictionary.Add(fileFolderName, tempList);
            }
        }

        public static bool searchFileFolderVisited(string folderName, string parentFolder, Dictionary<string,List<string>> fileFolderDictionary)
        {
            Thread.Sleep(100);

            foreach(var file in fileFolderDictionary)
            {
                //Console.ForegroundColor = ConsoleColor.Green;
                //Console.WriteLine("\nLooking for: " + folderName + " Whose parent is: " + parentFolder + " " + "Existing: " + file.Key.ToString() + " " + file.Value.ToString());
                //Console.ForegroundColor = ConsoleColor.Gray;

                if(file.Key==folderName)
                {
                    List<string> listValue = file.Value;

                    foreach(string parentFolderInList in listValue)
                    {
                        if(parentFolderInList == parentFolder)
                        {
                            //Console.ForegroundColor = ConsoleColor.Green;
                            //Console.WriteLine("HERE");
                            //Console.ForegroundColor = ConsoleColor.Gray;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static StreamReader createConnection(ref FtpWebRequest request,ref FtpWebResponse response,ref Stream stream, string folderName)
        {
            Thread.Sleep(2000);
            bool connectionEstablished = false;
            StreamReader reader = null;

            while(connectionEstablished==false)
            {
                readConfig();

                if(runningStatus == true)
                {
                    try
                    {
                        response = null;

                        do
                        {
                            request = (FtpWebRequest)WebRequest.Create(ftpAddress + folderName);
                            request.Timeout = 60000;
                            request.Credentials = new NetworkCredential(username, password);
                            request.KeepAlive = true;
                            request.Proxy = null;
                            request.UseBinary = true;
                            request.EnableSsl = false;

                            request.Method = WebRequestMethods.Ftp.ListDirectory;

                            //ServicePointManager.MaxServicePoints = 4;
                            //ServicePointManager.MaxServicePointIdleTime = 1000;

                            Console.WriteLine("\n    Going to Sleep");
                            Thread.Sleep(100);
                            Console.WriteLine("    Waking up");
                            Console.WriteLine();

                            response = (FtpWebResponse)request.GetResponse();

                            Console.WriteLine("\nResponse: " + response.StatusDescription);
                        }
                        while (response == null);

                        stream = response.GetResponseStream();
                        stream.ReadTimeout = 300000;
                        reader = new StreamReader(stream, true);

                        connectionEstablished = true;

                        return reader;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Inside Create Connection - Exception: " + e.Message);

                        if (e.Message.Contains("(550) File unavailable"))
                        {
                            if (directoryRootRescue == true)
                            {
                                Console.BackgroundColor = ConsoleColor.Red;
                                Console.WriteLine("Deletion detected at the root. Rescue is in progress.");
                                Console.BackgroundColor = ConsoleColor.Black;

                                string tempSourceDirectory = baseFolderChangeable + "\\" + folderName;

                                uploadDirectoryUsingBatchFile(tempSourceDirectory, ftpAddress, folderName);
                                uploadNonExistentFiles(folderName);

                                directoryRootRescueExecuted = true;
                            }
                            else
                            {
                                falseFolderDetected = true;
                                connectionEstablished = true;

                                falseFolderCounter++;

                                try
                                {
                                    String extractExt = extractExtension(folderName);

                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("\nAI Sequence Initiated \nAdding extension- " + "'" + extractExt + "'" + " to extension repository");
                                    Console.ForegroundColor = ConsoleColor.Gray;

                                    addExtension(extractExt);
                                }
                                catch (Exception e1)
                                {
                                    Console.WriteLine("Add Extensions: Message - " + e1.Message);
                                }

                                return null;
                            }
                        }

                        connectionEstablished = false;
                    }
                }
                Thread.Sleep(1000);
            }

            Console.WriteLine(reader.ToString());

            return reader;
        }

        public static bool checkFolderNotInFTP(string fileOrFolder)             //Not applicable for ftp due to permission issues
        {
            FileAttributes attr = File.GetAttributes(fileOrFolder);

            if((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool checkFolderInFTP(string fileOrFolder)            //string parsing for checking file extension
        {
            Thread.Sleep(100);

            int middle = 0, lo = 0, hi = fileExtensions.Length+falseFolderCounter;

            String tempExtension = extractExtension(fileOrFolder);

            Console.WriteLine("File Extension: " + tempExtension);

            while(lo<=hi)
            {
                middle = lo + (hi - lo) / 2;

                String tempExt = newFileExtensions[middle].ToString();

                if(tempExtension.CompareTo(tempExt)<0)
                {
                    hi = middle - 1;
                }
                else if(tempExtension.CompareTo(tempExt)>0)
                {
                    lo = middle + 1;
                }
                else
                {
                    Console.WriteLine("Is " + fileOrFolder + " Folder/File: File");

                    return false;
                }
            }

            Console.WriteLine("Is " + fileOrFolder + " Folder/File: Folder");

            return true;
        }

        public static void createFolder(string baseFolderChangeable, string subFolderName)
        {
            string pathString = System.IO.Path.Combine(baseFolderChangeable, subFolderName);

            System.IO.Directory.CreateDirectory(pathString);

            //string pathString1 = System.IO.Path.Combine(pathString, "Hi.html");

            //System.IO.File.Create(pathString1);
        }

        public static void createFileUsingBatchFile(string folderName,string fileName,string destination)
        {
            int exitCode;

            do
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Wait for download to complete ... ");
                Console.ForegroundColor = ConsoleColor.Gray;

                Thread.Sleep(500);

                ProcessStartInfo processInfo = new ProcessStartInfo();
                Process process = new Process();

                string argument = '"' + folderName + '"' + " " + '"' + fileName + '"' + " " + '"' + destination + '"' + " " + '"' + ftpOnlyAddress + '"' + " " + '"' + username + '"' + " " + '"' + password + '"';

                if(verboseOutput == true)
                {
                    processInfo.WindowStyle = ProcessWindowStyle.Normal;
                }
                else
                {
                    processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                }
                
                processInfo.FileName = downloadBatchFilePath;
                processInfo.Arguments = @argument;
                
                //processInfo.UseShellExecute = false;
                //processInfo.RedirectStandardError = true;
                //processInfo.RedirectStandardOutput = true;

                process.StartInfo = processInfo;
                process.Start();
                process.WaitForExit();

                Console.ForegroundColor = ConsoleColor.Red;
                /*while (process.StandardOutput.Peek() > -1)
                {
                    Console.WriteLine("Output: " + process.StandardOutput.ReadLine());
                }
                while (process.StandardError.Peek() > -1)
                {
                    Console.WriteLine("Error: " + process.StandardError.ReadLine());
                }*/
                exitCode = process.ExitCode;

                Console.WriteLine("Exit Code: " + exitCode);
                Console.ForegroundColor = ConsoleColor.Gray;

                Thread.Sleep(5000);

                process.Close();
            }
            while (exitCode != 0);

            Console.WriteLine("Download Completed: " + fileName);

            invalidNullExtensionCheck();
        }

        public static void uploadUsingBatchFile(string sourcePath,string destinationPath)
        {
            int exitCode;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Uploading: " + sourcePath + " to " + destinationPath);
            Console.ForegroundColor = ConsoleColor.Gray;

            try
            {
                do
                {
                    Thread.Sleep(500);

                    ProcessStartInfo processInfo = new ProcessStartInfo();
                    Process process = new Process();

                    string argument = '"' + sourcePath + '"' + " " + '"' + destinationPath + '"' + " " + '"' + ftpOnlyAddress + '"' + " " + '"' + username + '"' + " " + '"' + password + '"';

                    if (verboseOutput == true)
                    {
                        processInfo.WindowStyle = ProcessWindowStyle.Normal;
                    }
                    else
                    {
                        processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    }

                    processInfo.FileName = uploadBatchFilePath;
                    processInfo.Arguments = @argument;

                    process.StartInfo = processInfo;
                    process.Start();
                    process.WaitForExit();

                    exitCode = process.ExitCode;

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Uploading Exit Code: " + exitCode);
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                while (exitCode != 0);
            }
            catch(Exception e)
            {
                Console.WriteLine("Inside Upload Using Batch File - Exception - " + e.Message);
            }
        }

        public static void uploadDirectoryUsingBatchFile(string sourcePath,string destinationPath,string folderName)
        {
            int exitCode;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Uploading: " + folderName + " to " + destinationPath);
            Console.ForegroundColor = ConsoleColor.Gray;

            try
            {
                do
                {
                    ProcessStartInfo processInfo = new ProcessStartInfo();
                    Process process = new Process();

                    string argument = '"' + sourcePath + '"' + " " + '"' + destinationPath +'"' + " " + '"' + folderName +'"' + " " + '"' + ftpOnlyAddress + '"' + " " + username + " " + password;

                    if (verboseOutput == true)
                    {
                        processInfo.WindowStyle = ProcessWindowStyle.Normal;
                    }
                    else
                    {
                        processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    }

                    processInfo.FileName = uploadDirectoryBatchFilePath;
                    processInfo.Arguments = @argument;

                    process.StartInfo = processInfo;
                    process.Start();
                    process.WaitForExit();

                    exitCode = process.ExitCode;

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Directory Upload Exit Code: " + exitCode);
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                while (exitCode != 0);
            }
            catch(Exception e)
            {
                Console.WriteLine("Inside Upload Directory Using Batch File - Exception - " + e.Message);
            }
        }

        public static DateTime checkFTPFileModificationDate(string sourceFileFolderPath)
        {
            Thread.Sleep(1000);

            DateTime lastModifiedDate = new DateTime();
            bool dateFound = false;

            FtpWebRequest requestTimeCheck = null;
            FtpWebResponse responseTimeCheck = null;

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Inside Check FTP File Modification Date - " + "\nSource File Folder Path - " + sourceFileFolderPath);
            Console.ForegroundColor = ConsoleColor.Yellow;

            while(dateFound==false)
            {
                try
                {
                    requestTimeCheck = (FtpWebRequest)WebRequest.Create(sourceFileFolderPath);
                    requestTimeCheck.Credentials = new NetworkCredential(username, password);
                    requestTimeCheck.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                    requestTimeCheck.Timeout = 60000;

                    responseTimeCheck = (FtpWebResponse)requestTimeCheck.GetResponse();

                    lastModifiedDate = responseTimeCheck.LastModified;

                    

                    responseTimeCheck.Close();

                    dateFound = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Existing File Update Exception - " + e.Message);

                    responseTimeCheck.Close();

                    dateFound = false;
                }
            }
           
            return lastModifiedDate;
        }

        public static void uploadNonExistentFiles(string sourceFolderPath)
        {
            FtpWebRequest requestDirectoryCheck = null;
            FtpWebResponse responseDirectoryCheck = null;
            Stream responseStream = null;
            StreamReader readerDirectoryCheck = null;

            bool fileExists = false;
            
            string localDirectory = baseFolder + "\\" + sourceFolderPath;
            string ftpDirectory = ftpAddress + sourceFolderPath;

            List<string> localFilesFolders = new List<string>();
            List<string>ftpFiles = new List<string>();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Local: " + localDirectory + "\n\n\n" + "FTP: " + ftpDirectory);

            foreach (string fileName in Directory.GetFiles(localDirectory))
            {
                localFilesFolders.Add(Path.GetFileName(fileName));
            }
            //SUB DIRECTORIES
            foreach (string folderName in Directory.EnumerateDirectories(localDirectory))       //Previously used - Directory.GetFiles(path,pattern,boolean for including files inside subdirectories of the path) //Directory.EnumerateDirectories(path) enumerates files one by one while Directory.GetFiles(...) populates a complete string[] and then returns
            {
                localFilesFolders.Add(Path.GetFileName(folderName));
            }
            
            do
            {
                requestDirectoryCheck = (FtpWebRequest)WebRequest.Create(ftpDirectory);
                requestDirectoryCheck.Credentials = new NetworkCredential(username, password);
                requestDirectoryCheck.Method = WebRequestMethods.Ftp.ListDirectory;
                requestDirectoryCheck.Timeout = 60000;

                responseDirectoryCheck = (FtpWebResponse)requestDirectoryCheck.GetResponse();

                responseStream = responseDirectoryCheck.GetResponseStream();

                readerDirectoryCheck = new StreamReader(responseStream, true);

                while (!readerDirectoryCheck.EndOfStream && readerDirectoryCheck != null)
                {
                    ftpFiles.Add(readerDirectoryCheck.ReadLine());
                }
            }
            while (responseDirectoryCheck == null);

            //DisplayStart

            Console.WriteLine("\nLocal list: ");

            foreach(string item in localFilesFolders)
            {
                Console.WriteLine(item);
            }

            Console.WriteLine("\nFtp list: ");

            foreach(string item in ftpFiles)
            {
                Console.WriteLine(item);
            }

            Console.ForegroundColor = ConsoleColor.Gray;

            //DisplayEnd

            //CheckingStart

            foreach(string localItem in localFilesFolders)
            {
                fileExists = false;

                foreach(string ftpItem in ftpFiles)
                { 
                    if(localItem==ftpItem)
                    {
                        fileExists = true;
                        break;
                    }
                }

                if(fileExists==false)
                {
                    string localFileFolderPath = System.IO.Path.Combine(localDirectory,localItem);
                    string nestedPath = sourceFolderPath + "/" + localItem;

                    if(checkFolderInFTP(localItem)==false)
                    {
                        uploadUsingBatchFile(localFileFolderPath, sourceFolderPath);
                    }
                    else
                    {
                        uploadDirectoryUsingBatchFile(localFileFolderPath, sourceFolderPath, localItem);

                        Console.BackgroundColor = ConsoleColor.Blue;
                        Console.WriteLine("Nested Path: " + nestedPath + "\n\n\n" + "Source Path: " + sourceFolderPath);
                        Console.BackgroundColor = ConsoleColor.Black;

                        dictionaryFolderCounter++;
                        getFilesFolders(nestedPath,nestedPath,fileFolderDictionaryArray);
                    }
                }
            }

            //CheckingEnd

            responseDirectoryCheck.Close();
            responseStream.Close();
            readerDirectoryCheck.Close();
        }

        public static void deleteFalseLocalDirectory(string sourcePath)
        {
            string deletePath = System.IO.Path.Combine(baseFolder, sourcePath);

            Directory.Delete(deletePath, true);
        }

        public static String extractExtension(string sourcePath)
        {
            int i, j, lastSlash=0, dotPosition=0;
            char[] tempExt = new char[100];

            for (i = 0; i < 100; i++)
            {
                tempExt[i] = '\0';
            }

            for (i = 0; i < sourcePath.Length; i++)
            {
                if(sourcePath[i]=='\\' || sourcePath[i]=='/')
                {
                    lastSlash = i + 1;

                }
            }

            for (i = lastSlash; i < sourcePath.Length;i++)
            {
                if(sourcePath[i]=='.')
                {
                    dotPosition = i + 1;
                }
            }

            char[] tempExtShortened = new char[sourcePath.Length - dotPosition];

            if(dotPosition!=0)
            {
                for (i = dotPosition, j = 0; i < sourcePath.Length; i++, j++)
                {
                    tempExtShortened[j] = sourcePath[i];
                }
            }

            try
            {
                String extractExt = new String(tempExtShortened);

                return extractExt;
            }
            catch (Exception e1)
            {
                Console.WriteLine("Extract Extensions: Message - " + e1.Message);

                return null;
            }
        }

        public static void addExtension(string extractedExtension)
        {
            Thread.Sleep(100);

            int i = 0, j = 0, middle = 0, lo = 0, hi = fileExtensions.Length+falseFolderCounter, length = fileExtensions.Length+falseFolderCounter;

            while (lo <= hi)
            {
                middle = lo + (hi - lo) / 2;

                String tempExt = newFileExtensions[middle].ToString();

                if (extractedExtension.CompareTo(tempExt) < 0)
                {
                    hi = middle - 1;
                }
                else if (extractedExtension.CompareTo(tempExt) > 0)
                {
                    lo = middle + 1;
                }
                else
                {
                    //will never reach here 
                }
            }

            if(extractedExtension.CompareTo(newFileExtensions[middle].ToString())>0)
            {
                for(i=length-1;i>middle;i--)
                {
                    newFileExtensions[i + 1] = newFileExtensions[i];
                }

                newFileExtensions[middle + 1] = extractedExtension.TrimEnd('\0').ToString();

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Extension: " + extractedExtension + " added after " + newFileExtensions[middle]);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else if(extractedExtension.CompareTo(newFileExtensions[middle].ToString())<0)
            {
                for(i=length-1;i>=middle;i--)
                {
                    newFileExtensions[i + 1] = newFileExtensions[i];
                }

                newFileExtensions[middle] = extractedExtension.TrimEnd('\0').ToString();

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Extension: " + extractedExtension + " added before " + newFileExtensions[middle+1]);
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            writeNewExtensions = true;
        }

        public static void invalidNullExtensionCheck()
        {
            int i;

            Console.WriteLine("\nInvalid Null Extension Check\n");

            if (newFileExtensions[0].Equals(""))
            {
                Console.WriteLine("\nInvalid Found\n");

                for (i = 0; i < fileExtensions.Length + falseFolderCounter - 1; i++)
                {
                    newFileExtensions[i] = newFileExtensions[i + 1];
                }

                falseFolderCounter = falseFolderCounter - 1;
            }
        }

        public static String extractFileName(string sourcePath)
        {
            int i, j, lastSlash=-1;
            char[] tempFileName = new char[100];

            for (i = 0; i < 100;i++)
            {
                tempFileName[i] = '\0';
            }

            for (i = 0; i < sourcePath.Length; i++)
            {
                if(sourcePath[i]=='\\' || sourcePath[i]=='/')
                {
                    lastSlash = i + 1;
                }
            }

            char[] tempFileNameShortened = new char[sourcePath.Length - lastSlash];

            for (i = lastSlash, j = 0; i < sourcePath.Length; i++, j++)
            {
                tempFileNameShortened[j] = sourcePath[i];
            }

            String fileName = new String(tempFileNameShortened);

            return fileName;
        }

        public static void updateExtensionsRepository()
        {
            int i;

            if (writeNewExtensions == true)
            {
                using (StreamWriter file = new StreamWriter(fileExtensionsPath))
                {
                    for (i = 0; i < fileExtensions.Length + falseFolderCounter; i++)
                    {
                        if(i==fileExtensions.Length+falseFolderCounter-1)
                        {
                            file.Write(newFileExtensions[i]);
                        }
                        else
                        {
                            file.WriteLine(newFileExtensions[i]);
                        }
                    }
                }

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Extensions repository updated");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
    }
}
