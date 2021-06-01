// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-09-18
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DbAccess;

namespace GenericOdbc {
    public static class DbTemplateManager {
        public static IEnumerable<IDbTemplate> GetTemplates() {
            if (!Directory.Exists("DbTemplates")) return new List<IDbTemplate>();
            return
                new List<IDbTemplate> {new DbTemplate()}.Union(
                    Directory.GetFiles("DbTemplates").Select(file => new DbTemplate(file)));
        }

        public static IDbTemplate GetTemplate(string templateName) {
            return string.IsNullOrEmpty(templateName)
                       ? null
                       : GetTemplates().FirstOrDefault(template => template.Filename == templateName);
        }
    }
}