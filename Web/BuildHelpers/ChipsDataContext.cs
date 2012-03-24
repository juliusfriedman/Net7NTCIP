using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChipsWeb
{
    class ChipsDataContext : IDisposable
    {
        internal Session GetUserSession(Guid loginToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
