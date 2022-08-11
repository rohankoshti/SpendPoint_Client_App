using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpendPoint
{
    public class Config
    {
        public string InputFilesDirectory { get; set; }

        public string FirstInputCsv { get; set; } = "SummaryPane.csv";

        public string SecondInputCsv { get; set; } = "FilterRecap.csv";

        public string ThirdInputCsv { get; set; } = "RecapShortSummary.csv";

        public string FourthInputCsv { get; set; } = "ExportList.csv";

        public string FirstInputImage { get; set; } = "Estimates Plus";

        public string EmailAccount { get; set; } = "annmarie@spendpoint.com";

        public string EmailAccountDisplayName { get; set; } = "Ann Marie";

        public bool IsLocalOrderCreationAllowed { get; set; } = true;

        public bool IsFirstUse { get; set; } = false;

        public int MemoryChunk { get; set; } = 4;

        public string User { get; set; }

        public int ClientNumber { get; set; } = 1901;

        public bool CompanyVersionControls { get; set; } = false;
    }
}
