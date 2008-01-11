﻿#region Copyright (C) 2007-2008 Team MediaPortal

/*
    Copyright (C) 2007-2008 Team MediaPortal
    http://www.team-mediaportal.com
 
    This file is part of MediaPortal II

    MediaPortal II is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal II is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal II.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using SkinEngine;
using SkinEngine.Controls;
using MediaPortal.Core;
using MediaPortal.Core.Collections;
using MediaPortal.Core.Properties;
using SkinEngine.Properties;


public class Scriptlet : IScriptProperty
{
  public Property Get(IControl control, string param)
  {
    Control c = control as Control;
    if (c == null) return null;
    ListContainer list = c as ListContainer;
    if (list == null)
      return Get(c.Container, param);

    return new ListItemDependency(list);
  }

  public class ListItemDependency : Dependency
  {
    ListContainer _list;
    public ListItemDependency(ListContainer list)
    {
      _list = list;
      this.DependencyObject = _list.SelectedItemProperty;
      _list.SelectedSubItemIndexProperty.Attach(new PropertyChangedHandler(OnValueChanged));
      OnValueChanged(_list.SelectedItemProperty);
    }

    protected override void OnValueChanged(Property property)
    {
      if (_list.Items == null)
      {
        SetValue(false);
        return;
      }
      ListItem item = _list.SelectedItem;
      if (item == null)
      {
        if (_list.PageOffset < 0 || _list.PageOffset >= _list.Items.Count)
        {
          SetValue(false);
          return;
        }
        item = _list.Items[_list.PageOffset];
      }

      int index = _list.SelectedSubItemIndex;
      if (index + 1 < item.SubItems.Count)
      {
        SetValue(true);
      }
      else
      {
        SetValue(false);
      }

    }
  }

}
