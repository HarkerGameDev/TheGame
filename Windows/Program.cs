using System;
using System.IO;
using Source;
using Microsoft.Deployment.WindowsInstaller;

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
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Attempt to auto update
            try
            {
                //string tempFile = "Update.msi";
                string tempFile = Path.GetTempFileName();
                new System.Net.WebClient().DownloadFile("https://github.com/HarkerGameDev/TheGame/raw/master/install/GameInstaller.msi", tempFile);

                //Installer installer = (Installer)Activator.CreateInstance(Type.GetTypeFromProgID("WindowsInstaller.Installer"));
                //Database db = installer.OpenDatabase(tempFile, 0);
                //View dv = db.OpenView("SELECT `Value` FROM `Property` WHERE `Property`='ProductVersion'");
                //dv.Execute();
                //Record record = dv.Fetch();
                //string str = record.get_StringData(1);
                //dv.Close();

                using (Database db = new Database(tempFile))
                {
                    View dv = db.OpenView("SELECT `Value` FROM `Property` WHERE `Property`='ProductVersion'");
                    dv.Execute();
                    Record record = dv.Fetch();
                    string str = record.GetString(1);
                    dv.Close();
                    db.DeleteOnClose(tempFile);

                    Version newVer = new Version(str);
                    Version curVer = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                    if (newVer > curVer)
                    {
                        //Console.WriteLine(newVer + "\n" + curVer);
                        //Installer.SetInternalUI(InstallUIOptions.Basic);
                        Installer.InstallProduct(tempFile, "");
                    }
                }
            }
            catch (Exception e)
            {
                string logDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Game\";
                Directory.CreateDirectory(logDir);
                File.AppendAllText(logDir + "log.txt", e.ToString() + "\r\n");
            }

            using (var game = new Game1())
                game.Run();
        }
    }
#endif
}
