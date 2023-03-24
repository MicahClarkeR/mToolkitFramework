using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mToolkitPlatformComponentLibrary.Workspace.Files
{
    /// <summary>
    /// Represents a file in a tool workspace.
    /// </summary>
    public class mWorkspaceFile
    {
        /// <summary>
        /// The path of the file.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// The file info of the file.
        /// </summary>
        public FileInfo FileInfo { get { return new FileInfo(Path); } }

        /// <summary>
        /// The stream of the file.
        /// </summary>
        public mWorkspaceFileStream Stream { get; protected set; }

        /// <summary>
        /// The URI of the file.
        /// </summary>
        public Uri Uri { get { return new Uri(Path); } }

        /// <summary>
        /// Initializes a new instance of the <see cref="mWorkspaceFile"/> class.
        /// </summary>
        /// <param name="workspacePath">The path of the file.</param>
        public mWorkspaceFile(string workspacePath)
        {
            Path = workspacePath;
            SetFilePath(workspacePath);
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        public virtual void Delete()
        {
            File.Delete(Path);
        }

        /// <summary>
        /// Sets the contents of the file.
        /// </summary>
        /// <param name="contents">The content of the file.</param>
        public virtual void SetContents(string contents)
        {
            Stream.SetContents(Encoding.UTF8.GetBytes(contents));
        }

        /// <summary>
        /// Sets the contents of the file.
        /// </summary>
        /// <param name="contents">The content of the file.</param>
        public virtual void SetContents(byte[] contents)
        {
            Stream.SetContents(contents);
        }

        /// <summary>
        /// Sets the file path of the file.
        /// </summary>
        /// <param name="path">The file path of the file.</param>
        protected virtual void SetFilePath(string path)
        {
            Stream = new mWorkspaceFileStream(path);
        }

        /// <summary>
        /// Generates a new GUID.
        /// </summary>
        /// <returns>A new GUID.</returns>
        public static string GetGUID() => Guid.NewGuid().ToString();
    }
}
