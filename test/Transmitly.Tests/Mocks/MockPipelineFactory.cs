using Transmitly.Pipeline.Configuration;

namespace Transmitly.Tests.Mocks
{
	sealed class MockPipelineFactory : BasePipelineFactory
	{
		public MockPipelineFactory(IEnumerable<IPipeline> pipelines) : base(pipelines) { }
	}
}
