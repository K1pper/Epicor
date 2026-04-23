if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{

    bool isOk = true;



    var userCodes = Db.UDCodes.Where(r => r.Company == Session.CompanyID).Where(r => r.CodeTypeID == "MANDATA").ToList();
    if (userCodes == null) isOk = false;
    var printIp = userCodes.Where(r => r.CodeID == "PrintIp").FirstOrDefault();
    if (printIp == null) isOk = false;
    var printPort = userCodes.Where(r => r.CodeID == "PrintPort").FirstOrDefault();
    if (printPort == null) isOk = false;
    int printerPort = 0;
    var canParsePrinterPort = Int32.TryParse(printPort.CodeDesc, out printerPort);

    if (!canParsePrinterPort || printerPort <= 0) isOk = false;

    var response = ThisLib.GetZPL(PackNum);

    var zpl = response.zplResponse;
    var ErrorMessage = response.ErrorMessage;

    if (!string.IsNullOrEmpty(ErrorMessage)) isOk = false;

    if (isOk)
    {
        try
        {
            using (TcpClient client = new TcpClient())
            {
                client.Connect(printIp.CodeDesc, printerPort);

                using (NetworkStream stream = client.GetStream())
                {
                    byte[] data = Encoding.ASCII.GetBytes(zpl);
                    stream.Write(data, 0, data.Length);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}