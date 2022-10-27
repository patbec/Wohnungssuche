using System;
using System.IO;
using System.Text.Json;

namespace Wohnungssuche
{
  public static class Cache
  {
    public static bool IsKnown(Wohnung item)
    {
      return File.Exists(GetFilePath(item));
    }

    public static void MakeKnown(Wohnung item)
    {
      if (item == null) throw new ArgumentNullException(nameof(item));

      FileInfo cacheFile = new(GetFilePath(item));

      if (!cacheFile.Directory.Exists)
      {
        cacheFile.Directory.Create();
      }

      // Serialize the Wohnungs object and write it to the cache file.
      using var cacheFileData = cacheFile.Create();

      cacheFileData.Write(
        JsonSerializer.SerializeToUtf8Bytes(item));
    }

    private static string GetFilePath(Wohnung item)
    {
      return Path.Combine("cache", $"tmp-{item.Id}.json");
    }
  }
}