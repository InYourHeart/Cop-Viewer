using Google.Apis.Sheets.v4.Data;
using System;

namespace CoP_Viewer.Source.Model
{
    public class ClientSheet
    {
        //Summary
        private string SUMMARY_BUDGET_PREVIOUSBALANCE = "Summary!I13";
        private string SUMMARY_BUDGET_REVENUES = "Summary!I14";
        private string SUMMARY_BUDGET_EXPENDITURES = "Summary!I15";
        private string SUMMARY_REVENUES_TAX = "Summary!D21";
        private string SUMMARY_CURRENTTICK = "Summary!C29";
        private string SUMMARY_RECRUITMANPOWER_TOTAL = "Summary!N5";
        private string SUMMARY_VETERANMANPOWER_TOTAL = "Summary!N9";

        //Loans
        private string LOANS_PRINCIPAL_TOTAL = "Loans!F5:F33";
        private string LOANS_PRINCIPAL_PAYINGTHISTICK = "Loans!G5:G33";
        private string LOANS_PRINCIPAL_PAID = "Loans!H5:H33";

        //Payments
        private string PAYMENTS_INCOMING_TICKS = "Payments!I6:I20";
        private string PAYMENTS_OUTGOING_TICKS = "Payments!Q6:Q20";

        public string claimHex { get; set; }
        public string url { get; set; }
        public BatchGetValuesResponse? responseValues { get; set; }
        public double baseTax { get; set; }
        public double recruitManpowerBalance { get; set; }
        public double veteranManpowerBalance { get; set; }

        public List<string> GetBatchGetRanges()
        {
            List<string> ranges = new List<string>();

            ranges.Add(SUMMARY_BUDGET_PREVIOUSBALANCE);
            ranges.Add(SUMMARY_BUDGET_REVENUES);
            ranges.Add(SUMMARY_BUDGET_EXPENDITURES);
            ranges.Add(SUMMARY_REVENUES_TAX);
            ranges.Add(SUMMARY_CURRENTTICK);
            ranges.Add(SUMMARY_RECRUITMANPOWER_TOTAL);
            ranges.Add(SUMMARY_VETERANMANPOWER_TOTAL);

            ranges.Add(LOANS_PRINCIPAL_TOTAL);
            ranges.Add(LOANS_PRINCIPAL_PAYINGTHISTICK);
            ranges.Add(LOANS_PRINCIPAL_PAID);

            ranges.Add(PAYMENTS_INCOMING_TICKS);
            ranges.Add(PAYMENTS_OUTGOING_TICKS);

            return ranges;
        }

        private ValueRange createCellValueRange(string range,object value)
        {
            ValueRange valueRange = new ValueRange();
            valueRange.Range = range;
            valueRange.Values = [new List<object>()];
            valueRange.Values[0].Add(value);

            return valueRange;
        }

