using System;
using System.Collections.Generic;
using System.Text;

namespace MenuViews
{
    public sealed class MenuViewCounter
    {
        public int Views;

        public void RegisterView()
        {
            Interlocked.Increment(ref Views);
        }
    }
}
