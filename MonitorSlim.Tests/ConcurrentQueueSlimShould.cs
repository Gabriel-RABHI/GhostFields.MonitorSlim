using MonitorSlim.Collections;

namespace MonitorSlim.Tests
{
    public class ConcurrentQueueSlimShould
    {
        [Fact]
        public void AddItems()
        {
            var list = new ConcurrentList<int>();

            list.Add(1);
            list.Add(5);
            list.Add(8);
            list.Add(12);

            Assert.Equal(4, list.Count);
            Assert.Equal(1, list.Values.ElementAt(0));
            Assert.Equal(5, list.Values.ElementAt(1));
            Assert.Equal(8, list.Values.ElementAt(2));
            Assert.Equal(12, list.Values.ElementAt(3));
        }

        [Fact]
        public void AddAndRemoveItemsByIndex()
        {
            var list = new ConcurrentList<int>();

            var a1 = list.Add(1);
            var a2 = list.Add(5);
            var a3 = list.Add(8);
            var a4 = list.Add(12);

            list.RemoveIndex(a3);

            Assert.Equal(3, list.Count);
            Assert.Equal(1, list.Values.ElementAt(0));
            Assert.Equal(5, list.Values.ElementAt(1));
            Assert.Equal(12, list.Values.ElementAt(2));

            list.RemoveIndex(a1);

            Assert.Equal(2, list.Count);
            Assert.Equal(5, list.Values.ElementAt(0));
            Assert.Equal(12, list.Values.ElementAt(1));

            list.RemoveIndex(a2);

            Assert.Equal(1, list.Count);
            Assert.Equal(12, list.Values.ElementAt(0));

            list.RemoveIndex(a4);

            Assert.Equal(0, list.Count);
        }

        [Fact]
        public void AddAndRemoveItemsByValue()
        {
            var list = new ConcurrentList<int>();

            var a1 = list.Add(1);
            var a2 = list.Add(5);
            var a3 = list.Add(8);
            var a4 = list.Add(12);

            list.Remove(8);

            Assert.Equal(3, list.Count);
            Assert.Equal(1, list.Values.ElementAt(0));
            Assert.Equal(5, list.Values.ElementAt(1));
            Assert.Equal(12, list.Values.ElementAt(2));

            list.Remove(1);

            Assert.Equal(2, list.Count);
            Assert.Equal(5, list.Values.ElementAt(0));
            Assert.Equal(12, list.Values.ElementAt(1));

            list.Remove(5);

            Assert.Equal(1, list.Count);
            Assert.Equal(12, list.Values.ElementAt(0));

            list.Remove(12);

            Assert.Equal(0, list.Count);
        }

        [Fact]
        public void AddRemoveAndReinsertItems()
        {
            var list = new ConcurrentList<int>();

            var a1 = list.Add(1);
            var a2 = list.Add(5);
            var a3 = list.Add(8);
            var a4 = list.Add(12);

            list.RemoveIndex(a3);

            Assert.Equal(3, list.Count);
            Assert.Equal(1, list.Values.ElementAt(0));
            Assert.Equal(5, list.Values.ElementAt(1));
            Assert.Equal(12, list.Values.ElementAt(2));

            list.Add(18);

            Assert.Equal(4, list.Count);
            Assert.Equal(1, list.Values.ElementAt(0));
            Assert.Equal(5, list.Values.ElementAt(1));
            Assert.Equal(18, list.Values.ElementAt(2));
            Assert.Equal(12, list.Values.ElementAt(3));
        }
    }
}