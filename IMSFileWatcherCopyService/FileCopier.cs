using System;
using System.IO;
using System.Threading;
using System.ServiceProcess;

namespace IMSFileWatcherCopyService
{

    class FileCopier
    {
        /// <summary>
        /// Copies all files to the destination directory that haven't already been copied
        /// </summary>
        /// <remarks>
        /// Requires that the source file names be time stamps formatted as yyyyMMdd_HHmmss
        /// </remarks>
        /// <param name="src">Source Directory</param>
        /// <param name="dest">Destination Directory</param>
        internal static void CopyOldFiles(string src, string dest)
        {
            long newestFile = getNewestFile(dest);
            string[] srcFiles = Directory.GetFiles(src);

            for (int i = 0; i < srcFiles.Length; i++)
            {
                if (!Directory.Exists(src))
                {
                    Log.WriteErrorLog("Connection to source has failed");
                    Watcher.EstablishConnectionToSource();
                    srcFiles = Directory.GetFiles(src);
                    i = 0;
                }
                string srcFile = srcFiles[i];
                if (getDateStringAsLong(srcFile) > newestFile)
                    CopyFile(srcFile, src, dest);
            }
        }

        /// <summary>
        /// Copies a file from the source directory to the destination directory
        /// </summary>
        /// <remarks>
        /// Requires the srcFile name to be formatted as yyyyMMdd_HHmmss.
        /// Will create a folder named yyyyMM in the destination directory if one does not already exist.
        /// Attempts to make the copy 100 times before giving up and logging the failure.
        /// Thread sleeps for 1 second after a failed copy attempt before trying again.
        /// </remarks>
        /// <param name="srcFile">Source File Path</param>
        /// <param name="src">Source Directory</param>
        /// <param name="dest">Destination Directory</param>
        internal static void CopyFile(string srcFile, string src, string dest)
        {
            string destFile = srcFile.Replace(src, dest);
            destFile = destFile.Insert(destFile.LastIndexOf('\\') + 1, Path.GetFileName(destFile).Substring(0, 6) + '\\');
            if (!Directory.Exists(Path.GetDirectoryName(destFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(destFile));

            int tries = 0;
            int allowedTries = 100;
            bool fileCopied = false;
            while (tries < allowedTries)
            {
                tries++;
                try
                {
                    if (File.Exists(destFile))
                    {
                        Log.WriteErrorLog(String.Format("{0} already exists", Path.GetFileName(srcFile)));
                        fileCopied = false;
                        break;
                    }
                    else
                    {
                        File.Copy(srcFile, destFile);
                        fileCopied = true;
                        break;
                    }
                }
                catch
                {
                    Thread.Sleep(1000); //sleep for a second before trying again.
                    fileCopied = false;
                }
            }
            if (!fileCopied)
                Log.WriteErrorLog(String.Format("Unable to copy {0}", Path.GetFileName(srcFile)));
        }

        /// <summary>
        /// Finds the newest file by looking for the most recent time stamp
        /// </summary>
        /// <remarks>
        /// First looks at the folders in the detination directory to find the most recent folder.
        /// Folder names must be formatted as yyyyMM.
        /// Then looks at files in the most recent folder to find the most recent file
        /// File names must be formatted as yyyyMMdd_HHmmss
        /// </remarks>
        /// <param name="dest">Destination Directory</param>
        /// <returns>The newest files time stamp as long formatted as yyyyMMddHHmmss</returns>
        private static long getNewestFile(string dest)
        {
            string newestFolder = "";
            string newestFile = "";
            string[] destFolders;

            try
            {
                destFolders = Directory.GetDirectories(dest);
            }
            catch (Exception e)
            {
                Log.WriteErrorLog(e);
                destFolders = new string[0]; //This is not necessary once I get the permissions thing figured out
            }
            
            try
            {
                newestFolder = destFolders[0];
                foreach (string folder in destFolders)
                {
                    if (Convert.ToInt32(folder.Substring(folder.Length - 6)) >
                        Convert.ToInt32(newestFolder.Substring(newestFolder.Length - 6)))
                        newestFolder = folder;
                }
            }
            catch (Exception)
            {
                //newestFolder = dest + "\\000000";
                return 0;
            }

            try
            {
                string[] files = Directory.GetFiles(newestFolder);
                newestFile = files[0];
                foreach (string file in files)
                {
                    if (getDateStringAsLong(file) >
                        getDateStringAsLong(newestFile))
                        newestFile = file;
                }
            }
            catch (Exception)
            {
                //newestFile =  newestFolder + "\\00000000_000000.csv";
                return 0;
            }

            return getDateStringAsLong(newestFile);
        }

        /// <summary>
        /// Converts a timestamp to a long number
        /// </summary>
        /// <param name="file">File path with file name formatted as yyyyMMdd_HHmmss time stamp</param>
        /// <returns>yyyyMMddHHmmss as long</returns>
        private static long getDateStringAsLong(string file)
        {
            file = Path.GetFileNameWithoutExtension(file);
            return Convert.ToInt64(file.Substring(0, 8) + file.Substring(9, 6));
        }
    }
}
