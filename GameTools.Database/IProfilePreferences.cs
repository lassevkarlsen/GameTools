namespace GameTools.Database;

public interface IProfilePreferences
{
    Task<string> GetPreference(Guid profileId, string key);
    Task SetPreference(Guid profileId, string key, string value);
}