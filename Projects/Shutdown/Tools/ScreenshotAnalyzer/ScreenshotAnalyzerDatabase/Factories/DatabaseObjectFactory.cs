// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-12-05
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Xml;
using DbAccess;
using DbAccess.Structures;
using ScreenshotAnalyzerDatabase.Config;
using ScreenshotAnalyzerDatabase.Interfaces;

namespace ScreenshotAnalyzerDatabase.Factories {
    public static class DatabaseObjectFactory {
        public static IDbTable CreateTable(IDbProfile profile) {
            return new DbTable(){Profile = (DbProfile) profile};
        }

        public static IDbScreenshotGroup CreateScreenshotGroup(IDbTable dbTable) {
            DbScreenshotGroup scrGroup = new DbScreenshotGroup() { Table = (DbTable)dbTable };
            scrGroup.Save();
            return scrGroup;
        }

        public static IDbScreenshot CreateScreenshot(IDbScreenshotGroup dbScreenshotGroup) {
            return new DbScreenshot() { ScreenshotGroup = (DbScreenshotGroup)dbScreenshotGroup };
        }

        public static IDbOcrRectangle CreateRectangle(IDbScreenshot dbScreenshot, Point upperLeft, Point lowerRight, RectType type) {
            return new DbOcrRectangle((DbScreenshot) dbScreenshot, upperLeft, lowerRight, type);
        }

        public static IEnumerable<IDbOcrRectangle> LoadRectangles(IDbScreenshot dbScreenshot) {
            using (IDatabase conn = ((DbScreenshot)dbScreenshot).ScreenshotGroup.Table.Profile.GetOpenConnection()) {
                IEnumerable<IDbOcrRectangle> result = conn.DbMapping.Load<DbOcrRectangle>("scr_id = " + ((DbScreenshot)dbScreenshot).Id);
                foreach (DbOcrRectangle dbOcrRect in result)
                    dbOcrRect.DbScreenshot = (DbScreenshot) dbScreenshot;
                return result;
            }
            
        }
        //Load result or create a new one
        public static IDbResult LoadResult(IDbScreenshotGroup dbScreenshotGroup) {
            using (IDatabase conn = ((DbScreenshotGroup)dbScreenshotGroup).Table.Profile.GetOpenConnection()) {
                List<DbResult> result =
                    conn.DbMapping.Load<DbResult>("scr_group_id = " + ((DbScreenshotGroup) dbScreenshotGroup).Id);
                if (result.Count == 1) {
                    result[0].ScreenshotGroup = (DbScreenshotGroup) dbScreenshotGroup;
                    result[0].Load();
                    return result[0];
                }
            }
            return null;
        }

        public static IDbResult CreateResult(IDbScreenshotGroup dbScreenshotGroup) {
            return new DbResult((DbScreenshotGroup)dbScreenshotGroup);
        }

        public static IDbResultColumn CreateResultColumn(IDbResult dbResult, IDbScreenshotGroup dbScreenshotGroup, IDbOcrRectangle dbRect, string header) {
            return new DbResultColumn((DbResult) dbResult,(DbScreenshotGroup) dbScreenshotGroup,(DbOcrRectangle) dbRect, header);
        }

        public static IDbResultEntry CreateResultRowEntry(IDbResult dbResult, IDbScreenshot dbScreenshot, IDbOcrRectangle dbOcrRect, string textResult) {
            return new DbResultEntry((DbResult) dbResult, (DbScreenshot) dbScreenshot, (DbOcrRectangle) dbOcrRect, textResult);
        }

        public static void SetDbOcrRectangle(IDbResultEntry entry, IDbOcrRectangle dbRect) {
            ((DbResultEntry) entry).OcrRectangle = (DbOcrRectangle) dbRect;
        }
    }
}
