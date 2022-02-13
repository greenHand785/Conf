using System;
using System.Collections.Generic;

namespace SilentOrbit.ProtocolBuffers
{
    class ProtoMessage : ProtoType, IComment
    {
        public override Wire WireType
        {
            get { return Wire.LengthDelimited; }
        }

        public string Comments { get; set; }

        public Dictionary<int, Field> Fields = new Dictionary<int, Field>();
        public Dictionary<string, ProtoMessage> Messages = new Dictionary<string, ProtoMessage>();
        public Dictionary<string, ProtoEnum> Enums = new Dictionary<string, ProtoEnum>();

        public string SerializerType
        {
            get
            {
                if (this.OptionExternal || this.OptionType == "interface")
                {
                    return CsType + "Serializer";
                }
                else
                {
                    return CsType;
                }
            }
        }

        public string FullSerializerType
        {
            get
            {
                if (this.OptionExternal || this.OptionType == "interface")
                {
                    return FullCsType + "Serializer";
                }
                else
                {
                    return FullCsType;
                }
            }
        }

        public ProtoMessage(ProtoMessage parent, string package)
            : base(parent, package)
        {
            this.OptionType = "class";
        }

        public override string ToString()
        {
            return "message " + FullProtoName;
        }

        public bool IsUsingBinaryWriter
        {
            get
            {
                foreach (Field f in Fields.Values)
                {
                    if (f.IsUsingBinaryWriter)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Return the buffer size of the largest field specified in the .proto file.
        /// </summary>
        public int MaxFieldBufferSize()
        {
            if (BufferSize > 0)
            {
                return BufferSize;
            }

            int size = 0;
            foreach (var f in Fields.Values)
            {
                size = Math.Max(size, f.BufferSizeScan());
            }

            return size;
        }

        /// <summary>
        /// If all fields are constant then this messag eis constant too
        /// </summary>
        public override int WireSize
        {
            get
            {
                int totalSize = 0;
                foreach (Field f in Fields.Values)
                {
                    if (f.ProtoType.WireSize < 0)
                    {
                        return -1;
                    }

                    totalSize += f.ProtoType.WireSize;
                }
                return totalSize;
            }
        }

        public override void MarkImported()
        {
            base.MarkImported();

            foreach (var m in Messages.Values)
            {
                m.MarkImported();
            }

            foreach (var e in Enums.Values)
            {
                e.MarkImported();
            }
        }
    }
}
