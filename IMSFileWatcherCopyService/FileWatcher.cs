using System.IO;
using System.Threading;

namespace IMSFileWatcherCopyService
{
    class FileWatcher : FileSystemWatcher
    {
        private FileWatcher fileWatcher { get; set; }
        private FileWatcher() { }
        /// <summary>
        /// Establishes a SystemFileWatcher for the purpose of copying files from the
        /// source directory to the destination directory as soon as the files are
        /// created in the source directory
        /// </summary>
        /// <param name="src">Source Directory</param>
        /// <param name="dest">Destination Directory</param>
        public FileWatcher(string src, string dest)
        {
            fileWatcher = new FileWatcher();
            try
            {
                fileWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite |
                    NotifyFilters.FileName | NotifyFilters.DirectoryName;
                fileWatcher.Filter = "*.csv";
                fileWatcher.Created += new FileSystemEventHandler((sender, e) => OnCreated(sender, e, src, dest));
                fileWatcher.Renamed += new RenamedEventHandler((sender, e) => OnRenamed(sender, e, src, dest));
                fileWatcher.IncludeSubdirectories = true;
                fileWatcher.Path = src;
            }
            catch (System.Exception e)
            {
                Log.WriteErrorLog(e);
                throw;
            }
        }
        
        /// <summary>
        /// Enables the FileWatcher to raise events and logs that it has done so
        /// </summary>
        public void EnableWatcher()
        {
            fileWatcher.EnableRaisingEvents = true;
            Log.WriteErrorLog("Enabled file watcher");
        }

        /// <summary>
        /// Disables the FileWatcher from raising events and logs that it has done so
        /// </summary>
        public void DisableWatcher()
        {
            fileWatcher.EnableRaisingEvents = false;
            Log.WriteErrorLog("Disabled file watcher");
        }

        /// <summary>
        /// Copies all files the FileWatcher catches as Renamed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="src">Source Directory</param>
        /// <param name="dest">Destination Directory</param>
        private void OnRenamed(object sender, RenamedEventArgs e, string src, string dest)
        {
            ThreadPool.QueueUserWorkItem(_ => FileCopier.CopyFile(e.FullPath, src, dest));
        }

        /// <summary>
        /// Copies all files the FileWatcher catches as Created
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="src">Source Directory</param>
        /// <param name="dest">Destination Directory</param>
        private void OnCreated(object sender, FileSystemEventArgs e, string src, string dest)
        {
            ThreadPool.QueueUserWorkItem(_ => FileCopier.CopyFile(e.FullPath, src, dest));
        }
    }
}
