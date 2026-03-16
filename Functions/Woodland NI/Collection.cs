IsSuccess = false;
ErrorMessage = "Success";

try
{
    var auth = this.ThisLib.Authenticate();

    using (var client = new HttpClient())
    {
        using (var request = new HttpRequestMessage(HttpMethod.Post, auth.BaseURL + "/job?return=BARCODES,PALLETS"))
        {
            request.Headers.Add("Accept", "application/json");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.token);


            var payload = this.ThisLib.GetCollectionRequest(PackNum);

            var content = new StringContent(payload, null, "application/json");
            request.Content = content;

            var response = client.SendAsync(request).Result;

            var result = response.Content.ReadAsStringAsync().Result;

            Ice.Diagnostics.Log.WriteEntry("Result");
            Ice.Diagnostics.Log.WriteEntry(result);

            if (response.IsSuccessStatusCode)
            {
                using (var doc = JsonDocument.Parse(result))
                {
                    var dataElement = doc.RootElement.GetProperty("data");

                    jobNumber = dataElement.GetProperty("jobNumber").GetInt32().ToString();

                    // barcodes
                    var barcodeList = dataElement.GetProperty("barcodes")
                           .EnumerateArray()
                           .Select(b => b.GetProperty("barcode").GetString());

                    barcodes = string.Join(", ", barcodeList);
                }

            }
        }
    }
}
catch (Exception ex)
{
    Ice.Diagnostics.Log.WriteEntry("Ship Collection Exception: " + ex.Message);
    ErrorMessage = ex.Message;

    StringBuilder debugMessage = new StringBuilder();
    debugMessage.AppendLine("GHA:Ship Collection");
    debugMessage.AppendLine(ErrorMessage);
    Ice.Diagnostics.Log.WriteEntry(debugMessage.ToString());
}
