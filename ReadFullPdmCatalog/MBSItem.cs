using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadFullPdmCatalog
{
    public class MBSItem
    {
        public Int64 FileSize { get; set; }
        public string FileMonth { get; set; }
        public string FileDay { get; set; }
        public string FileYear { get; set; }
        public string FileTime { get; set; }
        public DateTime FileDateTime { get; set; }
        public string FilePath { get; set; }
        public string ItemName { get; set; }
        public string ItemRev { get; set; }
        public string ItemExt { get; set; }
        public string ItemSht { get; set; }
        public bool HasRev { get; set; }
        public bool HasSht { get; set; }
        public bool HasExt { get; set; }
    }
}