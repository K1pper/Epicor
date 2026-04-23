IsSuccess = false;
ErrorMessage = "Success";
JobNumber = string.Empty;
Barcodes = string.Empty;

if (CollectionRequested)
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        try
        {
            var auth = this.ThisLib.Authenticate();

            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, auth.BaseURL + "/job?return=BARCODES,PALLETS"))
                {
                    request.Headers.Add("Accept", "application/json");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.token);

                    var collectionRequest = this.ThisLib.GetCollectionRequest(PackNum, auth.Account, CollectionDate, DeliveryDate);

                    if (collectionRequest.IsSuccess == false)
                    {
                        ErrorMessage = collectionRequest.ErrorMessage;
                    }
                    else
                    {
                        var content = new StringContent(collectionRequest.Request, null, "application/json");
                        request.Content = content;

                        var response = client.SendAsync(request).Result;

                        var result = response.Content.ReadAsStringAsync().Result;
                        if (response.IsSuccessStatusCode)
                        {
                            using (var doc = JsonDocument.Parse(result))
                            {
                                var dataElement = doc.RootElement.GetProperty("data");

                                JobNumber = dataElement.GetProperty("jobNumber").GetInt32().ToString();

                                // barcodes
                                if (dataElement.TryGetProperty("barcodes", out
                                    var barcodesElement))
                                {
                                    var barcodeList = dataElement.GetProperty("barcodes")
                                      .EnumerateArray()
                                      .Select(b => b.GetProperty("barcode").GetString());

                                    Barcodes = string.Join(", ", barcodeList);
                                }
                            }
                            IsSuccess = true;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Ice.Diagnostics.Log.WriteEntry("Ship Collection Exception: " + ex.ToString());
            ErrorMessage = ex.ToString();

            StringBuilder debugMessage = new StringBuilder();
            debugMessage.AppendLine("GHA:Ship Collection");
            debugMessage.AppendLine(ErrorMessage);
            Ice.Diagnostics.Log.WriteEntry(debugMessage.ToString());
        }
    }
}
else
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        try
        {
            var pack = Db.UD101.Where(r => r.Company == Session.CompanyID).Where(r => r.Number01 == PackNum).FirstOrDefault();

            if (pack == null)
            {
                IsSuccess = false;
                ErrorMessage = "Pack not found";
            }
            else
            {
                var auth = this.ThisLib.Authenticate();

                using (var client = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("PATCH"), auth.BaseURL + "/job/" + pack.ShortChar05))
                    {
                        request.Headers.Add("Accept", "application/json");
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.token);

                        var content = new StringContent("{\n  \"data\": {\n    \"account\": \"" + Session.CompanyID + "\",\n    \"jobCancelled\": true\n  }\n}", null, "application/json");

                        request.Content = content;

                        var response = client.SendAsync(request).Result;

                        var result = response.Content.ReadAsStringAsync().Result;

                        IsSuccess = true;
                        ErrorMessage = "Cancelled";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Ice.Diagnostics.Log.WriteEntry("Ship Collection Exception: " + ex.ToString());
            ErrorMessage = ex.ToString();

            StringBuilder debugMessage = new StringBuilder();
            debugMessage.AppendLine("GHA:Ship Collection");
            debugMessage.AppendLine(ErrorMessage);
            Ice.Diagnostics.Log.WriteEntry(debugMessage.ToString());
        }
    }
}