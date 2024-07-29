using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using System.Configuration;
//using iTextSharp.Org.BouncyCastle.X509;
//using X509Certificate = ITextSharp.Org.BouncyCastle.X509.X509Certificate;
using E_imza.ImzaCS;
using tr.gov.tubitak.uekae.esya.api.common.util;
using tr.gov.tubitak.uekae.esya.api.certificate.validation.policy;
using tr.gov.tubitak.uekae.esya.api.certificate.validation;
using tr.gov.tubitak.uekae.esya.api.certificate.validation.check.certificate;
using tr.gov.tubitak.uekae.esya.api.xmlsignature;
using log4net;
using System.Reflection;
using System.Drawing;
using tr.gov.tubitak.uekae.esya.api.crypto.util;
using tr.gov.tubitak.uekae.esya.api.crypto.alg;
using tr.gov.tubitak.uekae.esya.api.infra.tsclient;
using tr.gov.tubitak.uekae.esya.api.asn.pkixtsp;
using tr.gov.tubitak.uekae.esya.api.cmssignature.signature;
using tr.gov.tubitak.uekae.esya.api.cmssignature;
using tr.gov.tubitak.uekae.esya.api.cmssignature.attribute;
using tr.gov.tubitak.uekae.esya.asn.util;
using iTextSharp.text.pdf.security;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace E_imza.ImzaCS
{
    public class PdfSigner
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public PdfSigner()
        {
            try
            {
                LicenseUtil.setLicenseXml(new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lisans.xml"), FileMode.Open, FileAccess.Read));
                DateTime expirationDate = LicenseUtil.getExpirationDate();
                Console.WriteLine("License expiration date : " + expirationDate.ToShortDateString());

            }
            catch (Exception e)
            {

                logger.Error(e);
            }
            //LicenseUtil.setLicenseXml(new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lisans.xml"), FileMode.Open, FileAccess.Read));

        }
        static public ICrlClient crl;
        static public List<ICrlClient> crlList;
        static public OcspClientBouncyCastle ocsp;
        private static readonly string policyFile;

        private static System.Object lockSign = new System.Object();
        private static System.Object lockToken = new System.Object();
        private X509Certificate2[] generateCertificateChain(X509Certificate2 signingCertificate)
        {
            X509Chain Xchain = new X509Chain();
            Xchain.ChainPolicy.ExtraStore.Add(signingCertificate);
            Xchain.Build(signingCertificate); // Whole chain!
            X509Certificate2[] chain = new X509Certificate2[Xchain.ChainElements.Count];
            int index = 0;
            foreach (X509ChainElement element in Xchain.ChainElements)
            {
                chain[index++] = element.Certificate;
            }
            return chain;
        }
        /// <summary>
        /// PDF imzalar.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="PDFContent"></param>
        /// <returns></returns>
        public byte[] SignPDF(PdfRequestDTO request, byte[] PDFContent)
        {
            //if (PDFContent == null || request == null)
            //{
            //    return null;
            //}
            X509Certificate2 signingCertificate;
            IExternalSignature externalSignature;
            this.SelectSignature(request, out signingCertificate, out externalSignature);
            X509Certificate2[] chain = generateCertificateChain(signingCertificate);
            ICollection<X509Certificate> Bouncychain = (ICollection<X509Certificate>)chainToBouncyCastle(chain);
            string CertName = chain[0].GetNameInfo(X509NameType.SimpleName, false);

            ocsp = new OcspClientBouncyCastle();
            crl = new CrlClientOnline((ICollection<Org.BouncyCastle.X509.X509Certificate>)Bouncychain);

            PdfReader pdfReader = new PdfReader(PDFContent);
            MemoryStream stream = new MemoryStream();
            PdfStamper pdfStamper = PdfStamper.CreateSignature(pdfReader, stream, '\0', "/Documents", true);

            // get zaman damgası
            byte[] data = Encoding.ASCII.GetBytes("test");
            byte[] digest = DigestUtil.digest(DigestAlg.SHA256, data);
            //TSClient tsClient = new TSClient();
            //TSSettings settings = new TSSettings("http://ts384.e-guven.com", 6734, "Me3SDY4R", DigestAlg.SHA256);
            //ETimeStampResponse response = tsClient.timestamp(digest, settings);
            //byte[] tsBytes = response.getContentInfo().getEncoded();
            PdfSignatureAppearance signatureAppearance = pdfStamper.SignatureAppearance;
            AcroFields pdfFormFields = pdfStamper.AcroFields;
            #region MyRegion
            //ITSAClient tsa = new TSAClientBouncyCastle("http://ts384.e-guven.com", "6734", "Me3SDY4R", 8192, "SHA-256");
            //ITSAClient tsa = new TSAClientBouncyCastle("http://ts384.e-guven.com", "6734", "Me3SDY4R", 8192, "SHA-256");
            //tsa.GetTimeStampToken(tsBytes);
            //var url = "http://ts384.e-guven.com";
            //var tsc = new TSAClientBouncyCastle(url, "6734", "Me3SDY4R", 4096, "SHA-512");
            #endregion

            // ITSAClient tSAClient = new TSAClientBouncyCastle("http://ts384.e-guven.com", "6734", "Me3SDY4R");
            //  var timeStamp = tSAClient.GetTimeStampToken(digest);
            crlList = new List<ICrlClient>();
            crlList.Add(crl);

            lock (lockSign)
            {


                iTextSharp.text.Rectangle rectangle = new iTextSharp.text.Rectangle((float)request.SignatureX, (float)request.SignatureY, (float)request.SignatureX + (float)request.SignatureSizeX, (float)request.SignatureY + (float)request.SignatureSizeY);

                signatureAppearance.Layer2Font = FontFactory.GetFont(BaseFont.TIMES_ROMAN, BaseFont.CP1257, 7f);
                signatureAppearance.Layer2Font.Color = BaseColor.BLACK;
                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(@"\LGO-COLOR.png");
                rectangle.Border = iTextSharp.text.Rectangle.BOX;
                rectangle.BorderWidth = 4f;
                signatureAppearance.Image = iTextSharp.text.Image.GetInstance(@"\LGO-COLOR.png");
                string imageURL = @"\LGO-COLOR.png";
                iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imageURL);
                // Resize image depend upon your need
                jpg.ScaleToFit(140f, 120f);
                //Give space before image
                jpg.SpacingBefore = 10f;
                //Give some space after the image
                jpg.SpacingAfter = 1f;
                signatureAppearance.Layer2Text = "Bu belge " + signatureAppearance.SignDate + " tarihinde " + CertName + " tarafından E-Imza ile imzalanmıştır.";
                // signatureAppearance.SignatureRenderingMode = PdfSignatureAppearance.RenderingMode.DESCRIPTION;
                signatureAppearance.SetVisibleSignature(rectangle, 1, Guid.NewGuid().ToString().Split('-')[0] + "-" + CertName + " E-Imza");
                //LtvTimestamp.Timestamp(signatureAppearance, tSAClient, "KamuSM");
                MakeSignature.SignDetached(signatureAppearance, externalSignature, (ICollection<Org.BouncyCastle.X509.X509Certificate>)Bouncychain, crlList, ocsp, null, 0, CryptoStandard.CMS);

                //signatureAppearance.SetVisibleSignature(new ITextSharp.iTextSharp.text.Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height), 1, null);
                //ITextSharp.iTextSharp.text.pdf.security.

            }
            return stream.ToArray();
        }

        public byte[] SetTimeStampSignature(byte[] pdf)
        {
            PdfReader pdfReader = new PdfReader(pdf);
            MemoryStream stream = new MemoryStream();
            PdfStamper pdfStamper = PdfStamper.CreateSignature(pdfReader, stream, '\0', "C:/Users/byilmaz/Documents", true);

            PdfSignatureAppearance signatureAppearance = pdfStamper.SignatureAppearance;

            ITSAClient tSAClient = new TSAClientBouncyCastle("http://ts384.e-guven.com", "6734", "Me3SDY4R");
            LtvTimestamp.Timestamp(signatureAppearance, tSAClient, "KamuSM");

            return stream.ToArray();
        }

        private static ICollection<Org.BouncyCastle.X509.X509Certificate> chainToBouncyCastle(X509Certificate2[] chain)
        {
            Org.BouncyCastle.X509.X509CertificateParser cp = new Org.BouncyCastle.X509.X509CertificateParser();

            ICollection<Org.BouncyCastle.X509.X509Certificate> Bouncychain = new List<Org.BouncyCastle.X509.X509Certificate>();
            int index = 0;
            foreach (var item in chain)
            {
                Bouncychain.Add(cp.ReadCertificate(item.RawData));
            }
            return Bouncychain;

        }
        private void SelectSignature(
           PdfRequestDTO request,
           out X509Certificate2 CERTIFICATE,
           out IExternalSignature externalSignature)
        {
            try
            {
                SmartCardManager smartCardManager = SmartCardManager.getInstance();
                var smartCardCertificate = smartCardManager.getSignatureCertificate(false, false);
                var signer = smartCardManager.getSigner(request.DonglePassword, smartCardCertificate);
                CERTIFICATE = smartCardCertificate.asX509Certificate2();
                externalSignature = new SmartCardSignature(signer, CERTIFICATE, "SHA-256");

            }
            catch (Exception ex)
            {
                CERTIFICATE = null;
                externalSignature = null;
                //MessageBox.Show(ex.Message);
            }

        }
    }
}