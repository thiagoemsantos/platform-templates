using DotNetApi.Application.DTOs;
using DotNetApi.Application.Services;
using Xunit;

namespace DotNetApi.Tests
{
    public class GreetingLinksAssemblerTests
    {
        [Fact]
        public void BuildLinks_ReturnsSelfAndCreateAlways()
        {
            var links = GreetingLinksAssembler.BuildLinks(1, 10, 100);
            Assert.Contains(links, l => l.Rel == "self");
            Assert.Contains(links, l => l.Rel == "create");
        }

        [Fact]
        public void BuildLinks_ReturnsPrev_WhenPageGreaterThanOne()
        {
            var links = GreetingLinksAssembler.BuildLinks(2, 10, 100);
            Assert.Contains(links, l => l.Rel == "prev");
        }

        [Fact]
        public void BuildLinks_ReturnsNext_WhenNotLastPage()
        {
            var links = GreetingLinksAssembler.BuildLinks(1, 10, 100);
            Assert.Contains(links, l => l.Rel == "next");
        }

        [Fact]
        public void BuildLinks_DoesNotReturnNext_WhenLastPage()
        {
            var links = GreetingLinksAssembler.BuildLinks(10, 10, 100);
            Assert.DoesNotContain(links, l => l.Rel == "next");
        }

        [Fact]
        public void BuildLinks_DoesNotReturnPrev_WhenFirstPage()
        {
            var links = GreetingLinksAssembler.BuildLinks(1, 10, 100);
            Assert.DoesNotContain(links, l => l.Rel == "prev");
        }

        [Fact]
        public void BuildLinks_HandlesZeroItems()
        {
            var links = GreetingLinksAssembler.BuildLinks(1, 10, 0);
            Assert.Contains(links, l => l.Rel == "self");
            Assert.Contains(links, l => l.Rel == "create");
            Assert.DoesNotContain(links, l => l.Rel == "next");
            Assert.DoesNotContain(links, l => l.Rel == "prev");
        }

        [Fact]
        public void BuildLinks_PageZero_ReturnsSelfAndCreateOnly()
        {
            var links = GreetingLinksAssembler.BuildLinks(0, 10, 100);
            Assert.Contains(links, l => l.Rel == "self");
            Assert.Contains(links, l => l.Rel == "create");
            Assert.DoesNotContain(links, l => l.Rel == "prev");
            Assert.Contains(links, l => l.Rel == "next");
        }

        [Fact]
        public void BuildLinks_NegativePage_ReturnsSelfAndCreateOnly()
        {
            var links = GreetingLinksAssembler.BuildLinks(-1, 10, 100);
            Assert.Contains(links, l => l.Rel == "self");
            Assert.Contains(links, l => l.Rel == "create");
            Assert.DoesNotContain(links, l => l.Rel == "prev");
            Assert.Contains(links, l => l.Rel == "next");
        }

        [Fact]
        public void BuildLinks_PageSizeZero_ReturnsSelfAndCreateOnly()
        {
            var links = GreetingLinksAssembler.BuildLinks(1, 0, 100);
            Assert.Contains(links, l => l.Rel == "self");
            Assert.Contains(links, l => l.Rel == "create");
            Assert.DoesNotContain(links, l => l.Rel == "next");
            Assert.DoesNotContain(links, l => l.Rel == "prev");
        }

        [Fact]
        public void BuildLinks_NegativeTotalItems_ReturnsSelfAndCreateOnly()
        {
            var links = GreetingLinksAssembler.BuildLinks(1, 10, -100);
            Assert.Contains(links, l => l.Rel == "self");
            Assert.Contains(links, l => l.Rel == "create");
            Assert.DoesNotContain(links, l => l.Rel == "next");
            Assert.DoesNotContain(links, l => l.Rel == "prev");
        }

        [Fact]
        public void BuildLinks_LargeVolume_ReturnsNextAndPrev()
        {
            var links = GreetingLinksAssembler.BuildLinks(50, 10, 1000);
            Assert.Contains(links, l => l.Rel == "self");
            Assert.Contains(links, l => l.Rel == "create");
            Assert.Contains(links, l => l.Rel == "next");
            Assert.Contains(links, l => l.Rel == "prev");
        }
    }
}
