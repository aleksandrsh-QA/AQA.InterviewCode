using Xunit;

namespace InvestCore.QA.Tests.Fixtures;

[CollectionDefinition("Api Integration Tests")]
public class SharedTestCollection : ICollectionFixture<InvestApiFixture>
{
}