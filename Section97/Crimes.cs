using Il2CppScheduleOne.Law;

namespace Section97.Crimes
{
    public class ArmedRobbery : Crime
    {
        public virtual string CrimeName
        {
            get => "Armed Robbery";
            set => base.CrimeName = value;
        }
    }
}