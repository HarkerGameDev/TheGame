using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace Game
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// Automatically updates application, and restarts application when done
        /// </summary>
        [STAThread]
        static void Main()
        {
#if !DEBUG && false
            // Attempt to auto update
            try
            {
                string tempFile = Path.GetTempFileName();
                new System.Net.WebClient().DownloadFile("https://github.com/HarkerGameDev/TheGame/raw/master/install/GameInstaller.msi", tempFile);

                using (Database db = new Database(tempFile))
                {
                    View dv = db.OpenView("SELECT `Value` FROM `Property` WHERE `Property`='ProductVersion'");
                    dv.Execute();
                    Record record = dv.Fetch();
                    string str = record.GetString(1);
                    dv.Close();
                    db.DeleteOnClose(tempFile);

                    Version newVer = new Version(str);
                    Version curVer = Assembly.GetExecutingAssembly().GetName().Version;
                    if (newVer > curVer)
                    {
                        ProcessStartInfo Info = new ProcessStartInfo();
                        Info.Arguments = "/C start /wait msiexec /i \"" + tempFile + "\" /passive & start \"\" \"" + Assembly.GetExecutingAssembly().Location + "\"";
                        Info.WindowStyle = ProcessWindowStyle.Hidden;
                        Info.CreateNoWindow = true;
                        Info.FileName = "cmd.exe";
                        Process.Start(Info);
                        Environment.Exit(1);
                    }
                }
            }
            catch (Exception e)
            {
                string logDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Game\";
                Directory.CreateDirectory(logDir);
                File.AppendAllText(logDir + "log.txt", e.ToString() + "\r\n");
            }
#endif

            using (var game = new Source.Game1())
                game.Run();
        }
    }
#endif
}
