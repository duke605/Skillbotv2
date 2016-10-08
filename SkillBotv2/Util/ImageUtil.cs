﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using SkillBotv2.Entities;
using SkillBotv2.Entities.Imgur;
using unirest_net.http;

namespace SkillBotv2.Util
{
    class ImageUtil
    {
        /// <summary>
        /// Converts a string into an image
        /// </summary>
        /// <param name="text">The text to be converted into an image</param>
        /// <returns>The text as an image</returns>
        public static Image ToImage(string text)
        {
            var font = new Font("Consolas", 10F);
            var textColor = Color.FromArgb(0x83, 0x94, 0x96);
            var backColor = Color.FromArgb(0x2e, 0x31, 0x36);

            //first, create a dummy bitmap just to get a graphics object
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            //measure the string to see how big the image needs to be
            SizeF textSize = drawing.MeasureString(text, font);

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            img = new Bitmap((int)textSize.Width, (int)textSize.Height);

            drawing = Graphics.FromImage(img);

            //paint the background
            drawing.Clear(backColor);

            //create a brush for the text
            Brush textBrush = new SolidBrush(textColor);

            drawing.DrawString(text, font, textBrush, 0, 0);

            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            return img;
        }

        /// <summary>
        /// Turns an image into a byte array
        /// </summary>
        /// <param name="img">The image to turn into a byte array</param>
        /// <returns>The image as a byte[]</returns>
        public static byte[] ImageToByteArray(Image img)
        {
            using (var ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Uploads an image to imgur
        /// </summary>
        /// <param name="img">The image to be uploaded</param>
        /// <returns>The link to the image</returns>
        public static async Task<string> PostToImgur(byte[] buffer)
        {
            var r = Unirest.post("https://api.imgur.com/3/image")
                .header("authorization", $"Client-ID {Secret.ImgurClientId}")
                .field("image", buffer)
                .field("type", "file")
                .asJsonAsync<ImgurResponse<ImageDetails>>().Result;

            if (!r.Body.Success)
                throw new Exception("Error when uploading imgur.");

            return r.Body.Data.Link;
        }

        /// <summary>
        /// Uploads an image to imgur
        /// </summary>
        /// <param name="img">The image to be uploaded</param>
        /// <returns>The link to the image</returns>
        public static async Task<string> PostToImgur(Image img)
            => await PostToImgur(ImageToByteArray(img));
    }
}
