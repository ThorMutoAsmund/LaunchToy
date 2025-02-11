using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaunchToy
{
    public class ListViewItemContainer<T>
    {
        public T Value { get; }
        public string Label { get; }
        public ListViewItemContainer(T value, string label)
        {
            this.Value = value;
            this.Label = label;
        }

        public override string ToString()
        {
            return this.Label;
        }
    }
}
