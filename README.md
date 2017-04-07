# FX.ReportUtility
A utility assembly for generating reports and attaching to records. See blog post at [http://customerfx.com/article/exporting-a-report-to-a-pdf-and-attaching-to-a-record-in-server-side-code-in-infor-crm-saleslogix/](http://customerfx.com/article/exporting-a-report-to-a-pdf-and-attaching-to-a-record-in-server-side-code-in-infor-crm-saleslogix/)

# Usage
The utility assembly can be used to export a report as a PDF or attach a PDF to a record as an attachment.

```C#
// export a report to a PDF and save as an attachment
var report = new FX.ReportUtility.CRMReport("System:My Report");
report.RecordSelectionFormula = "{ACCOUNT.ACCOUNTID} = '" + someAccountId + "'";
var attachment = report.CreateAttachment();
attachment.AccountId = someAccountId;
attachment.Save();
```

**Note:** for attaching to a record, it is the responsibility of the code using this assembly to set the references for the attachment record and then save it.

```C#
// export a report as a PDF 
var report = new FX.ReportUtility.CRMReport("System:My Report");
report.RecordSelectionFormula = "{ACCOUNT.ACCOUNTID} = '" + someAccountId + "'";
var exportedFile = report.SaveAsPDF();
```

**RecordSelectionFormula** property must be a valid Crystal record selection formula. Examples:

**Example #1:**
"{OPPORTUNITY.OPPORTUNITYID} = '" + someOpportunityId + "'"
 
**Example #2:**
"{OPPORTUNITYQUOTE.OPPORTUNITYQUOTEID} = '" + someOpportunityQuoteId + "' AND {OPPORTUNITY_CONTACT.ISPRIMARY} = 'T' AND NOT ({PRODUCT.NAME} = 'Custom Door Panel' OR {PRODUCT.NAME} = 'Custom Drawer Panel')"

You can also set parameters (prompts) in the report

```C#
// export a report as a PDF 
var report = new FX.ReportUtility.CRMReport("System:My Report");
report.RecordSelectionFormula = "{ACCOUNT.ACCOUNTID} = '" + someAccountId + "'";
report.SetParameter("MyParam1", "Some value");
report.SetParameter("MyParam2", 10);
var exportedFile = report.SaveAsPDF();
```