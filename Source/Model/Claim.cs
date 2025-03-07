namespace CoP_Viewer.Source.Model
{
    internal class Claim
    {
        public String name;
        public double totalTax;
        public double totalManpower;

        public Claim(String name)
        {
            this.name = name;
            this.totalTax = 0;
            this.totalManpower = 0;
        }
    }
}
