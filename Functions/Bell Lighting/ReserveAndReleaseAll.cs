var fn = "ReserveAndRelease";

Result = "";

ThisLib.Logger("=== Reserve and Release Jobs from dataset ===", fn);

try
{
    if (input.Tables.Count > 0)
    {
        foreach (DataRow r in input.Tables[0].Rows)
        {
            if ((bool)r["Calculated_Select"] == false) continue;

            string jobNum = r["JobProd_JobNum"].ToString();

            var result = ThisLib.ReserveAndRelease(jobNum);

            Result += result.Message;
        }
    }
}
catch (Exception ex)
{
    ThisLib.Logger($"Reserve and release {ex.ToString()}", fn);
    Result = ex.ToString();
}
