using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgCom
{
    public class Tuple<T,U>
    {
        public T Item1;
        public U Item2;
        public Tuple(T item1, U item2)
        {
            this.Item1 = item1;
            this.Item2 = item2;
        }
    }
}
