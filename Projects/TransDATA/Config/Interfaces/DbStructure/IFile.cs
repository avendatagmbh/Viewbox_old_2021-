// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-11
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace Config.Interfaces.DbStructure {
    public interface IFile : ITransferEntity {
        
        /// <summary>
        /// Assigned profile.
        /// </summary>
        IProfile Profile { get; }

        /// <summary>
        /// Path of the file.
        /// </summary>
        string Path{ get; set; }
        
        /// <summary>
        /// Size of the file.
        /// </summary>
        long Size { get; set; }
        
        /// <summary>
        /// Needed to handle splitted csv/binary files. Default value = 1.
        /// </summary>
        int Part { get; set; }
    }
}