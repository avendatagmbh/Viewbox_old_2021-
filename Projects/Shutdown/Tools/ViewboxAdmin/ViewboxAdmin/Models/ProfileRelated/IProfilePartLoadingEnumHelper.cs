using Part= SystemDb.SystemDb.Part;

namespace ViewboxAdmin.Models.ProfileRelated
{
    /// <summary>
    /// interface for some hard-to-read enum related manipulation
    /// </summary>
    public interface IProfilePartLoadingEnumHelper {
        int LengthOfEnum();
        Part GetNextPartEnum(Part part);
        bool IsNotLast(Part part);
       
    }
}
