using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace EasyDocExtraction.Model
{
    /// <summary>
    /// Represents the folder system in which Easy stores archives.
    /// </summary>
    public class EasyFolder
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EasyFolderId { get; set; }
        [Column(TypeName = "varchar"), MaxLength(100)]
        public string Name { get; set; }
        //public int ParentFolderId { get; set; }
        public override string ToString()
        {
            return "EasyFolder: " + Name;
        }
    }
}
