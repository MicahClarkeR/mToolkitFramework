using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mToolkitPlatformComponentLibrary.Workspace.Files
{
    /// <summary>
    /// Represents a reference to a file in a tool workspace.
    /// </summary>
    public class mWorkspaceReferenceFile : mWorkspaceFile
    {
        /// <summary>
        /// Gets a value indicating whether the source file has been deleted.
        /// </summary>
        public bool SourceDeleted { get; private set; } = false;

        /// <summary>
        /// Gets a value indicating whether the reference file is different from the source file.
        /// </summary>
        public bool IsDifferentFromSource => IsDifferentFromSourceMethod(this);

        /// <summary>
        /// The path of the source file.
        /// </summary>
        private string SourcePath;

        /// <summary>
        /// The file system watcher for the reference file.
        /// </summary>
        public FileSystemWatcher Watcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="mWorkspaceReferenceFile"/> class.
        /// </summary>
        /// <param name="workspacePath">The path of the workspace file.</param>
        /// <param name="sourcePath">The path of the source file.</param>
        public mWorkspaceReferenceFile(string workspacePath, string sourcePath) : base(workspacePath)
        {
            SourcePath = sourcePath;
            SetupWatcher(sourcePath);
        }

        /// <summary>
        /// Sets up the file system watcher for the reference file.
        /// </summary>
        /// <param name="path">The path of the source file.</param>
        private void SetupWatcher(string path)
        {
            FileInfo fileInfo = new FileInfo(path);

            if (fileInfo.Exists && fileInfo.DirectoryName != null)
            {
                Watcher = new FileSystemWatcher
                {
                    Path = fileInfo.DirectoryName,
                    NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                    Filter = fileInfo.Name
                };

                Watcher.Changed += new FileSystemEventHandler(OnChanged);
                Watcher.Created += new FileSystemEventHandler(OnChanged);
                Watcher.Deleted += new FileSystemEventHandler(OnChanged);
                Watcher.Renamed += new RenamedEventHandler(OnRenamed);
                Watcher.EnableRaisingEvents = true;
            }
        }

        /// <summary>
        /// Reloads the reference file.
        /// </summary>
        public void Reload()
        {
            FileInfo fI = FileInfo;
            string newWorkflowPath = $"{fI.Directory.FullName}{fI.Name}-{GetGUID()}.{fI.Extension}";
            mToolWorkspace.CopyFile(SourcePath, newWorkflowPath);
            mWorkspaceFileStream newWorlflowStream = new mWorkspaceFileStream(newWorkflowPath);
            mWorkspaceFileStream currentWorkflowStream = Stream;
            Stream = newWorlflowStream;
        }

        /// <summary>
        /// Handles the OnRenamed event of the file system watcher.
        /// </summary>
        private void OnRenamed(object source, RenamedEventArgs e)
        {
            Console.WriteLine($"File: {e.OldFullPath} renamed to {e.FullPath}");
            SetFilePath(SourcePath);

            Stream.OnChange?.Invoke(source, e);
        }

        /// <summary>
        /// Handles the OnChanged event of the file system watcher.
        /// </summary>
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");

            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                Reload();
            }
            else if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                SourceDeleted = true;
            }

            Stream.OnChange?.Invoke(source, e);
        }

        /// <summary>
        /// Determines whether the reference file is different from the source file.
        /// </summary>
        /// <param name="file">The reference file.</param>
        /// <returns>True if the reference file is different from the source file, false otherwise.</returns>
        private static bool IsDifferentFromSourceMethod(mWorkspaceReferenceFile file)
        {
            FileStream sourceStream = new mWorkspaceFileStream(file.SourcePath).GetFileStream();
            FileStream workspaceStream = file.Stream.GetFileStream();

            if (sourceStream.Length != workspaceStream.Length)
            {
                sourceStream.Close();
                workspaceStream.Close();
                return false;
            }

            int file1Byte, file2Byte;

            do
            {
                file1Byte = sourceStream.ReadByte();
                file2Byte = workspaceStream.ReadByte();
            }
            while ((file1Byte == file2Byte) && (file1Byte != -1));

            sourceStream.Close();
            workspaceStream.Close();

            return ((file1Byte - file2Byte) == 0);
        }
    }
}
