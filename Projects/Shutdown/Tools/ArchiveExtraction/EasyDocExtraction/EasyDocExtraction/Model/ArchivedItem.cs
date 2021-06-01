using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace EasyDocExtraction.Model
{
    public class ArchivedItem
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ArchivedItemId { get; set; }
        //[ForeignKey("EasyFolder")]
        public int? EasyFolderId { get; set; }
        [NotMapped()]
        public EasyFolder EasyFolder { get; set; }
        public virtual ICollection<ArchivedDocument> ArchivedDocuments { get; set; }
        public virtual ICollection<FieldValue> FieldValues { get; set; }
        /// <summary>
        /// If the fieldvalue collection differs from the fieldDefinition collection , 
        /// data are likely to be corrupted, in that case mapping between data and their meaning is 'impossible'
        /// </summary>
        public bool CorruptedData { get; set; }
    }
}
