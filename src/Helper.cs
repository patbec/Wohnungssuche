using System;
using System.IO;
using System.Net;
using System.Reflection;

namespace Wohnungssuche
{
  public static class Helper
  {
    public static Version GetVersion()
    {
      return Assembly.GetExecutingAssembly().GetName().Version;
    }

    /// <summary>
    /// Gets the specified environment variable.
    /// Additionally reading from a file is supported, see docker secrets for more informations.
    /// </summary>
    /// <param name="name">Environment variable with or without _FILE extension.</param>
    /// <returns>Contents of the environment variable.</returns>
    public static string GetEnvironmentVariable(string name)
    {
      string content = Environment.GetEnvironmentVariable(name);

      if (content == null)
      {
        // Checks if the environment variable is set as file.
        string filePath = Environment.GetEnvironmentVariable(name + "_FILE");

        if (filePath == null)
        {
          // Environment variable is not set.
          return null;
        }

        // Read the file (docker secret).
        content = File.ReadAllText(filePath);
      }

      return content;
    }
  }
}