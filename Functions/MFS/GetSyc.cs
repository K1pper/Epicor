/*
  Key1
    Table name
  
  Key2-5
    Table key fields    
    
  ShortChar01
    Transmit status for synchronisation with GHA Mobile Field Service
    N=None
    S=Sent change to field service
    R=Received confirmation of update from field service

  ShortChar02
    Source BPM (if used) for debugging/logging

  Character01
    Transmit error received from field service
    
  Character02
    Object sent to field service
    
  ShortChar03
    Last transmission sync time
  
  ShortChar04
    Last received sync time

*/

var now = DateTime.UtcNow;

var udCodes = this.Db.UDCodes
  .Where(r => r.Company == this.Session.CompanyID)
  .Where(r => r.CodeTypeID == "GHAMFS")
  .Where(r => r.CodeID == "MFSStatus")
  .FirstOrDefault();
    
if (udCodes == null) return;

var udtable = udCodes.CodeDesc;

string[] keyArray = keys.Split(',');
string key2 = string.Empty;
string key3 = string.Empty;
string key4 = string.Empty;
string key5 = string.Empty;
string source = string.Empty;

if (keyArray.Count() > 0) key2 = keyArray[0];
if (keyArray.Count() > 1) key3 = keyArray[1];
if (keyArray.Count() > 2) key4 = keyArray[2];
if (keyArray.Count() > 3) key5 = keyArray[3];
if (keyArray.Count() > 4) source = keyArray[4];

if (status != "X") error = "";

