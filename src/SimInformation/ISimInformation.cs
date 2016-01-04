using System.Collections.Generic;

namespace SimInformation
{
    public interface ISimInformation
    {
        IReadOnlyList<SimCard> GetSimCards();
    }
}