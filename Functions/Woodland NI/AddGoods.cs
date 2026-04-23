/* Add goods to Mandata */

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{

    string id = "";

    try
    {

        var auth = this.ThisLib.Authenticate();


        using (var client = new HttpClient())
        {
            var url = auth.BaseURL + "/job/" + JobNum + "/goods";

            var goodsRequest = new HttpRequestMessage(HttpMethod.Post, url);

            try
            {
                goodsRequest.Headers.Add("Accept", "application/json");
                goodsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.token);

                var Description = PalletSize > 1 ? $"{PalletNum} Long ({PalletSize.ToString()}z)" : $"{PalletNum}";

                var data = new
                {
                    data = new
                    {
                        line = 1,
                        description = Description,
                        palletType = PalletNum,
                        stackFactor = 1,
                        description2 = PalletNum
                    }
                };

                var request = JsonConvert.SerializeObject(data);

                var content = new StringContent(request, null, "application/json");
                goodsRequest.Content = content;

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
    catch (Exception ex)
    {
        Ice.Diagnostics.Log.WriteEntry("Add Goods : " + ex.Message);
    }
}