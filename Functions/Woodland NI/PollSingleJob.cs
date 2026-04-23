string fileName = "PollSingleJob";

try
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        var auth = this.ThisLib.Authenticate();

        /***************************************************************
         *
         *  Get new status events
         *
         ***************************************************************/
        string jobEventResult = string.Empty;
        bool jobEventSuccessStatusCode = false;

        string googleMapAPIKey = "";

        using (var client = new HttpClient())
        {
            var url = auth.BaseURL + "/job/jobevents/status?jobNumber=" + JobNum;

            var jobEventRequest = new HttpRequestMessage(HttpMethod.Get, url);

            try
            {
                jobEventRequest.Headers.Add("Accept", "application/json");
                jobEventRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.token);

                var jobEventResponse = client.SendAsync(jobEventRequest).Result;

                jobEventResult = jobEventResponse.Content.ReadAsStringAsync().Result;

                jobEventSuccessStatusCode = jobEventResponse.IsSuccessStatusCode;
                jobEventResponse.Dispose();
            }
            catch (Exception ex)
            {
                Ice.Diagnostics.Log.WriteEntry("Catch " + ex.Message);
            }
            finally
            {
                jobEventRequest.Dispose();
            }

            /***************************************************************
             *
             *  Loop through jobs retreived
             *
             ***************************************************************/

            if (jobEventSuccessStatusCode)
            {
                using (var doc = JsonDocument.Parse(jobEventResult))
                {

                    DateTime? collectDate = null;
                    DateTime? podDate = null;

                    decimal latitude = 0;
                    decimal longitude = 0;

                    int stage = 0;

                    string receivedBy = string.Empty;
                    string signatureUrl = string.Empty;
                    string map = string.Empty;
                    string signature = string.Empty;

                    bool collected = false;
                    bool delivered = false;

                    /***************************************************************
                     *
                     *  Get status
                     *
                     ***************************************************************/
                    var data = doc.RootElement.GetProperty("data");

                    var job = data.GetProperty("job");
                    var jobNumber = job.GetProperty("jobNumber").GetString();
                    var events = data.GetProperty("events").EnumerateArray();
                    foreach (var evnt in events)
                    {
                        var statusCode = evnt.GetProperty("statusCode").GetString();
                        var shipment = Db.UD101.Where(r => r.Company == Session.CompanyID).Where(r => r.ShortChar05 == jobNumber).FirstOrDefault();
                        if (shipment == null) continue;

                        if (statusCode == "X_PIC")
                        {
                            collected = true;

                            var collectionDate = evnt.GetProperty("eventTimestamp").GetString();
                            collectDate = DateTime.TryParse(collectionDate, out
                              var dt) ? dt : (DateTime?)null;
                        }

                        if (statusCode == "X_DEL")
                        {
                            delivered = true;

                            /***************************************************************
                             *
                             *  Get POD
                             *
                             ***************************************************************/
                            var podRequest = new HttpRequestMessage(HttpMethod.Get, auth.BaseURL + "/job/jobevents/pod?jobNumber=" + jobNumber);
                            try
                            {
                                podRequest.Headers.Add("Accept", "application/json");
                                podRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.token);

                                var podResponse = client.SendAsync(podRequest).Result;

                                var podResult = podResponse.Content.ReadAsStringAsync().Result;

                                using (var podDoc = JsonDocument.Parse(podResult))
                                {
                                    var podList = podDoc.RootElement.GetProperty("data").EnumerateArray();

                                    foreach (var pod in podList)
                                    {
                                        var _podDate = pod.GetProperty("podDate").GetString();
                                        _podDate = _podDate.SubString(0, 11);
                                        var _podTime = pod.GetProperty("podTime").GetString();

                                        DateTime _podDateTime;
                                        if (DateTime.TryParse($"{_podDate} {_podTime}", out _podDateTime)) ;
                                        {
                                            podDate = DateTime.Parse($"{_podDate} {_podTime}");
                                        }

                                        receivedBy = pod.GetProperty("podDetails3").GetString();
                                        signatureUrl = pod.GetProperty("signOnGlassUrl").GetString();
                                    }
                                }

                                podResponse.Dispose();
                            }
                            finally
                            {
                                podRequest.Dispose();
                            }

                            var locRequest = new HttpRequestMessage(HttpMethod.Get, auth.BaseURL + "/job/" + jobNumber + "/stages?return=STATUSDATES");
                            try
                            {
                                locRequest.Headers.Add("Accept", "application/json");
                                locRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.token);

                                var locResponse = client.SendAsync(locRequest).Result;

                                var locResult = locResponse.Content.ReadAsStringAsync().Result;

                                using (var locDoc = JsonDocument.Parse(locResult))
                                {
                                    var locData = locDoc.RootElement.GetProperty("data");

                                    if (locData.GetArrayLength() > 0)
                                    {
                                        var loc = locData[0];

                                        stage = loc.GetProperty("stage").GetInt16();

                                        var statusDates = loc.GetProperty("statusDates");
                                        var statusExtraDates = statusDates.GetProperty("statusExtraDates");

                                        foreach (var status in statusExtraDates.EnumerateArray())
                                        {
                                            if (status.GetProperty("statusCode").GetString() == "DEL")
                                            {
                                                latitude = status.GetProperty("statusLat").GetDecimal();
                                                longitude = status.GetProperty("statusLon").GetDecimal();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                locRequest.Dispose();
                            }

                            if (!string.IsNullOrEmpty(auth.MapKey) && latitude > 0)
                            {
                                var mapRequest = new HttpRequestMessage(HttpMethod.Get, $"https://maps.googleapis.com/maps/api/staticmap?center={latitude},{longitude}&markers=color:red%7Clabel%7C{latitude},{longitude}&zoom=16&size=600x300&key={auth.MapKey}");
                                try
                                {
                                    var mapResponse = client.SendAsync(mapRequest).Result;
                                    var mapBytes = mapResponse.Content.ReadAsByteArrayAsync().Result;

                                    map = Convert.ToBase64String(mapBytes);
                                }
                                catch
                                {

                                    continue;
                                }
                                finally
                                {
                                    mapRequest.Dispose();
                                }
                            }

                            /***************************************************************
                             *
                             *  Get Signature
                             *
                             ***************************************************************/
                            //stage is retreived from the call above
                            if (!string.IsNullOrEmpty(signatureUrl))
                            {
                                var signRequest = new HttpRequestMessage(HttpMethod.Get, $"{auth.BaseURL}{signatureUrl}");
                                try
                                {
                                    signRequest.Headers.Add("Accept", "application/json");
                                    signRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.token);

                                    var signResponse = client.SendAsync(signRequest).Result;

                                    var signBytes = signResponse.Content.ReadAsByteArrayAsync().Result;

                                    signature = Convert.ToBase64String(signBytes);
                                }
                                finally
                                {
                                    signRequest.Dispose();

                                }
                            }
                        }

                        /***************************************************************
                         *
                         *  Update shipment in Epicor
                         *
                         ***************************************************************/

                        this.CallService<Ice.Contracts.UD101SvcContract>(ud101Svc =>
                        {
                            try
                            {
                                Ice.Tablesets.UD101Tableset ud101Ts = ud101Svc.GetByID(shipment.Key1, "", "", "", "");

                                ud101Ts.UD101[0].ShortChar01 = statusCode;
                                if (collected)
                                {
                                    ud101Ts.UD101[0].Date03 = collectDate;
                                    if (collectDate != null) ud101Ts.UD101[0].ShortChar07 = ((DateTime)collectDate).ToString("dd/MM/yyyy hh:mm");
                                    ud101Ts.UD101[0].CheckBox02 = collected;
                                }
                                if (delivered)
                                {
                                    ud101Ts.UD101[0].Date04 = podDate;
                                    if (podDate != null) ud101Ts.UD101[0].ShortChar08 = ((DateTime)podDate).ToString("dd/MM/yyyy hh:mm");
                                    ud101Ts.UD101[0].CheckBox03 = delivered;
                                    ud101Ts.UD101[0].Number03 = latitude;
                                    ud101Ts.UD101[0].Number04 = longitude;
                                    ud101Ts.UD101[0].Character02 = map;
                                    ud101Ts.UD101[0].ShortChar06 = receivedBy;
                                    ud101Ts.UD101[0].Character01 = signature;
                                }
                                ud101Ts.UD101[0].RowMod = "U";

                                ud101Svc.Update(ref ud101Ts);

                                if (collected)
                                {
                                    ThisLib.Collected((int)shipment.Number01, true);
                                }

                                if (delivered)
                                {
                                    ThisLib.Delivered((int)shipment.Number01, true);
                                }

                            }
                            catch (Exception ex)
                            {
                                Ice.Diagnostics.Log.WriteEntry("Error updating master pack : " + ex.ToString());
                            }
                        });
                    }
                }
            }
        }
    }
}
catch (Exception ex)
{
    Ice.Diagnostics.Log.WriteEntry("Poll Jobs : " + ex.Message);
    Ice.Diagnostics.Log.WriteEntry("Poll Jobs : " + ex.StackTrace);
}
finally
{
    //ThisLib.UpdateLastPoll();
}