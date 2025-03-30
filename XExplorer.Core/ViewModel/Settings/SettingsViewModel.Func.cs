using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using Serilog;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace XExplorer.Core.ViewModel.Settings;

public partial class SettingsViewModel
{
    #region ZIP

    private void ExtractArchive(string archivePath, List<string> passwords, string outputDirectory)
    {
        if (string.IsNullOrEmpty(archivePath))
            throw new ArgumentException("Archive path cannot be null or empty.", nameof(archivePath));

        if (!File.Exists(archivePath))
            throw new FileNotFoundException("The archive file does not exist.", archivePath);

        if (string.IsNullOrEmpty(outputDirectory))
            throw new ArgumentException("Output directory cannot be null or empty.", nameof(outputDirectory));

        if (!Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory); // Ensure the output directory exists

        var success = false;
        var stb = new StringBuilder();
        stb.AppendLine();
        stb.AppendLine("----------------------------------------------------------------------------------------------------------------------------------------------");
        stb.AppendLine($"Start extracting archive [{archivePath}], target directory [{outputDirectory}]...");
        foreach (var password in passwords)
        {
            try
            {
                stb.AppendLine($"Trying password: {password}");

                var opt = string.IsNullOrWhiteSpace(password)
                    ? new ReaderOptions()
                    : new ReaderOptions() { Password = password };
                using var archive = ArchiveFactory.Open(File.OpenRead(archivePath), opt);
                foreach (var entry in archive.Entries)
                {
                    if (!entry.IsDirectory)
                    {
                        stb.AppendLine($"Extracting: {entry.Key}");
                        entry.WriteToDirectory(outputDirectory, new ExtractionOptions
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }

                stb.AppendLine($"Successfully extracted the archive using password: {password}");
                success = true;
                break; // Exit loop if extraction is successful
            }
            catch (InvalidOperationException ex)
            {
                stb.AppendLine($"Password '{password}' failed. Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                stb.AppendLine($"An error occurred: {ex.Message}");
            }
        }

        if (!success)
            stb.AppendLine($"Failed to extract the archive with the provided passwords.[{archivePath}]");
        else
            File.Delete(archivePath);

        stb.AppendLine("----------------------------------------------------------------------------------------------------------------------------------------------");
        Log.Information(stb.ToString());
    }

    #endregion
}