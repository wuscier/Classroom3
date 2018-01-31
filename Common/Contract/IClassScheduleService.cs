using Common.Model;

namespace Common.Contract
{
    public interface IClassScheduleService
    {
        int ReserveClass(Course course);
    }

}
