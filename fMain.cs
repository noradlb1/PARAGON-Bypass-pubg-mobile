using Memory;
using Microsoft.Win32;
using NetFwTypeLib;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Solace_Bypass
{
    public partial class fMain : Form
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct ProcessEntry32
        {
            public uint dwSize;
            public uint cntUsage;
            public uint th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public uint th32ModuleID;
            public uint cntThreads;
            public uint th32ParentProcessID;
            public int pcPriClassBase;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szExeFile;
        };
        public bool spawner = false;
        public Mem MemLib = new Mem();
        private static string container2;
        private static string container;
        public string userName = Environment.UserName;
        public string PID = null;
        public string GagaPath = null;
        public bool bStart_State = false;
        private bool fwEnabled = false;
        private const string clsidFireWall = "{304CE942-6E39-40D8-943A-B913C40C9CD4}";

        private bool mouseDown;
        private Point lastLocation;

        [DllImport("KERNEL32.DLL")]
        public static extern IntPtr CreateToolhelp32Snapshot(uint flags, uint processid);
        [DllImport("KERNEL32.DLL")]
        public static extern int Process32First(IntPtr handle, ref ProcessEntry32 pe);
        [DllImport("KERNEL32.DLL")]
        public static extern int Process32Next(IntPtr handle, ref ProcessEntry32 pe);
        /// <summary>
        /// Main methdd
        /// </summary>
        public fMain()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Form drag control part 1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fMain_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            lastLocation = e.Location;
        }
        /// <summary>
        /// Form drag control part 2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Location = new Point(
                    (this.Location.X - lastLocation.X) + e.X, (this.Location.Y - lastLocation.Y) + e.Y);

                this.Update();
            }
        }
        /// <summary>
        /// Form Drag control part 3
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fMain_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }




        /// <summary>
        /// Checks status of firewall if enabled returns true.
        /// </summary>
        private void CheckFirewall()
        {
            try
            {
                INetFwMgr mgrInstance = GetType();
                if (mgrInstance.LocalPolicy.CurrentProfile
                   .FirewallEnabled == false)
                {
                    //MessageBox.Show("Firewall Disabled");
                    fwEnabled = false;
                }
                else
                {
                    //MessageBox.Show("Firewall Enabled");
                    fwEnabled = true;
                }
            }
            catch (Exception e)
            {
                //
            }

        }
        /// <summary>
        /// Firewall control object type
        /// </summary>
        /// <returns></returns>
        private static NetFwTypeLib.INetFwMgr GetType()
        {
            Type tpCLSID = Type.GetTypeFromCLSID(new
               Guid(clsidFireWall));

            return Activator.CreateInstance(tpCLSID) as
               NetFwTypeLib.INetFwMgr;
        }
        /// <summary>
        /// This if for putting task delay in asynchronous threads where required.
        /// </summary>
        /// <param name="Time"></param>
        /// <returns></returns>
        private async Task PutTaskDelay(int Time)
        {
            await Task.Delay(Time);
        }

        /// <summary>
        /// Main Form Load method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fMain_Load(object sender, EventArgs e)
        {
            chNoAnim.Enabled = false;
            chFlyCar.Enabled = false;
            bInject.Enabled = false;
            chAim.Enabled = false;

            chLuffy.Enabled = false;
            chNoGrass.Enabled = false;
            chSpeedGame.Enabled = false;
            chNoShake.Enabled = false;

            chCross.Enabled = false;
            chInstant.Enabled = false;
            chZeroHS.Enabled = false;
            chNoRecoil.Enabled = false;

            chNoFog.Enabled = false;
            chMagic.Enabled = false;
            chSky.Enabled = false;
            chIpadView.Enabled = false;

            bSPAWN.Enabled = false;
            bBypass.Enabled = false;
            //Kill ADB
            labStatus.Text = "Trying to Delete Firewall Rules.";
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                CreateNoWindow = true,
                RedirectStandardInput = true,
                UseShellExecute = false
            };
            process.Start();
            using (process.StandardInput)
            {
                process.StandardInput.WriteLine("netsh advfirewall firewall delete rule name=INLOGIN");
                labStatus.Text = "Login Rule Deleted.";
                process.StandardInput.WriteLine("netsh advfirewall firewall delete rule name=INSPAWN");
                labStatus.Text = "Spawn Rule Deleted.";
                process.StandardInput.WriteLine("taskkill /f /im adb.exe");
                labStatus.Text = "ADB Closed.";
                process.StandardInput.Flush();
                process.StandardInput.Close();
                process.WaitForExit();
                process.Close();
            }
            File.WriteAllBytes("adb.exe", Properties.Resources.adb);
            File.WriteAllBytes("AdbWinApi.dll", Properties.Resources.AdbWinApi);
            //check Firewall
            labStatus.Text = "Checking Firewall.";
            CheckFirewall();
            if (fwEnabled)
            {
                labFW.ForeColor = Color.Green;
                labStatus.Text = "Finding Smartgaga...";
                try
                {
                    if(File.Exists(@"C:\Program Files (x86)\SmartGaGa\ProjectTitan\Engine\ProjectTitan.exe"))
                        GagaPath = @"C:\Program Files (x86)\SmartGaGa\ProjectTitan";
                    else if (File.Exists(@"D:\Program Files (x86)\SmartGaGa\ProjectTitan\Engine\ProjectTitan.exe"))
                        GagaPath = @"D:\Program Files (x86)\SmartGaGa\ProjectTitan";
                    else if (File.Exists(@"F:\Program Files (x86)\SmartGaGa\ProjectTitan\Engine\ProjectTitan.exe"))
                        GagaPath = @"F:\Program Files (x86)\SmartGaGa\ProjectTitan";
                    else if (File.Exists(@"E:\Program Files (x86)\SmartGaGa\ProjectTitan\Engine\ProjectTitan.exe"))
                        GagaPath = @"E:\Program Files (x86)\SmartGaGa\ProjectTitan";
                    else
                        MessageBox.Show("SmartGaGa not found","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
                catch
                {
                    GagaPath = "Directory not found";
                }
                //MessageBox.Show(GagaPath);
                if (GagaPath != null)
                {
                    //System.Windows.Forms.MessageBox.Show("netsh  advfirewall firewall add rule name=INSPAWN protocol=TCP dir=out program=" + GagaPath + "\\Engine\\ProjectTitan.exe\" remoteport=0-65535 action=block");
                    labStatus.Text = "Setting HOSTS file...";
                    labSmartGaga.ForeColor = Color.Green;
                    File.WriteAllBytes("C:\\Windows\\System32\\drivers\\etc\\hosts", Properties.Resources.hosts);
                    labHOSTS.ForeColor = Color.Green;
                    labStatus.Text = "Extracting Libraries...";
                    File.WriteAllBytes(GagaPath + "\\Engine\\sharefs\\libgamemaster.so", Properties.Resources.libgamemaster);
                    File.WriteAllBytes(GagaPath + "\\Engine\\sharefs\\libIMSDK.so", Properties.Resources.libIMSDK);
                    //File.WriteAllBytes(GagaPath + "\\Engine\\sharefs\\libtersafe.so", Properties.Resources.libtersafe);
                    //File.WriteAllBytes(GagaPath + "\\Engine\\sharefs\\libUE4.so", Properties.Resources.libUE4);
                    labStatus.Text = "Ready.";
                    labSelf.ForeColor = Color.Green;
                }
                else
                {
                    labStatus.Text = "Smartgaga not found.";
                    labSmartGaga.ForeColor = Color.Red;
                    MessageBox.Show("Smartgaga Not Installed","Smartgaga Problematic",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    this.Close();
                }

            }
            else
            {
                labStatus.Text = "Firewall is disabled.";
                labFW.ForeColor = Color.Red;
                MessageBox.Show("Your firewall is disabled. Please enable and try again.","Firewall Problematic",MessageBoxButtons.OK,MessageBoxIcon.Error);
                this.Close();
            }
            //check HOSTS file with CRC

        }
        
        /// <summary>
        /// Finds pointer to a process
        /// </summary>
        /// <returns></returns>
     
        private void bStart_Click(object sender, EventArgs e)
        {
            if (!bStart_State)
            {
                // hide Exit button as this button is gonna serve the same
                bExit.Enabled = false;
                //Emulator is not running -> Start
                foreach (var process in Process.GetProcessesByName("projecttitan"))
                {
                    process.Kill();
                }
                labStatus.Text = "Launching Smartgaga";
                Process.Start((GagaPath + "\\Engine\\Launcher.exe"));
                labStatus.Text = "Waiting for Homescreen.";
                DialogResult response = MessageBox.Show("Press Ok when the emulator is fully loaded and at home screen.", "Waiting for SmartGaga", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (response == DialogResult.OK)
                {
                    labStatus.Text = "Fixing Game start issue.";
                    //Apply fix here
                    Process fixProc = new Process();
                    fixProc.StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        CreateNoWindow = true,
                        RedirectStandardInput = true,
                        UseShellExecute = false
                    };
                    fixProc.Start();
                    using (fixProc.StandardInput)
                    {
                        fixProc.StandardInput.WriteLine("adb.exe kill-server");
                        fixProc.StandardInput.WriteLine("adb.exe start-server");
                        fixProc.StandardInput.WriteLine("adb fork-server server");
                        fixProc.StandardInput.WriteLine("adb.exe devices");
                        fixProc.StandardInput.WriteLine("adb root");
                        fixProc.StandardInput.WriteLine("adb remount");
                        fixProc.StandardInput.WriteLine("adb shell mkdir /data/data/com.tencent.tinput");
                        fixProc.StandardInput.WriteLine("adb shell mkdir /data/data/com.tencent.tinput/cache");
                        labInteg.ForeColor = Color.Green;
                        fixProc.StandardInput.Flush();
                        fixProc.StandardInput.Close();
                        fixProc.WaitForExit();
                        fixProc.Close();
                        labStatus.Text = "Fixed";
                    }

                }
                else if (response == DialogResult.Cancel)
                {
                    labStatus.Text = "Trying to Close/Crash smartgaga";
                    //crash emu here again
                    foreach (var process in Process.GetProcessesByName("projecttitan"))
                    {
                        process.Kill();
                    }
                }

                bStart_State = !bStart_State;//to identify for next click
                bStartStop.Text = "STOP";
                bBypass.Enabled = true;
            }
            else
            {
                //Emulator is running -> Stop
                foreach (var process in Process.GetProcessesByName("projecttitan"))
                {
                    process.Kill();
                }
                bStartStop.Text = "START";
                Process processa = new Process();
                processa.StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false
                };
                processa.Start();
                using (processa.StandardInput)
                {
                    labStatus.Text = "Removing System Wide Changes.";
                    processa.StandardInput.WriteLine("netsh advfirewall firewall delete rule name=INLOGIN");
                    processa.StandardInput.WriteLine("netsh advfirewall firewall delete rule name=INSPAWN");
                    labStatus.Text = "Resetting Hosts";
                    File.Create("C:\\Windows\\System32\\drivers\\etc\\hosts").Dispose();
                    labStatus.Text = "Stopping MrIN FS Service";
                    processa.StandardInput.WriteLine("sc stop MrIN");
                    processa.StandardInput.WriteLine("net stop MrIN");
                    processa.StandardInput.WriteLine("net delete MrIN");
                    processa.StandardInput.WriteLine("sc delete MrIN");
                    labStatus.Text = "Deleting Cache Files.";
                    processa.StandardInput.WriteLine("cd /d C:\\Users\\" + Environment.UserName + "\\ApPData\\Local\\Temp");
                    processa.StandardInput.WriteLine("del /f MrIN.sys");
                    labStatus.Text = "Stopping Debug Bridge.";
                    processa.StandardInput.WriteLine("taskkill /f /im adb.exe");
                    processa.StandardInput.Flush();
                    processa.StandardInput.Close();
                    processa.WaitForExit();
                    processa.Close();
                }
                File.Delete("adb.exe");
                File.Delete("AdbWinApi.dll");
                File.Delete("C:\\Users\\" + Environment.UserName + "\\ApPData\\Local\\Temp\\MrIN.sys");
                labStatus.Text = "Removing Extracted Libraries.";
                File.Delete(GagaPath + "\\Engine\\sharefs\\libgamemaster.so");
                File.Delete(GagaPath + "\\Engine\\sharefs\\libgamemaster.so");
                File.Delete(GagaPath + "\\Engine\\sharefs\\libIMSDK.so");
                File.Delete(GagaPath + "\\Engine\\sharefs\\libUE4.so");
                File.Delete(GagaPath + "\\Engine\\sharefs\\libtersafe.so");
                this.Close();
            }
        }

        private async void bBypass_Click(object sender, EventArgs e)
        {
            bSPAWN.Enabled = true;
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                CreateNoWindow = true,
                RedirectStandardInput = true,
                UseShellExecute = false
            };
            process.Start();
            using (process.StandardInput)
            {
                labStatus.Text = "LOGIN Rules applied.";
                //process.StandardInput.WriteLine("netsh advfirewall firewall add rule name=\"INLOGIN\" dir=out action=block protocol=any localip=any remoteip=15692,20371,23946,27042,27043,5403,5646,7312,7311,2384,2383,3013,18081,8085,8086,13003,10012,8081,8088,13004,5006,501,25046,5045,8080,11042,11041,2979,2986,17000,13004,13003,35000,10013,10012,15692,20371");
                process.StandardInput.WriteLine("adb.exe kill-server");
                process.StandardInput.WriteLine("adb.exe start-server");
                process.StandardInput.WriteLine("adb fork-server server");
                process.StandardInput.WriteLine("adb.exe devices");
                process.StandardInput.WriteLine("adb root");
                process.StandardInput.WriteLine("adb remount");
                labStatus.Text = "Emptying Databases";
                process.StandardInput.WriteLine("adb shell rm -rf /data/data/com.tencent.ig/databases/*");
                labDB.ForeColor = Color.Green;
                labStatus.Text = "Starting Game Activity.";
                process.StandardInput.WriteLine("adb shell am start -n com.tencent.ig/com.epicgames.ue4.SplashActivity");
                labStatus.Text = "Waiting for time Window";
                process.StandardInput.WriteLine("adb shell sleep 15");
                labStatus.Text = "Pushing Libraries.";
                process.StandardInput.WriteLine("adb shell mv -f /share/libgamemaster.so /data/app/com.tencent.ig-1/lib/arm/libgamemaster.so");
                process.StandardInput.WriteLine("adb shell mv -f /share/libIMSDK.so /data/app/com.tencent.ig-1/lib/arm/libIMSDK.so");
                process.StandardInput.WriteLine("adb shell mv -f /share/libtersafe.so /data/app/com.tencent.ig-1/lib/arm/libtersafe.so");
                process.StandardInput.WriteLine("adb shell mv -f /share/libUE4.so /data/app/com.tencent.ig-1/lib/arm/libUE4.so");
                labStatus.Text = "Libraries Pushed, continuing...";
                process.StandardInput.Flush();
                process.StandardInput.Close();
                process.WaitForExit();
                process.Close();
            }
            await PutTaskDelay(0);
            Exposer();
        }

        /// <summary>
        /// Service, needs testing
        /// </summary>
        public async void Exposer()
        {
            labStatus.Text = "Installing Service for Memory Exposure";
            string TempPath = "C:\\Users\\" + userName + "\\AppData\\Local\\Temp\\";
            if (File.Exists(TempPath + "MrIN" + ".sys"))
            {
                //stop service and delete the file??
                //Maybe later on deleted when the program memory changes. Verified.
            }
            else
            {
                File.WriteAllBytes(TempPath + "MrIN" + ".sys", Properties.Resources.MemoryExposer);
                var process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/c sc create " + "MrIN" + " binpath=" + TempPath + "MrIN" + ".sys start=demand type=filesys & net start " + "MrIN"; // & r
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit();//Service Created and Started

            }
            await AsyncMemoryModify();
        }

        private async Task<IntPtr> AsyncMemoryModify()
        {
            labStatus.Text = "Finding AndroidProcess.exe";
            IntPtr intPtr = IntPtr.Zero;
            uint num = 0U;
            //This takes a snapshot of the processes runniing and their heaps, threads and modules.
            //Essentially this ia "Picture of Memory."
            IntPtr snapshotPtr = CreateToolhelp32Snapshot(2U, 0U);
            if ((int)snapshotPtr > 0)
            {
                ProcessEntry32 processEntry = default(ProcessEntry32);
                processEntry.dwSize = (uint)Marshal.SizeOf(processEntry);
                for (int pidCounter = Process32First(snapshotPtr, ref processEntry); pidCounter == 1; pidCounter = Process32Next(snapshotPtr, ref processEntry))
                {
                    IntPtr memoryPtr = Marshal.AllocHGlobal((int)processEntry.dwSize);
                    Marshal.StructureToPtr(processEntry, memoryPtr, true);
                    ProcessEntry32 gagaEmulatorEntry = (ProcessEntry32)Marshal.PtrToStructure(memoryPtr, typeof(ProcessEntry32));
                    Marshal.FreeHGlobal(memoryPtr);
                    // AndroidProcess with maximum threads is found here
                    if (gagaEmulatorEntry.szExeFile.Contains("AndroidProcess") && gagaEmulatorEntry.cntThreads > num)
                    {
                        num = gagaEmulatorEntry.cntThreads;
                        intPtr = (IntPtr)gagaEmulatorEntry.th32ProcessID;
                    }
                    pidCounter = Process32Next(snapshotPtr, ref processEntry);
                }
                labPID.Text = Convert.ToString(intPtr);
                await PutTaskDelay(1000);
                labStatus.Text = "Bypassing now...";
                Bypass();
                processEntry = default(ProcessEntry32);//This line is removing reference to process. Clever
            }

            return intPtr;
        }

        public async void Bypass()
        {
            bool memoryStateChanged = false;
            bool memoryStateChanged2 = false;
            int counter = 1;
            if (!bStart_State)//emulator is not running
            {
                //status
                labStatus.ForeColor = Color.Red;
                labStatus.Text = "Smartgaga not running.";
            }
            else
            {
                labPID.ForeColor = Color.Red;
                MemLib.OpenProcess(Convert.ToInt32(labPID.Text));
                labStatus.Text = "Scanning...";                                 /* 00 00 A0 E3 1E FF 2F E1 14 D0 4D E2 00 A0 A0 E1 0C 06*/
                var enumerable = await MemLib.AoBScan(1879048192L, 2415919104L, "F0 4F 2D E9 1C B0 8D E2 14 D0 4D E2 00 A0 A0 E1 0C 06 9F E5 01 80 A0 E1", false, false, "");
                var enumerable2 = await MemLib.AoBScan(1879048192L, 2415919104L, "F0 4F 2D E9 1C B0 8D E2 14 D0 4D E2 00 A0 A0 E1 0C 06 9F E5 01 80 A0 E1", false, false, "");
                container = "0x" + enumerable.FirstOrDefault().ToString("X");
                container2 = "0x" + enumerable2.FirstOrDefault().ToString("X");
                Mem.MemoryProtection memoryProtection;
                Mem.MemoryProtection memoryProtection2;
                MemLib.ChangeProtection(container, Mem.MemoryProtection.ReadWrite, out memoryProtection, "");
                MemLib.ChangeProtection(container2, Mem.MemoryProtection.ReadWrite, out memoryProtection2, "");
                foreach (long num in enumerable)
                {
                    labStatus.Text = "Replacing...";                  
                    MemLib.WriteMemory(num.ToString("X"), "bytes", "00 00 A0 E3 1E FF 2F E1 14 D0 4D E2 00 A0 A0 E1 0C 06 9F E5 01 80 A0 E1", "", null);
                    memoryStateChanged = true;
                }
                foreach (long num2 in enumerable2)
                {
                                                                   //00 00 A0 E3 1E FF 2F E1 14 D0 4D E2 00 A0 A0 E1 0C 06 9F E5 01 80 A0 E1
                    labStatus.Text = "Replacing...(B7)";
                    MemLib.WriteMemory(num2.ToString("X"), "bytes", "00 00 A0 E3 1E FF 2F E1 14 D0 4D E2 00 A0 A0 E1 0C 06 9F E5 01 80 A0 E1", "", null);
                    memoryStateChanged2 = true;
                }

                if (memoryStateChanged == true && memoryStateChanged2 ==true)
                {
                    bBypass.Enabled = false;
                    labStatus.Text = "Bypassed Done.";
                    labStatus.ForeColor = Color.Cyan;
                    await PutTaskDelay(500);
                    if (memoryStateChanged == true)
                    {
                        Process process = new Process();
                        process.StartInfo = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            CreateNoWindow = true,
                            RedirectStandardInput = true,
                            UseShellExecute = false
                        };
                        process.Start();
                        using (process.StandardInput)
                        {
                            process.StandardInput.WriteLine("sc stop MrIN");
                            process.StandardInput.WriteLine("net stop MrIN");
                            process.StandardInput.WriteLine("net delete MrIN");
                            process.StandardInput.WriteLine("sc delete MrIN");
                            process.StandardInput.WriteLine("cd /d C:\\Users\\" + Environment.UserName + "\\ApPData\\Local\\Temp");
                            process.StandardInput.WriteLine("del /f MrIN.sys");
                            process.StandardInput.Flush();
                            process.StandardInput.Close();
                            process.WaitForExit();
                            process.Close();
                        }
                    }
                }
                else if(memoryStateChanged == true && memoryStateChanged2 == false)
                {
                    bBypass.Enabled = false;
                    labStatus.Text = "Bypassed Original Done";
                    labStatus.ForeColor = Color.Crimson;
                    await PutTaskDelay(500);
                    if (memoryStateChanged == true)
                    {
                        Process process = new Process();
                        process.StartInfo = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            CreateNoWindow = true,
                            RedirectStandardInput = true,
                            UseShellExecute = false
                        };
                        process.Start();
                        using (process.StandardInput)
                        {
                            process.StandardInput.WriteLine("sc stop MrIN");
                            process.StandardInput.WriteLine("net stop MrIN");
                            process.StandardInput.WriteLine("net delete MrIN");
                            process.StandardInput.WriteLine("sc delete MrIN");
                            process.StandardInput.WriteLine("cd /d C:\\Users\\" + Environment.UserName + "\\ApPData\\Local\\Temp");
                            process.StandardInput.WriteLine("del /f MrIN.sys");
                            process.StandardInput.Flush();
                            process.StandardInput.Close();
                            process.WaitForExit();
                            process.Close();
                        }
                    }
                }
                else if (counter < 4)//total tries for slow pcs
                {
                    labStatus.ForeColor = Color.Red;
                    labStatus.Text = "Bypassing.. Wait 30 seconds or less";
                    counter += 1;
                    await AsyncMemoryModify();
                }
                else
                {
                    labStatus.Text = "Memory Access Denied bu Anticheat.";
                    labStatus.ForeColor = Color.Red;
                }

                Mem.MemoryProtection memoryProtectionAgain;
                MemLib.ChangeProtection(container, Mem.MemoryProtection.ReadOnly, out memoryProtectionAgain, "");
            }
        }

        private void bExit_Click(object sender, EventArgs e)
        {
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                CreateNoWindow = true,
                RedirectStandardInput = true,
                UseShellExecute = false
            };
            process.Start();
            using (process.StandardInput)
            {
                labStatus.Text = "Removing Fireall Rules";
                process.StandardInput.WriteLine("netsh advfirewall firewall delete rule name=INLOGIN");
                process.StandardInput.WriteLine("netsh advfirewall firewall delete rule name=INSPAWN");
                labStatus.Text = "Resetting Hosts";
                File.Create("C:\\Windows\\System32\\drivers\\etc\\hosts").Dispose();
                labStatus.Text = "Stopping and deleting Services";
                process.StandardInput.WriteLine("sc stop MrIN");
                process.StandardInput.WriteLine("net stop MrIN");
                process.StandardInput.WriteLine("net delete MrIN");
                process.StandardInput.WriteLine("sc delete MrIN");
                process.StandardInput.WriteLine("cd /d C:\\Users\\" + Environment.UserName + "\\ApPData\\Local\\Temp");
                process.StandardInput.WriteLine("del /f MrIN.sys");
                labStatus.Text = "Trying to Stop ADB";
                process.StandardInput.WriteLine("taskkill /f /im adb.exe");
                process.StandardInput.Flush();
                process.StandardInput.Close();
                process.WaitForExit();
                process.Close();
            }
            labStatus.Text = "Deleting Debug Bridge";
            File.Delete("adb.exe");
            labStatus.Text = "Deleting Service";
            File.Delete("AdbWinApi.dll");
            File.Delete("C:\\Users\\" + Environment.UserName + "\\ApPData\\Local\\Temp\\MrIN.sys");
            labStatus.Text = "Deleting Extracted Libraries";
            File.Delete(GagaPath + "\\Engine\\sharefs\\libgamemaster.so");
            File.Delete(GagaPath + "\\Engine\\sharefs\\libgamemaster.so");
            File.Delete(GagaPath + "\\Engine\\sharefs\\libIMSDK.so");
            File.Delete(GagaPath + "\\Engine\\sharefs\\libUE4.so");
            File.Delete(GagaPath + "\\Engine\\sharefs\\libtersafe.so");
            labStatus.Text = "Exiting";
            Environment.Exit(0);
        }


        private void bClose_Click(object sender, EventArgs e)
        {
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                CreateNoWindow = true,
                RedirectStandardInput = true,
                UseShellExecute = false
            };
            process.Start();
            using (process.StandardInput)
            {
                labStatus.Text = "Trying to Delete Firewall Rules.";
                process.StandardInput.WriteLine("netsh advfirewall firewall delete rule name=INLOGIN");
                process.StandardInput.WriteLine("netsh advfirewall firewall delete rule name=INSPAWN");
                labStatus.Text = "Trying to Delete HOSTS";
                File.Create("C:\\Windows\\System32\\drivers\\etc\\hosts").Dispose();
                labStatus.Text = "Trying to Stop Services";
                process.StandardInput.WriteLine("sc stop MrIN");
                process.StandardInput.WriteLine("net stop MrIN");
                labStatus.Text = "Trying to Delete Services.";
                process.StandardInput.WriteLine("net delete MrIN");
                process.StandardInput.WriteLine("sc delete MrIN");
                labStatus.Text = "Trying to Delete Cache Files";
                process.StandardInput.WriteLine("cd /d C:\\Users\\" + Environment.UserName + "\\ApPData\\Local\\Temp");
                process.StandardInput.WriteLine("del /f MrIN.sys");
                labStatus.Text = "Trying to Stop Debug Bridge";
                process.StandardInput.WriteLine("taskkill /f /im adb.exe");
                process.StandardInput.Flush();
                process.StandardInput.Close();
                process.WaitForExit();
                process.Close();
            }
            labStatus.Text = "Trying to Delete ADB";
            File.Delete("adb.exe");
            File.Delete("AdbWinApi.dll");
            labStatus.Text = "Trying to Delete Service";
            File.Delete("C:\\Users\\" + Environment.UserName + "\\ApPData\\Local\\Temp\\MrIN.sys");
            labStatus.Text = "Trying to Delete Extracted Libraries";
            File.Delete(GagaPath + "\\Engine\\sharefs\\libgamemaster.so");
            File.Delete(GagaPath + "\\Engine\\sharefs\\libgamemaster.so");
            File.Delete(GagaPath + "\\Engine\\sharefs\\libIMSDK.so");
            File.Delete(GagaPath + "\\Engine\\sharefs\\libUE4.so");
            File.Delete(GagaPath + "\\Engine\\sharefs\\libtersafe.so");
            labStatus.Text = "Exiting";
            this.Close();
        }

        private void bMin_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void bTeam_Click(object sender, EventArgs e)
        {
            labStatus.Text = "Trying to Launch Browser";
            Process.Start("https://discord.gg/Vx8dJjnxGK");
            labStatus.Text = "Ready.";
        }

        private void bSPAWN_Click(object sender, EventArgs e)
        {
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                CreateNoWindow = true,
                RedirectStandardInput = true,
                UseShellExecute = false
            };
            process.Start();
            using (process.StandardInput)
            {
                if (!spawner)
                {
                    labStatus.Text = "Spawn Rule Applied.";
                    process.StandardInput.WriteLine("netsh  advfirewall firewall add rule name=INSPAWN protocol=TCP dir=out program=\""+GagaPath+"\\Engine\\ProjectTitan.exe\" remoteport=0-65535 action=block");
                    spawner = !spawner;
                    bSPAWN.Text = "FINISH";

                    chNoAnim.Enabled = true;
                    chFlyCar.Enabled = true;
                    bInject.Enabled = true;
                    chAim.Enabled = true;

                    chLuffy.Enabled = true;
                    chNoGrass.Enabled = true;
                    chSpeedGame.Enabled = true;
                    chNoShake.Enabled = true;

                    chCross.Enabled = true;
                    chInstant.Enabled = true;
                    chZeroHS.Enabled = true;
                    chNoRecoil.Enabled = true;

                    chNoFog.Enabled = true;
                    chMagic.Enabled = true;
                    chSky.Enabled = true;
                    chIpadView.Enabled = true;
                }
                else
                {
                    labStatus.Text = "Spawn Rule Deleted.";
                    process.StandardInput.WriteLine("netsh advfirewall firewall delete rule name=INSPAWN");
                    spawner = !spawner;
                    bSPAWN.Text = "SPAWN";
                    chNoAnim.Enabled = false;
                    chFlyCar.Enabled = false;
                    bInject.Enabled = false;
                    chAim.Enabled = false;

                    chLuffy.Enabled = false;
                    chNoGrass.Enabled = false;
                    chSpeedGame.Enabled = false;
                    chNoShake.Enabled = false;

                    chCross.Enabled = false;
                    chInstant.Enabled = false;
                    chZeroHS.Enabled = false;
                    chNoRecoil.Enabled = false;

                    chNoFog.Enabled = false;
                    chMagic.Enabled = false;
                    chSky.Enabled = false;
                    chIpadView.Enabled = false;
                }
                process.StandardInput.Flush();
                process.StandardInput.Close();
                process.WaitForExit();
                process.Close();
            }
        }

        private async void Helper(string searchV, string replaceV, int tries)
        {
            while (tries > 0)
            {
                labPID.ForeColor = Color.Yellow;
                MemLib.OpenProcess(Convert.ToInt32(labPID.Text));
                labStatus.Text = "Scanning..." + tries;
                var enumerable = await MemLib.AoBScan(0x0, 0x7fffffffffff, searchV, false, false, "");
                container = "0x" + enumerable.FirstOrDefault().ToString("X");
                Mem.MemoryProtection memoryProtection;
                MemLib.ChangeProtection(container, Mem.MemoryProtection.ReadWrite, out memoryProtection, "");
                foreach (long num in enumerable)
                {
                    labStatus.Text = "Replacing...";
                    MemLib.WriteMemory(num.ToString("X"), "bytes", replaceV, "", null);
                }
                Mem.MemoryProtection memoryProtectionAgain;
                MemLib.ChangeProtection(container, Mem.MemoryProtection.ReadOnly, out memoryProtectionAgain, "");
                tries -= 1;
            }
            labStatus.Text = "Done AOB Replcement.";
            labStatus.ForeColor = Color.Pink;
        }
        /// <summary>
        /// Removes/Allows access to a file/folder in Windows for secret files.
        /// While the SYSTEM is allowed full access, users are abstained.
        /// </summary>
        /// <param name="filewithpath"></param> Give path to the file or folder
        /// <param name="permissionargs"></param> Give permission "allow" or "deny"

        private void chNoFog_CheckedChanged(object sender, EventArgs e)
        {
            if (chNoFog.Checked)
            {
                Helper("44 65 66 61 75 6C 74 5F 5F 45 78 70 6F 6E 65 6E 74 69 61 6C 48 65 69 67 68 74 46 6F 67", "00 65 66 61 75 6C 74 5F 5F 45 78 70 6F 6E 65 6E 74 69 61 6C 48 65 69 67 68 74 46 6F 00", 3);
            }
            else
            {
                Helper("00 65 66 61 75 6C 74 5F 5F 45 78 70 6F 6E 65 6E 74 69 61 6C 48 65 69 67 68 74 46 6F 00", "44 65 66 61 75 6C 74 5F 5F 45 78 70 6F 6E 65 6E 74 69 61 6C 48 65 69 67 68 74 46 6F 67", 3);
            }
        }

        private void chIpadView_CheckedChanged(object sender, EventArgs e)
        {
            if(chIpadView.Checked)
            {
                Helper("00 00 B4 43 80 F7 5E 03 04 9F 41 03 30 48", "00 00 82 43 80 F7 5E 03 04 9F 41 03 30 48", 3);
            }
            else
            {
                Helper("00 00 82 43 80 F7 5E 03 04 9F 41 03 30 48", "00 00 B4 43 80 F7 5E 03 04 9F 41 03 30 48", 3);
            }
            
        }


        private void chCross_CheckedChanged(object sender, EventArgs e)
        {
            if (chCross.Checked)
            {
                Helper("44 1A 94 ED 01 0A 20 EE 00 0A 38 EE 10 0A 10 EE", "00 00 00 00 01 0A 20 EE 00 0A 38 EE 10 0A 10 EE", 3);
            }
            else
            {
                Helper("00 00 00 00 01 0A 20 EE 00 0A 38 EE 10 0A 10 EE", "44 1A 94 ED 01 0A 20 EE 00 0A 38 EE 10 0A 10 EE", 3);
            }
        }

        private void chNoRecoil_CheckedChanged(object sender, EventArgs e)
        {
            if (chNoRecoil.Checked)
            {
                                                       //52 65 63 6F 69 6C 49 6E 66 6F
                Helper("52 65 63 6F 69 6C 49 6E 66 6F", "00 65 63 6F 69 6C 49 6E 66 00", 10);
            }
            else
            {
                Helper("00 65 63 6F 69 6C 49 6E 66 00", "52 65 63 6F 69 6C 49 6E 66 6F", 10);
            }
        }

        private void chZeroHS_CheckedChanged(object sender, EventArgs e)
        {
            if (chZeroHS.Checked)
            {
                Helper("CD CC CC 3D 00 00 00 00 C0 F3 45 03 B4 F3 45 03 A8 F3 45 03 00 00", "00 00 20 42 00 00 00 00 C0 F3 45 03 B4 F3 45 03 A8 F3 45 03 00 00", 3);
            }
            else
            {
                Helper("00 00 20 42 00 00 00 00 C0 F3 45 03 B4 F3 45 03 A8 F3 45 03 00 00", "CD CC CC 3D 00 00 00 00 C0 F3 45 03 B4 F3 45 03 A8 F3 45 03 00 00", 3);
            }
        }

        private void chInstant_CheckedChanged(object sender, EventArgs e)
        {
            if (chInstant.Checked)
            {
                Helper("B8 41 00 00 C8 41 00 00 F4 41", "B8 43 00 00 AF 96 00 00 AF 43", 3);
            }
            else
            {
                Helper("B8 43 00 00 AF 96 00 00 AF 43", "B8 41 00 00 C8 41 00 00 F4 41", 3);
            }
        }

        private void chMagic_CheckedChanged(object sender, EventArgs e)
        {
            if (chMagic.Checked)
            {
                Helper("B8 41 00 00 C8 41 00 00 F4 41", "5C 43 00 00 B8 41 00 00 5C 43", 3);

            }
            else
            {
                Helper("5C 43 00 00 B8 41 00 00 5C 43", "B8 41 00 00 C8 41 00 00 F4 41", 3);

            }
        }

        private void chSky_CheckedChanged(object sender, EventArgs e)
        {
            if (chSky.Checked)
            {
                Helper("00 00 C8 42 00 00 00 00 00 E0 C4 48", "00 00 C8 C2 00 00 00 00 00 E0 C4 48", 3);
            }
            else
            {
                Helper("00 00 C8 C2 00 00 00 00 00 E0 C4 48", "00 00 C8 42 00 00 00 00 00 E0 C4 48", 3);
            }
        }

        private void chLuffy_CheckedChanged(object sender, EventArgs e)
        {
            if (chLuffy.Checked)
            {
                Helper("95 2D BC 41 00 80 6F 39", "00 00 C8 42 00 00 48 43", 3);
            }
            else
            {
                Helper("00 00 C8 42 00 00 48 43", "95 2D BC 41 00 80 6F 39", 3);
            }
        }

        private void chNoShake_CheckedChanged(object sender, EventArgs e)
        {
            if (chNoShake.Checked)
            {
                Helper("43 61 6D 65 72 61 53 68 61 6B 65 54", "00 61 6D 65 72 61 53 68 61 6B 65 00", 3);
            }
            else
            {
                Helper("00 61 6D 65 72 61 53 68 61 6B 65 00", "43 61 6D 65 72 61 53 68 61 6B 65 54", 3);
            }
        }

        private void chNoGrass_CheckedChanged(object sender, EventArgs e)
        {
            if (chNoGrass.Checked)
            {
                Helper("44 65 66 61 75 6C 74 5F 5F 4D 61 74 65 72 69 61 6C 45 78 70 72 65 73 73 69 6F 6E 4C 61 6E 64 73 63 61 70 65 47 72 61 73 73 4F 75 74 70 75 74", "00 65 66 61 75 6C 74 5F 5F 4D 61 74 65 72 69 61 6C 45 78 70 72 65 73 73 69 6F 6E 4C 61 6E 64 73 63 61 70 65 47 72 61 73 73 4F 75 74 70 75 00", 3);
            }
            else
            {
                Helper("00 65 66 61 75 6C 74 5F 5F 4D 61 74 65 72 69 61 6C 45 78 70 72 65 73 73 69 6F 6E 4C 61 6E 64 73 63 61 70 65 47 72 61 73 73 4F 75 74 70 75 00", "44 65 66 61 75 6C 74 5F 5F 4D 61 74 65 72 69 61 6C 45 78 70 72 65 73 73 69 6F 6E 4C 61 6E 64 73 63 61 70 65 47 72 61 73 73 4F 75 74 70 75 74", 3);
            }
        }

        private void chSpeedGame_CheckedChanged(object sender, EventArgs e)
        {
            if (chSpeedGame.Checked)
            {
                                                                                                             //00 00 80 3F 00 00 80 3F 00 00 80 3F 17 B7 D1 38 00 00 A0 41 6F 12 03 3A CD CC CC 3E 
                Helper("00 00 80 3F 00 00 80 3F 00 00 80 3F 17 B7 D1 38 00 00 A0 41 6F 12 03 3A CD CC CC 3E", "66 66 96 3F 00 00 80 3F 00 00 80 3F 17 B7 D1 38 00 00 A0 41 6F 12 03 3A CD CC CC 3E", 3);
            }
            else
            {
                Helper("66 66 96 3F 00 00 80 3F 00 00 80 3F 17 B7 D1 38 00 00 A0 41 6F 12 03 3A CD CC CC 3E", "00 00 80 3F 00 00 80 3F 00 00 80 3F 17 B7 D1 38 00 00 A0 41 6F 12 03 3A CD CC CC 3E", 3);
            }
        }

        private void chAim_CheckedChanged(object sender, EventArgs e)
        {
            if (chAim.Checked)
            {
                                                                                                          //00 00 B4 43 00 00 B4 C3 00 00 34 43 17 B7 D1 38 00 00 00 00 10 3A 03 EE 10 1A 02
                Helper("00 00 B4 43 00 00 B4 C3 00 00 34 43 17 B7 D1 38 00 00 00 00 10 3A 03 EE 10 1A 02", "00 00 B4 43 00 00 B4 C3 00 00 34 43 00 3C 1C 46 00 00 00 00 10 3A 03 EE 10 1A 02", 3);
            }
            else
            {
                Helper("00 00 B4 43 00 00 B4 C3 00 00 34 43 00 3C 1C 46 00 00 00 00 10 3A 03 EE 10 1A 02", "00 00 B4 43 00 00 B4 C3 00 00 34 43 17 B7 D1 38 00 00 00 00 10 3A 03 EE 10 1A 02", 3);
            }
        }

        private void chFlyCar_CheckedChanged(object sender, EventArgs e)
        {
            if(chFlyCar.Checked)
            {
                Helper("A5 0A 9F ED 10 0A 01 EE", "00 00 00 00 10 0A 01 EE", 3);
            }
            else
            {
                Helper("00 00 00 00 10 0A 01 EE", "A5 0A 9F ED 10 0A 01 EE", 3);
            }
        }

        private void chNoAnim_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void bInject_Click(object sender, EventArgs e)
        {

        }

        private void labTemp_Click(object sender, EventArgs e)
        {

        }

        private void fMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void labStatus_Click(object sender, EventArgs e)
        {

        }

        private void labPID_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Bypass Method Require KMOD driver, here memory.dll
        /// </summary>

        /// <summary>
        /// Start/Stop Button Click Callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

    }
}
