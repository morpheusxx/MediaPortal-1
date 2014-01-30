#region Copyright (C) 2005-2011 Team MediaPortal

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

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  public partial class GroupSelectionForm : Form
  {
    private readonly string _preselectedGroupName = string.Empty;
    private readonly List<object> _groups = new List<object>();

    readonly Dictionary<MediaTypeEnum, ICollection<string>> _knownGroupNames = new Dictionary<MediaTypeEnum, ICollection<string>>();

    private SelectionType _selectionType = SelectionType.ForDeleting;

    public enum SelectionType
    {
      ForDeleting,
      ForRenaming
    }

    public SelectionType Selection
    {
      get { return _selectionType; }
      set { _selectionType = value; }
    }

    public MediaTypeEnum MediaType { get; set; }

    public GroupSelectionForm()
    {
      _knownGroupNames[MediaTypeEnum.TV] = new HashSet<string> { TvConstants.TvGroupNames.Analog, TvConstants.TvGroupNames.DVBC, TvConstants.TvGroupNames.DVBS, TvConstants.TvGroupNames.DVBT };
      _knownGroupNames[MediaTypeEnum.Radio] = new HashSet<string> { TvConstants.RadioGroupNames.Analog, TvConstants.RadioGroupNames.DVBC, TvConstants.RadioGroupNames.DVBS, TvConstants.RadioGroupNames.DVBT };
      InitializeComponent();
    }

    public GroupSelectionForm(string preselectedGroupName)
    {
      InitializeComponent();

      _preselectedGroupName = preselectedGroupName;
    }

    private void mpButton1_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
      Close();
    }

    private void mpButton2_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    protected override void OnShown(EventArgs e)
    {
      LoadGroups();
      base.OnShown(e);
    }

    private void LoadGroups()
    {
      IList<ChannelGroup> tmp = ServiceAgents.Instance.ChannelGroupServiceAgent.ListAllChannelGroupsByMediaType(MediaType);
      foreach (ChannelGroup group in tmp)
      {
        bool isFixedGroupName = MediaType == MediaTypeEnum.TV && group.GroupName == TvConstants.TvGroupNames.AllChannels ||
                                MediaType == MediaTypeEnum.Radio && group.GroupName == TvConstants.RadioGroupNames.AllChannels ||
                                _selectionType == SelectionType.ForRenaming && _knownGroupNames[MediaType].Contains(group.GroupName);

        if (!isFixedGroupName)
        {
          _groups.Add(group);
        }
      }

      try
      {
        foreach (object group in _groups)
        {
          string name =
            group.GetType().InvokeMember("GroupName", System.Reflection.BindingFlags.GetProperty, null, group, null).
              ToString();

          listBox1.Items.Add(name);

          if (name == _preselectedGroupName)
          {
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
          }
        }
      }
      catch (Exception exp)
      {
        this.LogError("LoadGroups error: {0}", exp.Message);
      }

      if (listBox1.SelectedIndex <= -1 && listBox1.Items.Count > 0)
      {
        listBox1.SelectedIndex = 0;
      }
    }

    public object Group
    {
      get
      {
        if (_groups == null || listBox1.SelectedIndex <= -1)
        {
          return null;
        }

        return _groups[listBox1.SelectedIndex];
      }
    }
  }
}