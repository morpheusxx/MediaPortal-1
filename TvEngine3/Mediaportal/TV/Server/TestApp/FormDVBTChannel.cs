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
using System.Windows.Forms;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

namespace Mediaportal.TV.Server.TestApp
{
  public partial class FormDVBTChannel : Form
  {
    private DVBTChannel _channel = new DVBTChannel();

    public FormDVBTChannel()
    {
      InitializeComponent();
      comboBoxBandWidth.SelectedIndex = 1;
    }

    public IChannel Channel
    {
      get { return _channel; }
      set
      {
        _channel = (DVBTChannel)value;
        textboxFreq.Text = _channel.Frequency.ToString();
        textBoxONID.Text = _channel.NetworkId.ToString();
        textBoxTSID.Text = _channel.TransportId.ToString();
        textBoxSID.Text = _channel.ServiceId.ToString();
        comboBoxBandWidth.SelectedIndex = _channel.Bandwidth == 7000 ? 0 : 1;
      }
    }

    private void buttonOK_Click(object sender, EventArgs e)
    {
      _channel.Frequency = Int32.Parse(textboxFreq.Text);
      _channel.NetworkId = Int32.Parse(textBoxONID.Text);
      _channel.TransportId = Int32.Parse(textBoxTSID.Text);
      _channel.ServiceId = Int32.Parse(textBoxSID.Text);
      _channel.Bandwidth = comboBoxBandWidth.SelectedIndex == 0 ? 7000 : 8000;
      Close();
    }
  }
}