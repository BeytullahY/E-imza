using E_imza.ImzaCS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace E_imza
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {
        private PdfSigner _pdfSigner;
        public WebService1()
        {
            _pdfSigner = new PdfSigner();
        }

        [WebMethod]
        public void DocumentPost(PdfRequestDTO requestDTO)
        {
            try
            {
                var stampted = _pdfSigner.SignPDF(requestDTO, requestDTO.pdfContent);
            }
            catch (Exception ex)
            {
                throw new Exception("Dosyanın imzalanması sırasında bir hata oluştu.");
            }
        }
    }
}
