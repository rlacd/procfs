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
        private Dictionary<string, byte[]> pvalc = new Dictionary<string, byte[]>();

        public void Cleanup(string fileName, DokanFileInfo info)
        {
            (info.Context as MemoryStream)?.Dispose();
            info.Context = null;
        }

        public void CloseFile(string fileName, DokanFileInfo info)
        {
            (info.Context as MemoryStream)?.Dispose();
            info.Context = null;
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
                mcache.Clear();
                //pvalc.Clear(); //Clear content buffer (we are out of the process folder)
                foreach (var p in Process.GetProcesses())
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
                    //pvalc.Clear();
                    mcache.Clear();
                    var proc = pcache[p];
                    files.Add(new FileInformation()
                    {
                        FileName = "modules",
                        Attributes = FileAttributes.Directory,
                        CreationTime = DateTime.Now,
                        LastAccessTime = DateTime.Now,
                        LastWriteTime = DateTime.Now,
                        Length = 0
                    });
                    var presp = proc.Responding.ToString();
                    files.Add(CreateBasicFileInfo("IsResponding", presp.Length));
                    pvalc[p + "\\IsResponding"] = UTF8Encoding.UTF8.GetBytes(presp);

                    //files.Add(CreateBasicFileInfo("stdout", proc.StandardOutput.ReadToEnd().Length));
                    //files.Add(CreateBasicFileInfo("stderr", proc.StandardError.ReadToEnd().Length));
                    
                }
                else if(fileName == (p + "\\modules"))
                {
                    var proc = pcache[p];
                    foreach (ProcessModule mod in proc.Modules)
                    {
                        files.Add(new FileInformation()
                        {
                            FileName = mod.ModuleName,
                            CreationTime = DateTime.Now,
                            LastAccessTime = DateTime.Now,
                            LastWriteTime = DateTime.Now,
                            Length = 0,
                            Attributes = FileAttributes.Directory
                        });
                        mcache["\\" + proc.ProcessName + "." + proc.Id + "\\modules\\" + mod.ModuleName] = mod;
                    }
                }
                else
                {
                    foreach(var m in mcache.Keys)
                    {
                        if(fileName == m)
                        {
                            var mod = mcache[m];
                            files.Add(CreateBasicFileInfo("BaseAddress", 1));
                            //pvalc[m + "\\BaseAddress"] = UTF8Encoding.UTF8.GetBytes(mod.BaseAddress.ToString());
                            files.Add(CreateBasicFileInfo("FileName", mod.FileName.Length));
                            pvalc[m + "\\FileName"] = UTF8Encoding.UTF8.GetBytes(mod.FileName);
                            files.Add(CreateBasicFileInfo("VersionInfo", mod.FileVersionInfo.ToString().Length));
                            pvalc[m + "\\VersionInfo"] = UTF8Encoding.UTF8.GetBytes(mod.FileVersionInfo.ToString());
                            files.Add(CreateBasicFileInfo("Module", new FileInfo(mod.FileName).Length));
                            pvalc[m + "\\Module"] = File.ReadAllBytes(mod.FileName);
                            files.Add(CreateBasicFileInfo("EntryPoint", 1));
                            //pvalc[m + "\\EntryPoint"] = File.ReadAllBytes(mod.EntryPointAddress.ToString());
                            return DokanResult.Success;
                        }
                    }
                }
            }
            return DokanResult.Success;
        }

        private FileInformation CreateBasicFileInfo(string fname, long length = 0)
        {
            return new FileInformation()
            {
                FileName = fname,
                CreationTime = DateTime.Now,
                LastAccessTime = DateTime.Now,
                LastWriteTime = DateTime.Now,
                Length = length,
                Attributes = FileAttributes.Normal
            };
        }

        private FileInformation CreateBasicDirInfo(string fname)
        {
            return new FileInformation()
            {
                FileName = fname,
                CreationTime = DateTime.Now,
                LastAccessTime = DateTime.Now,
                LastWriteTime = DateTime.Now,
                Length = 0,
                Attributes = FileAttributes.Directory
            };
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
            try
            {
                ((MemoryStream)(info.Context)).Flush();
                return DokanResult.Success;
            }
            catch (IOException)
            {
                return DokanResult.Error;
            }
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
            try
            {
                //((MemoryStream)(info.Context)).Lock(offset, length);
                return DokanResult.Success;
            }
            catch (IOException)
            {
                return DokanResult.AccessDenied;
            }
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
            if(!pvalc.ContainsKey(fileName))
            {
                bytesRead = 0;
                return DokanResult.Success;
            }
            if (info.Context == null) // memory mapped read
            {
                using (var stream = new MemoryStream(pvalc[fileName]))
                {
                    stream.Position = offset;
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                }
            }
            else // normal read
            {
                var stream = info.Context as MemoryStream;
                lock (stream) //Protect from overlapped read
                {
                    stream.Position = offset;
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                }
            }
            return DokanResult.Success;
        }

        public NtStatus SetAllocationSize(string fileName, long length, DokanFileInfo info)
        {
            try
            {
                ((MemoryStream)(info.Context)).SetLength(length);
                return DokanResult.Success;
            }
            catch (Exception)
            {
                return DokanResult.DiskFull;
            }
        }

        public NtStatus SetEndOfFile(string fileName, long length, DokanFileInfo info)
        {
            try
            {
                ((MemoryStream)(info.Context)).SetLength(length);
                return DokanResult.Success;
            }
            catch (Exception)
            {
                return DokanResult.DiskFull;
            }
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
            return DokanResult.Success;
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
