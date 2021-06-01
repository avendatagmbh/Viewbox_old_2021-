using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocumentationGeneratorBusiness {
    class AttachmentGenerator {

        //Create Attachment 3 - Optimierungskriterien
        void CreateAttachment3() {
            //the following fields are fixed now, but that has to be changed later -> user input
            var customer = "Saint Gobain Diamantwerkzeuge GmbH & Co KG";
            var project = "Abschaltung des Altsystems SAP R/3 Wesseling 020";
            var name = "Anlage 3: Optimierungskriterien";
            var processor = "Peer Hildebrandt";

            var date = DateTime.Now;

            //create pdf for attachment 3 - use Project PdfGenerator -> therefor make yourself familiar with the pdf library itextsharp

        }

        //Create Attachment 5 - Viewübersicht
        void CreateAttachment5() {
            //like CreateAttachment3() for Attachment 5 - Viewübersicht
        }
    }
}
