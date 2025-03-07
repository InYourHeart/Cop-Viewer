namespace CoP_Viewer.Source.Model
{
    public class Region
    {
        public String name;
        public double taxModifier;
        public double manpowerModifier;

        public Region(String name, double taxModifier, double manpowerModifier)
        {
            this.name = name;
            this.taxModifier = taxModifier;
            this.manpowerModifier = manpowerModifier;
        }
    }
}
