
namespace ViewValidator.Structures {
    class CompareDataColl {

        MapModel map;
        DbConfigModel confModel;
        public bool valid; // Flag ob Spalten etc. definiert sind // Stil...naja

        public DbConfigModel ConfModel { get { return confModel; } }
        public bool Valid { get; set; }
        public MapModel Map { get { return map; } }
        public OptionModel Options { get; set; }


        public CompareDataColl(DbConfigModel configModel,MapModel _map){
            this.confModel = configModel;
            valid = false;
            this.map = _map;
            Options = new OptionModel();
        }


    }
}
