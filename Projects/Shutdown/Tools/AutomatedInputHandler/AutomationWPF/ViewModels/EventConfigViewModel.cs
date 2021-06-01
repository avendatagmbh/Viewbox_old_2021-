using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using AutomationWPF.Models;
using AutomationWPF.Events;

namespace AutomationWPF.ViewModels
{
    public class EventConfigViewModel : BaseViewModel
    {
        public EventConfigViewModel()
        {
            EventConfigModels = new ObservableCollection<EventConfigModel>();

            EventConfigModel model = new EventConfigModel();
            model.ToDoList = new ObservableCollection<Events.AutomatedEventBase>();
            model.ToDoList.Add(new AutomatedEventWait(300));
            model.ToDoList.Add(new AutomatedEventWait(300));

            EventConfigModel model2 = new EventConfigModel();
            model2.ToDoList = new ObservableCollection<Events.AutomatedEventBase>();
            model2.ToDoList.Add(new AutomatedEventWait(500));
            model2.ToDoList.Add(new AutomatedEventWait(500));

            EventConfigModel model3 = new EventConfigModel();
            model3.ToDoList = new ObservableCollection<Events.AutomatedEventBase>();
            model3.ToDoList.Add(new AutomatedEventWait(700));
            model3.ToDoList.Add(new AutomatedEventWait(700));

            model2.ChildEventConfigs = new ObservableCollection<EventConfigModel>();
            model2.ChildEventConfigs.Add(model3);

            model.ChildEventConfigs = new ObservableCollection<EventConfigModel>();
            model.ChildEventConfigs.Add(model2);

            EventConfigModels.Add(model);
            EventConfigModels.Add(model);
        }

        private ObservableCollection<EventConfigModel> o_EventConfigModels;
        public ObservableCollection<EventConfigModel> EventConfigModels
        {
            get
            {
                return o_EventConfigModels;
            }
            set
            {
                if (o_EventConfigModels != value)
                {
                    o_EventConfigModels = value;

                    RaisePropertyChanged(() => EventConfigModels);
                }
            }
        }
    }
}
