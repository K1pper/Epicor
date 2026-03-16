try
{
    var settings = this.ThisLib.GetSettings();
    BaseURL = settings.BaseURL;

    using (var client = new HttpClient())
    {
        using (var request = new HttpRequestMessage(HttpMethod.Post, settings.BaseURL + "/auth"))
        {
            request.Headers.Add("Accept", "application/json");

            var data = new
            {
                userName = settings.UserName,
                password = settings.Password,
                companyCode = settings.Company,
                depot = settings.Depot
            };

            var payload = JsonConvert.SerializeObject(data);
            var content = new StringContent(payload, null, "application/json");
            request.Content = content;

            var response = client.SendAsync(request).Result;

            var result = response.Content.ReadAsStringAsync().Result;

            Ice.Diagnostics.Log.WriteEntry(result);

            if (response.IsSuccessStatusCode)
            {

                dynamic d = JsonConvert.DeserializeObject(result);
                token = d.token;

                Ice.Diagnostics.Log.WriteEntry(token);
            }
        }
    }
}
catch (Exception ex)
{
    Ice.Diagnostics.Log.WriteEntry("Ship Authentication Exception: " + ex.Message);
}