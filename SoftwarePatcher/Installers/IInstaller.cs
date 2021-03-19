namespace SoftwarePatcher.Installers
{
    public interface IInstaller
    {
        bool Install(string source, string dirDestination, string dirTemp, bool deleteSource = true);
    }
}