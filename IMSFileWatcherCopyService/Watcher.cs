using System;
using System.IO;
using System.ServiceProcess;
using System.Threading;

namespace IMSFileWatcherCopyService
{
    public partial class Watcher : ServiceBase
    {
        private static NetworkConnectionInfo nci = new NetworkConnectionInfo();
        private static NetworkConnection SourceConnection;
        private FileWatcher fileWatcher;
        private bool maintainSourceConnection = true;

        public Watcher()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Log.WriteErrorLog("IMS File Watcher Copy service started");
            EstablishConnectionToSource();
            ThreadPool.QueueUserWorkItem(_ => FileCopier.CopyOldFiles(nci.SourceDirectory, nci.DestinationDirectory));
            fileWatcher = new FileWatcher(nci.SourceDirectory, nci.DestinationDirectory);
            fileWatcher.EnableWatcher();
            new Thread(delegate()
            {
                MaintainConnectionToSource();
            }).Start();
        }



        protected override void OnStop()
        {
            maintainSourceConnection = false;
            fileWatcher.DisableWatcher();
            fileWatcher.Dispose();
            SourceConnection.Dispose();
            Log.WriteErrorLog("IMS File Watcher Copy service stopped");
        }

        protected override void OnShutdown()
        {
            maintainSourceConnection = false;
            fileWatcher.Dispose();
            SourceConnection.Dispose();
            Log.WriteErrorLog("IMS File Watcher Copy service shutting down");
        }

        public static void EstablishConnectionToSource()
        {
            if(SourceConnection != null)
                SourceConnection.Dispose();
            SourceConnection = new NetworkConnection(nci.SourceServer, nci.SourceLoginCredentials);
            while (!Directory.Exists(nci.SourceDirectory)) //Check to ensure connection is made to Source
            {
                //Try every 2 minutes to make a connection if initial connection fails
                Thread.Sleep(2 * 60 * 1000);
                SourceConnection.Dispose();
                SourceConnection = new NetworkConnection(nci.SourceServer, nci.SourceLoginCredentials);
            }

            Log.WriteErrorLog(String.Format("Connected to {0}", nci.SourceServer)); //Log successful connection
        }

        private void MaintainConnectionToSource()
        {
            while (maintainSourceConnection)
            {
                bool needToExit = MaintainConnectionToSourceDelayTimer();
                if (needToExit)
                    break;
                    
                if (!Directory.Exists(nci.SourceDirectory))
                {
                    Log.WriteErrorLog(String.Format("Connection to {0} has failed", nci.SourceServer));
                    fileWatcher.DisableWatcher();
                    EstablishConnectionToSource();
                    ThreadPool.QueueUserWorkItem(_ => FileCopier.CopyOldFiles(nci.SourceDirectory, nci.DestinationDirectory));
                    fileWatcher.EnableWatcher();
                }
            }
        }

        private bool MaintainConnectionToSourceDelayTimer()
        {
            bool needToExit = false;
            TimeSpan delayTime = new TimeSpan(0, 15, 0);
            DateTime startTime = DateTime.Now;
            DateTime currentTime = DateTime.Now;
            TimeSpan elapsedTime = currentTime - startTime;
            while (elapsedTime<delayTime)
            {
                Thread.Sleep(60000);
                if (!maintainSourceConnection)
                {
                    needToExit = true;
                    break;
                }
                currentTime = DateTime.Now;
                elapsedTime = currentTime - startTime;
            }
            return needToExit;
        }
    } //.class
}//.namespace
