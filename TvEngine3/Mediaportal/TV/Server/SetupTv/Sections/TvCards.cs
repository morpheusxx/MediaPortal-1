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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DirectShowLib;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupTV.Dialogs;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class TvCards : SectionSettings
  {
  
    private bool _needRestart;
    private readonly Dictionary<string, CardType> cardTypes = new Dictionary<string, CardType>();
    private TabPage usbWINTV_tabpage;

    public delegate void ChangedEventHandler(object sender, EventArgs e);

    public event ChangedEventHandler TvCardsChanged;

    #region CardInfo class

    public class CardInfo
    {
      public Card card;

      public CardInfo(Card newcard)
      {
        card = newcard;
      }

      public override string ToString()
      {
        return card.Name;
      }
    }

    #endregion

    public TvCards()
      : this("TV Cards") {}

    public TvCards(string name)
      : base(name)
    {
      InitializeComponent();
    }

    private void UpdateMenu()
    {
      placeInHybridCardToolStripMenuItem.DropDownItems.Clear();
      IList<CardGroup> groups = ServiceAgents.Instance.CardServiceAgent.ListAllCardGroups();
      foreach (CardGroup group in groups)
      {
        ToolStripMenuItem item = new ToolStripMenuItem(group.Name);
        item.Tag = group;
        item.Click += placeInHybridCardToolStripMenuItem_Click;
        placeInHybridCardToolStripMenuItem.DropDownItems.Add(item);
      }
      ToolStripMenuItem itemNew = new ToolStripMenuItem("New...");
      itemNew.Click += placeInHybridCardToolStripMenuItem_Click;
      placeInHybridCardToolStripMenuItem.DropDownItems.Add(itemNew);
    }

    private void placeInHybridCardToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
      CardGroup group;
      if (menuItem.Tag == null)
      {
        GroupNameForm dlg = new GroupNameForm();
        if (dlg.ShowDialog(this) != DialogResult.OK)
        {
          return;
        }

        group = new CardGroup {Name = dlg.GroupName};
        ServiceAgents.Instance.CardServiceAgent.SaveCardGroup(group);
        UpdateMenu();
      }
      else
      {
        group = (CardGroup)menuItem.Tag;
      }
      ListView.SelectedIndexCollection indexes = mpListView1.SelectedIndices;
      if (indexes.Count == 0)
        return;
      for (int i = 0; i < indexes.Count; ++i)
      {
        ListViewItem item = mpListView1.Items[indexes[i]];
        Card card = (Card)item.Tag;
        CardGroupMap map = new CardGroupMap();
        map.IdCard = card.IdCard;        
        map.IdCardGroup = group.IdCardGroup;
        ServiceAgents.Instance.CardServiceAgent.SaveCardGroupMap(map);
        ServiceAgents.Instance.CardServiceAgent.SaveCard(card);
      }
      UpdateHybrids();
      ServiceAgents.Instance.ControllerServiceAgent.Restart();
    }

    public override void OnSectionDeActivated()
    {
      ReOrder();

      // DVB-IP cards
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("iptvCardCount", (int)iptvUpDown.Value);
      

      // WinTV CI
      CardInfo info = (CardInfo)mpComboBoxCard.SelectedItem;
      if (info != null)
      {
        ServiceAgents.Instance.SettingServiceAgent.SaveValue("winTvCiTuner", info.card.IdCard);        
      }

      if (_needRestart)
      {
        _needRestart = false;
        OnChanged(this, EventArgs.Empty);
      }
      base.OnSectionDeActivated();
    }

    public override void OnSectionActivated()
    {
      _needRestart = false;
      UpdateList();

      //IPTV
      iptvUpDown.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("iptvCardCount", 1);

      //WinTV CI
      int winTvTunerCardId = ServiceAgents.Instance.SettingServiceAgent.GetValue("winTvCiTuner", -1);
      mpComboBoxCard.SelectedIndex = -1;
      foreach (CardInfo cardInfo in mpComboBoxCard.Items)
      {
        if (cardInfo.card.IdCard == winTvTunerCardId)
        {
          mpComboBoxCard.SelectedItem = cardInfo;
          break;
        }
      }
      _needRestart = false;
    }

    private void UpdateList()
    {
      base.OnSectionActivated();
      mpListView1.Items.Clear();
      mpComboBoxCard.Items.Clear();
      IList<Card> cards = new List<Card>();
      try
      {
        cards = ServiceAgents.Instance.CardServiceAgent.ListAllCards(CardIncludeRelationEnum.None);
        foreach (Card card in cards)
        {
          cardTypes[card.DevicePath] = ServiceAgents.Instance.ControllerServiceAgent.Type(card.IdCard);
          mpComboBoxCard.Items.Add(new CardInfo(card));
        }
      }
      catch (Exception) {}
      try
      {
        cards = cards.OrderByDescending(c => c.Priority).ToList();
        foreach (Card card in cards)
        {
          string cardType = "";
          if (cardTypes.ContainsKey(card.DevicePath))
          {
            cardType = cardTypes[card.DevicePath].ToString();
          }
          ListViewItem item = mpListView1.Items.Add("", 0);
          item.SubItems.Add(card.Priority.ToString());
          if (card.Enabled)
          {
            item.Checked = true;
            item.Font = new Font(item.Font, FontStyle.Regular);
            item.Text = "Yes";
          }
          else
          {
            item.Checked = false;
            item.Font = new Font(item.Font, FontStyle.Strikeout);
            item.Text = "No";
          }
          item.SubItems.Add(cardType);

          //CAM and CAM limit don't apply to non-digital cards
          if (cardType.ToUpperInvariant().Contains("DVB") || cardType.ToUpperInvariant().Contains("ATSC"))
          {
            if (card.UseConditionalAccess)
            {
              item.SubItems.Add("Yes");
            }
            else
            {
              item.SubItems.Add("No");
            }

            item.SubItems.Add(card.DecryptLimit.ToString());
          }
          else
          {
            item.SubItems.Add("");
            item.SubItems.Add("");
          }

          item.SubItems.Add(card.IdCard.ToString());
          item.SubItems.Add(card.Name);

          //check if card is really available before setting to enabled.
          bool cardPresent = ServiceAgents.Instance.ControllerServiceAgent.IsCardPresent(card.IdCard);
          if (!cardPresent)
          {
            item.SubItems.Add("No");
          }
          else
          {
            item.SubItems.Add("Yes");
          }

          //EPG grabbing doesn't apply to non-digital cards
          if (cardType.ToUpperInvariant().Contains("DVB") || cardType.ToUpperInvariant().Contains("ATSC"))
          {
            if (!card.GrabEPG)
            {
              item.SubItems.Add("No");
            }
            else
            {
              item.SubItems.Add("Yes");
            }
          }
          else
          {
            item.SubItems.Add("");
          }

          item.SubItems.Add(card.DevicePath);
          item.Tag = card;
        }
      }
      catch (Exception)
      {
        MessageBox.Show(this, "Unable to access service. Is the TvService running??");
      }
      ReOrder();
      UpdateHybrids();
      checkWinTVCI();
      UpdateMenu();
      mpListView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
    }

    private void checkWinTVCI()
    {
      //check if the hauppauge wintv usb CI module is installed
      DsDevice[] capDevices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCapture);
      DsDevice usbWinTvDevice = null;
      for (int capIndex = 0; capIndex < capDevices.Length; capIndex++)
      {
        if (capDevices[capIndex].Name != null)
        {
          if (capDevices[capIndex].Name.ToUpperInvariant() == "WINTVCIUSBBDA SOURCE")
          {
            usbWinTvDevice = capDevices[capIndex];
            break;
          }
        }
      }
      if (usbWinTvDevice == null)
      {
        if (usbWINTV_tabpage == null)
        {
          usbWINTV_tabpage = tabControl1.TabPages[2];
          tabControl1.TabPages.RemoveAt(2);
        }
      }
      else if (usbWINTV_tabpage != null && tabControl1.TabCount == 2)
      {
        tabControl1.TabPages.Insert(2, usbWINTV_tabpage);
      }
    }

    private void buttonUp_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      try
      {
        ListView.SelectedIndexCollection indexes = mpListView1.SelectedIndices;
        if (indexes.Count == 0)
          return;
        for (int i = 0; i < indexes.Count; ++i)
        {
          int index = indexes[i];
          if (index > 0)
          {
            ListViewItem item = mpListView1.Items[index];
            mpListView1.Items.RemoveAt(index);
            mpListView1.Items.Insert(index - 1, item);
          }
        }
        ReOrder();
      }
      finally
      {
        mpListView1.EndUpdate();
      }
      _needRestart = true;
    }

    private void buttonDown_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      try
      {
        ListView.SelectedIndexCollection indexes = mpListView1.SelectedIndices;
        if (indexes.Count == 0)
          return;
        if (mpListView1.Items.Count < 2)
          return;
        for (int i = indexes.Count - 1; i >= 0; i--)
        {
          int index = indexes[i];
          ListViewItem item = mpListView1.Items[index];
          mpListView1.Items.RemoveAt(index);
          if (index + 1 < mpListView1.Items.Count)
            mpListView1.Items.Insert(index + 1, item);
          else
            mpListView1.Items.Add(item);
        }
        ReOrder();
      }
      finally
      {
        mpListView1.EndUpdate();
      }
      _needRestart = true;
    }

    private void ReOrder()
    {
      IList<Card> cards = new List<Card>();
      for (int i = 0; i < mpListView1.Items.Count; ++i)
      {
        mpListView1.Items[i].SubItems[1].Text = (mpListView1.Items.Count - i).ToString();

        Card card = (Card)mpListView1.Items[i].Tag;
        card.Priority = mpListView1.Items.Count - i;
        if (card.Enabled != mpListView1.Items[i].Checked)
          _needRestart = true;

        card.Enabled = mpListView1.Items[i].Checked;
        cards.Add(card);
        card.UnloadAllUnchangedRelationsForEntity();
      }

      ServiceAgents.Instance.CardServiceAgent.SaveCards(cards);
    }

    private void mpListView1_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
      e.Item.Font = e.Item.Checked
                      ? new Font(e.Item.Font, FontStyle.Regular)
                      : new Font(e.Item.Font, FontStyle.Strikeout);
      UpdateEditButtonState();
    }

    private void mpListView1_SelectedIndexChanged(object sender, EventArgs e)
    {
      bool enabled = mpListView1.SelectedItems.Count == 1;
      if (enabled)
      {
        Card card = (Card)mpListView1.SelectedItems[0].Tag;
        enabled = !ServiceAgents.Instance.ControllerServiceAgent.IsCardPresent(card.IdCard);
      }
      UpdateEditButtonState();
      buttonRemove.Enabled = enabled;
    }

    private void UpdateEditButtonState()
    {
      if (mpListView1.SelectedItems.Count == 1)
      {
        string cardType = mpListView1.SelectedItems[0].SubItems[2].Text.ToLowerInvariant();
        if (mpListView1.SelectedItems[0].Checked &&
            (cardType.Contains("dvb") || cardType.Contains("atsc") || cardType.Contains("analog")))
          // Only some cards can be edited
          buttonEdit.Enabled = true;
        else
          buttonEdit.Enabled = false;
      }
    }

    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e) {}

    private void UpdateHybrids()
    {
      treeView1.Nodes.Clear();
      IList<CardGroup> cardGroups = ServiceAgents.Instance.CardServiceAgent.ListAllCardGroups();
      foreach (CardGroup group in cardGroups)
      {
        TreeNode node = treeView1.Nodes.Add(group.Name);
        node.Tag = group;
        IList<CardGroupMap> cards = group.CardGroupMaps;
        foreach (CardGroupMap map in cards)
        {
          Card card = map.Card;
          TreeNode cardNode = node.Nodes.Add(card.Name);
          cardNode.Tag = card;
        }
      }
    }

    private void deleteCardToolStripMenuItem_Click(object sender, EventArgs e)
    {
      TreeNode node = treeView1.SelectedNode;
      if (node == null)
        return;
      Card card = node.Tag as Card;
      if (card == null)
        return;
      CardGroup group = node.Parent.Tag as CardGroup;
      if (group != null)
      {                

        IList<CardGroupMap> cards = group.CardGroupMaps;
        CardGroupMap cardGroupMap2Remove = null;
        foreach (CardGroupMap map in cards.Where(map => map.IdCard == card.IdCard))
        {
          cardGroupMap2Remove = map;
          //ServiceAgents.Instance.ChannelGroupServiceAgent.DeleteChannelGroupMap(map.idMap)          ;
          break;
        }

        if (cardGroupMap2Remove != null)
        {
          group.CardGroupMaps.Remove(cardGroupMap2Remove);
          ServiceAgents.Instance.CardServiceAgent.SaveCardGroup(group);
        }        
      }
      UpdateHybrids();
      ServiceAgents.Instance.ControllerServiceAgent.Restart();
    }

    private void deleteEntireHybridCardToolStripMenuItem_Click(object sender, EventArgs e)
    {
      TreeNode node = treeView1.SelectedNode;
      if (node == null)
        return;
      CardGroup group = node.Tag as CardGroup;
      if (group == null)
        return;
      ServiceAgents.Instance.CardServiceAgent.DeleteCardGroup(group.IdCardGroup);
      UpdateHybrids();
      ServiceAgents.Instance.ControllerServiceAgent.Restart();
    }

    private void buttonEdit_Click(object sender, EventArgs e)
    {
      ListView.SelectedIndexCollection indexes = mpListView1.SelectedIndices;
      if (indexes == null || indexes.Count == 0)
        return;
      ListViewItem item = mpListView1.Items[indexes[0]];
      ReOrder();
      UpdateList();
      FormEditCard dlg = new FormEditCard();
      dlg.Card = (Card)item.Tag;
      dlg.CardType = cardTypes[((Card)item.Tag).DevicePath].ToString();
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        // User clicked save...
        ServiceAgents.Instance.CardServiceAgent.SaveCard(dlg.Card);
        this.LogDebug("Saving settings for device {0}...", dlg.Card.Name);
        _needRestart = true;
        UpdateList();
      }
    }

    private void buttonRemove_Click(object sender, EventArgs e)
    {
      Card card = (Card)mpListView1.SelectedItems[0].Tag;

      DialogResult res = MessageBox.Show(this,
                                         "Are you sure you want to delete this card along with all the channel mappings? (channels will not be deleted)",
                                         "Delete card?", MessageBoxButtons.YesNo);

      if (res == DialogResult.Yes)
      {
        /*
        if (card.CardGroupMaps.Count > 0)
        {
          for (int i = card.CardGroupMaps.Count - 1; i > -1; i--)
          {
            CardGroupMap map = card.CardGroupMaps[i];
            //ServiceAgents.Instance.ChannelGroupServiceAgent.DeleteChannelGroupMap(map.idMap)          ;
            card.CardGroupMaps.RemoveAt(i);
          }
        }

        if (card.ChannelMaps.Count > 0)
        {
          for (int i = card.ChannelMaps.Count - 1; i > -1; i--)
          {
            ChannelMap map = card.ChannelMaps[i];
            ServiceAgents.Instance.ChannelGroupServiceAgent.DeleteChannelGroupMap(map.idMap)          ;
          }
        }
        */
        ServiceAgents.Instance.ControllerServiceAgent.CardRemove(card.IdCard);
        mpListView1.Items.Remove(mpListView1.SelectedItems[0]);
        _needRestart = true;
      }
    }

    private void mpComboBoxCard_SelectedIndexChanged(object sender, EventArgs e)
    {
      _needRestart = true;
    }

    private void linkLabelHybridCard_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      const string Url = "http://www.team-mediaportal.com/manual/TV-Server/Configuration/TVServers/HybridCards";
      System.Diagnostics.ProcessStartInfo sInfo = new System.Diagnostics.ProcessStartInfo(Url);
      System.Diagnostics.Process.Start(sInfo);
    }

    //Pass on the information for the plugin that was changed
    protected virtual void OnChanged(object sender, EventArgs e)
    {
      if (TvCardsChanged != null)
      {
        TvCardsChanged(sender, e);
      }
    }

    private void iptvUpDown_ValueChanged(object sender, EventArgs e)
    {
      _needRestart = true;
    }
  }
}