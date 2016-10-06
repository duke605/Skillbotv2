using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillBotv2.Exceptions
{
    class ControlledException : Exception
    {
        public ControlledException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}
