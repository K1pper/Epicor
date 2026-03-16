/*
field service get codetypeid
sfdc (classic and kinetic
check all fields are there and synced in both classic and kinetic
*/
try
{
    this.ThisLib.AddCodeType("GHA_POD_UC", "Parameters for GHA Proof of Delivery Automation");
    this.ThisLib.AddCode("GHA_POD_UC", "SavePhoLoc", "false", "The Store phone location.", true);
    this.ThisLib.AddCode("GHA_POD_UC", "ProcFlow", "SIG-PHO", "Process Flow.", true);
    this.ThisLib.AddCode("GHA_POD_UC", "ImageQual", "LOW", "Image Settings.", true);

    this.ThisLib.AddCodeType("GHAStock", "Simple Stock Take Parameters");
    this.ThisLib.AddCode("GHAStock", "CalendarID", "<Calendar>", "The Default Production Calendar ID used by Count Capacity and Count Selection.", false);
    this.ThisLib.AddCode("GHAStock", "CntActBAQ", "GHACountActivity", "The BAQ ID used to show recent Transactions in Count Processing / History.", true);
    this.ThisLib.AddCode("GHAStock", "CountRptID", "GHACountBinPart", "The BAQ Report ID to launch (Warehouse Bin Count) from within Count Selection.", true);
    this.ThisLib.AddCode("GHAStock", "DueWeeks", "1", "The number of weeks after calculated Due Date when the Count becomes Overdue.", true);
    this.ThisLib.AddCode("GHAStock", "InvAdjCode", "<Reason>", "The default Inventory Adjustment / Count Discrepancy Reason Code used in Count Processing.", false);
    this.ThisLib.AddCode("GHAStock", "ShowOHQ", "False", "Determines whether On Hand is shown in Count Selection BAQ Report.", false);
    this.ThisLib.AddCode("GHAStock", "ShowOHQPro", "False", "Determines whether On Hand / Variance Quantity is shown in Count Processing.", false);
    this.ThisLib.AddCode("GHAStock", "ShowEsCost", "False", "Determines whether Estimated Cost is shown in Count Processing.", false);
    this.ThisLib.AddCode("GHAStock", "CntVarBAQ", "GHACountBinPartVariance", "The BAQ ID used to show Transactions in Count Bin Variance.", true);
    this.ThisLib.AddCode("GHAStock", "CntHisBAQH", "GHACountBinSummary", "The BAQ ID used to show Transactions in Count History Summary.", true);
    this.ThisLib.AddCode("GHAStock", "CntHisBAQD", "GHACountBinHistory", "The BAQ ID used to show Transactions in Count History.", true);
    this.ThisLib.AddCode("GHAStock", "HonrManCnt", "False", "Honor Manual Count for Full physical count.", true);
    this.ThisLib.AddCode("GHAStock", "CntAppBAQ", "GHACountApproval", "The BAQ used to show count bin parts for approval", true);
    this.ThisLib.AddCode("GHAStock", "CntSelBAQ", "GHACountSelection", "The BAQ used to show count selection", true);
    this.ThisLib.AddCode("GHAStock", "CntBinEmRw", "3", "Option for 0 – 9 empty rows to be printed after each Warehouse / Bin in the Warehouse / Bin Count Report (default to 3).", true);
    this.ThisLib.AddCode("GHAStock", "CntSchBAQ", "GHAPartSearch", "The BAQ ID used to Add Parts in Count Processing.", true);
    this.ThisLib.AddCode("GHAStock", "IncPBInfo", "true", "Inc. Other Locations from Bin Info.", true);
    this.ThisLib.AddCode("GHAStock", "IncFrmInv", "true", "Inc. Other Locations from Inventory.", true);
    this.ThisLib.AddCode("GHAStock", "AddPtsLoc", "true", "Add Parts from Other Locations.", true);
    this.ThisLib.AddCode("GHAStock", "CountBy", "Q", "Quantity Adjument process will be based on this type of calculus.", false);

    this.ThisLib.AddCodeType("GHAAutoDMR", "Simple Stock Take Parameters");
    this.ThisLib.AddCode("GHAAutoDMR", "AutoFailOP", "False", "Automatically Fail on NonConformance Operations.", true);
    this.ThisLib.AddCode("GHAAutoDMR", "AutoFailIn", "False", "Automatically Fail on NonConformance Inventory.", true);
    this.ThisLib.AddCode("GHAAutoDMR", "AutoFailMt", "False", "Automatically Fail on NonConformance Material.", true);
    this.ThisLib.AddCode("GHAAutoDMR", "ReasonOP", "<Reason>", "Default Reason for Non Conformance Operations.", true);
    this.ThisLib.AddCode("GHAAutoDMR", "ReasonInv", "<Reason>", "Default Reason for Non Conformance Inventory.", true);
    this.ThisLib.AddCode("GHAAutoDMR", "ReasonMtl", "<Reason>", "Default Reason for Non Conformance Materials.", true);
    this.ThisLib.AddCode("GHAAutoDMR", "ReqMoveOP", "False", "Request Move for Non Conformance Operations.", true);
    this.ThisLib.AddCode("GHAAutoDMR", "ReqMoveInv", "False", "Request Move for Non Conformance Inventory.", true);
    this.ThisLib.AddCode("GHAAutoDMR", "ReqMoveMtl", "False", "Request Move for Non Conformance Materials.", true);
    this.ThisLib.AddCode("GHAAutoDMR", "CorrActOP", "False", "Create Corrective Action for Non Conformance Operations.", true);
    this.ThisLib.AddCode("GHAAutoDMR", "CorrActInv", "False", "Create Corrective Action for Non Conformance Inventory.", true);
    this.ThisLib.AddCode("GHAAutoDMR", "CorrActMtl", "False", "Create Corrective Action for Non Conformance Materials.", true);
    this.ThisLib.AddCode("GHAAutoDMR", "CostToDMR", "False", "Move Cost to DMR for Non Conformance Operations.", true);
    this.ThisLib.AddCode("GHAAutoDMR", "DefInsptor", "<Inspector>", "Default Inspector for DMR creation..", true);
    this.ThisLib.AddCode("GHAAutoDMR", "TempJob", "<Job>", "Job Disposal Job Number.", true);
    this.ThisLib.AddCode("GHAAutoDMR", "LeadTime", "1", "The number of days used to calculatethe due date of the DMR disposal job from today.", true);
    this.ThisLib.AddCode("GHAAutoDMR", "JobEng", "False", "Engineer Disposal Job.", true);
    this.ThisLib.AddCode("GHAAutoDMR", "JobRel", "False", "Release Disposal Job.", true);

    this.ThisLib.AddCodeType("GHAReports", "Parameters for GHA Report Suite");
    this.ThisLib.AddCode("GHAReports", "ColBorders", "#92D3E9", "Colour used for Report borders/lines (set as 'Debug' to show 'TableGuid' and 'Version').", true);
    this.ThisLib.AddCode("GHAReports", "ColPrimary", "LightGray", "Primary colour used for Report headers", true);
    this.ThisLib.AddCode("GHAReports", "ColRowEven", "WhiteSmoke", "Colour used for alternate (even) Report data rows", true);
    this.ThisLib.AddCode("GHAReports", "ColRowOdd", "Transparent", "Colour used for alternate (odd) Report data rows", true);
    this.ThisLib.AddCode("GHAReports", "ColSecond", "#92D3E9", "Secondary colour used for Report additional text", true);
    this.ThisLib.AddCode("GHAReports", "ColTxt", "Black", "Colour used for Report text", true);
    this.ThisLib.AddCode("GHAReports", "ColTxtHdr", "Black", "Colour used for Report header text", true);
    this.ThisLib.AddCode("GHAReports", "ColTxtTitl", "#92D3E9", "Colour used for Subreport title text", true);
    this.ThisLib.AddCode("GHAReports", "AccEmail", "-", "Account Contact Details Email", true);
    this.ThisLib.AddCode("GHAReports", "AccPhone", "-", "Account Contact Details Phone Number", true);
    this.ThisLib.AddCode("GHAReports", "SODateType", "NeedByDate", "Sales Order OTIF Data Field", true);
    this.ThisLib.AddCode("GHAReports", "PODateType", "DueDate", "Purchase Order OTIF Data Field", true);
    this.ThisLib.AddCode("GHAReports", "SplitDay", "False", "Split day into AM/PM for PO arrival.", true);

    this.ThisLib.AddCodeType("GHACRM", "Parameters for GHA CRM Automation");
    this.ThisLib.AddCode("GHACRM", "ExchAccnt", "-", "Email Exchange Account to review.", true);
    this.ThisLib.AddCode("GHACRM", "Password", "-", "Password.", true);
    this.ThisLib.AddCode("GHACRM", "InEmail", "InEmail", "Call Type for Incoming Email.", true);
    this.ThisLib.AddCode("GHACRM", "OutEmail", "OutEmail", "Call Type for Outcoming Email.", true);
    this.ThisLib.AddCode("GHACRM", "ClientID", "-", "Azure (Application) ClientID", true);
    this.ThisLib.AddCode("GHACRM", "TenantID", "-", "Azure (Directory) TenantID", true);
    this.ThisLib.AddCode("GHACRM", "ClntSecret", "-", "Azure Client secret ID", true);

    var plant = Db.Plant
      .Where(r => r.Company == this.Session.CompanyID)
      .Where(r => r.Plant1 == this.Session.PlantID)
      .FirstOrDefault();

    if (plant != null)
    {
        var FieldSvcCode = plant.UDField<string>("GHA_MFS_UserCodeType_c");

        if (!string.IsNullOrEmpty(FieldSvcCode))
        {
            this.ThisLib.AddCodeType(FieldSvcCode, "Parameters for GHA Mobile Field Service Automation");
            this.ThisLib.AddCode(FieldSvcCode, "UDCodes", "-", "User Code to use", true);
            this.ThisLib.AddCode(FieldSvcCode, "SerialUD1", "-", "Serial Number UD Field 1", false);
            this.ThisLib.AddCode(FieldSvcCode, "JobMRate", "0", "Job Mileage Rate", true);
            this.ThisLib.AddCode(FieldSvcCode, "JobTRate", "0", "Travel Part Rate", true);
            this.ThisLib.AddCode(FieldSvcCode, "JobPause", "-", "Job Pause", true);
            this.ThisLib.AddCode(FieldSvcCode, "TravelPs", "-", "Travel Pause", true);
            this.ThisLib.AddCode(FieldSvcCode, "JobSkip", "-", "Job Skip", true);
            this.ThisLib.AddCode(FieldSvcCode, "StkAdj", "-", "Stock Adj Reason Code", true);
            this.ThisLib.AddCode(FieldSvcCode, "IssueStock", "0", "Issue zero QOH", true);
            this.ThisLib.AddCode(FieldSvcCode, "ManTrvTime", "0", "Travel Time Mandatory", true);
            this.ThisLib.AddCode(FieldSvcCode, "ManTrvMil", "0", "Travel Distance Mandatory", true);
            this.ThisLib.AddCode(FieldSvcCode, "CalLabRat", "-", "Labour Rates", true);
            this.ThisLib.AddCode(FieldSvcCode, "DTEM", "-", "Default Labour Time Entry Method", true);
            this.ThisLib.AddCode(FieldSvcCode, "DTrvTim", "-", "Default Travel Time Entry Method", true);
            this.ThisLib.AddCode(FieldSvcCode, "DTrvTimCap", "0", "Default Travel Time Cap", true);
            this.ThisLib.AddCode(FieldSvcCode, "DMileCap", "0", "Default Mileage Cap", true);
            this.ThisLib.AddCode(FieldSvcCode, "DMinJobCh", "0", "Default Minimum Job Charge", true);
            this.ThisLib.AddCode(FieldSvcCode, "GroupJob", "N", "Group Job By Duration", true);
            this.ThisLib.AddCode(FieldSvcCode, "GroupJobWC", "0", "Group Job By week Commencing On", true);
            this.ThisLib.AddCode(FieldSvcCode, "JobLabInc", "0", "Default Labour Increment", true);
            this.ThisLib.AddCode(FieldSvcCode, "DMinTrvTim", "0", "Default Minimum Travel Time", true);
            this.ThisLib.AddCode(FieldSvcCode, "JobTravInc", "0", "Default Travel Time Increment", true);
            this.ThisLib.AddCode(FieldSvcCode, "MTEM", "-", "Maintenance Labour Time Entry Method", true);
            this.ThisLib.AddCode(FieldSvcCode, "MTrvTim", "-", "Maintenance Travel Time Entry Method", true);
            this.ThisLib.AddCode(FieldSvcCode, "MTrvTimCap", "0", "Maintenance Travel Time Cap", true);
            this.ThisLib.AddCode(FieldSvcCode, "MMileCap", "0", "Maintenance Mileage Cap", true);
            this.ThisLib.AddCode(FieldSvcCode, "MMinJobCh", "0", "Maintenance Minimum Job Charge", true);
            this.ThisLib.AddCode(FieldSvcCode, "MJobLabInc", "0", "Maintenance Labour Increment", true);
            this.ThisLib.AddCode(FieldSvcCode, "MMinTrvTim", "0", "Maint Minimum Travel Time", true);
            this.ThisLib.AddCode(FieldSvcCode, "MJobTrvInc", "0", "Maint Travel Time Increment", true);
            this.ThisLib.AddCode(FieldSvcCode, "CloseJob", "false", "Close call Job on Approval", true);
            this.ThisLib.AddCode(FieldSvcCode, "ShowZerStk", "false", "Show zero stock items", true);
            this.ThisLib.AddCode(FieldSvcCode, "HideCosts", "false", "Hide Costs Page", true);
            this.ThisLib.AddCode(FieldSvcCode, "Burden", "-", "Burden", true);
            this.ThisLib.AddCode(FieldSvcCode, "RetRound", "0", "Round Travel Combined", true);
            this.ThisLib.AddCode(FieldSvcCode, "CapRetTrvl", "0", "Cap Travel Combined", true);
            this.ThisLib.AddCode(FieldSvcCode, "CapRetMlge", "0", "Cap Mileage Combined", true);
        }
    }

    this.ThisLib.AddCodeType("GHAMFS", "Parameters for GHA Mobile Field Service Automation");
    this.ThisLib.AddCode("GHAMFS", "LicenceKey", "0", "Licence Key", true);
    this.ThisLib.AddCode("GHAMFS", "JobDaysBef", "0", "Job Display Setting - Days Before", true);
    this.ThisLib.AddCode("GHAMFS", "JobDaysAft", "0", "Job Display Setting - Days After", true);
    this.ThisLib.AddCode("GHAMFS", "JobMileage", "-", "Mileage Part", true);
    this.ThisLib.AddCode("GHAMFS", "JobTravel", "-", "Travel Part", true);
    this.ThisLib.AddCode("GHAMFS", "JobExpense", "-", "Expense Part", true);
    this.ThisLib.AddCode("GHAMFS", "MFSStatus", "-", "MFS Status", true);
    this.ThisLib.AddCode("GHAMFS", "MFSLog", "-", "MFS Log", true);
    this.ThisLib.AddCode("GHAMFS", "FxCostPart", "-", "Fixed Cost Part", true);
    this.ThisLib.AddCode("GHAMFS", "StkAdj", "-", "Stock Adj Reason Code", true);
    this.ThisLib.AddCode("GHAMFS", "ImageQual", "LOW", "Image Settings", true);
    this.ThisLib.AddCode("GHAMFS", "TestEnvir", "false", "Test Environment", true);
    this.ThisLib.AddCode("GHAMFS", "ChckPOStat", "false", "Check PO Num", true);
    this.ThisLib.AddCode("GHAMFS", "ActCosts", "false", "Actual Costs", true);
    this.ThisLib.AddCode("GHAMFS", "LocFreq", "15", "App Location Frequency (s)", true);
    this.ThisLib.AddCode("GHAMFS", "EmpFilter", "RESOURCEGRPID", "Filter for employees on scheduling page", true);
    this.ThisLib.AddCode("GHAMFS", "JobGroupDn", "N", "Jobs Grouped By", true);
    this.ThisLib.AddCode("GHAMFS", "JobGroupWC", "0", "Jobs Grouped Start of Week", true);
    this.ThisLib.AddCode("GHAMFS", "DocPhoto", "<DocType>", "Document type for photo attachments", true);
    this.ThisLib.AddCode("GHAMFS", "DocExpense", "<DocType>", "Document type for expense attachments", true);
    this.ThisLib.AddCode("GHAMFS", "SerialUF1", "-", "Serial Number User Field 1", false);
    this.ThisLib.AddCode("GHAMFS", "SerialUF2", "-", "Serial Number User Field 2", false);
    this.ThisLib.AddCode("GHAMFS", "SerialUF3", "-", "Serial Number User Field 3", false);
    this.ThisLib.AddCode("GHAMFS", "SerialUF4", "-", "Serial Number User Field 4", false);
    this.ThisLib.AddCode("GHAMFS", "SerialUD1", "-", "Serial Number UD Field 1", false);
    this.ThisLib.AddCode("GHAMFS", "SerialUD2", "-", "Serial Number UD Field 2", false);
    this.ThisLib.AddCode("GHAMFS", "SerialUD3", "-", "Serial Number UD Field 3", false);
    this.ThisLib.AddCode("GHAMFS", "SerialUD4", "-", "Serial Number UD Field 4", false);
    this.ThisLib.AddCode("GHAMFS", "LineInv", "0", "Line Level Invoicing", true);
    this.ThisLib.AddCode("GHAMFS", "LineInvRep", "-", "Line Level Invoicing Default Report Style.", true);
    this.ThisLib.AddCode("GHAMFS", "Beta", "0", "Beta Test", true);

    this.ThisLib.AddCodeType("GHAAPAuto", "Parameters for GHA AP Automation");
    this.ThisLib.AddCode("GHAAPAuto", "DefMatchTy", "POR", "Default Matching Type", true);
    this.ThisLib.AddCode("GHAAPAuto", "DefGrpFrq", "-", "Default Group Frequency", true);
    this.ThisLib.AddCode("GHAAPAuto", "HeadTolP", "0", "Header Tolerance %", true);
    this.ThisLib.AddCode("GHAAPAuto", "HeadTolV", "0", "Header Tolerance Value", true);
    this.ThisLib.AddCode("GHAAPAuto", "LineTolP", "0", "Line Tolerance %", true);
    this.ThisLib.AddCode("GHAAPAuto", "LineTolV", "0", "Line Tolerance Value", true);
    this.ThisLib.AddCode("GHAAPAuto", "DefSecGrp", "-", "Default Approval Security Group", true);
    this.ThisLib.AddCode("GHAAPAuto", "DefAPGrp", "-", "Default Review AP Group Prefix", true);
    this.ThisLib.AddCode("GHAAPAuto", "DefMAPGrp", "-", "Default Matched AP Group Prefix", true);
    this.ThisLib.AddCode("GHAAPAuto", "MailUprcDc", "Email Address for Unprocessed Documents", "-", true);
    this.ThisLib.AddCode("GHAAPAuto", "MtFullOver", "0", "Enable full override", true);

    this.ThisLib.AddCodeType("GHA_PO_APP", "Parameters for GHA PO Approval");
    this.ThisLib.AddCode("GHA_PO_APP", "GHA_PO_APP", "0", "Licence Key", true);
    this.ThisLib.AddCode("GHA_PO_APP", "ReqApvAct", "RA02", "Requisition Action ID used for requisition approvals", true);
    this.ThisLib.AddCode("GHA_PO_APP", "ReqPenAct", "RA01", "Requisition Action ID used for determining requisitions that have been sent for approval", true);

    this.ThisLib.AddCodeType("GHA_MIMS", "GHA MIMS");
    this.ThisLib.AddCode("GHA_MIMS", "NoKeyboard", "-", "Determines if the keyboard is globally disabled in MIMS.", true);
    this.ThisLib.AddCode("GHA_MIMS", "PickType", "-", "Picking Type.", true);
    this.ThisLib.AddCode("GHA_MIMS", "NextOnly", "False", "Display Next Pick Only.", true);
    this.ThisLib.AddCode("GHA_MIMS", "OrdersBy", "ORD", "Display Order Count By", true);
    this.ThisLib.AddCode("GHA_MIMS", "PackSlip", "-", "Default Report Style.", true);
    this.ThisLib.AddCode("GHA_MIMS", "ShipType", "-", "Shipment Type.", true);
    this.ThisLib.AddCode("GHA_MIMS", "StagType", "-", "Staging Options.", true);
    this.ThisLib.AddCode("GHA_MIMS", "GIScan", "False", "Force Scan of Receipt Location.", true);
    this.ThisLib.AddCode("GHA_MIMS", "GIPrim", "False", "Force Scan of Receipt Location.", true);
    this.ThisLib.AddCode("GHA_MIMS", "JobScan", "False", "Force Scan of Receipt Location.", true);
    this.ThisLib.AddCode("GHA_MIMS", "KBScan", "False", "Force Scan of Receipt Location.", true);
    this.ThisLib.AddCode("GHA_MIMS", "MSScan", "False", "Force Scan of Receipt Location.", true);
    this.ThisLib.AddCode("GHA_MIMS", "NCRScan", "False", "Force Scan of Receipt Location.", true);
    this.ThisLib.AddCode("GHA_MIMS", "NCRImag", "-", "Photo Size", true);
    this.ThisLib.AddCode("GHA_MIMS", "POReceipt", "-", "Default report style for PO Receipt.", true);
    this.ThisLib.AddCode("GHA_MIMS", "RMAReceipt", "-", "Default report style for RMA Receipt.", true);
    this.ThisLib.AddCode("GHA_MIMS", "JobRctStk", "-", "Default report style for Job Receipt to Stock", true);
    this.ThisLib.AddCode("GHA_MIMS", "JobRctJob", "-", "Default report style for Job Receipt to Job.", true);
    this.ThisLib.AddCode("GHA_MIMS", "StkReceipt", "-", "Default report style.", true);
    this.ThisLib.AddCode("GHA_MIMS", "KBReceipt", "-", "Default report style.", true);
    this.ThisLib.AddCode("GHA_MIMS", "NCRTags", "-", "Default report style.", true);
    this.ThisLib.AddCode("GHA_MIMS", "TFReceipt", "-", "Default report style for Transfer Orders.", true);
    this.ThisLib.AddCode("GHA_MIMS", "PickOpt", "-", "Picking Options.", true);
    this.ThisLib.AddCode("GHA_MIMS", "PutAway", "False", "PutAway", true);
    this.ThisLib.AddCode("GHA_MIMS", "ShowMWConf", "False", "Show Move WIP Confirmation", true);
    this.ThisLib.AddCode("GHA_MIMS", "ShowNegQty", "False", "Show Negative Quantities", true);
    this.ThisLib.AddCode("GHA_MIMS", "Push", "False", "Dashboard Drive Picking", true);
    this.ThisLib.AddCode("GHA_MIMS", "MatQueueTb", "-", "Honour Employee Settings", true);

    this.ThisLib.AddCodeType("GHA_Misc", "Misc Parameters for GHA");
    this.ThisLib.AddCode("GHA_Misc", "RebuildFrm", "00:00", "Rebuild Database From Time", true);
    this.ThisLib.AddCode("GHA_Misc", "RebuildTo", "00:00", "Rebuild Database To Time", true);
    this.ThisLib.AddCode("GHA_Misc", "RebuildAny", "False", "Rebuid Anytime", true);
    this.ThisLib.AddCode("GHA_Misc", "BPMTest", "False", "BPM Logging Table", true);
    this.ThisLib.AddCode("GHA_Misc", "BPMStatus", "-", "BPM Logging Table", true);
    this.ThisLib.AddCode("GHA_Misc", "BPMLicKey", "-", "BPM Licence Key", true);
    this.ThisLib.AddCode("GHA_Misc", "MscMtlBook", "False", "BPM Licence Key", true);

    this.ThisLib.AddCodeType("GHA_PTE", "GHA Project Time and Expense Parameters");
    this.ThisLib.AddCode("GHA_PTE", "Attachment", "-", "", true);
    this.ThisLib.AddCode("GHA_PTE", "DefExpTax", "-", "Default expense Tax Liability", true);
    this.ThisLib.AddCode("GHA_PTE", "DefExpTaxV", "-", "Default expense Tax Liability inc VAT", true);
    this.ThisLib.AddCode("GHA_PTE", "DefResGrp", "-", "Default resource group for indirect time", true);
    this.ThisLib.AddCode("GHA_PTE", "FutureExp", "False", "Future Expense", true);
    this.ThisLib.AddCode("GHA_PTE", "FutureTime", "False", "Future Time", true);
    this.ThisLib.AddCode("GHA_PTE", "LicenceKey", "-", "Licence Key", true);
    this.ThisLib.AddCode("GHA_PTE", "TestEnvir", "False", "Test Environment", true);

    this.ThisLib.AddCodeType("GHA_SFDC", "Parameters for GHA Shop Floor Data Capture");
    this.ThisLib.AddCode("GHA_SFDC", "SecGroup", "-", "The security group use to control access to the Admin Settings on the app.", true);
    this.ThisLib.AddCode("GHA_SFDC", "TPMTable", "-", "The UD table to store TPM header and TPM result records.", true);
    this.ThisLib.AddCode("GHA_SFDC", "PromptSecs", "5", "Prompt Countdown", true);
    this.ThisLib.AddCode("GHA_SFDC", "WrnOverPrd", "True", "Warn of overproduction", true);

}
catch (Exception ex)
{
    Ice.Diagnostics.Log.WriteEntry($"CreateParameters Error: {ex.Message}");
}
