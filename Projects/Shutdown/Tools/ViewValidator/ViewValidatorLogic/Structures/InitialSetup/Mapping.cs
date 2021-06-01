// -----------------------------------------------------------
// Created by Benjamin Held - 15.08.2011 11:40:36
// Copyright AvenDATA 2011
// -----------------------------------------------------------

namespace ViewValidatorLogic.Structures.InitialSetup {
    public class Mapping {
        public string Source { get; set; }
        public string Destination { get; set; }
        public bool Used { get; set; }

        public Mapping() {

        }
        public Mapping(string source, string dest){
            this.Source = source;
            this.Destination = dest;
        }

        public string Get(int which) {
            if (which == 0) return Source;
            else return Destination;
        }
    }
}
