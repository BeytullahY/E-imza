using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace E_imza.ImzaCS
{
    public class PdfRequestDTO
    {
        public string DonglePassword { get; set; }
        public byte[] pdfContent { get; set; }
        public double SignatureX;
        public double SignatureY;
        public double SignatureSizeX;
        public double SignatureSizeY;
        public int SignaturePage;
    }
}