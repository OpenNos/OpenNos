using System.Collections.Generic;

namespace OpenNos.PathFinder
{
    internal class MinHeap
    {
        #region Members

        private List<Node> array = new List<Node>();

        #endregion

        #region Properties

        public int Count
        {
            get
            {
                return array.Count;
            }
        }

        #endregion

        #region Methods

        public Node Pop()
        {
            Node ret = array[0];
            array[0] = array[array.Count - 1];
            array.RemoveAt(array.Count - 1);

            int c = 0;
            while (c < array.Count)
            {
                int min = c;
                if (2 * c + 1 < array.Count && array[2 * c + 1].CompareTo(array[min]) == -1)
                {
                    min = 2 * c + 1;
                }
                if (2 * c + 2 < array.Count && array[2 * c + 2].CompareTo(array[min]) == -1)
                {
                    min = 2 * c + 2;
                }

                if (min == c)
                {
                    break;
                }
                else
                {
                    Node tmp = array[c];
                    array[c] = array[min];
                    array[min] = tmp;
                    c = min;
                }
            }

            return ret;
        }

        public void Push(Node element)
        {
            array.Add(element);
            int c = array.Count - 1;
            int parent = (c - 1) >> 1;
            while (c > 0 && array[c].CompareTo(array[parent]) < 0)
            {
                Node tmp = array[c];
                array[c] = array[parent];
                array[parent] = tmp;
                c = parent;
                parent = (c - 1) >> 1;
            }
        }

        #endregion
    }
}