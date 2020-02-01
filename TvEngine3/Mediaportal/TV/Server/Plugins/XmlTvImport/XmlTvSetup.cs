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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Mediaportal.TV.Server.Plugins.XmlTvImport.util;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.XmlTvImport
{
  public partial class XmlTvSetup : SectionSettings
  {


    private const string _shortTimePattern24Hrs = "HH:mm";
    private const string _shortTimePattern12Hrs = "hh:mm";

    private readonly IChannelGroupService _channelGroupServiceAgent = ServiceAgents.Instance.ChannelGroupServiceAgent;
    private readonly ISettingService _settingServiceAgent = ServiceAgents.Instance.SettingServiceAgent;
    private readonly IChannelService _channelServiceAgent = ServiceAgents.Instance.ChannelServiceAgent;

    public XmlTvSetup()
      : this("XmlTv")
    {
      ServiceAgents.Instance.AddGenericService<IXMLTVImportService>(); //XMLTVImportService
    }

    public XmlTvSetup(string name)
      : base(name)
    {
      InitializeComponent();
    }

    public override void OnSectionDeActivated()
    {
      SaveSettings();
      base.OnSectionDeActivated();
    }

    public override void SaveSettings()
    {
      _settingServiceAgent.SaveValue("xmlTv", textBoxFolder.Text);
      _settingServiceAgent.SaveValue("xmlTvUseTimeZone", checkBox1.Checked);
      _settingServiceAgent.SaveValue("xmlTvImportXML", cbImportXML.Checked);
      _settingServiceAgent.SaveValue("xmlTvImportLST", cbImportLST.Checked);
      _settingServiceAgent.SaveValue("xmlTvTimeZoneHours", textBoxHours.Text);
      _settingServiceAgent.SaveValue("xmlTvTimeZoneMins", textBoxMinutes.Text);
      _settingServiceAgent.SaveValue("xmlTvDeleteBeforeImport", checkBoxDeleteBeforeImport.Checked);
      _settingServiceAgent.SaveValue("xmlTvRemoteURL", txtRemoteURL.Text);

      var DTFI = new DateTimeFormatInfo();
      DTFI.ShortDatePattern = _shortTimePattern24Hrs;
      DateTime xmlTvRemoteScheduleTime = dateTimePickerScheduler.Value;
      _settingServiceAgent.SaveValue("xmlTvRemoteScheduleTime", xmlTvRemoteScheduleTime);
      _settingServiceAgent.SaveValue("xmlTvRemoteSchedulerEnabled", chkScheduler.Checked);
      _settingServiceAgent.SaveValue("xmlTvRemoteSchedulerDownloadOnWakeUpEnabled", radioDownloadOnWakeUp.Checked);
    }

    private class CBChannelGroup
    {
      public string groupName;
      public int idGroup;

      public CBChannelGroup(string groupName, int idGroup)
      {
        this.groupName = groupName;
        this.idGroup = idGroup;
      }

      public override string ToString()
      {
        return groupName;
      }
    }

    public override void OnSectionActivated()
    {
      UpdateRadioButtonsState();
      textBoxFolder.Text = _settingServiceAgent.GetValue("xmlTv", XmlTvImporter.DefaultOutputFolder);
      checkBox1.Checked = _settingServiceAgent.GetValue("xmlTvUseTimeZone", false);
      cbImportXML.Checked = _settingServiceAgent.GetValue("xmlTvImportXML", true);
      cbImportLST.Checked = _settingServiceAgent.GetValue("xmlTvImportLST", false);
      checkBoxDeleteBeforeImport.Checked = _settingServiceAgent.GetValue("xmlTvDeleteBeforeImport", true);
      textBoxHours.Text = _settingServiceAgent.GetValue("xmlTvTimeZoneHours", 0).ToString();
      textBoxMinutes.Text = _settingServiceAgent.GetValue("xmlTvTimeZoneMins", 0).ToString();

      UpdateLastImportUiDetails();

      chkScheduler.Checked = (_settingServiceAgent.GetValue("xmlTvRemoteSchedulerEnabled", false));
      radioDownloadOnWakeUp.Checked = (_settingServiceAgent.GetValue("xmlTvRemoteSchedulerDownloadOnWakeUpEnabled", false));
      radioDownloadOnSchedule.Checked = !radioDownloadOnWakeUp.Checked;

      txtRemoteURL.Text = _settingServiceAgent.GetValue("xmlTvRemoteURL", "http://www.mysite.com/tvguide.xml");

      DateTime dt = DateTime.Now;
      DateTimeFormatInfo DTFI = new DateTimeFormatInfo();
      DTFI.ShortDatePattern = _shortTimePattern24Hrs;

      try
      {
        dt = DateTime.Parse(_settingServiceAgent.GetValue("xmlTvRemoteScheduleTime", "06:30"), DTFI);
      }
      catch
      {
        // maybe 12 hr time (us) instead. lets re-parse it.
        try
        {
          DTFI.ShortDatePattern = _shortTimePattern12Hrs;
          dt = DateTime.Parse(_settingServiceAgent.GetValue("xmlTvRemoteScheduleTime", "06:30"), DTFI);
        }
        catch
        {
          //ignore
        }
      }

      dateTimePickerScheduler.Value = dt;

      UpdateLastTransferUiDetails();

      // load all distinct groups
      try
      {
        comboBoxGroup.Items.Clear();
        comboBoxGroup.Items.Add(new CBChannelGroup("", -1));
        comboBoxGroup.Tag = "";

        IEnumerable<ChannelGroup> channelGroups = _channelGroupServiceAgent.ListAllChannelGroups();
        foreach (ChannelGroup cg in channelGroups)
        {
          comboBoxGroup.Items.Add(new CBChannelGroup(cg.GroupName, cg.ChannelGroupId));
        }
      }
      catch (Exception e)
      {
        this.LogError("Failed to load groups {0}", e.Message);
      }
    }

    private void buttonBrowse_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog dlg = new FolderBrowserDialog();
      dlg.SelectedPath = textBoxFolder.Text;
      dlg.Description = "Specify xmltv folder";
      dlg.ShowNewFolderButton = true;
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        textBoxFolder.Text = dlg.SelectedPath;
      }
    }

    private void buttonRefresh_Click(object sender, EventArgs e)
    {
      String name = null;


      try
      {
        textBoxAction.Text = "Loading";
        Refresh();

        this.LogDebug("Loading all channels from the tvguide[s]");
        // used for partial matches
        TstDictionary guideChannels = new TstDictionary();

        Dictionary<string, Channel> guideChannelsExternald = new Dictionary<string, Channel>();

        List<Channel> lstTvGuideChannels = readChannelsFromTVGuide();

        if (lstTvGuideChannels == null)
          return;

        // convert to Dictionary
        foreach (Channel ch in lstTvGuideChannels)
        {
          string tName = ch.DisplayName.Replace(" ", "").ToLowerInvariant();
          if (!guideChannels.ContainsKey(tName))
            guideChannels.Add(tName, ch);

          // used to make sure that the available mapping is used by default
          if (ch.ExternalId != null && !ch.ExternalId.Trim().Equals(""))
          {
            // need to check this because we can have channels with multiple display-names 
            // and they're currently handles as one channel/display-name.
            // only in the mapping procedure of course
            if (!guideChannelsExternald.ContainsKey(ch.ExternalId))
              guideChannelsExternald.Add(ch.ExternalId, ch);
          }
        }

        this.LogDebug("Loading all channels from the database");

        CBChannelGroup chGroup = (CBChannelGroup)comboBoxGroup.SelectedItem;

        IList<Channel> channels;

        bool loadRadio = checkBoxLoadRadio.Checked;
        if (loadRadio)
        {
          channels = _channelServiceAgent.GetAllChannelsByGroupId(chGroup.idGroup).ToList();
        }
        else
        {
          channels = _channelServiceAgent.GetAllChannelsByGroupIdAndMediaType(chGroup.idGroup, MediaTypeEnum.TV).ToList();
        }

        progressBar1.Minimum = 0;
        progressBar1.Maximum = channels.Count;
        progressBar1.Value = 0;

        dataGridChannelMappings.Rows.Clear();

        int row = 0;

        if (channels.Count == 0)
        {
          MessageBox.Show("No tv-channels available to map");
          return;
        }
        // add as many rows in the datagrid as there are channels
        dataGridChannelMappings.Rows.Add(channels.Count);

        DataGridViewRowCollection rows = dataGridChannelMappings.Rows;

        // go through each channel and try to find a matching channel
        // 1: matching display-name (non case-sensitive)
        // 2: partial search on the first word. The first match will be selected in the dropdown

        foreach (Channel ch in channels)
        {
          Boolean alreadyMapped = false;
          DataGridViewRow gridRow = rows[row++];

          DataGridViewTextBoxCell idCell = (DataGridViewTextBoxCell)gridRow.Cells["Id"];
          DataGridViewTextBoxCell channelCell = (DataGridViewTextBoxCell)gridRow.Cells["tuningChannel"];
          DataGridViewTextBoxCell providerCell = (DataGridViewTextBoxCell)gridRow.Cells["tuningChannel"];
          DataGridViewCheckBoxCell showInGuideCell = (DataGridViewCheckBoxCell)gridRow.Cells["ShowInGuide"];

          channelCell.Value = ch.DisplayName;
          idCell.Value = ch.ChannelId;
          showInGuideCell.Value = ch.VisibleInGuide;

          DataGridViewComboBoxCell guideChannelComboBox = (DataGridViewComboBoxCell)gridRow.Cells["guideChannel"];

          // always add a empty item as the first option
          // these channels will not be updated when saving
          guideChannelComboBox.Items.Add("");

          // Start by checking if there's an available mapping for this channel
          Channel matchingGuideChannel = null;

          if (ch.ExternalId != null && guideChannelsExternald.ContainsKey(ch.ExternalId))
          {
            matchingGuideChannel = guideChannelsExternald[ch.ExternalId];
            alreadyMapped = true;
          }
          // no externalId mapping available, try using the name
          if (matchingGuideChannel == null)
          {
            string tName = ch.DisplayName.Replace(" ", "").ToLowerInvariant();
            if (guideChannels.ContainsKey(tName))
              matchingGuideChannel = (Channel)guideChannels[tName];
          }

          Boolean exactMatch = false;
          Boolean partialMatch = false;

          if (!alreadyMapped)
          {
            if (matchingGuideChannel != null)
            {
              exactMatch = true;
            }
            else
            {
              // No name mapping found

              // do a partial search, default off
              if (checkBoxPartialMatch.Checked)
              {
                // do a search using the first word(s) (skipping the last) of the channelname
                name = ch.DisplayName.Trim();
                int spaceIdx = name.LastIndexOf(" ");
                if (spaceIdx > 0)
                {
                  name = name.Substring(0, spaceIdx).Trim();
                }
                else
                {
                  // only one word so we'll do a partial match on the first 3 letters
                  if (name.Length > 3)
                    name = name.Substring(0, 3);
                }

                try
                {
                  // Note: the partial match code doesn't work as described by the author
                  // so we'll use PrefixMatch method (created by a codeproject user)
                  ICollection partialMatches = guideChannels.PrefixMatch(name.Replace(" ", "").ToLowerInvariant());

                  if (partialMatches != null && partialMatches.Count > 0)
                  {
                    IEnumerator pmE = partialMatches.GetEnumerator();
                    pmE.MoveNext();
                    matchingGuideChannel = (Channel)guideChannels[(string)pmE.Current];
                    partialMatch = true;
                  }
                }
                catch (Exception ex)
                {
                  this.LogError(ex, "Error while searching for matching guide channel");
                }
              }
            }
          }
          // add the channels 
          // set the first matching channel in the search above as the selected

          Boolean gotMatch = false;

          string ALREADY_MAPPED = "Already mapped (got external id)";
          string EXACT_MATCH = "Exact match";
          string PARTIAL_MATCH = "Partial match";
          string NO_MATCH = "No match";

          DataGridViewCell cell = gridRow.Cells["matchType"];

          foreach (DictionaryEntry de in guideChannels)
          {
            Channel guideChannel = (Channel)de.Value;

            String itemText = guideChannel.DisplayName + " (" + guideChannel.ExternalId + ")";

            guideChannelComboBox.Items.Add(itemText);

            if (!gotMatch && matchingGuideChannel != null)
            {
              if (guideChannel.DisplayName.ToLowerInvariant().Equals(matchingGuideChannel.DisplayName.ToLowerInvariant()))
              {
                // set the matchtype row color according to the type of match(already mapped,exact, partial, none)
                if (alreadyMapped)
                {
                  cell.Style.BackColor = Color.White;
                  cell.ToolTipText = ALREADY_MAPPED;
                  // hack so that we can order the grid by mappingtype
                  cell.Value = "";
                }
                else if (exactMatch)
                {
                  cell.Style.BackColor = Color.Green;
                  cell.ToolTipText = EXACT_MATCH;
                  cell.Value = "  ";
                }
                else if (partialMatch)
                {
                  cell.Style.BackColor = Color.Yellow;
                  cell.ToolTipText = PARTIAL_MATCH;
                  cell.Value = "   ";
                }

                guideChannelComboBox.Value = itemText;
                guideChannelComboBox.Tag = ch.ExternalId;

                gotMatch = true;
              }
            }
          }
          if (!gotMatch)
          {
            cell.Style.BackColor = Color.Red;
            cell.ToolTipText = NO_MATCH;
            cell.Value = "    ";
          }
          progressBar1.Value++;
        }
        textBoxAction.Text = "Finished";
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Failed loading channels/mappings : channel {0}", name);
        textBoxAction.Text = "Error";
      }
    }

    private List<Channel> readChannelsFromTVGuide()
    {
      var listChannels = new List<Channel>();
      string folder = _settingServiceAgent.GetValue("xmlTv", XmlTvImporter.DefaultOutputFolder);
      string selFolder = textBoxFolder.Text;

      // use the folder set in the gui if it doesn't match the one set in the database
      // these might be different since it isn't saved until the user clicks ok or
      // moves to another part of the gui
      if (!folder.Equals(selFolder))
      {
        folder = selFolder.Trim();
      }
      Boolean importXML = false;
      Boolean importLST = false;

      string fileName;

      if (cbImportXML.Checked)
      {
        fileName = folder + @"\tvguide.xml";

        if (System.IO.File.Exists(fileName))
        {
          importXML = true;
        }
        else
        {
          MessageBox.Show("tvguide.xml file not found at path [" + fileName + "]");
          return null;
        }
      }

      fileName = folder + @"\tvguide.lst";

      if (cbImportLST.Checked)
      {
        if (System.IO.File.Exists(fileName))
        {
          importLST = true;
        }
        else
        {
          MessageBox.Show("tvguide.lst file not found at path [" + fileName + "]");
          return null;
        }
      }

      if (importXML || importLST)
      {
        if (importXML)
        {
          fileName = folder + @"\tvguide.xml";
          /*
          bool canRead = false;
          bool canWrite = false;
          IOUtil.CheckFileAccessRights(fileName, ref canRead, ref canWrite);

          if (canRead)
          {
              // all ok, get channels
              this.LogDebug(@"plugin:xmltv loading " + fileName);
              listChannels.AddRange(readTVGuideChannelsFromFile(fileName));
          }
          else
          {
              MessageBox.Show("Can't open tvguide.xml for reading");
              this.LogError(@"plugin:xmltv StartImport - Exception when reading [" + fileName + "].");
          }*/

          try
          {
            //check if file can be opened for reading....
            IOUtil.CheckFileAccessRights(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);

            // all ok, get channels
            this.LogDebug(@"plugin:xmltv loading " + fileName);

            listChannels.AddRange(readTVGuideChannelsFromFile(fileName));
          }
          catch (Exception e)
          {
            MessageBox.Show("Can't open tvguide.xml for reading");
            this.LogError(@"plugin:xmltv StartImport - Exception when reading [" + fileName + "] : " + e.Message);
          }
        }

        if (importLST)
        {
          fileName = folder + @"\tvguide.lst";

          FileStream streamIn = null;
          StreamReader fileIn = null;

          try
          {
            // open file
            Encoding fileEncoding = Encoding.Default;
            streamIn = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            fileIn = new StreamReader(streamIn, fileEncoding, true);

            // ok, start reading
            while (!fileIn.EndOfStream)
            {
              string tvguideFileName = fileIn.ReadLine();
              if (tvguideFileName.Length == 0) continue;

              if (!System.IO.Path.IsPathRooted(tvguideFileName))
              {
                // extend by directory
                tvguideFileName = System.IO.Path.Combine(folder, tvguideFileName);
              }

              this.LogDebug(@"plugin:xmltv loading " + tvguideFileName);

              // get channels
              listChannels.AddRange(readTVGuideChannelsFromFile(tvguideFileName));
            }
            fileIn.Close();
            streamIn.Close();
          }
          catch (Exception e)
          {
            MessageBox.Show("Can't read file(s) from the tvguide.lst");
            this.LogError(@"plugin:xmltv StartImport - Exception when reading [" + fileName + "] : " + e.Message);
          }
          finally
          {
            try
            {
              if (fileIn != null)
                fileIn.Close();

              if (streamIn != null)
                streamIn.Close();
            }
            catch (Exception) { }
          }
        }
      }
      return listChannels;
    }


    /// <summary>
    /// Reads all the channels from a tvguide.xml file
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    private List<Channel> readTVGuideChannelsFromFile(String tvguideFilename)
    {
      List<Channel> channels = new List<Channel>();
      XmlTextReader xmlReader = new XmlTextReader(tvguideFilename);

      int iChannel = 0;

      try
      {
        if (xmlReader.ReadToDescendant("tv"))
        {
          // get the first channel
          if (xmlReader.ReadToDescendant("channel"))
          {
            do
            {
              String id = xmlReader.GetAttribute("id");
              if (string.IsNullOrEmpty(id))
              {
                this.LogError("  channel#{0} doesnt contain an id", iChannel);
              }
              else
              {
                // String displayName = null;

                XmlReader xmlChannel = xmlReader.ReadSubtree();
                xmlChannel.ReadStartElement(); // read channel
                // now, xmlChannel is positioned on the first sub-element of <channel>
                List<string> displayNames = new List<string>();

                while (!xmlChannel.EOF)
                {
                  if (xmlChannel.NodeType == XmlNodeType.Element)
                  {
                    switch (xmlChannel.Name)
                    {
                      case "display-name":
                      case "Display-Name":
                        displayNames.Add(xmlChannel.ReadString());
                        //else xmlChannel.Skip();
                        break;
                      // could read more stuff here, like icon...
                      default:
                        // unknown, skip entire node
                        xmlChannel.Skip();
                        break;
                    }
                  }
                  else
                    xmlChannel.Read();
                }
                foreach (string displayName in displayNames)
                {
                  if (displayName != null)
                  {
                    Channel channel = new Channel { ExternalId = id, DisplayName = displayName };
                    channels.Add(channel);
                  }
                }
              }
              iChannel++;
            } while (xmlReader.ReadToNextSibling("channel"));
          }
        }
      }
      catch { }
      finally
      {
        if (xmlReader != null)
        {
          xmlReader.Close();
          xmlReader = null;
        }
      }

      return channels;
    }

    private void buttonSave_Click(object sender, EventArgs e)
    {
      try
      {
        // loading all Channels is much faster then loading them one by one
        // for each mapping
        IEnumerable<Channel> allChanels = _channelServiceAgent.ListAllChannels();
        Dictionary<int, Channel> dAllChannels = allChanels.ToDictionary(ch => ch.ChannelId);

        progressBar1.Value = 0;
        progressBar1.Minimum = 0;
        progressBar1.Maximum = dataGridChannelMappings.Rows.Count;
        textBoxAction.Text = "Saving mappings";

        foreach (DataGridViewRow row in dataGridChannelMappings.Rows)
        {
          int id = (int)row.Cells["Id"].Value;
          string guideChannelAndexternalId = (string)row.Cells["guideChannel"].Value;

          string externalId = null;

          if (guideChannelAndexternalId != null)
          {
            int startIdx = guideChannelAndexternalId.LastIndexOf("(") + 1;
            // the length is the same as the length - startingidex -1 (-1 -> remove trailing )) 
            externalId = guideChannelAndexternalId.Substring(startIdx, guideChannelAndexternalId.Length - startIdx - 1);
          }

          Channel channel = dAllChannels[id];
          channel.ExternalId = externalId == null ? "" : externalId;
          //channel.Persist();
          progressBar1.Value++;
        }
        _channelServiceAgent.SaveChannels(dAllChannels.Values.ToList());

        textBoxAction.Text = "Mappings saved";
      }
      catch (Exception ex)
      {
        textBoxAction.Text = "Save failed";
        this.LogError(ex, "Error while saving channelmappings");
      }
    }

    private void buttonManualImport_Click(object sender, EventArgs e)
    {
      SaveSettings();
      ServiceAgents.Instance.PluginService<IXMLTVImportService>().ImportNow();
      UpdateLastImportUiDetails();
    }

    private void UpdateLastImportUiDetails()
    {
      labelLastImport.Text = _settingServiceAgent.GetValue("xmlTvResultLastImport", string.Empty);
      labelChannels.Text = _settingServiceAgent.GetValue("xmlTvResultChannels", string.Empty);
      labelPrograms.Text = _settingServiceAgent.GetValue("xmlTvResultPrograms", string.Empty);
      labelStatus.Text = _settingServiceAgent.GetValue("xmlTvResultStatus", string.Empty);
    }

    private void buttonExport_Click(object sender, EventArgs e)
    {
      string folder = _settingServiceAgent.GetValue("xmlTv", XmlTvImporter.DefaultOutputFolder);
      string selFolder = textBoxFolder.Text;

      // use the folder set in the gui if it doesn't match the one set in the database
      // these might be different since it isn't saved until the user clicks ok or
      // moves to another part of the gui
      if (!folder.Equals(selFolder))
      {
        folder = selFolder.Trim();
      }

      if (System.IO.Directory.Exists(folder))
        saveFileExport.InitialDirectory = folder;

      saveFileExport.ShowDialog();
    }

    private void saveFileExport_FileOk(object sender, CancelEventArgs e)
    {
      try
      {
        Stream stream = saveFileExport.OpenFile();

        Encoding fileEncoding = Encoding.Default;
        StreamWriter fileOut = new StreamWriter(stream, fileEncoding);

        foreach (DataGridViewRow row in dataGridChannelMappings.Rows)
        {
          string guideChannelAndexternalId = (string)row.Cells["guideChannel"].Value;
          string externalId = null;

          if (guideChannelAndexternalId != null)
          {
            int startIdx = guideChannelAndexternalId.LastIndexOf("(") + 1;
            // the length is the same as the length - startingidex -1 (-1 -> remove trailing )) 
            externalId = guideChannelAndexternalId.Substring(startIdx, guideChannelAndexternalId.Length - startIdx - 1);
            fileOut.WriteLine("channel=" + externalId);
          }
        }
        try
        {
          fileOut.Flush();
          fileOut.Close();
        }
        catch (Exception)
        {
          // ignore
        }
      }
      catch (UnauthorizedAccessException ex)
      {
        MessageBox.Show("Can't open the file for writing");
        this.LogError(ex, "Failed to export guidechannels");
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Failed to export guidechannels");
      }
    }

    private void buttonBrowse_Click_1(object sender, EventArgs e)
    {
      folderBrowserDialogTVGuide.ShowDialog();
      textBoxFolder.Text = folderBrowserDialogTVGuide.SelectedPath;
    }

    private void btnGetNow_Click(object sender, EventArgs e)
    {
      SaveSettings();
      ServiceAgents.Instance.PluginService<IXMLTVImportService>().RetrieveRemoteFileNow();
      UpdateLastTransferUiDetails();
    }

    private void UpdateLastTransferUiDetails()
    {
      lblLastTransferAt.Text = _settingServiceAgent.GetValue("xmlTvRemoteScheduleLastTransfer", string.Empty);
      lblTransferStatus.Text = _settingServiceAgent.GetValue("xmlTvRemoteScheduleTransferStatus", string.Empty);
    }

    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {
      //persist stuff when changing tabs in the plugin.
      SaveSettings();

      //load status
      UpdateLastImportUiDetails();
      UpdateLastTransferUiDetails();
    }

    private void chkScheduler_CheckedChanged(object sender, EventArgs e)
    {
      UpdateRadioButtonsState();
    }

    private void UpdateRadioButtonsState()
    {
      radioDownloadOnSchedule.Enabled = chkScheduler.Checked;
      radioDownloadOnWakeUp.Enabled = chkScheduler.Checked;
    }
  }
}