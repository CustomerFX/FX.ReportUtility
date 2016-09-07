using System;
using System.IO;
using System.Linq;
using System.Data.OleDb;
using log4net;
using Sage.Platform;
using Sage.Platform.Application;
using Sage.Platform.Data;
using Sage.Entity.Interfaces;
using Sage.SalesLogix.DelphiBridge;
using Sage.SalesLogix.Plugins;
using SlxReporting;

namespace FX.ReportUtility
{
    /* NOTES---
     * 
     * ReportPlugin parameter must be in the format of Family:ReportPluginName
     * 
     * 
     * RecordSelectionFormula must be a valid Crystal record selection formula. Examples:
     * 
     * Example #1:
     * "{OPPORTUNITY.OPPORTUNITYID} = '" + someOpportunityId + "'"
     * 
     * Example #2:
     * "{OPPORTUNITYQUOTE.OPPORTUNITYQUOTEID} = '" + someOpportunityQuoteId + "' AND {OPPORTUNITY_CONTACT.ISPRIMARY} = 'T' AND NOT ({PRODUCT.NAME} = 'Custom Door Panel' OR {PRODUCT.NAME} = 'Custom Drawer Panel')"
     * 
     * 
     * For attaching to a record, it is the responsibility of the code using this assembly to set the references for the attachment record and then save it. For example:
     * 
     * var report = new CRMReport("System:My Report");
     * var attachment = report.CreateAttachment("{OPPORTUNITY.OPPORTUNITYID} = '" + someOpportunityId + "'");
     * attachment.OpportunityId = someOpportunityId;
     * attachment.AccountId = someAccountId;
     * attachment.Save();
     * 
     */

    public class CRMReport
    {
        public CRMReport(string ReportPlugin)
        {
            this.ReportPlugin = ReportPlugin;
        }

        public string ReportPlugin { get; set; }

        private string ReportFamily
        {
            get
            {
                var parts = GetReportPluginParts();
                return parts[0];
            }
        }

        private string ReportName
        {
            get
            {
                var parts = GetReportPluginParts();
                return parts[1];
            }
        }

        private ILog Log = LogManager.GetLogger(typeof(CRMReport));

        public IAttachment CreateAttachment(string RecordSelectionFormula, string Description = null)
        {
            var reportOutput = SaveAsPDF(RecordSelectionFormula);
            return GetAttachmentForFile(reportOutput, Description);
        }

        public string SaveAsPDF(string RecordSelectionFormula, string OutputFileName = null, string OutputFilePath = null)
        {
            if (OutputFileName == null) OutputFileName = string.Format("{0}-{1}.pdf", this.ReportName, Environment.TickCount);
            if (OutputFilePath == null || !Directory.Exists(OutputFilePath)) OutputFilePath = Path.GetTempPath();
            var fileName = Path.Combine(OutputFilePath, SanitizeFileName(OutputFileName));
            Log.Debug("File name to output report to: " + fileName);

            var plugin = Plugin.LoadByName(this.ReportName, this.ReportFamily, PluginType.CrystalReport);
            if (plugin != null)
            {
                var tempRpt = Path.GetTempFileName() + ".rpt";
                using (var stream = new MemoryStream(plugin.Blob.Data))
                {
                    using (var reader = new DelphiBinaryReader(stream))
                    {
                        reader.ReadComponent(true);
                        using (var file = File.OpenWrite(tempRpt))
                        {
                            stream.CopyTo(file);
                        }
                    }
                }

                using (var report = new SlxReport())
                {
                    report.Load(tempRpt);
                    report.UpdateDbConnectionInfo(this.ConnectionString);
                    report.RecordSelectionFormula = RecordSelectionFormula;
                    report.ExportAsPdfEx(fileName, plugin.Name, true);
                    report.Close();
                }
            }
            else
                throw new ArgumentException("The report plugin for " + this.ReportPlugin + "can not be found.");

            return fileName;
        }

        private IAttachment GetAttachmentForFile(string FileToAttach, string Description = null)
        {
            var attach = Sage.Platform.EntityFactory.Create<IAttachment>();
            attach.Description = (string.IsNullOrEmpty(Description) ? Path.GetFileName(FileToAttach) : Description);
            attach.InsertFileAttachment(FileToAttach);

            attach = EntityFactory.GetRepository<IAttachment>().FindFirstByProperty("Id", attach.Id.ToString());

            // workaround for bug in attachment
            if (FileToAttach.ToLower() != Path.Combine(this.AttachmentPath, attach.FileName).ToLower())
            {
                try
                {
                    File.Move(FileToAttach, Path.Combine(this.AttachmentPath, attach.FileName));
                }
                catch { }
            }

            return attach;

        }

        private string AttachmentPath
        {
            get
            {
                using (var conn = new OleDbConnection(this.ConnectionString))
                {
                    using (var cmd = new OleDbCommand("select a.attachmentpath from branchoptions a inner join systeminfo b on a.sitecode = b.primaryserver where b.dbtype = 1", conn))
                    {
                        conn.Open();
                        var path = cmd.ExecuteScalar().ToString();
                        if (!string.IsNullOrEmpty(path))
                        {
                            if (!path.EndsWith("\\")) path += "\\";
                            try
                            {
                                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                            }
                            catch { }
                        }
                        return path;
                    }
                }
            }
        }

        private string ConnectionString
        {
            get
            {
                return ApplicationContext.Current.Services.Get<IDataService>().GetConnectionString();
            }
        }

        private string SanitizeFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        private string[] GetReportPluginParts()
        {
            var reportPluginParts = ReportPlugin.Split(':');
            if (reportPluginParts.Length != 2)
                throw new ArgumentException("The report plugin name for the ReportPlugin argument is not valid. The parameter must be in the format of Family:ReportPluginName.");

            return reportPluginParts;
        }
    }
}
