namespace Aeter.Ratio.Serialization
{
    public class EmptyGraphTraveller { }
    public class EmptyGraphTraveller<T> : EmptyGraphTraveller, IGraphTraveller<T>
    {
        public EmptyGraphTraveller() { }
        public void Travel(IWriteVisitor visitor, T graph)
        {
        }

        public void Travel(IReadVisitor visitor, T graph)
        {
        }

        public void Travel(IWriteVisitor visitor, object graph)
        {
        }

        public void Travel(IReadVisitor visitor, object graph)
        {
        }
    }
}
