using System;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace CoP_Viewer.Source.UI
{
    public class InfoView : ListBox
    {
        private Dictionary<String, Entry> entryDict;

        public InfoView()
        {
            entryDict = new Dictionary<String, Entry>();

            List<Entry> entryList =
            [
                new Entry("Claim", null, "", ""),
                new Entry("City", null, "", ""),
                new Entry("Region", null, "", ""),
                new Entry("Occupied by", null, "", ""),
                new Entry("Devastation", null, "", "%"),
                new Entry("Tax", null, "", " £"),
                new Entry("Manpower", null, "", " men"),
            ];

            foreach (Entry entry in entryList)
            {
                entryDict.Add(entry.getKey(), entry);
            }
        }

        public void ResizeView(int newHeight, int newWidth, Point newLocation)
        {
            this.Height = newHeight;
            this.Width = newWidth;
            this.Location = newLocation;
        }

        public void setPixelInfoViewEntryValue(String key, String value)
        {
            setEntryValue(key, value);
        }

        public void setPixelInfoViewEntryValue(String key, double value)
        {
            setEntryValue(key, value);
        }

        private void setEntryValue(String key, String value)
        {
            entryDict[key].setValue(value);

            this.Items.Clear();

            foreach (Entry entry in entryDict.Values)
            {
                if (entry.Text != "")
                {
                    this.Items.Add(entry.Text);
                }
            }
        }

        private void setEntryValue(String key, double value)
        {
            if (value > 1000000)
            {
                setEntryValue(key, value.ToString("0,,.##M", CultureInfo.InvariantCulture));
            }
            else
            {
                setEntryValue(key, value.ToString());
            }
        }
    }
}
