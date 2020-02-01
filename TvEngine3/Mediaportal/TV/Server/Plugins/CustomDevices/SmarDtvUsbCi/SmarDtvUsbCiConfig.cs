﻿#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using DirectShowLib;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.SmarDtvUsbCi
{
  public partial class SmarDtvUsbCiConfig : SectionSettings
  {
    private ReadOnlyCollection<SmarDtvUsbCiProduct> _products = null;
    private MPComboBox[] _tunerSelections = null;
    private MPLabel[] _installStateLabels = null;

    public SmarDtvUsbCiConfig()
      : this("SmarDTV USB CI")
    {
    }

    public SmarDtvUsbCiConfig(string name)
      : base(name)
    {
      this.LogDebug("SmarDTV USB CI config: constructing");
      _products = SmarDtvUsbCiProduct.GetProductList();
      _tunerSelections = new MPComboBox[_products.Count];
      _installStateLabels = new MPLabel[_products.Count];
      InitializeComponent();
      this.LogDebug("SmarDTV USB CI config: constructed");
    }

    public override void SaveSettings()
    {
      this.LogDebug("SmarDTV USB CI config: saving settings");
      for (int i = 0; i < _products.Count; i++)
      {
        Card selectedTuner = (Card)_tunerSelections[i].SelectedItem;
        if (_tunerSelections[i].Enabled && selectedTuner != null)
        {
          this.LogDebug("  {0} linked to tuner {1} {2}", _products[i].ProductName, selectedTuner.Name, selectedTuner.DevicePath);
          ServiceAgents.Instance.SettingServiceAgent.SaveValue(_products[i].DbSettingName, selectedTuner.DevicePath);
        }
      }
      base.SaveSettings();
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("SmarDTV USB CI config: activated");
      IList<Card> allTuners = ServiceAgents.Instance.CardServiceAgent.ListAllCards();
      DsDevice[] captureDevices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCapture);

      try
      {
        for (int i = 0; i < _products.Count; i++)
        {
          this.LogDebug("SmarDTV USB CI config: product {0}...", _products[i].ProductName);

          // Populate the tuner selection fields and set current values.
          string tunerDevicePath = ServiceAgents.Instance.SettingServiceAgent.GetValue(_products[i].DbSettingName, string.Empty);
          _tunerSelections[i].Items.Clear();
          _tunerSelections[i].SelectedIndex = -1;

          foreach (Card tuner in allTuners)
          {
            CardType tunerType = ServiceAgents.Instance.ControllerServiceAgent.Type(tuner.CardId);
            if (tunerType == CardType.Analog || tunerType == CardType.Unknown)
            {
              continue;
            }
            _tunerSelections[i].Items.Add(tuner);
            if (tuner.DevicePath.Equals(tunerDevicePath))
            {
              this.LogDebug("  currently linked to tuner {0} {1}", tuner.Name, tuner.DevicePath);
              _tunerSelections[i].SelectedItem = tuner;
            }
          }

          // Check whether the CI is installed in the system. We disable the
          // selection field if it is not installed.
          bool found = false;
          _tunerSelections[i].Enabled = false;
          foreach (DsDevice device in captureDevices)
          {
            if (device.Name != null)
            {
              if (device.Name.Equals(_products[i].WdmDeviceName))
              {
                this.LogDebug("  WDM driver installed");
                _installStateLabels[i].Text = "The " + _products[i].ProductName + " is installed with the WDM driver.";
                _installStateLabels[i].ForeColor = Color.Orange;
                found = true;
                break;
              }
              else if (device.Name.Equals(_products[i].BdaDeviceName))
              {
                this.LogDebug("  BDA driver installed");
                _installStateLabels[i].Text = "The " + _products[i].ProductName + " is installed correctly.";
                _installStateLabels[i].ForeColor = Color.ForestGreen;
                _tunerSelections[i].Enabled = true;
                found = true;
                break;
              }
            }
          }
          if (!found)
          {
            this.LogDebug("  driver not installed");
            _installStateLabels[i].Text = "The " + _products[i].ProductName + " is not detected.";
            _installStateLabels[i].ForeColor = Color.Red;
          }
        }
      }
      finally
      {
        foreach (DsDevice d in captureDevices)
        {
          d.Dispose();
        }
      }

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("SmarDTV USB CI config: deactivated");
      SaveSettings();
      base.OnSectionDeActivated();
    }

    public override bool CanActivate
    {
      get
      {
        // The section can always be activated (disabling it might be confusing for people), but we don't
        // necessarily enable all of the tuner selection fields.
        return true;
      }
    }
  }
}
