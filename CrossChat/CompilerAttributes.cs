namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public sealed class NullableAttribute : Attribute
    {
        public readonly byte[] NullableFlags;
        public NullableAttribute(byte flag) => NullableFlags = [flag]; // Fix for IDE0300
        public NullableAttribute(byte[] flags) => NullableFlags = flags;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Delegate)]
    public sealed class NullableContextAttribute(byte flag) : Attribute // Fix for IDE0290 (Primary Constructor)
    {
        public readonly byte Flag = flag;
    }
}