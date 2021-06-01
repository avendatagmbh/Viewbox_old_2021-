using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;

namespace ViewboxAdmin
{
    public class IoCResolver : IIoCResolver {
        public IoCResolver(IComponentContext context) { this.Context = context; }

        public IComponentContext Context { get; private set; }

        public T Resolve<T>() { return Context.Resolve<T>(); }
    }
}
