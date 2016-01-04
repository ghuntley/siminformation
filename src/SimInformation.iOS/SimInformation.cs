using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimInformation.iOS
{
    public class SimInformation : ISimInformation
    {
        public IReadOnlyList<SimCard> GetSimCards()
        {
            throw new NotImplementedException();
        }
    }
}
