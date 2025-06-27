namespace CoP_Viewer.Source.UI
{
    public class Entry
    {
        private String key;
        private String prefix;
        private String suffix;

        public String Text;

        public Entry(String key, String? value, String prefix, String suffix)
        {
            this.key = key;
            this.prefix = prefix == null ? "" : prefix;
            this.suffix = suffix == null ? "" : suffix;

            setValue(value);
        }

        public String getKey()
        {
            return key;
        }

        public void setValue(String? newValue)
        {
            if (newValue == null)
            {
                this.Text = "";
            }
            else if (newValue == "")
            {
                this.Text = key + ": N/A";
            }
            else
            {
                this.Text = key + ": " + prefix + newValue + suffix;
            }
        }
    }
}