try 
{
  switch (udtable)
  {
case "UD100":
      var ds100 = new UD100Tableset();
      
      var UD100 = this.Db.UD100.Where(r => r.Company == this.Session.CompanyID)
        .Where(r => r.Key1 == key1)
        .Where(r => r.Key2 == key2)
        .Where(r => r.Key3 == key3)
        .Where(r => r.Key4 == key4)
        .Where(r => r.Key5 == key5).FirstOrDefault();
      if (UD100 == null)
      {
        this.CallService<Ice.Contracts.UD100SvcContract>(x =>{x.GetaNewUD100(ref ds100);});
        ds100.UD100[0].Key1 = key1;
        ds100.UD100[0].Key2 = key2;
        ds100.UD100[0].Key3 = key3;
        ds100.UD100[0].Key4 = key4;
        ds100.UD100[0].Key5 = key5;;
        ds100.UD100[0].ShortChar01 = status;
        ds100.UD100[0].ShortChar02 = source;
        ds100.UD100[0].Character01 = error;
        ds100.UD100[0].ShortChar03 = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssz");
        this.CallService<Ice.Contracts.UD100SvcContract>(x =>{x.Update(ref ds100);});
      }
      else
      {
        UD100.ShortChar01 = status;
        UD100.Character01 = error;
        UD100.ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      }
      this.CallService<Ice.Contracts.UD100SvcContract>(x =>{x.GetaNewUD100A(ref ds100, key1, key2, "", "", "GHA-MFS");});
      ds100.UD100A[0].Key1 = key1;
      ds100.UD100A[0].Key2 = key2;
      ds100.UD100A[0].Key3 = key3;
      ds100.UD100A[0].Key4 = key4;
      ds100.UD100A[0].Key5 = key5;
      ds100.UD100A[0].ChildKey1 = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssz");
      ds100.UD100A[0].ChildKey2 = "";
      ds100.UD100A[0].ChildKey3 = "";
      ds100.UD100A[0].ChildKey4 = "";
      ds100.UD100A[0].ChildKey5 = "";
      ds100.UD100A[0].ShortChar01 = status;
      ds100.UD100A[0].ShortChar02 = source;
      ds100.UD100A[0].Character01 = error;
      ds100.UD100A[0].Character02 = data;
      ds100.UD100A[0].ShortChar03 = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssz");
      this.CallService<Ice.Contracts.UD100SvcContract>(x =>{x.Update(ref ds100);});
     
      break;
case "UD101":
      var ds101 = new UD101Tableset();
      
      var UD101 = this.Db.UD101.Where(r => r.Company == this.Session.CompanyID)
        .Where(r => r.Key1 == key1)
        .Where(r => r.Key2 == key2)
        .Where(r => r.Key3 == key3)
        .Where(r => r.Key4 == key4)
        .Where(r => r.Key5 == key5).FirstOrDefault();
      if (UD101 == null)
      {
        this.CallService<Ice.Contracts.UD101SvcContract>(x =>{x.GetaNewUD101(ref ds101);});
        ds101.UD101[0].Key1 = key1;
        ds101.UD101[0].Key2 = key2;
        ds101.UD101[0].Key3 = key3;
        ds101.UD101[0].Key4 = key4;
        ds101.UD101[0].Key5 = key5;;
        ds101.UD101[0].ShortChar01 = status;
        ds101.UD101[0].ShortChar02 = source;
        ds101.UD101[0].Character01 = error;
        ds101.UD101[0].ShortChar03 = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssz");
        this.CallService<Ice.Contracts.UD101SvcContract>(x =>{x.Update(ref ds101);});
      }
      else
      {
        UD101.ShortChar01 = status;
        UD101.Character01 = error;
        UD101.ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      }
      this.CallService<Ice.Contracts.UD101SvcContract>(x =>{x.GetaNewUD101A(ref ds101, key1, key2, "", "", "GHA-MFS");});
      ds101.UD101A[0].Key1 = key1;
      ds101.UD101A[0].Key2 = key2;
      ds101.UD101A[0].Key3 = key3;
      ds101.UD101A[0].Key4 = key4;
      ds101.UD101A[0].Key5 = key5;
      ds101.UD101A[0].ChildKey1 = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssz");
      ds101.UD101A[0].ChildKey2 = "";
      ds101.UD101A[0].ChildKey3 = "";
      ds101.UD101A[0].ChildKey4 = "";
      ds101.UD101A[0].ChildKey5 = "";
      ds101.UD101A[0].ShortChar01 = status;
      ds101.UD101A[0].ShortChar02 = source;
      ds101.UD101A[0].Character01 = error;
      ds101.UD101A[0].Character02 = data;
      ds101.UD101A[0].ShortChar03 = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssz");
      this.CallService<Ice.Contracts.UD101SvcContract>(x =>{x.Update(ref ds101);});
     
      break;      
case "UD102":
      var ds102 = new UD102Tableset();
      
      var UD102 = this.Db.UD102.Where(r => r.Company == this.Session.CompanyID)
        .Where(r => r.Key1 == key1)
        .Where(r => r.Key2 == key2)
        .Where(r => r.Key3 == key3)
        .Where(r => r.Key4 == key4)
        .Where(r => r.Key5 == key5).FirstOrDefault();
      if (UD102 == null)
      {
        this.CallService<Ice.Contracts.UD102SvcContract>(x =>{x.GetaNewUD102(ref ds102);});
        ds102.UD102[0].Key1 = key1;
        ds102.UD102[0].Key2 = key2;
        ds102.UD102[0].Key3 = key3;
        ds102.UD102[0].Key4 = key4;
        ds102.UD102[0].Key5 = key5;;
        ds102.UD102[0].ShortChar01 = status;
        ds102.UD102[0].ShortChar02 = source;
        ds102.UD102[0].Character01 = error;
        ds102.UD102[0].ShortChar03 = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssz");
        this.CallService<Ice.Contracts.UD102SvcContract>(x =>{x.Update(ref ds102);});
      }
      else
      {
        UD102.ShortChar01 = status;
        UD102.Character01 = error;
        UD102.ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      }
      this.CallService<Ice.Contracts.UD102SvcContract>(x =>{x.GetaNewUD102A(ref ds102, key1, key2, "", "", "GHA-MFS");});
      ds102.UD102A[0].Key1 = key1;
      ds102.UD102A[0].Key2 = key2;
      ds102.UD102A[0].Key3 = key3;
      ds102.UD102A[0].Key4 = key4;
      ds102.UD102A[0].Key5 = key5;
      ds102.UD102A[0].ChildKey1 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      ds102.UD102A[0].ChildKey2 = "";
      ds102.UD102A[0].ChildKey3 = "";
      ds102.UD102A[0].ChildKey4 = "";
      ds102.UD102A[0].ChildKey5 = "";
      ds102.UD102A[0].ShortChar01 = status;
      ds102.UD102A[0].ShortChar02 = source;
      ds102.UD102A[0].Character01 = error;
      ds102.UD102A[0].Character02 = data;
      ds102.UD102A[0].ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      this.CallService<Ice.Contracts.UD102SvcContract>(x =>{x.Update(ref ds102);});
     
      break;
case "UD103":
      var ds103 = new UD103Tableset();
      
      var UD103 = this.Db.UD103.Where(r => r.Company == this.Session.CompanyID)
        .Where(r => r.Key1 == key1)
        .Where(r => r.Key2 == key2)
        .Where(r => r.Key3 == key3)
        .Where(r => r.Key4 == key4)
        .Where(r => r.Key5 == key5).FirstOrDefault();
      if (UD103 == null)
      {
        this.CallService<Ice.Contracts.UD103SvcContract>(x =>{x.GetaNewUD103(ref ds103);});
        ds103.UD103[0].Key1 = key1;
        ds103.UD103[0].Key2 = key2;
        ds103.UD103[0].Key3 = key3;
        ds103.UD103[0].Key4 = key4;
        ds103.UD103[0].Key5 = key5;;
        ds103.UD103[0].ShortChar01 = status;
        ds103.UD103[0].ShortChar02 = source;
        ds103.UD103[0].Character01 = error;
        ds103.UD103[0].ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
        this.CallService<Ice.Contracts.UD103SvcContract>(x =>{x.Update(ref ds103);});
      }
      else
      {
        UD103.ShortChar01 = status;
        UD103.Character01 = error;
        UD103.ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      }
      this.CallService<Ice.Contracts.UD103SvcContract>(x =>{x.GetaNewUD103A(ref ds103, key1, key2, "", "", "GHA-MFS");});
      ds103.UD103A[0].Key1 = key1;
      ds103.UD103A[0].Key2 = key2;
      ds103.UD103A[0].Key3 = key3;
      ds103.UD103A[0].Key4 = key4;
      ds103.UD103A[0].Key5 = key5;
      ds103.UD103A[0].ChildKey1 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      ds103.UD103A[0].ChildKey2 = "";
      ds103.UD103A[0].ChildKey3 = "";
      ds103.UD103A[0].ChildKey4 = "";
      ds103.UD103A[0].ChildKey5 = "";
      ds103.UD103A[0].ShortChar01 = status;
      ds103.UD103A[0].ShortChar02 = source;
      ds103.UD103A[0].Character01 = error;
      ds103.UD103A[0].Character02 = data;
      ds103.UD103A[0].ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      this.CallService<Ice.Contracts.UD103SvcContract>(x =>{x.Update(ref ds103);});
     
      break;
case "UD104":
      var ds104 = new UD104Tableset();
      
      var UD104 = this.Db.UD104.Where(r => r.Company == this.Session.CompanyID)
        .Where(r => r.Key1 == key1)
        .Where(r => r.Key2 == key2)
        .Where(r => r.Key3 == key3)
        .Where(r => r.Key4 == key4)
        .Where(r => r.Key5 == key5).FirstOrDefault();
      if (UD104 == null)
      {
        this.CallService<Ice.Contracts.UD104SvcContract>(x =>{x.GetaNewUD104(ref ds104);});
        ds104.UD104[0].Key1 = key1;
        ds104.UD104[0].Key2 = key2;
        ds104.UD104[0].Key3 = key3;
        ds104.UD104[0].Key4 = key4;
        ds104.UD104[0].Key5 = key5;;
        ds104.UD104[0].ShortChar01 = status;
        ds104.UD104[0].ShortChar02 = source;
        ds104.UD104[0].Character01 = error;
        ds104.UD104[0].ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
        this.CallService<Ice.Contracts.UD104SvcContract>(x =>{x.Update(ref ds104);});
      }
      else
      {
        UD104.ShortChar01 = status;
        UD104.Character01 = error;
        UD104.ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      }
      this.CallService<Ice.Contracts.UD104SvcContract>(x =>{x.GetaNewUD104A(ref ds104, key1, key2, "", "", "GHA-MFS");});
      ds104.UD104A[0].Key1 = key1;
      ds104.UD104A[0].Key2 = key2;
      ds104.UD104A[0].Key3 = key3;
      ds104.UD104A[0].Key4 = key4;
      ds104.UD104A[0].Key5 = key5;
      ds104.UD104A[0].ChildKey1 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      ds104.UD104A[0].ChildKey2 = "";
      ds104.UD104A[0].ChildKey3 = "";
      ds104.UD104A[0].ChildKey4 = "";
      ds104.UD104A[0].ChildKey5 = "";
      ds104.UD104A[0].ShortChar01 = status;
      ds104.UD104A[0].ShortChar02 = source;
      ds104.UD104A[0].Character01 = error;
      ds104.UD104A[0].Character02 = data;
      ds104.UD104A[0].ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      this.CallService<Ice.Contracts.UD104SvcContract>(x =>{x.Update(ref ds104);});
     
      break;
case "UD105":
      var ds105 = new UD105Tableset();
      
      var UD105 = this.Db.UD105.Where(r => r.Company == this.Session.CompanyID)
        .Where(r => r.Key1 == key1)
        .Where(r => r.Key2 == key2)
        .Where(r => r.Key3 == key3)
        .Where(r => r.Key4 == key4)
        .Where(r => r.Key5 == key5).FirstOrDefault();
      if (UD105 == null)
      {
        this.CallService<Ice.Contracts.UD105SvcContract>(x =>{x.GetaNewUD105(ref ds105);});
        ds105.UD105[0].Key1 = key1;
        ds105.UD105[0].Key2 = key2;
        ds105.UD105[0].Key3 = key3;
        ds105.UD105[0].Key4 = key4;
        ds105.UD105[0].Key5 = key5;;
        ds105.UD105[0].ShortChar01 = status;
        ds105.UD105[0].ShortChar02 = source;
        ds105.UD105[0].Character01 = error;
        ds105.UD105[0].ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
        this.CallService<Ice.Contracts.UD105SvcContract>(x =>{x.Update(ref ds105);});
      }
      else
      {
        UD105.ShortChar01 = status;
        UD105.Character01 = error;
        UD105.ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      }
      this.CallService<Ice.Contracts.UD105SvcContract>(x =>{x.GetaNewUD105A(ref ds105, key1, key2, "", "", "GHA-MFS");});
      ds105.UD105A[0].Key1 = key1;
      ds105.UD105A[0].Key2 = key2;
      ds105.UD105A[0].Key3 = key3;
      ds105.UD105A[0].Key4 = key4;
      ds105.UD105A[0].Key5 = key5;
      ds105.UD105A[0].ChildKey1 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      ds105.UD105A[0].ChildKey2 = "";
      ds105.UD105A[0].ChildKey3 = "";
      ds105.UD105A[0].ChildKey4 = "";
      ds105.UD105A[0].ChildKey5 = "";
      ds105.UD105A[0].ShortChar01 = status;
      ds105.UD105A[0].ShortChar02 = source;
      ds105.UD105A[0].Character01 = error;
      ds105.UD105A[0].Character02 = data;
      ds105.UD105A[0].ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      this.CallService<Ice.Contracts.UD105SvcContract>(x =>{x.Update(ref ds105);});
     
      break;
case "UD106":
      var ds106 = new UD106Tableset();
      
      var UD106 = this.Db.UD106.Where(r => r.Company == this.Session.CompanyID)
        .Where(r => r.Key1 == key1)
        .Where(r => r.Key2 == key2)
        .Where(r => r.Key3 == key3)
        .Where(r => r.Key4 == key4)
        .Where(r => r.Key5 == key5).FirstOrDefault();
      if (UD106 == null)
      {
        this.CallService<Ice.Contracts.UD106SvcContract>(x =>{x.GetaNewUD106(ref ds106);});
        ds106.UD106[0].Key1 = key1;
        ds106.UD106[0].Key2 = key2;
        ds106.UD106[0].Key3 = key3;
        ds106.UD106[0].Key4 = key4;
        ds106.UD106[0].Key5 = key5;;
        ds106.UD106[0].ShortChar01 = status;
        ds106.UD106[0].ShortChar02 = source;
        ds106.UD106[0].Character01 = error;
        ds106.UD106[0].ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
        this.CallService<Ice.Contracts.UD106SvcContract>(x =>{x.Update(ref ds106);});
      }
      else
      {
        UD106.ShortChar01 = status;
        UD106.Character01 = error;
        UD106.ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      }
      this.CallService<Ice.Contracts.UD106SvcContract>(x =>{x.GetaNewUD106A(ref ds106, key1, key2, "", "", "GHA-MFS");});
      ds106.UD106A[0].Key1 = key1;
      ds106.UD106A[0].Key2 = key2;
      ds106.UD106A[0].Key3 = key3;
      ds106.UD106A[0].Key4 = key4;
      ds106.UD106A[0].Key5 = key5;
      ds106.UD106A[0].ChildKey1 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      ds106.UD106A[0].ChildKey2 = "";
      ds106.UD106A[0].ChildKey3 = "";
      ds106.UD106A[0].ChildKey4 = "";
      ds106.UD106A[0].ChildKey5 = "";
      ds106.UD106A[0].ShortChar01 = status;
      ds106.UD106A[0].ShortChar02 = source;
      ds106.UD106A[0].Character01 = error;
      ds106.UD106A[0].Character02 = data;
      ds106.UD106A[0].ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      this.CallService<Ice.Contracts.UD106SvcContract>(x =>{x.Update(ref ds106);});
     
      break;
case "UD107":
      var ds107 = new UD107Tableset();
      
      var UD107 = this.Db.UD107.Where(r => r.Company == this.Session.CompanyID)
        .Where(r => r.Key1 == key1)
        .Where(r => r.Key2 == key2)
        .Where(r => r.Key3 == key3)
        .Where(r => r.Key4 == key4)
        .Where(r => r.Key5 == key5).FirstOrDefault();
      if (UD107 == null)
      {
        this.CallService<Ice.Contracts.UD107SvcContract>(x =>{x.GetaNewUD107(ref ds107);});
        ds107.UD107[0].Key1 = key1;
        ds107.UD107[0].Key2 = key2;
        ds107.UD107[0].Key3 = key3;
        ds107.UD107[0].Key4 = key4;
        ds107.UD107[0].Key5 = key5;;
        ds107.UD107[0].ShortChar01 = status;
        ds107.UD107[0].ShortChar02 = source;
        ds107.UD107[0].Character01 = error;
        ds107.UD107[0].ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
        this.CallService<Ice.Contracts.UD107SvcContract>(x =>{x.Update(ref ds107);});
      }
      else
      {
        UD107.ShortChar01 = status;
        UD107.Character01 = error;
        UD107.ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      }
      this.CallService<Ice.Contracts.UD107SvcContract>(x =>{x.GetaNewUD107A(ref ds107, key1, key2, "", "", "GHA-MFS");});
      ds107.UD107A[0].Key1 = key1;
      ds107.UD107A[0].Key2 = key2;
      ds107.UD107A[0].Key3 = key3;
      ds107.UD107A[0].Key4 = key4;
      ds107.UD107A[0].Key5 = key5;
      ds107.UD107A[0].ChildKey1 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      ds107.UD107A[0].ChildKey2 = "";
      ds107.UD107A[0].ChildKey3 = "";
      ds107.UD107A[0].ChildKey4 = "";
      ds107.UD107A[0].ChildKey5 = "";
      ds107.UD107A[0].ShortChar01 = status;
      ds107.UD107A[0].ShortChar02 = source;
      ds107.UD107A[0].Character01 = error;
      ds107.UD107A[0].Character02 = data;
      ds107.UD107A[0].ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      this.CallService<Ice.Contracts.UD107SvcContract>(x =>{x.Update(ref ds107);});
     
      break;
case "UD108":
      var ds108 = new UD108Tableset();
      
      var UD108 = this.Db.UD108.Where(r => r.Company == this.Session.CompanyID)
        .Where(r => r.Key1 == key1)
        .Where(r => r.Key2 == key2)
        .Where(r => r.Key3 == key3)
        .Where(r => r.Key4 == key4)
        .Where(r => r.Key5 == key5).FirstOrDefault();
      if (UD108 == null)
      {
        this.CallService<Ice.Contracts.UD108SvcContract>(x =>{x.GetaNewUD108(ref ds108);});
        ds108.UD108[0].Key1 = key1;
        ds108.UD108[0].Key2 = key2;
        ds108.UD108[0].Key3 = key3;
        ds108.UD108[0].Key4 = key4;
        ds108.UD108[0].Key5 = key5;;
        ds108.UD108[0].ShortChar01 = status;
        ds108.UD108[0].ShortChar02 = source;
        ds108.UD108[0].Character01 = error;
        ds108.UD108[0].ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
        this.CallService<Ice.Contracts.UD108SvcContract>(x =>{x.Update(ref ds108);});
      }
      else
      {
        UD108.ShortChar01 = status;
        UD108.Character01 = error;
        UD108.ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      }
      this.CallService<Ice.Contracts.UD108SvcContract>(x =>{x.GetaNewUD108A(ref ds108, key1, key2, "", "", "GHA-MFS");});
      ds108.UD108A[0].Key1 = key1;
      ds108.UD108A[0].Key2 = key2;
      ds108.UD108A[0].Key3 = key3;
      ds108.UD108A[0].Key4 = key4;
      ds108.UD108A[0].Key5 = key5;
      ds108.UD108A[0].ChildKey1 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      ds108.UD108A[0].ChildKey2 = "";
      ds108.UD108A[0].ChildKey3 = "";
      ds108.UD108A[0].ChildKey4 = "";
      ds108.UD108A[0].ChildKey5 = "";
      ds108.UD108A[0].ShortChar01 = status;
      ds108.UD108A[0].ShortChar02 = source;
      ds108.UD108A[0].Character01 = error;
      ds108.UD108A[0].Character02 = data;
      ds108.UD108A[0].ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      this.CallService<Ice.Contracts.UD108SvcContract>(x =>{x.Update(ref ds108);});
     
      break;
case "UD109":
      var ds109 = new UD109Tableset();
      
      var UD109 = this.Db.UD109.Where(r => r.Company == this.Session.CompanyID)
        .Where(r => r.Key1 == key1)
        .Where(r => r.Key2 == key2)
        .Where(r => r.Key3 == key3)
        .Where(r => r.Key4 == key4)
        .Where(r => r.Key5 == key5).FirstOrDefault();
      if (UD109 == null)
      {
        this.CallService<Ice.Contracts.UD109SvcContract>(x =>{x.GetaNewUD109(ref ds109);});
        ds109.UD109[0].Key1 = key1;
        ds109.UD109[0].Key2 = key2;
        ds109.UD109[0].Key3 = key3;
        ds109.UD109[0].Key4 = key4;
        ds109.UD109[0].Key5 = key5;;
        ds109.UD109[0].ShortChar01 = status;
        ds109.UD109[0].ShortChar02 = source;
        ds109.UD109[0].Character01 = error;
        ds109.UD109[0].ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
        this.CallService<Ice.Contracts.UD109SvcContract>(x =>{x.Update(ref ds109);});
      }
      else
      {
        UD109.ShortChar01 = status;
        UD109.Character01 = error;
        UD109.ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      }
      this.CallService<Ice.Contracts.UD109SvcContract>(x =>{x.GetaNewUD109A(ref ds109, key1, key2, "", "", "GHA-MFS");});
      ds109.UD109A[0].Key1 = key1;
      ds109.UD109A[0].Key2 = key2;
      ds109.UD109A[0].Key3 = key3;
      ds109.UD109A[0].Key4 = key4;
      ds109.UD109A[0].Key5 = key5;
      ds109.UD109A[0].ChildKey1 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      ds109.UD109A[0].ChildKey2 = "";
      ds109.UD109A[0].ChildKey3 = "";
      ds109.UD109A[0].ChildKey4 = "";
      ds109.UD109A[0].ChildKey5 = "";
      ds109.UD109A[0].ShortChar01 = status;
      ds109.UD109A[0].ShortChar02 = source;
      ds109.UD109A[0].Character01 = error;
      ds109.UD109A[0].Character02 = data;
      ds109.UD109A[0].ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      this.CallService<Ice.Contracts.UD109SvcContract>(x =>{x.Update(ref ds109);});
     
      break;
case "UD110":
      var ds110 = new UD110Tableset();
      
      var UD110 = this.Db.UD110.Where(r => r.Company == this.Session.CompanyID)
        .Where(r => r.Key1 == key1)
        .Where(r => r.Key2 == key2)
        .Where(r => r.Key3 == key3)
        .Where(r => r.Key4 == key4)
        .Where(r => r.Key5 == key5).FirstOrDefault();
      if (UD110 == null)
      {
        this.CallService<Ice.Contracts.UD110SvcContract>(x =>{x.GetaNewUD110(ref ds110);});
        ds110.UD110[0].Key1 = key1;
        ds110.UD110[0].Key2 = key2;
        ds110.UD110[0].Key3 = key3;
        ds110.UD110[0].Key4 = key4;
        ds110.UD110[0].Key5 = key5;;
        ds110.UD110[0].ShortChar01 = status;
        ds110.UD110[0].ShortChar02 = source;
        ds110.UD110[0].Character01 = error;
        ds110.UD110[0].ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
        this.CallService<Ice.Contracts.UD110SvcContract>(x =>{x.Update(ref ds110);});
      }
      else
      {
        UD110.ShortChar01 = status;
        UD110.Character01 = error;
        UD110.ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      }
      this.CallService<Ice.Contracts.UD110SvcContract>(x =>{x.GetaNewUD110A(ref ds110, key1, key2, "", "", "GHA-MFS");});
      ds110.UD110A[0].Key1 = key1;
      ds110.UD110A[0].Key2 = key2;
      ds110.UD110A[0].Key3 = key3;
      ds110.UD110A[0].Key4 = key4;
      ds110.UD110A[0].Key5 = key5;
      ds110.UD110A[0].ChildKey1 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      ds110.UD110A[0].ChildKey2 = "";
      ds110.UD110A[0].ChildKey3 = "";
      ds110.UD110A[0].ChildKey4 = "";
      ds110.UD110A[0].ChildKey5 = "";
      ds110.UD110A[0].ShortChar01 = status;
      ds110.UD110A[0].ShortChar02 = source;
      ds110.UD110A[0].Character01 = error;
      ds110.UD110A[0].Character02 = data;
      ds110.UD110A[0].ShortChar03 = now.ToString("yyyy-MM-ddTHH:mm:ssz");
      this.CallService<Ice.Contracts.UD110SvcContract>(x =>{x.Update(ref ds110);});
     
      break;
    default:
      break;
  }
}
catch (Exception ex)
{
  Ice.Diagnostics.Log.WriteEntry(ex.Message);
}
