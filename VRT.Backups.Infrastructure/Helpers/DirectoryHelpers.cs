namespace VRT.Backups.Infrastructure.Helpers;
public static class DirectoryHelpers
{
    public static string GetExecutingAssemblyDirectory()
    {
        return AppContext.BaseDirectory;
    }
}
