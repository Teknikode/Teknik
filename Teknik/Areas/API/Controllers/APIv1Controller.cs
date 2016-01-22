using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Upload;
using Teknik.Controllers;
using Teknik.Helpers;
using Teknik.Models;

namespace Teknik.Areas.API.Controllers
{
    public class APIv1Controller : DefaultController
    {
        private TeknikEntities db = new TeknikEntities();

        [AllowAnonymous]
        public ActionResult Index()
        {
            return Redirect(Url.SubRouteUrl("help", "Help.Topic", new { topic = "API" }));
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Upload(HttpPostedFileWrapper file, string contentType = null, bool encrypt = false, bool saveKey = false, string key = null, int keySize = 0, string iv = null, int blockSize = 0, bool genDeletionKey = false)
        {
            if (file != null)
            {
                if (file.ContentLength <= Config.UploadConfig.MaxUploadSize)
                {
                    // convert file to bytes
                    byte[] fileData = null;
                    int contentLength = file.ContentLength;
                    using (var binaryReader = new BinaryReader(file.InputStream))
                    {
                        fileData = binaryReader.ReadBytes(file.ContentLength);
                    }

                    // Need to grab the contentType if it's empty
                    if (string.IsNullOrEmpty(contentType))
                    {
                        contentType = (string.IsNullOrEmpty(file.ContentType)) ? "application/octet-stream" : file.ContentType;
                    }

                    // Initialize the key size and block size if empty
                    if (keySize <= 0)
                        keySize = Config.UploadConfig.KeySize;
                    if (blockSize <= 0)
                        blockSize = Config.UploadConfig.BlockSize;

                    byte[] data = null;
                    // If they want us to encrypt the file first, do that here
                    if (encrypt)
                    {
                        // Generate key and iv if empty
                        if (string.IsNullOrEmpty(key))
                        {
                            key = Utility.RandomString(keySize);
                        }
                        if (string.IsNullOrEmpty(iv))
                        {
                            iv = Utility.RandomString(blockSize);
                        }

                        data = AES.Encrypt(fileData, key, iv);
                        if (data == null || data.Length <= 0)
                        {
                            return Json(new { error = "Unable to encrypt file" });
                        }
                    }

                    // Save the file data
                    Upload.Models.Upload upload = Uploader.SaveFile((encrypt) ? data : fileData, contentType, contentLength, iv, key, keySize, blockSize);

                    if (upload != null)
                    {
                        // Save the key to the DB if they wanted it to be
                        if (saveKey)
                        {
                            upload.Key = key;
                            db.Entry(upload).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        // Generate delete key if asked to
                        if (genDeletionKey)
                        {
                            string delKey = Utility.RandomString(Config.UploadConfig.DeleteKeyLength);
                            upload.DeleteKey = delKey;
                            db.Entry(upload).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        // Pull all the information together 
                        var returnData = new
                        {
                            url = Url.SubRouteUrl("upload", "Upload.Download", new { file = upload.Url }),
                            fileName = upload.Url,
                            contentType = contentType,
                            contentLength = contentLength,
                            key = key,
                            keySize = keySize,
                            iv = iv,
                            blockSize = blockSize,
                            deletionKey = upload.DeleteKey

                        };
                        return Json(new { result = returnData });
                    }
                    return Json(new { error = "Unable to save file" });
                }
                else
                {
                    return Json(new { error = "File Too Large" });
                }
            }

            return Json(new { error = "Invalid Upload Request" });
        }
    }
}