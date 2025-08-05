using CoP_Viewer.Source.Util;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json.Linq;
using System;

namespace CoP_Viewer.Source.Model
{
    public class MasterSheet
    {
        private string CLAIM_HEX = "A:A";
        private string CLAIM_NAME = "B:B";
        private string CLAIM_RECRUITCASUALTIES = "C:C";
        private string CLAIM_VETERANCASUALTIES = "D:D";
        private string CLAIM_TAXMODIFIER = "E:E";
        private string CLAIM_TRADE = "F:F";
        private string CLAIM_PLAYER = "G:G";
        private string CLAIM_SHEET_URL = "H:H";

        private string GLOBAL_CURRENT_TICK = "J3";

        private string CITY_NAME = "";
        private string CITY_XCoords = "";
        private string CITY_YCoords = "";

        private string REGION_NAME = "P:P";
        private string REGION_HEX = "Q:Q";
        private string REGION_TAXMODIFIER = "R:R";
        private string REGION_RECRUITMANPOWER = "S:S";
        private string REGION_VETERANMANPOWER = "T:T";

        private string TERRAIN_NAME = "V:V";
        private string TERRAIN_HEX = "W:W";
        private string TERRAIN_BASETAX = "X:X";
        private string TERRAIN_BASEMANPOWER = "Y:Y";

        public string url { get; set; }

        public double currentTick { get; set; }

        public BatchGetValuesResponse? responseValues { get; set; }

        public MasterSheet(string url)
        {
            this.url = url;
        }

        public string CurrentTickCell()
        {
            return GLOBAL_CURRENT_TICK;
        }

        public List<string> ClientSheetsRanges()
        {
            return [CLAIM_HEX, CLAIM_NAME, CLAIM_SHEET_URL, CLAIM_RECRUITCASUALTIES, CLAIM_VETERANCASUALTIES];
        }

        public BatchUpdateValuesRequest GetBatchUpdateValuesRequest()
        {
            var values = responseValues.ValueRanges;
            BatchUpdateValuesRequest request = new BatchUpdateValuesRequest();
            List<ValueRange> ranges = new List<ValueRange>();

            currentTick = Convert.ToDouble(values[0].Values[0][0]);

            if (currentTick < 1) {
                throw new IndexOutOfRangeException("Master sheet current tick cannot be less than 1");
            }

            ranges.Add(SheetHandler.createCellValueRange(GLOBAL_CURRENT_TICK, currentTick + 1));

            request.Data = ranges;

            return request;
        }
    }
}
