//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
    [DataContract(IsReference = true)]
    [KnownType(typeof(Channel))]
    public partial class TvMovieMapping: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public int IdMapping
        {
            get { return _idMapping; }
            set
            {
                if (_idMapping != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'IdMapping' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _idMapping = value;
                    OnPropertyChanged("IdMapping");
                }
            }
        }
        private int _idMapping;
    
        [DataMember]
        public int IdChannel
        {
            get { return _idChannel; }
            set
            {
                if (_idChannel != value)
                {
                    ChangeTracker.RecordOriginalValue("IdChannel", _idChannel);
                    if (!IsDeserializing)
                    {
                        if (Channel != null && Channel.IdChannel != value)
                        {
                            Channel = null;
                        }
                    }
                    _idChannel = value;
                    OnPropertyChanged("IdChannel");
                }
            }
        }
        private int _idChannel;
    
        [DataMember]
        public string StationName
        {
            get { return _stationName; }
            set
            {
                if (_stationName != value)
                {
                    _stationName = value;
                    OnPropertyChanged("StationName");
                }
            }
        }
        private string _stationName;
    
        [DataMember]
        public string TimeSharingStart
        {
            get { return _timeSharingStart; }
            set
            {
                if (_timeSharingStart != value)
                {
                    _timeSharingStart = value;
                    OnPropertyChanged("TimeSharingStart");
                }
            }
        }
        private string _timeSharingStart;
    
        [DataMember]
        public string TimeSharingEnd
        {
            get { return _timeSharingEnd; }
            set
            {
                if (_timeSharingEnd != value)
                {
                    _timeSharingEnd = value;
                    OnPropertyChanged("TimeSharingEnd");
                }
            }
        }
        private string _timeSharingEnd;

        #endregion
        #region Navigation Properties
    
        [DataMember]
        public Channel Channel
        {
            get { return _channel; }
            set
            {
                if (!ReferenceEquals(_channel, value))
                {
                    var previousValue = _channel;
                    _channel = value;
                    FixupChannel(previousValue);
                    OnNavigationPropertyChanged("Channel");
                }
            }
        }
        private Channel _channel;

        #endregion
        #region ChangeTracking
    
        protected virtual void OnPropertyChanged(String propertyName)
        {
            if (ChangeTracker.State != ObjectState.Added && ChangeTracker.State != ObjectState.Deleted)
            {
                ChangeTracker.State = ObjectState.Modified;
            }
            if (_propertyChanged != null)
            {
                _propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    
        protected virtual void OnNavigationPropertyChanged(String propertyName)
        {
            if (_propertyChanged != null)
            {
                _propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged{ add { _propertyChanged += value; } remove { _propertyChanged -= value; } }
        private event PropertyChangedEventHandler _propertyChanged;
        private ObjectChangeTracker _changeTracker;
    
        [DataMember]
        public ObjectChangeTracker ChangeTracker
        {
            get
            {
                if (_changeTracker == null)
                {
                    _changeTracker = new ObjectChangeTracker();
                    _changeTracker.ObjectStateChanging += HandleObjectStateChanging;
                }
                return _changeTracker;
            }
            set
            {
                if(_changeTracker != null)
                {
                    _changeTracker.ObjectStateChanging -= HandleObjectStateChanging;
                }
                _changeTracker = value;
                if(_changeTracker != null)
                {
                    _changeTracker.ObjectStateChanging += HandleObjectStateChanging;
                }
            }
        }
    
        private void HandleObjectStateChanging(object sender, ObjectStateChangingEventArgs e)
        {
            if (e.NewState == ObjectState.Deleted)
            {
                ClearNavigationProperties();
            }
        }
    
        // This entity type is the dependent end in at least one association that performs cascade deletes.
        // This event handler will process notifications that occur when the principal end is deleted.
        internal void HandleCascadeDelete(object sender, ObjectStateChangingEventArgs e)
        {
            if (e.NewState == ObjectState.Deleted)
            {
                this.MarkAsDeleted();
            }
        }
    
        protected bool IsDeserializing { get; private set; }
    
        [OnDeserializing]
        public void OnDeserializingMethod(StreamingContext context)
        {
            IsDeserializing = true;
        }
    
        [OnDeserialized]
        public void OnDeserializedMethod(StreamingContext context)
        {
            IsDeserializing = false;
            ChangeTracker.ChangeTrackingEnabled = true;
        }
    
        protected virtual void ClearNavigationProperties()
        {
            Channel = null;
        }

        #endregion
        #region Association Fixup
    
        private void FixupChannel(Channel previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.TvMovieMappings.Contains(this))
            {
                previousValue.TvMovieMappings.Remove(this);
            }
    
            if (Channel != null)
            {
                if (!Channel.TvMovieMappings.Contains(this))
                {
                    Channel.TvMovieMappings.Add(this);
                }
    
                IdChannel = Channel.IdChannel;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("Channel")
                    && (ChangeTracker.OriginalValues["Channel"] == Channel))
                {
                    ChangeTracker.OriginalValues.Remove("Channel");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("Channel", previousValue);
                }
                if (Channel != null && !Channel.ChangeTracker.ChangeTrackingEnabled)
                {
                    Channel.StartTracking();
                }
            }
        }

        #endregion
    }
}
