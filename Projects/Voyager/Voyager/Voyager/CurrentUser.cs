using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Domain.Manager;
using DbAccess;
using Domain;
using Domain.Entities.Tables;
using Voyager.Models;

namespace Voyager {
    public static class CurrentUser {


        public static void GetCurrentUser(Employee newuser) {
            User = newuser;
            UserModel = new ToursModels();
            UserModel.Firm = Manager.LoadUserFirm(Config.Conf, User.UserFirm);
            UserModel.UserFirmAddress = Manager.LoadAddress(Config.Conf, UserModel.Firm.Address);
            UserModel.UserAddress = Manager.LoadAddress(Config.Conf, User.Address);
            UserModel.Clients  = Manager.LoadClientList(Config.Conf, UserModel.Firm.UserFirmID);
            UserModel.Counter = 1;
            UserModel.EditState = false;
        }


        public static Employee User {
            get {
                return (Employee)HttpContext.Current.Session["user"];
            }
            set {
                HttpContext.Current.Session.Add("user", value);
            }
        }

        public static ToursModels UserModel {
            get {
                return (ToursModels)HttpContext.Current.Session["tour"];
            }
            set {
                HttpContext.Current.Session.Add("tour", value);
            }
        }

        public static ToursModels BeforSave(ToursModels model) {
            for (int i = 0; i < model.Dates.Count; i++) {
          
                if (model.Dates[i].EndTime == null) {
                    model.Dates[i].EndTime = DateTime.Now;
                }
                if (model.Dates[i].StartTime == null) {
                    model.Dates[i].StartTime = DateTime.Now;
                }
            }
            return model;
        }
    }
}