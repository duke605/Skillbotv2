using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillBotv2.Entities.Imgur
{
    class ImageDetails
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public long Datetime { get; set; }
        public string Type { get; set; }
        public bool Animated { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Size { get; set; }
        public int Views { get; set; }
        public int Bandwidth { get; set; }
        public bool Favorite { get; set; }
        public int AccountId { get; set; }
        public bool IsAd { get; set; }
        public bool InGallery { get; set; }
        public string Deletehash { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
    }
}
