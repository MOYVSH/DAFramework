using System;
using DAFramework;
using DAFramework.ReferencePool;

namespace DAFramework.Procedure
{
    public class ProcedureNodeAddedEventArgs : IReference
    {
        public Type AddedProcedureNodeType { get; private set; }
        public string AddedProcedureNodeName { get; private set; }
        public void Clear()
        {
            AddedProcedureNodeType = null;
            AddedProcedureNodeName = string.Empty;
        }
        public static ProcedureNodeAddedEventArgs Create(Type type)
        {
            var eventArgs = DA.Reference.Acquire<ProcedureNodeAddedEventArgs>();
            eventArgs.AddedProcedureNodeType= type;
            if (type != null)
                eventArgs.AddedProcedureNodeName= type.Name;
            return eventArgs;
        }
        public static void Release(ProcedureNodeAddedEventArgs eventArgs)
        {
            DA.Reference.Release(eventArgs);
        }
    }
}
