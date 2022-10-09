namespace Wohnungssuche
{
  public static class Cache
  {
    private static string GetFilePath(Wohnung item)
    {
      if (item == null) throw new ArgumentNullException(nameof(item));

      return Path.Combine(Path.GetTempPath(), $"Wohnung-{item.Id}.tmp");
    }
    public static bool IsKnown(Wohnung item)
    {
      return File.Exists(GetFilePath(item));
    }

    public static void MakeKnown(Wohnung item)
    {
      File.Create(GetFilePath(item)).Close();
    }
  }
}