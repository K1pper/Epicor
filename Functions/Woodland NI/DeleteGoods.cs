/* Remove goods from Mandata */

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{

    string id = "";

    try
    {

        var auth = this.ThisLib.Authenticate();

        using (var client = new HttpClient())
        {
            var url = auth.BaseURL + "/job/" + JobNumber + "/goods";

            var goodsRequest = new HttpRequestMessage(HttpMethod.Get, url);

            try
            {
                goodsRequest.Headers.Add("Accept", "application/json");
                goodsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.token);

                var goodsResponse = client.SendAsync(goodsRequest).Result;

                var goodsResult = goodsResponse.Content.ReadAsStringAsync().Result;

                using (var goodsDoc = JsonDocument.Parse(goodsResult))
                {
                    var goodsList = goodsDoc.RootElement.GetProperty("data").EnumerateArray();

                    foreach (var g in goodsList)
                    {
                        var _palletNum = g.GetProperty("palletType").GetString();

                        if (_palletNum == PalletNum)
                        {
                            id = g.GetProperty("id").ToString();
                        }
                    }
                }

                goodsResponse.Dispose();
            }
            catch (Exception ex)
            {
                Ice.Diagnostics.Log.WriteEntry("Catch " + ex.Message);
            }
            finally
            {
                goodsRequest.Dispose();
            }
        }

        if (!string.IsNullOrEmpty(id))
        {
            using (var client = new HttpClient())
            {
                var url = auth.BaseURL + "/job/" + JobNumber + "/goods/" + id;

                var goodsRequest = new HttpRequestMessage(HttpMethod.Delete, url);

                try
                {
                    goodsRequest.Headers.Add("Accept", "application/json");
                    goodsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.token);

                    var goodsResponse = client.SendAsync(goodsRequest).Result;

                    goodsResponse.Dispose();
                }
                catch (Exception ex)
                {
                    Ice.Diagnostics.Log.WriteEntry("Catch " + ex.Message);
                }
                finally
                {
                    goodsRequest.Dispose();
                }
            }
        }

    }
    catch (Exception ex)
    {
        Ice.Diagnostics.Log.WriteEntry("Delete Goods : " + ex.Message);
    }
}