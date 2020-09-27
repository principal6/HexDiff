using System.Collections.Generic;

namespace TreeStructure
{
    public class Tree<T>
    {
        public class Node
        {
            public T            Data;
            public Node         Parent;
            public List<Node>   Children;

            public Node appendChild(T data)
            {
                Node childNode = new Node();
                childNode.Data = data;
                childNode.Parent = this;

                if (Children == null)
                {
                    Children = new List<Node>();
                }
                Children.Add(childNode);

                return childNode;
            }
        }

        private Node    _rootNode;

        public bool makeRootNode(T data)
        {
            _rootNode = new Node();
            _rootNode.Data = data;
            return true;
        }

        public ref Node getRootNode()
        {
            return ref _rootNode;
        }

        private Node searchNodeInternal(Node currNode, string key)
        {
            if (currNode == null)
            {
                return null;
            }

            if (currNode.Data.Equals(key))
            {
                return currNode;
            }

            if (currNode.Children != null)
            {
                foreach (Node childNode in currNode.Children)
                {
                    Node found = searchNodeInternal(childNode, key);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }

        public Node findNode(string key)
        {
            return searchNodeInternal(_rootNode, key);
        }
    }
}
