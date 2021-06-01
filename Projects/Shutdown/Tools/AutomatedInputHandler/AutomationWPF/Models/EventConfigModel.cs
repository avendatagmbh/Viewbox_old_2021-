using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutomationWPF.Events;
using AutomationWPF.Helpers;
using System.Collections.ObjectModel;

namespace AutomationWPF.Models
{
    public class EventConfigModel : NotificationObject
    {
        private ObservableCollection<AutomatedEventBase> o_ToDoList;
        /// <summary>
        /// List of events that will be executed.
        /// </summary>
        public ObservableCollection<AutomatedEventBase> ToDoList
        {
            get
            {
                return o_ToDoList;
            }
            set
            {
                if (o_ToDoList != value)
                {
                    o_ToDoList = value;

                    RaisePropertyChanged(() => ToDoList);
                }
            }
        }

        private ObservableCollection<EventConfigModel> o_ChildEventConfigs;
        /// <summary>
        /// List of child configs.
        /// </summary>
        public ObservableCollection<EventConfigModel> ChildEventConfigs
        {
            get
            {
                return o_ChildEventConfigs;
            }
            set
            {
                if (o_ChildEventConfigs != value)
                {
                    o_ChildEventConfigs = value;

                    RaisePropertyChanged(() => ChildEventConfigs);
                }
            }
        }

        private string o_Header;
        public string Header
        {
            get
            {
                if(string.IsNullOrWhiteSpace(o_Header))
                    o_Header = ToDoList[0].GetString();

                return o_Header;
            }
        }

        private bool o_IsSelected = true;
        public bool IsSelected
        {
            get
            {
                return o_IsSelected;
            }
            set
            {
                if (o_IsSelected != value)
                {
                    o_IsSelected = value;

                    RaisePropertyChanged(() => IsSelected);
                }
            }
        }

        private bool o_IsExpanded = true;
        public bool IsExpanded
        {
            get
            {
                return o_IsExpanded;
            }
            set
            {
                if (o_IsExpanded != value)
                {
                    o_IsExpanded = value;

                    RaisePropertyChanged(() => IsExpanded);
                }
            }
        }

        private bool o_IsVisible = true;
        public bool IsVisible
        {
            get
            {
                return o_IsVisible;
            }
            set
            {
                if (o_IsVisible != value)
                {
                    o_IsVisible = value;

                    RaisePropertyChanged(() => IsVisible);
                }
            }
        }
    }
}
