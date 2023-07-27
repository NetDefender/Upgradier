using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Upgradier.Core;

public interface ISourceProvider
{
    string Name { get; }

    Task<IEnumerable<Source>> GetSourcesAsync(CancellationToken cancellationToken);
}
