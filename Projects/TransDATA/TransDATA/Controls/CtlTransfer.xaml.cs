// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-06
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Base.Localisation;
using Business;
using Config.Interfaces.DbStructure;
using Business.Interfaces;
using Business.Structures.DataTransferAgents;
using TransDATA.Models;

namespace TransDATA.Controls {
    /// <summary>
    /// Interaktionslogik für CtlTransfer.xaml
    /// </summary>
    public partial class CtlTransfer : UserControl {
        public CtlTransfer() { InitializeComponent(); }

        TransferModel Model { get { return DataContext as TransferModel; } }

        private void BtnCancelClick(object sender, RoutedEventArgs e) {
            Model.Cancel();
        }
    }
}