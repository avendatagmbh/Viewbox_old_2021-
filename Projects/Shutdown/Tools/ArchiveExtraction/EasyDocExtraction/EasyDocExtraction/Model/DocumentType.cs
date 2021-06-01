using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace EasyDocExtraction.Model
{
    public class DocumentType
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DocumentTypeId { get; set; }
        [Column(TypeName = "varchar"), MaxLength(30)]
        public string Name { get; set; }
    }

}
