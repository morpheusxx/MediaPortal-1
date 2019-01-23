﻿using System;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;

namespace Mediaportal.TV.Server.TVLibrary.Services
{
  public class SettingService : ISettingService
  {
    
    public Setting GetSetting(string tagName)
    {
      return SettingsManagement.GetSetting(tagName);
    }

    public Setting GetSettingWithDefaultValue(string tagName, string defaultValue)
    {
      return SettingsManagement.GetSetting(tagName, defaultValue);
    }

    public void SaveSetting(string tagName, string defaultValue)
    {
      SettingsManagement.SaveSetting(tagName, defaultValue);
    }

    public void SaveValue (string tagName, int defaultValue)
    {
      SettingsManagement.SaveValue(tagName, defaultValue);
    }

    public void SaveValue (string tagName, double defaultValue)
    {
      SettingsManagement.SaveValue(tagName, defaultValue);
    }

    public void SaveValue (string tagName, bool defaultValue)
    {
      SettingsManagement.SaveValue(tagName, defaultValue);
    }

    public void SaveValue (string tagName, string defaultValue)
    {
      SettingsManagement.SaveValue(tagName, defaultValue);
    }

    public void SaveValue (string tagName, DateTime defaultValue)
    {
      SettingsManagement.SaveValue(tagName, defaultValue);
    }

    public int GetValue(string tagName, int defaultValue)
    {
      return SettingsManagement.GetValue(tagName, defaultValue);
    }

    public double GetValue(string tagName, double defaultValue)
    {
      return SettingsManagement.GetValue(tagName, defaultValue);
    }

    public bool GetValue(string tagName, bool defaultValue)
    {
      return SettingsManagement.GetValue(tagName, defaultValue);
    }

    public string GetValue(string tagName, string defaultValue)
    {
      return SettingsManagement.GetValue(tagName, defaultValue);
    }

    public DateTime GetValue(string tagName, DateTime defaultValue)
    {
      return SettingsManagement.GetValue(tagName, defaultValue);
    }

    public void DeleteSetting(string tagName)
    {
      SettingsManagement.DeleteSetting(tagName);
    } 
  }
}
