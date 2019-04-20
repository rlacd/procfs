using DokanNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace procfs
{
    public class ProcFS : IDokanOperations
    {
        private Dictionary<string, FileInformation> fileInfos = new Dictionary<string, FileInformation>();
        private Dictionary<string, Process> pcache = new Dictionary<string, Process>();
        private Dictionary<string, ProcessModule> mcache = new Dictionary<string, ProcessModule>();

        public void Cleanup(string fileName, DokanFileInfo info)
        {

        }

        public void CloseFile(string fileName, DokanFileInfo info)
        {

        }

        public NtStatus CreateFile(string fileName, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, DokanFileInfo info)
        {
            return NtStatus.Success;
        }

        public NtStatus DeleteDirectory(string fileName, DokanFileInfo info)
        {
            return DokanResult.NotImplemented;
        }

        public NtStatus DeleteFile(string fileName, DokanFileInfo info)
        {
            return DokanResult.NotImplemented;
        }

        public NtStatus FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info)
        {
            files = new List<FileInformation>();
            if(fileName == "\\")
            {
                pcache.Clear();
                foreach(var p in Process.GetProcesses())
                {
                    try
                    {
                        FileInformation fi = new FileInformation()
                        {
                            Attributes = FileAttributes.Directory,
                            CreationTime = DateTime.Now,
                            LastAccessTime = DateTime.Now,
                            FileName = p.ProcessName + "." + p.Id,
                            LastWriteTime = DateTime.Now,
                            Length = p.WorkingSet64
                        };
                        fileInfos["\\" + p.ProcessName + "." + p.Id] = fi;
                        files.Add(fi);
                        pcache["\\" + p.ProcessName + "." + p.Id] = p;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                return DokanResult.Success;
            }
            foreach(var p in pcache.Keys)
            {
                if(fileName == p)
                {
                    files.Add(new FileInformation()
                    {
                        FileName = "modules",
                        Attributes = FileAttributes.Directory,
                        CreationTime = DateTime.Now,
                        LastAccessTime = DateTime.Now,
                        LastWriteTime = DateTime.Now,
                        Length = 0
                    });
                }
                else if(fileName == (p + "\\modules"))
                {
                    var proc = pcache[p];
                    foreach (ProcessModule mod in proc.Modules)
                    {

                        mcache["\\" + proc.ProcessName + "." + proc.Id + "\\" + mod.FileName] = mod;
                    }
                }
            }
            return DokanResult.Success;
        }

        public NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files, DokanFileInfo info)
        {
            files = new List<FileInformation>();
            return DokanResult.NotImplemented;
        }

        public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, DokanFileInfo info)
        {
            streams = new List<FileInformation>();
            return DokanResult.NotImplemented;
        }

        public NtStatus FlushFileBuffers(string fileName, DokanFileInfo info)
        {
            return DokanResult.NotImplemented;
        }

        public NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, DokanFileInfo info)
        {
            freeBytesAvailable = 1024 * 1024 * 1024;
            totalNumberOfBytes = 0;
            totalNumberOfFreeBytes = 1024 * 1024 * 1024;
            return DokanResult.Success;
        }

        public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info)
        {
            if(fileInfos.ContainsKey(fileName))
            {
                fileInfo = fileInfos[fileName];
            }
            else
            {
                fileInfo = new FileInformation();
                fileInfo.FileName = "null";
                fileInfo.Length = 0;
                fileInfo.Attributes = FileAttributes.Normal | FileAttributes.Hidden;
            }
            return DokanResult.Success;
        }

        public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info)
        {
            security = null;
            return DokanResult.NotImplemented;
        }

        public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, out uint maximumComponentLength, DokanFileInfo info)
        {
            volumeLabel = "ProcFS";
            features = FileSystemFeatures.ReadOnlyVolume;
            fileSystemName = "PROCFS";
            maximumComponentLength = 256;
            return DokanResult.Success;
        }

        public NtStatus LockFile(string fileName, long offset, long length, DokanFileInfo info)
        {
            return DokanResult.NotImplemented;
        }

        public NtStatus Mounted(DokanFileInfo info)
        {
            return DokanResult.Success;
        }

        public NtStatus MoveFile(string oldName, string newName, bool replace, DokanFileInfo info)
        {
            return DokanResult.NotImplemented;
        }

        public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, DokanFileInfo info)
        {
            bytesRead = 0;
            return DokanResult.Success;
        }

        public NtStatus SetAllocationSize(string fileName, long length, DokanFileInfo info)
        {
            return DokanResult.NotImplemented;
        }

        public NtStatus SetEndOfFile(string fileName, long length, DokanFileInfo info)
        {
            return DokanResult.NotImplemented;
        }

        public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info)
        {
            return DokanResult.NotImplemented;
        }

        public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info)
        {
            return DokanResult.NotImplemented;
        }

        public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, DokanFileInfo info)
        {
            return DokanResult.NotImplemented;
        }

        public NtStatus UnlockFile(string fileName, long offset, long length, DokanFileInfo info)
        {
            return DokanResult.NotImplemented;
        }

        public NtStatus Unmounted(DokanFileInfo info)
        {
            return DokanResult.Success;
        }

        public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, DokanFileInfo info)
        {
            bytesWritten = 0;
            return DokanResult.Success;
        }
    }
}
