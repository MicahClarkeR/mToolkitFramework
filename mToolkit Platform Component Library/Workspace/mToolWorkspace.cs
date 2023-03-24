using mToolkitPlatformComponentLibrary.Workspace.Files;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;

namespace mToolkitPlatformComponentLibrary.Workspace
{
    /// <summary>
    /// Represents a workspace for a tool, containing files and directories.
    /// </summary>
    public class mToolWorkspace
    {
        /// <summary>
        /// The path to the workspace.
        /// </summary>
        public readonly string Path;

        /// <summary>
        /// The root directory of the workspace.
        /// </summary>
        private readonly DirectoryInfo Root;

        /// <summary>
        /// A dictionary of files in the workspace.
        /// </summary>
        private readonly Dictionary<string, mWorkspaceFile> Files = new Dictionary<string, mWorkspaceFile>();

        /// <summary>
        /// The owner of the workspace.
        /// </summary>
        private readonly mTool Owner;

        /// <summary>
        /// Initializes a new instance of the <see cref="mToolWorkspace"/> class.
        /// </summary>
        /// <param name="owner">The owner of the workspace.</param>
        public mToolWorkspace(mTool owner)
        {
            Owner = owner;
            Path = $"{Owner.CurrentParentDirectory.FullName}\\Workspace\\";
            Root = new DirectoryInfo(Path);
        }

        /// <summary>
        /// Creates a new file in the workspace with the specified key.
        /// </summary>
        /// <param name="key">The key of the new file.</param>
        /// <param name="content">The content of the new file.</param>
        /// <param name="overwrite">Whether to overwrite an existing file with the same key.</param>
        /// <returns>The new file.</returns>
        public mWorkspaceFile Create(string key, byte[]? content = null, bool overwrite = false)
        {
            if (Files.ContainsKey(key))
            {
                if (!overwrite)
                    return Files[key];

                Remove(key);
            }

            string workspacePath = $"{Path}{key}";
            mWorkspaceFile file = new mWorkspaceFile(workspacePath);

            if (content?.Length != 0 && content != null)
                file.SetContents(content);

            Files.Add(key, file);
            return file;
        }

        /// <summary>
        /// Creates a new file in the workspace with the specified key and content.
        /// </summary>
        /// <param name="key">The key of the new file.</param>
        /// <param name="content">The content of the new file.</param>
        /// <returns>The new file.</returns>
        public mWorkspaceFile Create(string key, string? content)
        {
            return Create(key, Encoding.UTF8.GetBytes(content ?? string.Empty));
        }

        /// <summary>
        /// Copies a file from the source path into the workspace with the specified key.
        /// </summary>
        /// <param name="sourcePath">The path of the source file to copy.</param>
        /// <param name="key">The key of the new file in the workspace.</param>
        /// <returns>The new reference file, or null if the file already exists in the workspace but is not a reference file.</returns>
        public mWorkspaceReferenceFile? CopyFileIntoWorkspace(string sourcePath, string key)
        {
            if (Files.ContainsKey(key))
            {
                mWorkspaceFile suspectFile = Files[key];

                if (suspectFile.GetType() != typeof(mWorkspaceReferenceFile))
                {
                    Owner.CurrentLog.Error($"File {sourcePath} exists in workspace but is not a Reference File, instead it is {suspectFile.GetType().Name}");
                    return null;
                }

                mWorkspaceReferenceFile suspect = (mWorkspaceReferenceFile)suspectFile;

                if (!suspect.IsDifferentFromSource)
                    return suspect;
                else
                {
                    Remove(key);
                }
            }

            string workspacePath = $"{Path}{key}";
            CopyFile(sourcePath, workspacePath);

            mWorkspaceReferenceFile file = new mWorkspaceReferenceFile(workspacePath, sourcePath);
            Files.Add(key, file);
            return file;
        }

        /// <summary>
        /// Removes the file with the specified key from the workspace.
        /// </summary>
        /// <param name="key">The key of the file to remove.</param>
        public void Remove(string key)
        {
            if (Files.ContainsKey(key))
            {
                mWorkspaceFile file = Files[key];
                file.Delete();
                Files.Remove(key);
            }
        }

        /// <summary>
        /// Removes all files from the workspace.
        /// </summary>
        public void Clear()
        {
            string[] keys = Files.Keys.ToArray();
            foreach (string key in keys)
                Remove(key);

            if (Root.GetFiles()?.Length == 0)
                Root.Delete(true);
        }

        /// <summary>
        /// Gets the files at the specified path in the workspace.
        /// </summary>
        /// <param name="path">The path to get the files at.</param>
        /// <returns>An array of file names.</returns>
        public string[] GetFilesAt(string path)
        {
            if (Directory.Exists($"{Path}{path}"))
                return Directory.GetFiles($"{Path}{path}").Select(s => s.Replace(Path, "")).ToArray();

            return new string[0];
        }

        /// <summary>
        /// Copies a file from the source path to the target path.
        /// </summary>
        /// <param name="source">The source path of the file.</param>
        /// <param name="target">The target path of the file.</param>
        internal static void CopyFile(string source, string target)
        {
            FileInfo tempFileInfo = new FileInfo(target);
            Directory.CreateDirectory(tempFileInfo.DirectoryName);
            File.Copy(source, target, true);
        }
    }
}
