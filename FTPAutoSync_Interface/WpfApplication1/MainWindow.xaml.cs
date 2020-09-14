using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;

using Microsoft.Win32.TaskScheduler;
using System.Net;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string codeBase = Assembly.GetExecutingAssembly().CodeBase;
        public static string currentDirectory = System.IO.Path.GetDirectoryName(codeBase);

        public static string folderNamesRetrieved = null;
        public static string[] configRetrieved = new string[3];
        public static string ftpAddressFilePath = null;
        public static string ftpAddressFilePathLocal = null;
        public static string folderNamesFilePath = null;
        public static string folderNamesFilePathLocal = null;
        public static string modifiedCurrentDirectory = null;
        public static String versionConfigDirectory = null;
        public static string username, password;

        public static bool verboseCheckBool;

        public static int previousLength;
        public static bool firstEdit;

        public static bool runningStatus;

        public static string interval;

        public static string applicationPath;
        public static string applicationExecutionPath;

        public static string greenImagePath;
        public static string redImagePath;

        public static string versionConfigPath = null;
        public static string versionConfigPathLocal = null;
        public static string version = null;

        int i, j, k, length = currentDirectory.Length;
        char[] temp = null;
        char[] temp1 = null;

        Thread th = null;

        System.IO.FileInfo fInfo1;
        System.IO.FileInfo fInfo2;

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                //MessageBox.Show(codeBase);

                temp = new char[length - 4];
                const string replacement = "Release";

                for (i = 0; i < length; i++)
                {
                    if (i > 5)
                    {
                        temp[i - 6] = currentDirectory[i];
                    }

                    if (i == length - 5)
                    {
                        for (j = 0; j < 7; j++, i++)
                        {
                            temp[i - 6] = replacement[j];
                        }
                    }
                }

                modifiedCurrentDirectory = new string(temp);

                //MessageBox.Show(modifiedCurrentDirectory);

                temp1 = new char[currentDirectory.Length - 29];

                for (i = 0; i < (currentDirectory.Length - 23); i++)
                {
                    if (i > 5)
                    {
                        temp1[i - 6] = currentDirectory[i];
                    }
                }

                try
                {
                    String versionConfigDirectoryTemp = new String(temp1);

                    versionConfigDirectory = System.IO.Path.Combine(versionConfigDirectoryTemp, "UpdateAgent");

                    //MessageBox.Show(currentDirectory);
                    //MessageBox.Show(versionConfigDirectory);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.StackTrace);
                }

                ftpAddressFilePath = modifiedCurrentDirectory + @"\resourceFiles\config.txt";
                ftpAddressFilePathLocal = new Uri(ftpAddressFilePath).LocalPath;

                folderNamesFilePath = modifiedCurrentDirectory + @"\resourceFiles\ftpSourceFolders.txt";
                folderNamesFilePathLocal = new Uri(folderNamesFilePath).LocalPath;

                versionConfigPath = versionConfigDirectory + @"\config.txt";
                versionConfigPathLocal = new Uri(versionConfigPath).LocalPath;

                folderNamesRetrieved = System.IO.File.ReadAllText(folderNamesFilePathLocal);
                folderNames.Text = folderNamesRetrieved;

                greenImagePath = System.IO.Path.Combine(currentDirectory + @"\green.jpg");
                redImagePath = System.IO.Path.Combine(currentDirectory + @"\red.jpg");

                //MessageBox.Show("Version Config Path Local: " + versionConfigPathLocal);

                try
                {
                    String[] lines = System.IO.File.ReadAllLines(versionConfigPathLocal);

                    version = lines[1];
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                fInfo1 = new System.IO.FileInfo(ftpAddressFilePath);
                fInfo2 = new System.IO.FileInfo(folderNamesFilePath);

                try
                {
                    configRetrieved = System.IO.File.ReadAllLines(ftpAddressFilePathLocal);
                    ftpAddress.Text = configRetrieved[0];

                    previousLength = folderNamesRetrieved.Length;

                    if (configRetrieved[1] == "False")               //verbose output
                    {
                        verboseCheckbox.IsChecked = false;
                        verboseCheckBool = false;
                    }
                    else
                    {
                        verboseCheckbox.IsChecked = true;
                        verboseCheckBool = true;
                    }

                    Process[] ftpAutoSyncProcess = Process.GetProcessesByName("FTPAutoSync");

                    if (ftpAutoSyncProcess.Length > 0)
                    {

                        statusImage.Source = new BitmapImage(new Uri(greenImagePath));
                    }
                    else
                    {
                        statusImage.Source = new BitmapImage(new Uri(redImagePath));
                    }

                    firstEdit = true;

                    interval = configRetrieved[2];

                    intervalTextBox.Text = interval;

                    username = configRetrieved[3];
                    password = configRetrieved[4];

                    if (username == "" || username == " ")
                    {
                        username = "anonymous";
                    }
                    if (password == "" || password == " ")
                    {
                        password = "abc@abc.com";
                    }

                    usernameTextBox.Text = username;
                    passwordTextBox.Text = password;
                }
                catch (IndexOutOfRangeException e)
                {
                    MessageBox.Show("config file: corrupted\nInitiating automatic repair\nResetting configuration");

                    string repairText = "ftp://0.0.0.0/" + "\r\nTrue\r\n3\nanonymous\abc@abc.com";

                    System.IO.File.WriteAllText(ftpAddressFilePathLocal, repairText);
                }

                //updateRegistry();

                //Running Status Checking Thread
                runCheck r = new runCheck(statusImage, redImagePath, greenImagePath);
                th = new Thread(r.DoWork);

                th.IsBackground = true;
                th.Start();
                //Running Status Checking

                //.NET Framework Version 4.5 Check
                if (!NET45Availability())
                {
                    try
                    {
                        MessageBox.Show(".NET Framework Version 4.5 has not been found in your system. .NET Framework Version 4.5 will now be installed.", "FTPAutoSync Configure");
                        Process.Start(modifiedCurrentDirectory + "\\resourceFiles\\dotNetFx45_Full_setup.exe");
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.ToString());
                    }
                }
                //.NET Framework Version 4.5 Check

                //Task Scheduler Entry Check
                if (!isTaskRegistered())
                {
                    registerTask();
                    MessageBox.Show("Task Registered!", "FTPAutoSync Configure");
                }
                //Task Scheduler Entry Check
            }
            catch(Exception e)
            {
                MessageBox.Show(e.StackTrace);
            }
        }

        public void registerTask()
        {
            int i;

            try
            {
                using (TaskService ts = new TaskService())
                {
                    TaskDefinition td = ts.NewTask();
                    td.RegistrationInfo.Author = "FTPAutoSync";
                    td.RegistrationInfo.Description = "Syncs files from the specified folder in the ftp to the local system";

                    td.Principal.RunLevel = TaskRunLevel.Highest;

                    td.Settings.AllowDemandStart = true;
                    td.Settings.Enabled = true;
                    td.Settings.Priority = ProcessPriorityClass.High;
                    td.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;
                    td.Settings.AllowHardTerminate = true;
                    td.Settings.DisallowStartIfOnBatteries = false;
                    td.Settings.Hidden = false;
                    td.Settings.StopIfGoingOnBatteries = false;
                    td.Settings.RunOnlyIfIdle = false;

                    string osVal = getOS();

                    if(osVal.Contains("Windows XP"))
                    {
                        td.Settings.Compatibility = TaskCompatibility.V1;
                    }
                    else if(osVal.Contains("Windows Vista"))
                    {
                        td.Settings.Compatibility = TaskCompatibility.V2;
                    }
                    else if (osVal.Contains("Windows 7"))
                    {
                        td.Settings.Compatibility = TaskCompatibility.V2_1;
                    }
                    else if (osVal.Contains("Windows 8"))
                    {
                        td.Settings.Compatibility = TaskCompatibility.V2_2;
                    }
                    else if (osVal.Contains("Windows 8.1"))
                    {
                        td.Settings.Compatibility = TaskCompatibility.V2_2;
                    }
                    else if (osVal.Contains("Windows 10"))
                    {
                        td.Settings.Compatibility = TaskCompatibility.V2_3;
                    }
                    else
                    {
                        td.Settings.Compatibility = TaskCompatibility.V2_3;
                    }

                    for (i = 0; i <= 23; i++)
                    {
                        DailyTrigger dt = new DailyTrigger();
                        dt.StartBoundary = DateTime.Today + TimeSpan.FromHours(i);
                        dt.DaysInterval = 1;

                        td.Triggers.Add(dt);
                    }

                    td.Actions.Add(new ExecAction(modifiedCurrentDirectory + "\\FTPAutoSync.exe"));

                    const string taskName = "FTPAutoSync";
                    ts.RootFolder.RegisterTaskDefinition(taskName, td);
                }
            }
            catch(Exception e)
            {
                MessageBox.Show("Run as Administrator");
                Environment.Exit(1);
            }
        }

        public string getOS()
        {
            RegistryKey rk;
            string registryPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\";
            string value = null;

            try
            {
                if (Environment.Is64BitOperatingSystem)        //64-bit
                {
                    rk = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
                }
                else
                {
                    rk = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32);
                }

                rk = rk.OpenSubKey(registryPath, true);

                if (rk != null)
                {
                    value = rk.GetValue("ProductName").ToString();
                }
            }
            catch(Exception e)
            {
                MessageBox.Show("Run as administrator");
                Environment.Exit(1);
            }

            return value;
        }

        public void updateRegistry()
        {
            RegistryKey rk;
            string registryPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\";
            string value = null;

            try
            {
                if (Environment.Is64BitOperatingSystem)        //64-bit
                {
                    rk = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
                }
                else
                {
                    rk = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32);
                }

                rk = rk.OpenSubKey(registryPath, true);

                if (rk != null)
                {
                    value = rk.GetValue("Userinit").ToString();
                }

                applicationExecutionPath = modifiedCurrentDirectory + "\\FTPAutoSync.exe";
                applicationPath = modifiedCurrentDirectory + "\\FTPAutoSync.exe,";

                if (!value.Contains(applicationPath))
                {
                    string newRegistryValue = value + applicationPath;

                    rk.SetValue("Userinit", newRegistryValue);

                    MessageBox.Show("Registry entry completed. Path - " + applicationPath);
                }
                else
                {
                    MessageBox.Show("Path already exists");
                }
            }
            catch (System.Security.SecurityException e)
            {
                MessageBox.Show("Please run the setup file as administrator");
                System.Environment.Exit(1);
            }
        }

        public void carryOutSubmission(bool reSubmission)
        {
            if(reSubmission == false)
            {
                //Anonymous Check Thread
                anonymousCheck aCheck = new anonymousCheck(this, "NoButton", ftpAddress.Text, usernameTextBox.Text, passwordTextBox.Text);
                Thread th = new Thread(aCheck.DoWork);

                th.IsBackground = true;
                th.Start();
                //Anonymous Check Thread
            }

            fInfo1.IsReadOnly = false;
            fInfo2.IsReadOnly = false;

            if (ftpAddress.Text == null)
            {
                //MessageBox.Show("Please enter an ftp address");
            }
            else if (folderNames.Text == null)
            {
                //MessageBox.Show("Please enter folder names");
            }

            int i, slashCounter = 0;

            for (i = 0; i < ftpAddress.Text.Length; i++)
            {
                if (ftpAddress.Text[i] == '/' && i > 5)
                {
                    slashCounter++;
                }
            }

            char[] tempCharArrayLocal = new char[ftpAddress.Text.Length - slashCounter];

            for (i = 0, slashCounter = 0; i < ftpAddress.Text.Length; i++)
            {
                if (ftpAddress.Text[i] == '/' && i > 5)
                {
                    slashCounter++;
                }
                if (slashCounter < 1)
                {
                    tempCharArrayLocal[i] = ftpAddress.Text[i];
                }
            }

            string slashRemovalString = new string(tempCharArrayLocal);

            string ftpText = null;

            if (intervalTextBox.Text != "")
            {
                int temp;

                Int32.TryParse(intervalTextBox.Text, out temp);

                if (temp < 60)
                {
                    interval = intervalTextBox.Text;
                }
                else
                {
                    if(reSubmission == false)
                    {
                        MessageBox.Show("Interval has to be between 0 and 59");
                    }

                    intervalTextBox.Text = interval;
                }
            }

            if (!ftpAddress.Text.Contains("ftp://"))
            {
                ftpText = "ftp://" + slashRemovalString + "/\r\n" + verboseCheckBool.ToString() + "\r\n" + interval + "\r\n" + usernameTextBox.Text + "\r\n" + passwordTextBox.Text;
            }
            else
            {
                ftpText = slashRemovalString + "/\r\n" + verboseCheckBool.ToString() + "\r\n" + interval + "\r\n" + usernameTextBox.Text + "\r\n" + passwordTextBox.Text;
            }

            if (ftpAddress.Text == "")
            {
                ftpText = configRetrieved[0] + "\r\n" + verboseCheckBool.ToString() + "\r\n" + interval + "\r\n" + usernameTextBox.Text + "\r\n" + passwordTextBox.Text;
                ftpAddress.Text = ftpText;
            }

            System.IO.File.WriteAllText(ftpAddressFilePathLocal, ftpText);

            if (folderNames.Text.Length >0 && (firstEdit == false || folderNames.Text[0] == 32))
            {
                bool spaceFound = false;
                char[] tempCharArray = new char[folderNames.Text.Length];

                for (i = 0, k = 0; i < folderNames.Text.Length; i++)
                {
                    if (spaceFound == true && folderNames.Text[i] != 32)
                    {
                        spaceFound = false;
                    }

                    if (i == 0 && folderNames.Text[i] == 32)
                    {
                        spaceFound = true;
                    }

                    if (folderNames.Text[i] == 13)
                    {
                        if (i < folderNames.Text.Length - 2 && folderNames.Text[i + 1] == 10 && folderNames.Text[i + 2] == 32)
                        {
                            i++;                //skipping the carriage (13) //loop increment will skip the line feed (10)
                            spaceFound = true;
                        }
                        else if (i == folderNames.Text.Length - 2)
                        {
                            //empty line
                            i++;    //skipping the carriage
                            spaceFound = true;
                        }
                        else
                        {
                            //legal carriages
                        }
                    }

                    if (spaceFound == false)
                    {
                        tempCharArray[k] = folderNames.Text[i];
                        k++;
                    }
                }

                char[] filteredCharArray = new char[k];

                for (i = 0; i < k; i++)
                {
                    filteredCharArray[i] = tempCharArray[i];
                }

                String tempString = new String(filteredCharArray);

                System.IO.File.WriteAllText(folderNamesFilePathLocal, tempString);
            }
            else
            {
                System.IO.File.WriteAllText(folderNamesFilePathLocal, folderNames.Text);
            }

            if(reSubmission == false)
            {
                MessageBox.Show("Changes Saved! Please wait for the changes to take effect!", "FTPAutoSync Configure");
            }

            fInfo1.IsReadOnly = true;
            fInfo2.IsReadOnly = true;
        }

        private void startButtonClicked(object sender, RoutedEventArgs e)
        {
            carryOutSubmission(false);

            System.Timers.Timer reSubmitOne = new System.Timers.Timer(30000);
            reSubmitOne.Elapsed += reSubmitOne_Elapsed;
            reSubmitOne.Enabled = true;

            System.Timers.Timer reSubmitTwo = new System.Timers.Timer(30000);
            reSubmitTwo.Elapsed += reSubmitOne_Elapsed;
            reSubmitTwo.Enabled = true;
        }

        private void startButtonKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.ToString() == "Return")
            {
                carryOutSubmission(false);

                System.Timers.Timer reSubmitOne = new System.Timers.Timer(30000);
                reSubmitOne.Elapsed += reSubmitOne_Elapsed;
                reSubmitOne.Enabled = true;

                System.Timers.Timer reSubmitTwo = new System.Timers.Timer(30000);
                reSubmitTwo.Elapsed += reSubmitOne_Elapsed;
                reSubmitTwo.Enabled = true;
            }
        }

        void reSubmitOne_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                carryOutSubmission(true);
            }));
        }

        private void verboseChecked(object sender, RoutedEventArgs e)
        {
            verboseCheckBool = true;
        }

        private void verboseUnchecked(object sender, RoutedEventArgs e)
        {
            verboseCheckBool = false;
        }

        private void folderNamesKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.ToString() == "Return")
            {
                if ((folderNames.Text.Length != previousLength || (folderNames.Text.Length == previousLength && firstEdit == true)) && folderNames.Text.Length != 0)
                {
                    string tempText = folderNames.Text.TrimEnd();
                    folderNames.Text = tempText + "\r\n";
                    folderNames.CaretIndex = folderNames.Text.Length + 1;

                    previousLength = folderNames.Text.Length;

                    firstEdit = false;
                }
            }
        }

        private void realStartButtonClicked(object sender, RoutedEventArgs e)
        {
            carryOutStart();
        }

        private void realStartButtonKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key.ToString() == "Return")
            {
                carryOutStart();
            }
        }

        private void carryOutStart()
        {
            Process[] ftpAutoSyncProcess = Process.GetProcessesByName("FTPAutoSync");
            Process[] ftpAutoSyncProcess_one = Process.GetProcessesByName("FTPDownload4");

            if(ftpAutoSyncProcess.Length >= 1 || ftpAutoSyncProcess_one.Length >=1)
            {
                MessageBox.Show("FTPAutoSync is already running!","FTPAutoSync Configure");
            }
            else
            {
                Process.Start(modifiedCurrentDirectory + "\\FTPAutoSync.exe");
            }
        }

        private void aboutButtonClicked(object sender, RoutedEventArgs e)
        {
            carryOutAbout();
        }

        private void aboutButtonKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.ToString() == "Return")
            {
                carryOutAbout();
            }
        }

        private void carryOutAbout()
        {
            MessageBox.Show("FTP Auto Sync\n\nKeep your files in the ftp synced to your PC\n\nAuthor: IH\n\nVersion: " + version + "\n\nAcknowledgements: Thank you\nTariq Tonmoy bhai,\nSanjary Rahman bhai,\nAbdul Aziz\nfor your invaluable counsel and support\n\nFeatures: File extension discovery,\n              Auto Sycning,\n              FTP file deletion rescue", "About FTPAutoSync");
        }

        private void windowClosing(object sender, CancelEventArgs e)
        {
            th.Abort();
        }

        private bool NET45Availability()
        {
            RegistryKey rk;
            string registryPath = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
            int value = 0;

            try
            {
                if (Environment.Is64BitOperatingSystem)        //64-bit
                {
                    rk = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
                }
                else
                {
                    rk = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32);
                }

                rk = rk.OpenSubKey(registryPath, false);

                if (rk != null)
                {
                    value = (int)rk.GetValue("Release");
                }

                if(value >= 378389)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
                Environment.Exit(1);

                return false;
            }
        }

        private bool isTaskRegistered()
        {
            using (TaskService ts = new TaskService())
            {
                foreach (Microsoft.Win32.TaskScheduler.Task task in ts.RootFolder.Tasks)
                {
                    if (task.Name == "FTPAutoSync")
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        //to be carried out by anonymousCheck thread
        public int carryOutAnonymous(String s, String ftpAddressText, String usernameTextBoxText, String passwordTextBoxText)
        {
            MessageBox.Show("Checking if anonymous access is allowed");

            try
            {
                FtpWebRequest w = (FtpWebRequest)WebRequest.Create(ftpAddressText);
                w.Timeout = 60000;

                w.Method = WebRequestMethods.Ftp.ListDirectory;

                FtpWebResponse w1 = (FtpWebResponse)w.GetResponse();

                if (s == "Button")
                {
                    MessageBox.Show("Anonymous access is allowed");
                }

                return 0;
            }
            catch(Exception e)
            {
                if(e.Message.Contains("The remote server returned an error: (530) Not logged in."))
                {
                    if(s == "Button")
                    {
                        MessageBox.Show("Anonymous access is not allowed\nPlease contact administrator for username and password");  
     
                        return 1;
                    }
                    else if(s == "NoButton" && (usernameTextBoxText == "" || passwordTextBoxText == ""))
                    {
                        MessageBox.Show("Please contact administrator for username and password");

                        return 2;
                    }
                }
                else if(e.Message.Contains("Unable to connect to the remote server"))
                {
                    MessageBox.Show("Check FTP connection");
                }
            }

            return 3;
        }

        private void anonymousClicked(object sender, RoutedEventArgs e)
        {
            anonymousCheck aCheck = new anonymousCheck(this, "Button", ftpAddress.Text, usernameTextBox.Text, passwordTextBox.Text);
            Thread th = new Thread(aCheck.DoWork);

            th.IsBackground = true;
            th.Start();
        }

        private void anonymousKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key.ToString() == "Return")
            {
                anonymousCheck aCheck = new anonymousCheck(this, "Button", ftpAddress.Text, usernameTextBox.Text, passwordTextBox.Text);
                Thread th = new Thread(aCheck.DoWork);

                th.IsBackground = true;
                th.Start();
            }
        }
    }

    public class runCheck
    {
        public Image sImage;
        public static string redPath, greenPath;

        public runCheck(Image img, string red, string green)
        {
            sImage = img;
            redPath = red;
            greenPath = green;
        }

        public void DoWork()
        {   
            while(true)
            {
                Process[] ftpAutoSyncProcess = Process.GetProcessesByName("FTPAutoSync");
                
                if (ftpAutoSyncProcess.Length < 1)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,new System.Action(()=>
                    {
                        sImage.Source = new BitmapImage(new Uri(redPath));
                    }));
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new System.Action(() =>
                    {
                        sImage.Source = new BitmapImage(new Uri(greenPath));
                    }));
                }

                Thread.Sleep(1000);
            }
        }
    }

    public class anonymousCheck
    {
        public MainWindow mainWindow;
        public String button, ftpAddress, usernameTextBox, passwordTextBox;

        public anonymousCheck(MainWindow mW, String b, String ftp, String username, String password)
        {
            mainWindow = mW;

            ftpAddress = ftp;
            usernameTextBox = username;
            passwordTextBox = password;

            button = b;
        }

        public void DoWork()
        {
            int returnValue = mainWindow.carryOutAnonymous(button, ftpAddress, usernameTextBox, passwordTextBox);

            if (returnValue == 0)        //anonymous access is allowed
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                {
                    mainWindow.usernameTextBox.Text = "anonymous";
                    mainWindow.passwordTextBox.Text = "abc@abc.com";
                }));
            }
            else if (returnValue == 1)    //anonymous access is not allowed - button
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                {
                    mainWindow.usernameTextBox.Text = "(required)";
                    mainWindow.passwordTextBox.Text = "(required)";
                }));
            }
            else if (returnValue == 2)    //anonymous access is not allowed - no button
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                {
                    mainWindow.usernameTextBox.Text = "(required)";
                    mainWindow.passwordTextBox.Text = "(required)";
                }));
            }
        }
    }
}
