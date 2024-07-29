using E_imza.ImzaCS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace E_imza
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

       //private void CallHelloWorldMethod(PdfRequestDTO pdfRequestDTO)
       // {
       //     try
       //     {
       //         // Web servisin URL'si
       //         string serviceUrl = "https://localhost:44397/EimzaWS.asmx?op=HelloWorld";

       //         // JSON formatında dönüştür
       //         string jsonData = JsonConvert.SerializeObject(pdfRequestDTO);

       //         // WebClient oluştur
       //         using (WebClient client = new WebClient())
       //         {
       //             // İsteği doğru formatta ayarla
       //             client.Headers[HttpRequestHeader.ContentType] = "application/json";
       //             client.Encoding = System.Text.Encoding.UTF8;

       //             // Web servis metoduna HTTP POST isteği gönder
       //             string response = client.UploadString(serviceUrl, "HelloWorld", jsonData);

       //             // Cevabı ekrana yazdır
       //             Response.Write("HelloWorld metodu çağrısı başarılı. Cevap: " + response);
       //         }
       //     }
       //     catch (Exception ex)
       //     {
       //         Response.Write("HelloWorld metodu çağrısı sırasında hata oluştu: " + ex.Message);
       //     }
       // }
        protected void btnUpload_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] fileBytes;
                using (Stream stream = fileInput.PostedFile.InputStream)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        fileBytes = ms.ToArray();
                    }
                }
                PdfRequestDTO requestDTO = new PdfRequestDTO
                {
                    pdfContent = fileBytes
                };

                var api = new WebService1();
                api.DocumentPost(requestDTO);

                //CallHelloWorldMethod(requestDTO);


                // İşlem başarılı mesajını kullanıcıya göster
                Response.Write("Dosya yükleme başarılı!");
            }
            catch (Exception ex)
            {
                Response.Write("Hata: " + ex.Message);
            }

        }
    }
}
