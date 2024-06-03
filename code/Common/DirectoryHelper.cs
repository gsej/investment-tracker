using System.Reflection;

namespace Common;

public static class DirectoryHelper
{
    // We walk up the from where we're running from to find the SampleData folder, unless we've been given a configured directory
    // in appsettings.json, in which case that will be used. The file structure is important and the names of the files are significant. 
    // This is documented in the readme in the SampleData folder.
    private const string DefaultDataFolderName = "SampleData";

    public static string GetDataFolder(string configuredDataFolder)
    {
        var dataFolder = configuredDataFolder;

        if (!string.IsNullOrWhiteSpace(dataFolder))
        {
            return dataFolder;
        }

        var location = Assembly.GetExecutingAssembly().Location;
        var directory = new FileInfo(location).Directory;

        while (directory != null && directory.EnumerateDirectories().All(d => d.Name != DefaultDataFolderName))
        {
            directory = directory.Parent;
        }

        if (directory == null)
        {
            throw new InvalidOperationException("could not find data folder");
        }

        dataFolder = directory.EnumerateDirectories().Single(d => d.Name == DefaultDataFolderName).FullName;

        return dataFolder;
    }
}
