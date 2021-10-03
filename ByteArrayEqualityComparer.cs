using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CatalogSyncher
{
    public class ByteArrayEqualityComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] x, byte[] y)
        {
            if(ReferenceEquals(x, y))
            {
                return true;
            }
            if(x == null || y == null)
            {
                return false;
            }
            if(x.Length != y.Length)
            {
                return false;
            }
            for (int i = 0; i < x.Length; i++)
            {
                if(x[i] != y[i])
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode([DisallowNull] byte[] obj)
        {
            int hash = 17;
            //пишу unchecked, чтобы не было исключения при переполнении
            unchecked
            {
                foreach (var item in obj)
                {
                    hash = hash*31 + item.GetHashCode();
                }
            }
            return hash;
        }
    }
}