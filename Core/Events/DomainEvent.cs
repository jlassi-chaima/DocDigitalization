using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Events
{
    public abstract class DomainEvent : IDomainEvent
    {
        public DefaultIdType Id { get; }
        public DateTime CreationDate { get; }

        public IDictionary<string, object> MetaData { get; }

        protected DomainEvent()
        {
            Id = DefaultIdType.NewGuid();
            CreationDate = DateTime.UtcNow;
            MetaData = new Dictionary<string, object>();
        }
    }
}