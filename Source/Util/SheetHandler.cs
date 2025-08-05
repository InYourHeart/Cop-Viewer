using CoP_Viewer.Source.Model;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace CoP_Viewer.Source.Util
{
    public class SheetHandler
    {
        private const string CREDENTIALS_KEY = "Credentials\\key.json";
        private const string MASTER_SHEET_URL_FILE = "Credentials\\master_sheet_url.txt";

        private SheetsService service;

        private MasterSheet masterSheet;
        private List<ClientSheet>? clientSheets;

        private Dictionary<int, Claim> claimList;

        public SheetHandler(Dictionary<int, Claim> claimList)
        {
            var credential = GoogleCredential.FromFile(AppDomain.CurrentDomain.BaseDirectory + CREDENTIALS_KEY).CreateScoped(SheetsService.Scope.Spreadsheets);

            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential
            });
            Logger.logInfo("Initialized SheetsService from file");

            var masterSheetURL = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + MASTER_SHEET_URL_FILE);
            masterSheet = new MasterSheet(masterSheetURL);
            Logger.logInfo("Set Master Sheet from file at URL " + masterSheetURL);

            clientSheets = null;
            this.claimList = claimList;
            Logger.logInfo("SheetHandler initialized");
        }

        public SheetHandler()
        {
            //TODO Google sheets initialization
            throw new NotImplementedException();
        }

        public void TickSheets()
        {
            Logger.logInfo("Starting tick...");

            //Get the values for all the sheets before making updates to any for validation.
            //We don't want the Tick to stop midway and leave some sheets updated and others not.
            masterSheet.responseValues = BatchGet(masterSheet.url, new List<string> { masterSheet.CurrentTickCell() });
            BatchUpdateValuesRequest masterSheetUpdateValues = masterSheet.GetBatchUpdateValuesRequest();

            GetClientSheetsInfo();

            List<BatchUpdateValuesRequest> updateValuesList = new List<BatchUpdateValuesRequest>();

            foreach(ClientSheet sheet in clientSheets)
            {
                //Null is a valid value for client sheet URLs, usually when the claim is an NPC, so we don't want to throw an Exception
                if (sheet.url != null && !sheet.url.Equals("null"))
                {
                    sheet.responseValues = BatchGet(sheet.url, sheet.GetBatchGetRanges());

                    updateValuesList.Add(sheet.GetBatchUpdateValuesRequest());
                }
            }

            //Update the master sheet
            if (BatchUpdate(masterSheet.url, masterSheetUpdateValues))
            {
                Logger.logInfo("Updated master sheet " + masterSheet.url);
            }
            else
            {
                Logger.logInfo("Failed to update master sheet " + masterSheet.url);
            }

            //Update the client sheets
            for (int i = 0; i < clientSheets.Count; i++)
            {
                ClientSheet sheet = clientSheets[i];

                //Skipping over nulls in the same way will match client sheets with their respective entry in updateValuesList
                if (sheet.url != null && !sheet.url.Equals("null"))
                {
                    if (BatchUpdate(sheet.url, updateValuesList[i]))
                    {
                        Logger.logInfo("Updated sheet " + sheet.url);
                    }
                    else
                    {
                        Logger.logInfo("Failed to update sheet " + sheet.url);
                    }
                }
            }
        }

        private bool GetClientSheetsInfo()
        {
            Logger.logInfo("Retrieving client sheets URLs from Master Sheet at URL " + masterSheet.url);

            clientSheets = new List<ClientSheet>();

            BatchGetValuesResponse? response = BatchGet(masterSheet.url, masterSheet.ClientSheetsRanges());

            if (response == null)
            {
                Logger.logError("Failed to retrieve client sheet URLs");
                return false;
            }

            for (int i = 2; i < response.ValueRanges[0].Values.Count; i++)
            {
                string? claimKey = response.ValueRanges[0].Values[i][0].ToString();
                string? claimName = response.ValueRanges[1].Values[i][0].ToString();
                string? sheetURL = response.ValueRanges[2].Values[i][0].ToString();
                double recruitCasualties = Convert.ToDouble(response.ValueRanges[3].Values[i][0].ToString());
                double veteranCasualties = Convert.ToDouble(response.ValueRanges[4].Values[i][0].ToString());

                if (claimKey != null && sheetURL != null && !sheetURL.Equals("null"))
                {
                    int hexKey = PixelHandler.hexToInt(claimKey);

                    Logger.logInfo("Retrieved client sheet URL - Claim name: " + claimName + ", URL: " + sheetURL);
                    Claim c;
                    claimList.TryGetValue(hexKey, out c);
                    ClientSheet cs = new ClientSheet();
                    cs.url = sheetURL;
                    cs.claimHex = claimKey;
                    cs.baseTax = c.totalTax;
                    cs.recruitManpowerBalance = c.totalManpower * 0.08 - recruitCasualties;
                    cs.veteranManpowerBalance = c.totalManpower * 0.02 - veteranCasualties;
                    cs.masterSheetCurrentTick = masterSheet.currentTick;

                    clientSheets.Add(cs);
                }
            }

            return clientSheets.Count > 0;
        }

        private ValueRange? Get(string spreadsheetId, string range)
        {
            try
            {
                var request = service.Spreadsheets.Values.Get(spreadsheetId, range);

                return request.Execute();
            } catch (Exception ex)
            {
                Logger.logError("SheetHandler.Get - " + ex.Message);
                return null;
            }
        }

        private bool Update(ValueRange body, string spreadsheetId, string range)
        {
            try
            {
                var update = service.Spreadsheets.Values.Update(body, spreadsheetId, range);
                update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

                UpdateValuesResponse response = update.Execute();
                Logger.logInfo("SheetHandler.Update - Updated " + response.UpdatedCells + " cells in range " + response.UpdatedRange + " ");
                
                return true;
            }
            catch (Exception ex)
            {
                Logger.logError("Update - " + ex.Message);
                return false;
            }
        }

        private BatchGetValuesResponse? BatchGet(string spreadsheetId, List<string> ranges)
        {
            try
            {
                var request = service.Spreadsheets.Values.BatchGet(spreadsheetId);
                request.Ranges = ranges;
                request.ValueRenderOption = SpreadsheetsResource.ValuesResource.BatchGetRequest.ValueRenderOptionEnum.UNFORMATTEDVALUE;

                return request.Execute();
            }
            catch (Exception ex)
            {
                Logger.logError("SheetHandler.BatchGet - " + ex.Message);
                return null;
            }
        }

        private bool BatchUpdate(string spreadsheetId, BatchUpdateValuesRequest request)
        {
            try
            {
                request.ValueInputOption = "USER_ENTERED";
                var update = service.Spreadsheets.Values.BatchUpdate(request, spreadsheetId);

                BatchUpdateValuesResponse response = update.Execute();

                foreach (UpdateValuesResponse responses in response.Responses)
                {
                    Logger.logInfo("SheetHandler.BatchUpdate - Updated " + responses.UpdatedCells + " cells in range " + responses.UpdatedRange + " ");
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.logError("BatchUpdate - " + ex.Message);
                return false;
            }
        }

        public static ValueRange createCellValueRange(string range, object value)
        {
            ValueRange valueRange = new ValueRange();
            valueRange.Range = range;
            valueRange.Values = [new List<object>()];
            valueRange.Values[0].Add(value);

            return valueRange;
        }
    }
}
