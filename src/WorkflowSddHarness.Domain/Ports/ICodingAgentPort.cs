using System.Threading;
using System.Threading.Tasks;
using WorkflowSddHarness.Domain.Model;

namespace WorkflowSddHarness.Domain.Ports;

public interface ICodingAgentPort
{
    Task<CodingAgentResult> RunAsync(CodingAgentRequest request, CancellationToken cancellationToken = default);
}
