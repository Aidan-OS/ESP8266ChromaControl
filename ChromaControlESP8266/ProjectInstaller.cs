using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;
using System.ServiceProcess;
using System.IO;

namespace ChromaControlESP8266
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();

            this.AfterInstall += new InstallEventHandler(ServiceInstaller_AfterInstall);
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

            String installLocation = this.Context.Parameters["assemblypath"];
            String ipAddresses = this.Context.Parameters["WEBADDRESSES"];

            String[] splits = installLocation.Split('\\');
            String installRootDir = "";
            for (int i = 0; i < splits.Length - 1; i++)
            {
                installRootDir += splits[i] + "\\";
            }

            File.WriteAllText(installRootDir + "ipaddresses.txt", ipAddresses);
        }

        void ServiceInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            using (ServiceController sc = new ServiceController(serviceInstaller1.ServiceName))
            {
                sc.Start();
            }
        }
    }
}
