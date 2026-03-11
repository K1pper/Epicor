            const string UDParamsID = "PartParams";
            const string QuickPartEntryProc = "ghaQuickPartEntry";
            const string BPMData_ProcessKey = "ShortChar01";
            const string BPMData_PartNum = "ShortChar02";
            const string BPMData_PartDescription = "Character01";
            const string BPMData_TypeCode = "ShortChar03";
            const string BPMData_UOMClassID = "ShortChar04";
            const string BPMData_IUM = "ShortChar05";
            const string BPMData_SalesUM = "ShortChar06";
            const string BPMData_PUM = "ShortChar07";
            const string BPMData_ClassId = "ShortChar08";
            const string BPMData_ProdCode = "ShortChar09";
            const string BPMData_SearchWord = "ShortChar10";
            const string BPMData_VendorID = "Character02";
            const string BPMData_VendPartNum = "Character03";
            const string BPMData_Cost = "Number01";
            const string BPMData_PrimWhse = "Character04";
            const string BPMData_PrimBin = "Character05";
            const string BPMData_MinimumQty = "Number02";
            const string BPMData_MaximumQty = "Number03";
            const string BPMData_UrgentMultQty = "Number04";
            const string BPMData_LeadTime = "Number05";
            const string BPMData_PlanTimeFence = "Number06";
            const string BPMData_DaysOfSupply = "Number07";
            const string BPMData_ReschedInDelta = "Number08";
            const string BPMData_ReschedOutDelta = "Number09";
            const string BPMData_BaseUnitPrice = "Number10";
            const string BPMData_SecondCurrencyUnitPrice = "Number11";
            const string BPMData_SuppCurrency = "Character06";
            const string BPMData_CommodityCode = "Character07";
           

            //var callContextBpmData = context.BpmData.FirstOrDefault();


            // Execute only if the process is triggered from the UI (by using the a given Process Key).
            if (!callContextBpmData[BPMData_ProcessKey].ToString().Equals(QuickPartEntryProc, StringComparison.OrdinalIgnoreCase))
                return;
            

            using (var scope = IceContext.CreateDefaultTransactionScope())
            {

                // Validate Required/Valid values
                if (string.IsNullOrWhiteSpace(callContextBpmData[BPMData_PartNum].ToString()))
                    throw new BLException(string.Format("Part Number is required."));
                    
                var part = Erp.Tables.Part.FindFirstByPrimaryKey(Db, Session.CompanyID, callContextBpmData[BPMData_PartNum].ToString());
                if (part != null)
                    throw new BLException(string.Format("Part Number {0} already exists.", callContextBpmData[BPMData_PartNum].ToString()));

                var validTypeCodes = new List<string> { "M", "P", "K" };
                if (!validTypeCodes.Any(r => r == callContextBpmData[BPMData_TypeCode].ToString().ToUpper()))
                    throw new BLException(string.Format("Part Type Code {0} is invalid.", callContextBpmData[BPMData_TypeCode].ToString()));


                if (string.IsNullOrWhiteSpace(callContextBpmData[BPMData_SearchWord].ToString()))
                    throw new BLException("Search Word is required.");

                var vendorID = callContextBpmData[BPMData_VendorID].ToString();
                var vendor = Db.Vendor.FirstOrDefault(r => r.Company == Session.CompanyID && r.VendorID == vendorID);
                if (vendor == null)
                    throw new BLException(string.Format("Vendor ID {0} is invalid", callContextBpmData[BPMData_VendorID].ToString()));

                //if (string.IsNullOrWhiteSpace(callContextBpmData[BPMData_VendPartNum].ToString()))
                  //  throw new BLException("Supplier Part Number is required.");

                if ((decimal)callContextBpmData[BPMData_Cost] <= 0m)
                    throw new BLException("Cost is required.");

                var warehse = Erp.Tables.Warehse.FindFirstByPrimaryKey(Db, Session.CompanyID, callContextBpmData[BPMData_PrimWhse].ToString());
                if (warehse == null)
                    throw new BLException(string.Format("Warehouse {0} is invalid", callContextBpmData[BPMData_PrimWhse].ToString()));

                var whseBin = Erp.Tables.WhseBin.FindFirstByPrimaryKey(Db, Session.CompanyID, callContextBpmData[BPMData_PrimWhse].ToString(), callContextBpmData[BPMData_PrimBin].ToString());
                if (whseBin == null)
                    throw new BLException(string.Format("Bin {0} is invalid for Warehouse {1}", callContextBpmData[BPMData_PrimBin].ToString(), callContextBpmData[BPMData_PrimWhse].ToString()));



                if ((decimal)callContextBpmData[BPMData_BaseUnitPrice] <= 0m)
                    throw new BLException("Sales Unit Price is required.");


                var curr = Currency.FindFirstByPrimaryKey(Db, Session.CompanyID, "EUR");

                bool isEURBaseCurrency = curr != null && curr.BaseCurr;

                if (!isEURBaseCurrency && (decimal)callContextBpmData[BPMData_SecondCurrencyUnitPrice] <= 0m)
                    throw new BLException("Sales Unit Price in EURO is required.");


                //Create the Part 
                var dsPart = new PartTableset();
                using (var svc = Ice.Assemblies.ServiceRenderer.GetService<Erp.Contracts.PartSvcContract>(Db))
                    svc.GetNewPart(ref dsPart);

                // Part Number
                var ttPart = dsPart.Part.FirstOrDefault(r => r.Added());
                using (var svc = Ice.Assemblies.ServiceRenderer.GetService<Erp.Contracts.PartSvcContract>(Db))
                    svc.ChangePartNum(callContextBpmData[BPMData_PartNum].ToString(), ref dsPart);

                // Part Description
                ttPart.PartDescription = callContextBpmData[BPMData_PartDescription].ToString();
                ttPart.SearchWord = callContextBpmData[BPMData_SearchWord].ToString();

                // Type Code
                if (!ttPart.TypeCode.Equals(callContextBpmData[BPMData_TypeCode].ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    using (var svc = Ice.Assemblies.ServiceRenderer.GetService<Erp.Contracts.PartSvcContract>(Db))
                        svc.ChangePartTypeCode(callContextBpmData[BPMData_TypeCode].ToString(), ref dsPart);
                }
                
                // Commodity Code  ICommCode.CommodityCode
                var commodityCode = callContextBpmData[BPMData_CommodityCode].ToString();
                if(!string.IsNullOrEmpty(commodityCode))
                {
                  var iCommCode = Erp.Tables.ICommCode.FindFirstByPrimaryKey(Db, Session.CompanyID, commodityCode);
                  if(iCommCode == null)
                    throw new BLException(string.Format("Commodity Code {0} is not valid.", commodityCode));
                    
                  ttPart.CommodityCode = commodityCode;
                }

                // UOMs
                ttPart.UOMClassID = callContextBpmData[BPMData_UOMClassID].ToString();
                ttPart.IUM = callContextBpmData[BPMData_IUM].ToString();
                ttPart.SalesUM = callContextBpmData[BPMData_SalesUM].ToString();
                ttPart.PUM = callContextBpmData[BPMData_PUM].ToString();

                // Class ID
                ttPart.ClassID = callContextBpmData[BPMData_ClassId].ToString();

                // Product Group
                if (!ttPart.ProdCode.Equals(callContextBpmData[BPMData_ProdCode].ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    using (var svc = Ice.Assemblies.ServiceRenderer.GetService<Erp.Contracts.PartSvcContract>(Db))
                        svc.ChangePartProdCode(callContextBpmData[BPMData_ProdCode].ToString(), ref dsPart);
                }


                // Save the Part (this will result in a PartPlant record created for the Part.
                using (var svc = Ice.Assemblies.ServiceRenderer.GetService<Erp.Contracts.PartSvcContract>(Db))
                    svc.Update(ref dsPart);


                // Workout Part Plant Data
                var bittPartPlant = dsPart.PartPlant.FirstOrDefault();
                var ttPartPlant = new PartPlantRow();
                BufferCopy.Copy(bittPartPlant, ttPartPlant);
                ttPartPlant.RowMod = IceRow.ROWSTATE_UPDATED;
                dsPart.PartPlant.Add(ttPartPlant);

                using (var svc = Ice.Assemblies.ServiceRenderer.GetService<Erp.Contracts.PartSvcContract>(Db))
                    svc.ChangePartPlantVendorID(callContextBpmData[BPMData_VendorID].ToString(), ref dsPart);


                ttPartPlant.PrimWhse = callContextBpmData[BPMData_PrimWhse].ToString();
                ttPartPlant.MinimumQty = (decimal)callContextBpmData[BPMData_MinimumQty];
                ttPartPlant.MaximumQty = (decimal)callContextBpmData[BPMData_MaximumQty];
                ttPartPlant.MfgLotMultiple = (decimal)callContextBpmData[BPMData_UrgentMultQty];
                ttPartPlant.LeadTime = Convert.ToInt32(callContextBpmData[BPMData_LeadTime]);
                ttPartPlant.PlanTimeFence = Convert.ToInt32(callContextBpmData[BPMData_PlanTimeFence]);
                ttPartPlant.DaysOfSupply = Convert.ToInt32(callContextBpmData[BPMData_DaysOfSupply]);
                ttPartPlant.ReschedInDelta = Convert.ToInt32(callContextBpmData[BPMData_ReschedInDelta]);
                ttPartPlant.ReschedOutDelta = Convert.ToInt32(callContextBpmData[BPMData_ReschedOutDelta]);


                // Read Default Buyer ID.
                var udCode = Ice.Tables.UDCodes.FindFirstByPrimaryKey(Db, Session.CompanyID, UDParamsID, "BuyerID");
                if (udCode != null && udCode.IsActive)
                    ttPartPlant.BuyerID = udCode.CodeDesc;



                // Save the PartPlant changes.
                using (var svc = Ice.Assemblies.ServiceRenderer.GetService<Erp.Contracts.PartSvcContract>(Db))
                    svc.Update(ref dsPart);

                // Update the PartWhse.PrimBinNum with callContextBpmData[BPMData_PrimBinNum].ToString()
                var bittPartWhse = dsPart.PartWhse.FirstOrDefault(r => r.WarehouseCode.Equals(callContextBpmData[BPMData_PrimWhse].ToString(), StringComparison.OrdinalIgnoreCase));
                var ttPartWhse = new PartWhseRow();
                BufferCopy.Copy(bittPartWhse, ttPartWhse);
                ttPartWhse.RowMod = IceRow.ROWSTATE_UPDATED;
                dsPart.PartWhse.Add(ttPartWhse);

                ttPartWhse.PrimBinNum = callContextBpmData[BPMData_PrimBin].ToString();

                // Save the PartWhse changes.
                using (var svc = Ice.Assemblies.ServiceRenderer.GetService<Erp.Contracts.PartSvcContract>(Db))
                    svc.Update(ref dsPart);

                // Create Supplier Price List
                var dsVendPart = new VendPartTableset();
                using (var svc = Ice.Assemblies.ServiceRenderer.GetService<Erp.Contracts.VendPartSvcContract>(Db))
                    svc.GetNewVendPart(ref dsVendPart, "", "", "", vendor.VendorNum);

                var refPartNum = ttPart.PartNum;
                var refUOMCode = "";
                string serialWarn;
                string questString;
                bool multipleMatch;
                
                using (var svc = Ice.Assemblies.ServiceRenderer.GetService<Erp.Contracts.VendPartSvcContract>(Db))
                    svc.ChangePart(ref refPartNum, new Guid(), "", ref refUOMCode, out serialWarn, out questString, out multipleMatch, ref dsVendPart);

                using (var svc = Ice.Assemblies.ServiceRenderer.GetService<Erp.Contracts.VendPartSvcContract>(Db))
                    svc.Update(ref dsVendPart);


                // Create Price Break
                using (var svc = Ice.Assemblies.ServiceRenderer.GetService<Erp.Contracts.VendPartSvcContract>(Db))
                    svc.GetNewVendPBrk(ref dsVendPart, ttPart.PartNum, "", "", vendor.VendorNum, DateTime.Today);

                var ttVendPBrk = dsVendPart.VendPBrk.FirstOrDefault();
                ttVendPBrk.BreakQty = 1;
                ttVendPBrk.PriceModifier = Convert.ToDecimal(callContextBpmData[BPMData_Cost]);
                ttVendPBrk.PUM = dsVendPart.VendPart.FirstOrDefault().PUM;
                ttVendPBrk.VendorNumCurrencyCode = callContextBpmData[BPMData_SuppCurrency].ToString();
                
                using (var svc = Ice.Assemblies.ServiceRenderer.GetService<Erp.Contracts.VendPartSvcContract>(Db))
                    svc.ChangePriceModifier(ref dsVendPart);

                using (var svc = Ice.Assemblies.ServiceRenderer.GetService<Erp.Contracts.VendPartSvcContract>(Db))
                    svc.Update(ref dsVendPart);


                // Create Supplier Part
                if(!string.IsNullOrEmpty(callContextBpmData[BPMData_VendPartNum].ToString()))
                {
                    var dsSupplierPart = new SupplierPartTableset();
                    using (var svc = Ice.Assemblies.ServiceRenderer.GetService<Erp.Contracts.SupplierPartSvcContract>(Db))
                        svc.GetNewSupplierPart(ref dsSupplierPart, ttPart.PartNum , vendor.VendorNum, callContextBpmData[BPMData_VendPartNum].ToString(), 0);
    
                    using (var svc = Ice.Assemblies.ServiceRenderer.GetService<Erp.Contracts.SupplierPartSvcContract>(Db))
                        svc.Update(ref dsSupplierPart);
                }

                // Read Base Currency Price List Codes
                var priceListToUpdate = new List<string>();
                var pricesToSet = new List<decimal>();


                //Base Currency
                udCode = Ice.Tables.UDCodes.FindFirstByPrimaryKey(Db, Session.CompanyID, UDParamsID, "PriceLstID");
                if (udCode != null && udCode.IsActive)
                {
                    priceListToUpdate.Add(udCode.CodeDesc);
                    pricesToSet.Add(Convert.ToDecimal(callContextBpmData[BPMData_BaseUnitPrice]));
                }

                //Second Currency
                udCode = Ice.Tables.UDCodes.FindFirstByPrimaryKey(Db, Session.CompanyID, UDParamsID, "2ndPriceLs");
                if (udCode != null && udCode.IsActive)
                {
                    priceListToUpdate.Add(udCode.CodeDesc);
                    pricesToSet.Add(Convert.ToDecimal(callContextBpmData[BPMData_SecondCurrencyUnitPrice]));
                }

                int iCounter = 0;
                foreach (var priceListCode in priceListToUpdate)
                {

                    var price = pricesToSet[iCounter];
                    if (price <= decimal.Zero)
                        continue;  //Next iteration

                    var priceLst = PriceLst.FindFirstByPrimaryKey(Db, Session.CompanyID, priceListCode);

                    if (priceLst == null)
                        throw new BLException(string.Format("Base Currency Price List {0} was not found.", priceListCode));

                    var dsPriceLst = new PriceLstTableset();
                    using (var svc = Ice.Assemblies.ServiceRenderer.GetService<Erp.Contracts.PriceLstSvcContract>(Db))
                        svc.GetNewPriceLstParts(ref dsPriceLst, priceListCode, ttPart.PartNum);

                    var ttPriceLstParts = dsPriceLst.PriceLstParts.FirstOrDefault();

                    using (var svc = Ice.Assemblies.ServiceRenderer.GetService<Erp.Contracts.PriceLstSvcContract>(Db))
                    {
                        bool opTrackMulti;
                        string opUOM;
                        svc.ChangePartNum(ttPart.PartNum, priceListCode, out opTrackMulti, out opUOM);
                        ttPriceLstParts.PartNum = ttPart.PartNum;
                        ttPriceLstParts.UOMCode = opUOM;
                    }

                    ttPriceLstParts.BasePrice = price;

                    using (var svc = Ice.Assemblies.ServiceRenderer.GetService<Erp.Contracts.PriceLstSvcContract>(Db))
                        svc.Update(ref dsPriceLst);

                    iCounter++;
                }

                scope.Complete();
            }