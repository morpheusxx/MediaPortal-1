using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Context;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities.Cache;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class SettingsManagement
  {
    private static int _epgKeepDuration;
    public static int EpgKeepDuration
    {
      get
      {
        if (_epgKeepDuration == 0)
        {
          // first time query settings, caching
          _epgKeepDuration = GetValue("epgKeepDuration", 24);
        }
        return _epgKeepDuration;
      }
    }


    /// <summary>
    /// saves a value to the database table "Setting"
    /// </summary>
    public static Setting SaveSetting(string tagName, string value)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        Setting setting = context.Settings.FirstOrDefault(s => s.Tag == tagName);
        if (setting == null)
        {
          setting = new Setting { Value = value, Tag = tagName };
          context.Settings.Add(setting);
        }
        else
        {
          setting.Value = value;
        }

        context.SaveChanges(true);
        return setting;
      }
    }

    public static Setting GetOrSaveSetting(string tagName, string defaultValue)
    {
      if (defaultValue == null)
      {
        return null;
      }
      if (string.IsNullOrEmpty(tagName))
      {
        return null;
      }

      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        Setting setting = context.Settings.FirstOrDefault(s => s.Tag == tagName);
        if (setting == null)
        {
          setting = new Setting { Value = defaultValue, Tag = tagName };
          context.Settings.Add(setting);
          context.SaveChanges(true);
        }
        return setting;
      }
    }

    /// <summary>
    /// gets a value from the database table "Setting"
    /// </summary>
    /// <returns>A Setting object with the stored value, if it doesn't exist a empty string will be the value</returns>
    public static Setting GetSetting(string tagName)
    {
      var setting = GetOrSaveSetting(tagName, string.Empty);
      return setting;
    }

    /// <summary>
    /// Deletes a setting from the database
    /// </summary>
    public static void DeleteSetting(string tagName)
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        var setting = context.Settings.FirstOrDefault(s => s.Tag == tagName);
        if (setting == null)
          return;
        context.Settings.Remove(setting);
        context.SaveChanges(true);
      }
    }

    //public static Setting GetSetting(string tagName)
    //{
    //  Setting setting = EntityCacheHelper.Instance.SettingCache.GetOrUpdateFromCache(tagName,
    //              delegate
    //                {
    //                  using (TvEngineDbContext context = new TvEngineDbContext())
    //                  {
    //                    return settingsRepository.GetSetting(tagName);
    //                  }
    //                }
    //    );
    //  return setting;
    //}

    //public static void SaveSetting(string tagName, string value)
    //{
    //  using (ISettingsRepository settingsRepository = new SettingsRepository(true))
    //  {
    //    Setting setting = settingsRepository.SaveSetting(tagName, value);
    //    EntityCacheHelper.Instance.SettingCache.AddOrUpdateCache(tagName, setting);
    //  }
    //}

    public static void SaveValue(string tagName, int defaultValue)
    {
      SaveSetting(tagName, defaultValue.ToString(CultureInfo.InvariantCulture));
    }

    public static void SaveValue(string tagName, double defaultValue)
    {
      SaveSetting(tagName, defaultValue.ToString(CultureInfo.InvariantCulture));
    }

    public static void SaveValue(string tagName, bool defaultValue)
    {
      SaveSetting(tagName, defaultValue.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
    }

    public static void SaveValue(string tagName, string defaultValue)
    {
      SaveSetting(tagName, defaultValue);
    }

    public static void SaveValue(string tagName, DateTime defaultValue)
    {
      SaveSetting(tagName, defaultValue.ToString(CultureInfo.InvariantCulture));
    }

    public static Setting GetSetting(string tagName, string defaultValue)
    {
      //Setting setting = EntityCacheHelper.Instance.SettingCache.GetFromCache(tagName);

      //if (setting == null)
      //{
      //  using (ISettingsRepository settingsRepository = new SettingsRepository(true))
      //  {
      //    setting = settingsRepository.GetOrSaveSetting(tagName, defaultValue);
      var setting = GetOrSaveSetting(tagName, defaultValue);
      //    EntityCacheHelper.Instance.SettingCache.AddOrUpdateCache(tagName, setting);
      //  }
      //}
      return setting;
    }

    public static int GetValue(string tagName, int defaultValue)
    {
      Setting setting = GetSetting(tagName, defaultValue.ToString(CultureInfo.InvariantCulture));
      int number;
      bool parsed = int.TryParse(setting.Value, out number);
      if (!parsed)
      {
        number = defaultValue;
      }
      return number;
    }

    public static double GetValue(string tagName, double defaultValue)
    {
      Setting setting = GetSetting(tagName, defaultValue.ToString(CultureInfo.InvariantCulture));
      double number;
      bool parsed = double.TryParse(setting.Value, out number);
      if (!parsed)
      {
        number = defaultValue;
      }
      return number;
    }

    public static bool GetValue(string tagName, bool defaultValue)
    {
      Setting setting = GetSetting(tagName, defaultValue.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
      return setting.Value == "true";
    }

    public static string GetValue(string tagName, string defaultValue)
    {
      Setting setting = GetSetting(tagName, defaultValue);
      return setting.Value;
    }

    public static DateTime GetValue(string tagName, DateTime defaultValue)
    {
      Setting setting = GetSetting(tagName, defaultValue.ToString(CultureInfo.InvariantCulture));
      return string.IsNullOrEmpty(setting.Value) ? DateTime.MinValue : DateTime.Parse(setting.Value, CultureInfo.InvariantCulture);
    }

    public static IList<Setting> ListAllSettings()
    {
      using (TvEngineDbContext context = new TvEngineDbContext())
      {
        IQueryable<Setting> settings = context.Settings;
        return settings.ToList();
      }
    }

    //public static void DeleteSetting(string tagName)
    //{
    //  using (ISettingsRepository settingsRepository = new SettingsRepository(true))
    //  {
    //    settingsRepository.DeleteSetting(tagName);
    //  }
    //}
  }
}