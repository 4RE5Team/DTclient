using System;using System.Drawing;using System.Windows.Forms;using System.Text;using System.IO;using System.Diagnostics;using System.ComponentModel;using System.Linq;using System.Net;using System.Collections.Generic;using System.IO.Compression;using System.Web;using System.Threading;using System.Text.RegularExpressions;using System.Reflection;using System.Runtime.InteropServices;namespace DTinjector{    public static class injector    {        #region Win32        [DllImport("KERNEL32.DLL")]        private static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);        [DllImport("KERNEL32.DLL")]        private static extern bool Process32First(IntPtr hSnapShot, ref PROCESSENTRY32 pe);        [DllImport("KERNEL32.DLL")]        private static extern bool Process32Next(IntPtr Handle, ref PROCESSENTRY32 lppe);        [DllImport("KERNEL32.DLL")]        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);        [DllImport("KERNEL32.DLL")]        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAdress, UIntPtr dwSize, uint flAllocationType, uint flProtect);        [DllImport("KERNEL32.DLL")]        private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAdress, uint dwSize, uint dwFreeType);        [DllImport("KERNEL32.DLL")]        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, int lpNumberOfBytesWritten);        [DllImport("KERNEL32.DLL")]        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr se, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, uint lpThreadId);        [DllImport("KERNEL32.DLL", CharSet = CharSet.Ansi)]        private extern static IntPtr GetProcAddress(IntPtr hModule, string lpProcName);        [DllImport("KERNEL32.DLL", CharSet = CharSet.Ansi)]        private static extern IntPtr GetModuleHandle(string lpModuleName);        [DllImport("KERNEL32.DLL")]        private static extern bool CloseHandle(IntPtr hObject);        [DllImport("KERNEL32.DLL")]        private static extern int GetLastError();        [DllImport("KERNEL32.DLL")]        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliSeconds);        private const uint TH32CS_SNAPPROCESS = 0x00000002;        private const uint PROCESS_ALL_ACCESS = (uint)(0x000F0000 | 0x00100000 | 0xFFF);        private const uint MEM_COMMIT = 0x1000;        private const uint MEM_RESERVE = 0x2000;        private const uint MEM_DECOMMIT = 0x4000;        private const uint MEM_RELEASE = 0x8000;        private const uint PAGE_EXECUTE_READWRITE = 0x40;        private const uint PAGE_READWRITE = 0X4;        private const uint WAIT_ABANDONED = 0x00000080;        private const uint WAIT_OBJECT_0 = 0x00000000;        private const uint WAIT_TIMEOUT = 0x00000102;        private const uint WAIT_FAILED = 0xFFFFFFFF;        [StructLayout(LayoutKind.Sequential)]        private struct PROCESSENTRY32        {            public int dwSize;            public uint cntUsage;            public uint th32ProcessID;            public IntPtr th32DefaultHeapID;            public uint th32ModuleID;            public uint cntThreads;            public uint th32ParentProcessID;            public int pcPriClassBase;            public uint dwFlags;            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]            public string szExeFile;        };        [StructLayout(LayoutKind.Sequential)]        private struct SECURITY_ATTRIBUTES        {            public int nLength;            public IntPtr lpSecurityDescriptor;            public int bInheritHandle;        }        #endregion        static List<PROCESSENTRY32> ProcessList;         public static void InitProcessList()        {            ProcessList = new List<PROCESSENTRY32>();        }        public static void ClearProcessList()        {            ProcessList.Clear();        }        public static uint GetPIDbyName(string PName)        {            foreach (PROCESSENTRY32 p in ProcessList)            {                if (string.Compare(p.szExeFile, PName) == 0)                {                    return p.th32ProcessID;                }            }            return 0;        }        public static void Inject()        {			string tempdir = Path.GetTempPath();			if (!File.Exists(tempdir+@"\dtclient.dll"))			{				Download("https://github.com/DuckpvpTeam/DTclient/raw/main/releases/dtclient.dll", tempdir+@"\dtclient.dll");			}			string DllName = tempdir+@"\dtclient.dll";			uint ProcessID = GetPIDbyName("Minecraft.Windows");            try            {                IntPtr hProcess = new IntPtr(0);                 IntPtr hModule = new IntPtr(0);                 IntPtr Injector = new IntPtr(0);                 IntPtr hThread = new IntPtr(0);                 int LenWrite = DllName.Length + 1;                hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, ProcessID);                if (hProcess != IntPtr.Zero)                {                    hModule = VirtualAllocEx(hProcess, IntPtr.Zero, (UIntPtr)LenWrite, MEM_COMMIT, PAGE_EXECUTE_READWRITE);                    if (hModule != IntPtr.Zero)                    {                        ASCIIEncoding Encoder = new ASCIIEncoding();                        int Written = 0;                        if (WriteProcessMemory(hProcess, hModule, Encoder.GetBytes(DllName), LenWrite, Written))                        {                            Injector = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");                            if (Injector != IntPtr.Zero)                            {                                hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, Injector, hModule, 0, 0);                                if (hThread != IntPtr.Zero)                                {                                    uint Result = WaitForSingleObject(hThread, 10 * 1000);                                    if (Result != WAIT_FAILED || Result != WAIT_ABANDONED                                       || Result != WAIT_OBJECT_0 || Result != WAIT_TIMEOUT)                                    {                                        if (VirtualFreeEx(hProcess, hModule, 0, MEM_RELEASE))                                        {                                            if (hThread != IntPtr.Zero)                                            {                                                CloseHandle(hThread);                                                MessageBox.Show("DTclient injected", "Success", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);                                            }                                            else throw new Exception("Error");                                        }                                        else throw new Exception("Error");                                    }                                    else throw  new Exception("Error");                                }                                else throw new Exception("Error");                            }                            else throw new Exception("Error");                        }                        else throw new Exception("Error");                    }                    else throw new Exception("Error");                }                else throw new Exception("Minecraft isn't opened");            }            catch (Exception e)            {                MessageBox.Show(e.Message, e.Source);            }        }		private static void execute_cmd(string cmd)        {            						string callcommand = "/c " + cmd ;						ProcessStartInfo processInfo;			Process process;						string output = "";						processInfo = new ProcessStartInfo("cmd.exe", callcommand);			processInfo.CreateNoWindow = true;			processInfo.UseShellExecute = false;			processInfo.RedirectStandardOutput = true;			process = Process.Start(processInfo);        }		private static void Download(string url, string outPath)		{			string tempdir = Path.GetTempPath();										url = '"' + url + '"';						outPath = '"' + outPath + '"';						string str = "(New-Object System.Net.WebClient).DownloadFile(" + url + ", " + outPath + ")";						outPath = tempdir + @"\download.ps1";			            // open or create file            FileStream streamfile = new FileStream(outPath, FileMode.OpenOrCreate, FileAccess.Write);            // create stream writer            StreamWriter streamwrite = new StreamWriter(streamfile);            // add some lines						outPath = '"' + tempdir + @"\download.ps1" + '"';									// string powershelldownloadtxt = "" + url +"\  "            streamwrite.WriteLine(str);            // clear streamwrite data            streamwrite.Flush();            // close stream writer            streamwrite.Close();            // close stream file            streamfile.Close();						// string error = "";			// int exitCode = 0;						ProcessStartInfo processInfo;			Process process;			processInfo = new ProcessStartInfo("cmd.exe", "/c powershell " + tempdir + @"\download.ps1");			processInfo.CreateNoWindow = true;			processInfo.UseShellExecute = false;			processInfo.RedirectStandardOutput = true;			process = Process.Start(processInfo);			process.WaitForExit();					execute_cmd("if exist " + tempdir + @"\download.ps1 (del " + tempdir + @"\download.ps1)");		}    }}