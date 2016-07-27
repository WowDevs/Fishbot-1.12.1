using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace FishBot
{
    public partial class MainWindow : Form
    {
        private static Hook wowHook;
        private static Lua lua;

        private static bool IsFishing;

        private static List<ulong> lastBobberGuid;
        private int Caught;
        private IntPtr FirstObj;
        private bool Fish;

        public MainWindow()
        {
            InitializeComponent();
        }

        private static string Exe_Version => File.GetLastWriteTime(System.Reflection.Assembly.GetEntryAssembly().Location).ToString("yyyy.MM.dd");

        private readonly int LocalVersion = int.Parse(Application.ProductVersion.Split('.')[0]);

        private void Form1_Load(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = string.Format(toolStripStatusLabel1.Text, Exe_Version);
            toolStripStatusLabel3.Text = string.Format(toolStripStatusLabel3.Text, LocalVersion);

            Log.Initialize(LogTextBox, this);

            Shown += MainWindow_Shown;
            FormClosing += MainWindow_FormClosing;
        }

        private void MainWindow_Shown(object sender, EventArgs e)
        {
            try
            {
                Log.Write("Attempting to connect to running WoW.exe process...", Color.Black);

                var proc = Process.GetProcessesByName("WoW").FirstOrDefault();

                while (proc == null)
                {
                    var res = MessageBox.Show("Please open WoW, and login, and select your character before using the bot.", "FishBot", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);

                    if (res == DialogResult.Cancel)
                    {
                        Application.Exit();
                        return;
                    }

                    proc = Process.GetProcessesByName("WoW").FirstOrDefault();
                }

                wowHook = new Hook(proc);
                wowHook.InstallHook();
                lua = new Lua(wowHook);

                Log.Write("Connected to process with ID = " + proc.Id, Color.Black);

                textBox1.Text = wowHook.Memory.ReadString(Offsets.PlayerName, Encoding.UTF8, 512, true);

                Log.Write("Base Address = " + wowHook.Process.BaseOffset().ToString("X"));

                Log.Write("Target GUID = " + wowHook.Memory.Read<ulong>(Offsets.TargetGUID, true));

                var objMgr = wowHook.Memory.Read<IntPtr>(Offsets.CurMgrPointer, true);
                var curObj = wowHook.Memory.Read<IntPtr>(IntPtr.Add(objMgr, (int) Offsets.FirstObjectOffset));

                FirstObj = curObj;

                Log.Write("First object located @ memory location 0x" + FirstObj.ToString("X"), Color.Black);

                //Thread mouseOver = new Thread(delegate() 
                //    { 
                //        for (;;)
                //        {
                //            Log.Write("MouseOverGUID = " + wowHook.Memory.Read<UInt64>(Offsets.MouseOverGUID, false).ToString("X"));
                //            Thread.Sleep(1000);
                //        }
                //    });
                //mouseOver.Start();

                //lua.DoString("DoEmote('dance')");

                Log.Write("Click 'Fish' to begin fishing.", Color.Green);
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message, Color.Red);
            }
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            wowHook?.DisposeHooking();
        }

        private void cmdFish_Click(object sender, EventArgs e)
        {
            lastBobberGuid = new List<ulong>();

            cmdStop.Enabled = true;
            cmdFish.Enabled = false;

            SystemSounds.Asterisk.Play();

            Fish = !Fish;

            while (Fish)
            {
                try
                {
                    Application.DoEvents();

                    if (!IsFishing)
                    {
                        Log.Write("Fishing...", Color.Black);
                        lua.CastSpellByName("Fishing");
                        Thread.Sleep(200); // Give the lure a chance to be placed in the water before we start scanning for it
                        // 200 ms is a good length, most people play with under that latency
                        IsFishing = true;
                    }

                    var curObj = FirstObj;

                    while (curObj.ToInt64() != 0 && (curObj.ToInt64() & 1) == 0)
                    {
                        var type = wowHook.Memory.Read<int>(curObj + Offsets.Type);
                        var cGUID = wowHook.Memory.Read<ulong>(curObj + Offsets.LocalGUID);

                        //if (cGUID == )

                        if (lastBobberGuid.Count == 5) // Only keep the last 5 bobber GUID's (explained below * )
                        {
                            lastBobberGuid.RemoveAt(0);
                            lastBobberGuid.TrimExcess();
                        }

                        if ((type == 5) && !lastBobberGuid.Contains(cGUID)) // 5 = Game Object, and ensure that we not finding a bobber we already clicked
                        {
                            // * wow likes leaving the old bobbers in the game world for a while
                            var objectName = wowHook.Memory.ReadString(
                                wowHook.Memory.Read<IntPtr>(
                                    wowHook.Memory.Read<IntPtr>(curObj + Offsets.ObjectName1) + Offsets.ObjectName2
                                    ),
                                Encoding.UTF8, 50
                                );

                            if (objectName == "Fishing Bobber")
                            {
                                var bobberState = wowHook.Memory.Read<byte>(curObj + Offsets.BobberState);

                                if (bobberState == 1) // Fish has been caught
                                {
                                    Caught++;
                                    textBox2.Text = Caught.ToString();

                                    Log.Write("Caught something, hopefully a fish!", Color.Black);

                                    wowHook.Memory.Write(Offsets.MouseOverGUID, cGUID);
                                    Thread.Sleep(100);

                                    //lua.DoString(string.Format("InteractUnit('mouseover')"));
                                    lua.OnRightClickObject((uint) curObj, 1);

                                    lastBobberGuid.Add(cGUID);
                                    Thread.Sleep(200);

                                    IsFishing = false;
                                    break;
                                }
                            }
                        }

                        var nextObj = wowHook.Memory.Read<IntPtr>(IntPtr.Add(curObj, (int) Offsets.NextObjectOffset));
                        if (nextObj == curObj)
                            break;
                        curObj = nextObj;
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex.Message, Color.Red);
                }
            }
        }

        private void cmdStop_Click(object sender, EventArgs e)
        {
            cmdStop.Enabled = false;
            cmdFish.Enabled = true;

            SystemSounds.Asterisk.Play();
            Fish = false;
        }
    }
}