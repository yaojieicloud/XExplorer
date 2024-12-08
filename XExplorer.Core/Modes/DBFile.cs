namespace XExplorer.Core.Modes;

/// <summary>
/// Represents a database file within the XExplorer.Core.Modes namespace.
/// This class provides functionality to handle and manage database files
/// utilized by the application. It serves as an abstraction layer over
/// the actual file operations, ensuring encapsulation and ease of use.
/// </summary>
public class DBFile
{
    /// <summary>
    /// Gets or sets the name of the database file.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the directory path where the database file is located.
    /// </summary>
    public string Dir { get; set; }

    /// <summary>
    /// Gets or sets the full name of the database file, including its directory path.
    /// </summary>
    public string FullName { get; set; }
}