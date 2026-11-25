using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAFramework.Entity
{
    public sealed partial class EntityManager : IEntityManager
    {
        private enum EntityStatus : byte
        {
            Unknown = 0,
            WillInit,
            Inited,
            WillShow,
            Showed,
            WillHide,
            Hidden,
            WillRecycle,
            Recycled
        }
    }
}
