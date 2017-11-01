namespace INFOIBV
{
    public class GraphNode
    {
        public int label;
        public GraphNode ParentNode = null;

        public GraphNode(int label)
        {
            this.label = label;
        }
    }
}