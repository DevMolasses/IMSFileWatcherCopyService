using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IMSFileWatcherCopyService
{
    public class NetworkConnectionInfo
    {
        public string SourceServer { get; set; }
        public string SourceDirectory { get; set; }
        public string DestinationServer { get; set; }
        public string DestinationDirectory { get; set; }
        public System.Net.NetworkCredential SourceLoginCredentials { get; set; }

        public NetworkConnectionInfo()
        {
            #region Directory Paths
            SourceServer = Properties.Settings.Default.SourceServer;
            SourceDirectory = SourceServer + Properties.Settings.Default.SourceDirectory;
            DestinationServer = Properties.Settings.Default.DestinationServer;
            DestinationDirectory = DestinationServer + Properties.Settings.Default.DestinationDirectory;
            #endregion

            #region Login Credentials
            SourceLoginCredentials = new System.Net.NetworkCredential();
            SourceLoginCredentials.Domain = Properties.Settings.Default.SourceDomain;
            SourceLoginCredentials.UserName = Properties.Settings.Default.SourceUsername;
            SourceLoginCredentials.Password = Properties.Settings.Default.SourcePassword;
            #endregion
        }
    }
}