        public BatchUpdateValuesRequest GetBatchUpdateValuesRequest()
        {
            if (responseValues == null)
            {
                throw new NullReferenceException("BatchGetValuesResponse was not previously set");
            }

            var values = responseValues.ValueRanges;
            BatchUpdateValuesRequest request = new BatchUpdateValuesRequest();
            List<ValueRange> ranges = new List<ValueRange>();

            double oldPreviousBalance = Convert.ToDouble(values[0].Values[0][0]);
            double oldRevenues = Convert.ToDouble(values[1].Values[0][0]);
            double oldExpenditures = Convert.ToDouble(values[2].Values[0][0]);
            double oldTick = Convert.ToDouble(values[4].Values[0][0]);
            double oldRecruitManpower = Convert.ToDouble(values[5].Values[0][0]);
            double oldVeteranManpower = Convert.ToDouble(values[6].Values[0][0]);
            ValueRange principalTotals = values[7];
            ValueRange oldPrincipalPayingThisTick = values[8];
            ValueRange oldPrincipalPaid = values[9];
            ValueRange oldPaymentsIncomingTicks = values[10];
            ValueRange oldPaymentsOutgoingTicks = values[11];

            ranges.Add(createCellValueRange(SUMMARY_BUDGET_PREVIOUSBALANCE,oldPreviousBalance + oldRevenues - oldExpenditures));
            ranges.Add(createCellValueRange(SUMMARY_REVENUES_TAX, baseTax));
            ranges.Add(createCellValueRange(SUMMARY_RECRUITMANPOWER_TOTAL, oldRecruitManpower + recruitManpowerBalance * 0.05));
            ranges.Add(createCellValueRange(SUMMARY_VETERANMANPOWER_TOTAL, oldVeteranManpower + veteranManpowerBalance * 0.05));
            
            ValueRange newPrincipalsPayingThisTick = new ValueRange();
            newPrincipalsPayingThisTick.Range = LOANS_PRINCIPAL_PAYINGTHISTICK;
            newPrincipalsPayingThisTick.Values = new List<IList<object>>();
            ValueRange newPrincipalsPaid = new ValueRange();
            newPrincipalsPaid.Range = LOANS_PRINCIPAL_PAID;
            newPrincipalsPaid.Values = new List<IList<object>>();
            for (int i = 0; i < oldPrincipalPaid.Values.Count; i++)
            {
                double principalTotal = Convert.ToDouble(principalTotals.Values[i][0]);
                double principalPaid = Convert.ToDouble(oldPrincipalPaid.Values[i][0]);

                newPrincipalsPaid.Values.Add(new List<object>());
                if (principalTotal <= principalPaid)
                {
                    newPrincipalsPaid.Values[i].Add(principalPaid);
                }
                else
                {
                    double principalPayingThisTick = Convert.ToDouble(oldPrincipalPayingThisTick.Values[i][0]);
                    newPrincipalsPaid.Values[i].Add(principalPaid + principalPayingThisTick);
                }

                newPrincipalsPayingThisTick.Values.Add(new List<object>());
                newPrincipalsPayingThisTick.Values[i].Add(0);
            }
            ranges.Add(newPrincipalsPayingThisTick);
            ranges.Add(newPrincipalsPaid);

            ValueRange newPaymentsIncomingTicks = new ValueRange();
            newPaymentsIncomingTicks.Range = PAYMENTS_INCOMING_TICKS;
            newPaymentsIncomingTicks.Values = new List<IList<object>>();
            ValueRange newPaymentsOutgoingTicks = new ValueRange();
            newPaymentsOutgoingTicks.Range = PAYMENTS_OUTGOING_TICKS;
            newPaymentsOutgoingTicks.Values = new List<IList<object>>();
            for (int i = 0; i < oldPaymentsIncomingTicks.Values.Count; i++)
            {
                double incomingTick = Convert.ToDouble(oldPaymentsIncomingTicks.Values[i][0]);
                double outgoingTick = Convert.ToDouble(oldPaymentsOutgoingTicks.Values[i][0]);

                newPaymentsIncomingTicks.Values.Add(new List<object>());
                if (incomingTick > 0)
                {
                    newPaymentsIncomingTicks.Values[i].Add(incomingTick - 1);
                } else
                {
                    newPaymentsIncomingTicks.Values[i].Add(0);
                }

                newPaymentsOutgoingTicks.Values.Add(new List<object>());
                if (outgoingTick > 0)
                {
                    newPaymentsOutgoingTicks.Values[i].Add(outgoingTick - 1);
                }
                else
                {
                    newPaymentsOutgoingTicks.Values[i].Add(0);
                }
            }
            ranges.Add(newPaymentsIncomingTicks);
            ranges.Add(newPaymentsOutgoingTicks);

            ranges.Add(createCellValueRange(SUMMARY_CURRENTTICK, oldTick + 1));

            request.Data = ranges;

            return request;
        }
    }
}
