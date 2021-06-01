using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace EasyDocExtraction.Model
{
    /// <summary>
    /// Holds information about a single field of an archivedItem (the metadata ) 
    /// the definition of the field is defined by the fileddefinition id coupled with the easyfolderid
    /// (field definitions are specific to the DB/folder)
    /// </summary>
    public class FieldValue
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FieldValueId { get; set; }
        [Column(Order = 0)]//,ForeignKey("FieldDefinition")]
        public int FieldDefinitionId { get; set; }
        [Column(Order = 1)]//,ForeignKey("FieldDefinition")
        public int EasyFolderId { get; set; }

        [Column(TypeName = "text"), MaxLength(65535)]
        public string Value { get; set; }
        [NotMapped]
        public virtual FieldDefinition FieldDefinition { get; set; }

        public override string ToString()
        {
            return string.Format("{0} : {1}", Value, FieldDefinition.ToString());
        }
    }
}
