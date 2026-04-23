if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{

    this.CallService<Ice.Contracts.UserCodesSvcContract>(userCodesSvc =>
    {
        try
        {
            var userCodesTs = new Ice.Tablesets.UserCodesTableset();
            userCodesTs = userCodesSvc.GetByID("MANDATA");

            var test = userCodesTs.UDCodes.Where(r => r.Company == Session.CompanyID).Where(r => r.CodeID == "Test").FirstOrDefault();
            if (test == null)
            {
                //Ice.Diagnostics.Log.WriteEntry("test is null");
            }
            else
            {
                var codeId = test.CodeDesc.ToLower() == "true" ? "TLastPoll" : "LastPoll";
                var lastPoll = userCodesTs.UDCodes.Where(r => r.Company == Session.CompanyID).Where(r => r.CodeID == codeId).FirstOrDefault();
                if (lastPoll == null)
                {
                    //Ice.Diagnostics.Log.WriteEntry("last poll is null");
                }
                else
                {

                    lastPoll.CodeDesc = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                    lastPoll.RowMod = "U";


                    userCodesSvc.Update(ref userCodesTs);
                }
            }



        }
        catch (Exception ex)
        {
            Ice.Diagnostics.Log.WriteEntry($"Error inUpdate Last Poll function (Create master pack): {ex}");
            return;
        }
    });

}