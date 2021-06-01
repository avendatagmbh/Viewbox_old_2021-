// -----------------------------------------------------------
// Created by Benjamin Held - 22.07.2011 15:45:06
// Copyright AvenDATA 2011
// -----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBalanceKitBackup {
    class CommandLineParser {
        string[] Args { get; set; }
        string Divider { get; set; }

        public CommandLineParser(string[] args, string divider) {
            this.Args = args;
            this.Divider = divider;
        }

        public bool HasSwitch(string name) {
            foreach (string arg in Args)
                if (arg.Equals(Divider + name)) return true;
            return false;
        }

        public string Argument(string name) {
            for (int i = 0; i < Args.Length; ++i) {
                if (Args[i].Equals(Divider + name) && i < Args.Length - 1)
                    return Args[i + 1];
            }
            throw new Exception("Das Argument " + Divider+name + " wurde nicht korrekt angegeben.");
        }
    }
}
