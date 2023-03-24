using System.IO.Compression;
using System.Reflection;

namespace mToolkitPlatformComponentLibrary
{
    /// <summary>
    /// Provides functionality to manage tools in the tool repository.
    /// </summary>
    public class ToolRepository
    {
        /// <summary>
        /// The directory where the tool repository is located.
        /// </summary>
        private static readonly string RepositoryDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Repository");

        /// <summary>
        /// Retrieves the list of zip files available in the tool repository.
        /// </summary>
        /// <returns>An array of file paths of the zip files in the repository.</returns>
        public static string[] GetRepoItems()
        {
            return Directory.GetFiles(RepositoryDirectory, "*.zip");
        }

        /// <summary>
        /// Installs a tool from a zip file to the tools directory.
        /// </summary>
        /// <param name="zipFile">The path to the zip file containing the tool to be installed.</param>
        public static void InstallTool(string zipFile, string? folderName)
        {
            // Create the Tools directory if it does not exist
            EnsureToolsDirectoryExists();

            string extractPath = GetUniqueToolDirectoryPath(zipFile, folderName);

            ZipFile.ExtractToDirectory(zipFile, extractPath);
        }

        /// <summary>
        /// Ensures the tools directory exists. If it does not exist, creates it.
        /// </summary>
        private static void EnsureToolsDirectoryExists()
        {
            if (!Directory.Exists(mToolkit.ToolDirectory))
            {
                Directory.CreateDirectory(mToolkit.ToolDirectory);
            }
        }

        /// <summary>
        /// Gets a unique directory path for the tool, based on the zip file name.
        /// If the path already exists, adds a random string to the path to make it unique.
        /// </summary>
        /// <param name="zipFile">The path to the zip file containing the tool.</param>
        /// <returns>A unique directory path for the tool.</returns>
        private static string GetUniqueToolDirectoryPath(string zipFile, string? folderName)
        {
            string extractPath = Path.Combine(mToolkit.ToolDirectory, Path.GetFileNameWithoutExtension(zipFile));

            if (folderName != null)
            {
                extractPath = $"{Directory.GetParent(extractPath).FullName}\\{folderName}";
            }

            if (Directory.Exists(extractPath))
            {
                string random = Guid.NewGuid().ToString()[0..4];
                extractPath = $"{extractPath} ({random})";
            }

            return extractPath;
        }
    }

}
