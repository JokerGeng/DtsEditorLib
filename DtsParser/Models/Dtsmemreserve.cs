using System;
using System.Collections.Generic;
using System.Text;
using DtsParser.AST;

namespace DtsParser.Models
{
    public class Dtsmemreserve
    {
        public DtsValue Address { get; }
        public DtsValue Size { get; }

        public Dtsmemreserve(DtsValue address, DtsValue size)
        {
            this.Address = address;
            this.Size = size;
        }

        public override string ToString()
        {
            return $"/memreserve/ {Address.ToString()} {Size.ToString()};";
        }
    }
}
