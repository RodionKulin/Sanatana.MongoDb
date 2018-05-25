using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Operations = MongoDB.Driver.Core.Operations;

namespace Sanatana.MongoDb
{
    internal static class CursorTypeExtensions
    {
        public static Operations.CursorType ToCore(this CursorType cursorType)
        {
            switch (cursorType)
            {
                case CursorType.NonTailable:
                    return Operations.CursorType.NonTailable;
                case CursorType.Tailable:
                    return Operations.CursorType.Tailable;
                case CursorType.TailableAwait:
                    return Operations.CursorType.TailableAwait;
                default:
                    throw new ArgumentException("Unrecognized CursorType.", "cursorType");
            }
        }
    }
}
