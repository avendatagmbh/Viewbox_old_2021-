using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace EasyDocExtraction.Model
{
    public class ArchivedDocument
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ArchivedDocumentId { get; set; }
        [ForeignKey("ArchivedItem")]
        public int ArchivedItemId { get; set; }
        //[ForeignKey("DocumentType")]
        public int? DocumentTypeId { get; set; }
        [Column(TypeName = "LONGBLOB")]
        public byte[] DocumentData { get; set; }

        /// <summary>
        /// The original document path at the time where the export was made (used for debugging information).
        /// </summary>
        [Column(TypeName = "varchar")]
        public string DocumentPath { get; set; }
        public virtual ArchivedItem ArchivedItem { get; set; }
        [NotMapped()]
        public DocumentType DocumentType { get; set; }
    }
}
