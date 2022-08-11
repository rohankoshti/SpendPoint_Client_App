//using Shell32;
using System;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace SpendPoint
{
    static class Program
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool createdNew = true;
            using (Mutex mutex = new Mutex(true, "MyApplicationName", out createdNew))
            {
                if (createdNew)
                {
                    //string downloadsPath = new KnownFolder(KnownFolderType.Downloads).Path;
                    //PinUnpinTaskBar(Directory.GetCurrentDirectory() + @"\SpendPoint.exe", true);
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Form1());
                }
                else
                {
                    Process current = Process.GetCurrentProcess();
                    foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                    {
                        if (process.Id != current.Id)
                        {
                            SetForegroundWindow(process.MainWindowHandle);
                            break;
                        }
                    }
                }
            }
        }

        //private static void PinUnpinTaskBar(string filePath, bool pin)
        //{
        //    if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);

        //    // create the shell application object
        //    Shell shellApplication = new ShellClass();

        //    string path = Path.GetDirectoryName(filePath);
        //    string fileName = Path.GetFileName(filePath);

        //    Folder directory = shellApplication.NameSpace(path);
        //    FolderItem link = directory.ParseName(fileName);

        //    FolderItemVerbs verbs = link.Verbs();
        //    for (int i = 0; i < verbs.Count; i++)
        //    {
        //        FolderItemVerb verb = verbs.Item(i);
        //        string verbName = verb.Name.Replace(@"&", string.Empty).ToLower();

        //        if ((pin && verbName.Equals("pin to taskbar")) || (!pin && verbName.Equals("unpin from taskbar")))
        //        {
        //            verb.DoIt();
        //        }
        //    }

        //    shellApplication = null;
        //}
    }
}
