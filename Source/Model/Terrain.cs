namespace CoP_Viewer.Source.Model
{
    public class Terrain
    {
        public String name;
        public String hex;
        public int baseManpower;

        public Terrain(String name, String hex, int baseManpower)
        {
            this.name = name;
            this.hex = hex;
            this.baseManpower = baseManpower;
        }
    }
}
