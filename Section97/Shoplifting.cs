using Il2CppScheduleOne.Law;

namespace Section97
{
    public class Shoplifting : Crime
    {

        public override string CrimeName
        {
            get => nameof(Shoplifting);
            set => base.CrimeName = value;
        }
    }
}