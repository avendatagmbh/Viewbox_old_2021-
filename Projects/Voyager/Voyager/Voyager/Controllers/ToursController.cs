using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Voyager.Models;
using DbAccess.Structures;
using Domain;
using Domain.Entities.Tables;
using Domain.Manager;
using Domain.TextDaten;

namespace Voyager.Controllers
{
   [Authorize]
    public class ToursController : Controller{

       [HttpGet]
       public ActionResult Index() {

           ToursModels model = GetModel();
           if (model == null) return null;
           return View(model);
       }

       [HttpPost]
       public ActionResult Index(ToursModels mod) {
           ToursModels model =GetModel();
           try {
                   //if (model.editedStart.AddressString != "" && model.editedEnd.AddressString != "") {
                   //    CurrentUser.BeforSave(model);
                   //    Manager.Save(Config.Conf, model.Date);
                   //} else {
                   //    ModelState.AddModelError("", "Nicht alle Felder sind ausgefüllt.");
                   //}
           } catch (Exception ex) {
              
           }
           return View(model);
       }

       public ActionResult Add() {
           CurrentUser.UserModel.Counter++;
           ToursModels model = GetModel();
           return View("Index",model);
       }

       public ActionResult Address(int ID) {
           for (int i = 0; i < CurrentUser.UserModel.AddressStatebook.Count; i++) {
               if (CurrentUser.UserModel.AddressStatebook[(uint)i]) {
                 SetAddressToDate(i,CurrentUser.UserModel.AddressStatebook.Count, ID, CurrentUser.UserModel.Dates);
                    if (ID == CurrentUser.UserModel.UserAddress.AddressID) {
                       CurrentUser.UserModel.Addressbook[(uint)i] = CurrentUser.UserModel.UserAddress;
                       CurrentUser.UserModel.AddressStatebook[(uint)i] = false;
                   } else {
                       CurrentUser.UserModel.Addressbook[(uint)i] = CurrentUser.UserModel.UserFirmAddress;
                       CurrentUser.UserModel.AddressStatebook[(uint)i] = false;
                   }
               }
           }
              
        ToursModels model = GetModel();
        return View("Index",model);
       }

      [HttpGet]
      public PartialViewResult EditAddress() {

           ToursModels model = GetModel();
           return PartialView(model);
       }

       [HttpPost]
       public RedirectToRouteResult EditAddress(string data) {
           //TODO

           if (ModelState.IsValid) {
               for (int i = 0; i < CurrentUser.UserModel.AddressStatebook.Count; i++) {
                   if (CurrentUser.UserModel.AddressStatebook[(uint)i]) {
                       //CurrentUser.UserModel.Addressbook[(uint)i] = mod.newEditedAddresse;
                       CurrentUser.UserModel.AddressStatebook[(uint)i] = false;
                   }
               }
               return RedirectToAction("Index");
           } else {
               ModelState.AddModelError("", "Nicht alle Felder geben einen Wert zurück.");
           }
           return RedirectToAction("EditAddressValid");
       }

       //[HttpGet]
       public ActionResult EditAddressValid() {
           ToursModels model = GetModel();
           model.Counter = CurrentUser.UserModel.Counter;
           model.EditState = true; 
           return View("Index", model);
       }

       public PartialViewResult AddressForm(string text) {
           State(text);
           ToursModels model = GetModel();
           return PartialView(model);
       }

       
       public PartialViewResult ClientSelect() {
           ToursModels model = GetModel();
           return PartialView(model);
       }


       public ActionResult ClientSelected(int selectedclientID) {
           int index = selectedclientID - 1;
           Client selectedClient = CurrentUser.UserModel.Clients.ElementAt(index).Value;
           Address clientaddress = Manager.LoadAddress(Config.Conf, selectedClient.Address);
           int i = 0;
           while (CurrentUser.UserModel.AddressStatebook.ContainsKey((uint)i)) {

               if (CurrentUser.UserModel.AddressStatebook.ElementAt(i).Value == true) {
                   CurrentUser.UserModel.Addressbook[(uint)i] = clientaddress;
                   CurrentUser.UserModel.AddressStatebook[(uint)i] = false;
                   if (i != 0) CurrentUser.UserModel.Dates[(i - 1)].Client = selectedclientID;
                   SetAddressToDate(i, CurrentUser.UserModel.AddressStatebook.Count, selectedclientID, CurrentUser.UserModel.Dates);
               }
               i++;
           }
           ToursModels model = GetModel();
           return View( "Index",model);
       }

      
       public PartialViewResult DateForm() {
           return PartialView();
       }

       [HttpGet]
       public ViewResult DateEdit(string rr, int srhour, string srmin, string date, string a) {
           DateTime datetime;
           DateTime.TryParse(String.Format("{0} {1} :{2}", date, srhour, srmin), out datetime);
           if (rr == "Startzeit") {
               CurrentUser.UserModel.Dates[int.Parse(a[0].ToString())].StartTime = datetime;
           } else {
               CurrentUser.UserModel.Dates[int.Parse(a[0].ToString())-1].EndTime = datetime;
           }
           ToursModels model = new ToursModels();
           return View("Index", model);
       }

       private ToursModels GetModel() {
           ToursModels model = new ToursModels();
           model.UserAddress = CurrentUser.UserModel.UserAddress;
           model.Firm = CurrentUser.UserModel.Firm;
           model.UserFirmAddress = CurrentUser.UserModel.UserFirmAddress;
           model.Clients = CurrentUser.UserModel.Clients;
           model.Counter = CurrentUser.UserModel.Counter;
           model.EditState = CurrentUser.UserModel.EditState;
           model.Dates = CurrentUser.UserModel.Dates;
           model.Addressbook = CurrentUser.UserModel.Addressbook;
           model.AddressStatebook = CurrentUser.UserModel.AddressStatebook;
           return model;
       }

       private void State(string str) {

           for (int i = 0; i < CurrentUser.UserModel.AddressStatebook.Count; i++) {
               CurrentUser.UserModel.AddressStatebook[(uint)i] = false;
           }
               if (str[0] == 'S') {
                   if (CurrentUser.UserModel.AddressStatebook.ContainsKey(0)) {
                       CurrentUser.UserModel.AddressStatebook[0] = true;
                   } else {
                       CurrentUser.UserModel.AddressStatebook.Add(0, true);
                   }

               } else {
                   uint i = (uint)int.Parse(str[0].ToString());
                   if (CurrentUser.UserModel.AddressStatebook.ContainsKey(i)) {
                       CurrentUser.UserModel.AddressStatebook[i] = true;
                   } else {
                       CurrentUser.UserModel.AddressStatebook.Add(i, true);
                   }
               }
       }

       private void SetAddressToDate(int addressstate, int MaxCounter, int ID, Dictionary<int,Date> dates) {
           
           Date date = new Date();
           if (addressstate == 0) {
               dates[0].StartAddress = ID; 
               return; 
           }
           if (addressstate == (MaxCounter-1)) {
               dates[MaxCounter - 2].EndAddress = ID;
               return;
           }
           dates[addressstate].StartAddress = ID;
           dates[(addressstate - 1)].EndAddress = ID;
           return;
       }

    }
}
