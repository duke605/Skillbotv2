using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillBotv2.Entities
{
    class ImgurResponse<T>
    {
        public T Data { get; set; }
        public int Status { get; set; }
        public bool Success { get; set; }
    }
}
