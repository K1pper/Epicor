List<Erp.Tablesets.OrderRelRow> OrderRels = ttOrderRel.FindAll(row => row.Updated() || row.RowMod == "A").ToList();

Erp.Tables.Company  Company_xRow;
Company_xRow = (from Company_Row in Db.Company
              where Company_Row.Company1 == Session.CompanyID
              select Company_Row).FirstOrDefault();

string endPoint = Company_xRow.AWVA_AddressValidationEndpoint_c + "/UPS?RunByBPM=true";
bool AWValidateAddressIsEnabled = Company_xRow.AWVA_EnableBPMs_c;

if(AWValidateAddressIsEnabled)
{
  foreach(Erp.Tablesets.OrderRelRow OrderRel in OrderRels)
  {
    if(OrderRel?.UseOTS == null)
      return;
         
    bool AddressRequiresValidation = (!(bool)OrderRel["AWVA_ExcludeOTSAddressFromValidation_c"] && OrderRel.UseOTS);
    if(!AddressRequiresValidation)
      return; // Exit the program if validation is not required for the address
      
    string AddressDescription = "OTS Address for order line " + OrderRel.OrderLine.ToString() + ", rel " + OrderRel.OrderRelNum.ToString();
    
    bool DebuggingEnabled = false;
    bool AddressIsValid = false;
    string rawResponse = String.Empty;
    
    string CompanyName = (OrderRel.OTSName != null? OrderRel.OTSName.ToUpper() : String.Empty);
    string Address1 = (OrderRel.OTSAddress1 != null? OrderRel.OTSAddress1.ToUpper() : String.Empty);
    string Address2 = (OrderRel.OTSAddress2 != null? OrderRel.OTSAddress2.ToUpper() : String.Empty);
    string Address3 = (OrderRel.OTSAddress3 != null? OrderRel.OTSAddress3.ToUpper() : String.Empty);
    string City = (OrderRel.OTSCity != null? OrderRel.OTSCity.ToUpper() : String.Empty);
    string State = (OrderRel.OTSState != null? OrderRel.OTSState.ToUpper() : String.Empty);
    string Zip = (OrderRel.OTSZIP != null? OrderRel.OTSZIP.ToUpper() : String.Empty);
    string CountryCode = String.Empty;
    int? CountryNum = OrderRel?.OTSCountryNum;
    
    string InputSummary = "Address Values: " + Environment.NewLine + 
                          "CompanyName: " + CompanyName + Environment.NewLine +
                          "Address1: " + Address1 + Environment.NewLine + 
                          "Address2: " + Address2 + Environment.NewLine + 
                          "Address3: " + Address3 + Environment.NewLine + 
                          "City: " + City + Environment.NewLine + 
                          "State: " + State + Environment.NewLine + 
                          "Zip: " + Zip + Environment.NewLine + 
                          "CountryNum: " + CountryNum + Environment.NewLine;
                        
    
    if(CountryNum != 0 && CountryNum != null)
    {
      using(var svcCountry = Ice.Assemblies.ServiceRenderer.GetService<Erp.Contracts.CountrySvcContract>(Db))
      {  
          Erp.Tablesets.CountryTableset dsCountry = svcCountry.GetByID((int)CountryNum);
          CountryCode = dsCountry.Country[0].ISOCode.ToUpper();
      }
    }
    
    bool USOrCAAddressHasBeenEntered = !String.IsNullOrEmpty(Address1) &&
                                 !String.IsNullOrEmpty(City) &&
                                 !String.IsNullOrEmpty(State) &&
                                 !String.IsNullOrEmpty(Zip) &&
                                 (CountryCode == "US" || CountryCode == "CA");
          
    if(USOrCAAddressHasBeenEntered)
    {
      string newLine = System.Environment.NewLine;
    
      StringBuilder wsResponse = new StringBuilder(); 
      string bodyContent = "CompanyName=" + CompanyName + "&" + 
                         "AddressLine1=" + Address1 + "&" + 
                         "AddressLine2=" + Address2 + "&" + 
                         "AddressLine3=" + Address3 + "&" + 
                         "City=" + City + "&" + 
                         "StateOrProvince=" + State + "&" + 
                         "ZipOrPostalCode=" + Zip + "&" + 
                         "CountryCode=" + CountryCode;
      
      try
      {
        var bodyData = Encoding.Default.GetBytes(bodyContent);
        var request =  (HttpWebRequest)WebRequest.Create(endPoint);
        request.Method = "POST";
        request.ContentType = "application/x-www-form-urlencoded";      
        request.ContentLength = bodyData.Length;
        using(var requestStreamWriter = request.GetRequestStream())
        {
          requestStreamWriter.Write(bodyData, 0, bodyData.Length);
        }
  
        HttpWebResponse response = (HttpWebResponse)(request.GetResponse());
        rawResponse = ((StreamReader)(new StreamReader((Stream)(response.GetResponseStream())))).ReadToEnd();
        
        string ErrorMessage =  rawResponse.Split('|').ToList().First();
        AddressIsValid = (rawResponse.Split('|').ToList()[3] == "Y");
        
        if(!AddressIsValid && !String.IsNullOrEmpty(rawResponse.Split('|').ToList()[5]))
        {
          // Executes if there is a suggested address and UPS says address is invalid
          
          dynamic SuggestedAddress = JsonConvert.DeserializeObject<dynamic>(rawResponse.Split('|').ToList()[5]);
          List<string> AddressLines = (((JArray)SuggestedAddress["AddressLine"]).ToObject<string[]>()).ToList();   
          
          /* bool AddressLinesMatch =  (AddressLines.First().ToUpper() == Address1 &&
                                    (AddressLines.Count > 1 ? AddressLines[1].ToUpper() == Address2 : true) &&
                                    (AddressLines.Count > 2 ? AddressLines[2].ToUpper() == Address3 : true));
            
          bool InputAddressMatchesSuggestedAddress = AddressLinesMatch &&
                                                     City == SuggestedAddress["PoliticalDivision2"].ToString().ToUpper() &&                                                     
                                                     State == SuggestedAddress["PoliticalDivision1"].ToString().ToUpper() &&                                                     
                                                     Zip == SuggestedAddress["PostcodePrimaryLow"].ToString().ToUpper() &&                                                 
                                                     CountryCode == SuggestedAddress["CountryCode"].ToString().ToUpper();
          
          if(InputAddressMatchesSuggestedAddress)
            AddressIsValid = true;
         */ 
        
         if(DebuggingEnabled)
            this.PublishInfoMessage("Endpoint: " + endPoint + Environment.NewLine + Environment.NewLine +  
                                   "Response: " + rawResponse,
                                   Ice.Common.BusinessObjectMessageType.Information, Ice.Bpm.InfoMessageDisplayMode.Individual,       
                                   "Debug Message",               
                                   "AWValidateAddressBPM");     
                                                     
        }
        
        if(!String.IsNullOrEmpty(ErrorMessage))
        {
          Ice.Diagnostics.Log.WriteEntry("AWValidateAddress Issue communicating with service" + Environment.NewLine + Environment.NewLine +
                                 "Address Type: " + AddressDescription + Environment.NewLine +
                                 "Time Stamp: " + DateTime.Now.ToString() + Environment.NewLine +
                                 "Web Service Response: " + rawResponse + Environment.NewLine +
                                 InputSummary);
          throw new Ice.Common.BusinessObjectException(new Ice.Common.BusinessObjectMessage("AWValidateAddress " + Environment.NewLine +
                                                                                    ErrorMessage) {Type= Ice.Common.BusinessObjectMessageType.Information});
        }
        
      }
      catch (Exception e)
      {
         throw new Ice.Common.BusinessObjectException(new Ice.Common.BusinessObjectMessage("AWValidateAddress " + Environment.NewLine + 
                                                                                          "Error communicating with address validation service: " + e.Message) {Type= Ice.Common.BusinessObjectMessageType.Information});
      }
    
      
      if(!AddressIsValid)
      {
         Ice.Diagnostics.Log.WriteEntry("AWValidateAddress Invalid Address" + Environment.NewLine + Environment.NewLine + 
                                       "Address Type: " + AddressDescription + Environment.NewLine +
                                       "Time Stamp: " + DateTime.Now.ToString() + Environment.NewLine +
                                       "Web Service Response: " + rawResponse + Environment.NewLine +
                                       InputSummary);
        
         throw new Ice.Common.BusinessObjectException(new Ice.Common.BusinessObjectMessage("AWValidateAddress " + Environment.NewLine +
                                                                                          AddressDescription  + " is invalid.") {Type= Ice.Common.BusinessObjectMessageType.Information});
      }  
    }
  }
}