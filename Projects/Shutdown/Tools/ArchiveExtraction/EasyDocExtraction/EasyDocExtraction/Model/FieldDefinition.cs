using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace EasyDocExtraction.Model
{
    public class FieldDefinition
    {
        /// <summary>
        /// Is the code value of the field as in the extracted text document from Easy.
        /// </summary>
        [Key, Column(Order = 0), DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int FieldDefinitionId { get; set; }
        /// <summary>
        /// As each Easy DB has on set of field header/definition and one codeId/fielddefinition 
        /// can have different meaning according the Easy DB, we need a extra PK/FK on EasyFolderId
        /// </summary>
        [Key, Column(Order = 1)]//, ForeignKey("EasyFolder")]
        public int EasyFolderId { get; set; }

        [Column(TypeName = "varchar"), MaxLength(100)]
        public string FieldName { get; set; }
        [NotMapped()]
        public FieldType FieldType { get; set; }
        /// <summary>
        /// helper property for FieldType , bypasses the screwed mapping between Mysql and EF
        /// </summary>
        [Column("FieldType")]
        public byte FieldType2 { get { return (byte)FieldType; } set { FieldType = (FieldType)value; } }
        [NotMapped()]
        public FieldFormat FieldFormat { get; set; }
        /// <summary>
        /// helper property for FieldFormat , bypasses the screwed mapping between Mysql and EF
        /// </summary>
        [Column("FieldFormat")]
        public byte FieldFormat2 { get { return (byte)FieldFormat; } set { FieldFormat = (FieldFormat)value; } }
        [NotMapped()]
        public virtual EasyFolder EasyFolder { get; set; }

        public override string ToString()
        {
            return string.Join(", ", (from p in this.GetType().GetProperties() select p.Name + "=" + p.GetValue(this, null)).ToArray());
        }
    }
}
