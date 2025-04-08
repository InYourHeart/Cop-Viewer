namespace CoP_Viewer.Source.Model
{
    public class Terrain
    {
        public String name;
        public String hex;
        public int baseTax;
        public int baseManpower;

        public Terrain(String name, String hex, int baseTax, int baseManpower)
        {
            this.name = name;
            this.hex = hex;
            this.baseTax = baseTax;
            this.baseManpower = baseManpower;
        }
    }
}
