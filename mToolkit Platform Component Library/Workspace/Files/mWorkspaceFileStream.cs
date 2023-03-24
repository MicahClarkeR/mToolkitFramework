using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace mToolkitPlatformComponentLibrary.Workspace.Files
{
    /// <summary>
    /// Represents a stream of a file in a tool workspace.
    /// </summary>
    public class mWorkspaceFileStream
    {
        /// <summary>
        /// The file system event handler for the stream.
        /// </summary>
        public FileSystemEventHandler? OnChange;

        /// <summary>
        /// Gets a file stream for the stream.
        /// </summary>
        /// <returns>A file stream.</returns>
        public FileStream GetFileStream() => CreateFileStream(Path);

        /// <summary>
        /// Gets a memory stream for the stream.
        /// </summary>
        /// <returns>A memory stream.</returns>
        public MemoryStream GetMemoryStream() => GetMemoryStream(Path);

        /// <summary>
        /// Gets the bytes of the file for the stream.
        /// </summary>
        /// <returns>An array of bytes.</returns>
        public byte[] GetFileBytes() => GetFileBytes(Path);

        /// <summary>
        /// Gets the string contents of the file for the stream.
        /// </summary>
        /// <returns>The string contents of the file.</returns>
        public string GetStringContents() => File.ReadAllText(Path);

        /// <summary>
        /// The path of the file for the stream.
        /// </summary>
        private readonly string Path;

        /// <summary>
        /// Initializes a new instance of the <see cref="mWorkspaceFileStream"/> class.
        /// </summary>
        /// <param name="path">The path of the file for the stream.</param>
        /// <param name="content">The content of the file.</param>
        public mWorkspaceFileStream(string path, byte[] content = null)
        {
            Path = $"{path}";
            Directory.CreateDirectory(new FileInfo(Path).DirectoryName);

            if (content != null)
                SetContents(content);
        }

        /// <summary>
        /// Sets the contents of the file for the stream.
        /// </summary>
        /// <param name="data">The content to set.</param>
        public void SetContents(byte[] data)
        {
            FileStream stream = GetFileStream();
            stream.Flush();
            stream.Write(data, 0, data.Length);
            stream.Close();
        }

        /// <summary>
        /// Creates a file stream for the specified file path.
        /// </summary>
        /// <param name="path">The file path to create the file stream for.</param>
        /// <returns>A file stream for the specified file path.</returns>
        internal static FileStream CreateFileStream(string path) => File.Open(path, FileMode.OpenOrCreate);

        /// <summary>
        /// Gets a memory stream for the specified file path.
        /// </summary>
        /// <param name="path">The file path to get the memory stream for.</param>
        /// <returns>A memory stream for the specified file path.</returns>
        internal static MemoryStream GetMemoryStream(string path) => new MemoryStream(GetFileBytes(path));

        /// <summary>
        /// Gets the bytes of the file at the specified path.
        /// </summary>
        /// <param name="path">The path of the file to get the bytes for.</param>
        /// <returns>An array of bytes for the file at the specified path.</returns>
        internal static byte[] GetFileBytes(string path) => File.ReadAllBytes(path);
    }

}
