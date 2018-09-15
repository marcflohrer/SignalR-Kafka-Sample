using System;
namespace StockProcessor.Repositories.Core {
    // source: https://github.com/jhewlett/ValueObject
    [AttributeUsage (AttributeTargets.Property | AttributeTargets.Field)]
    public class IgnoreMemberAttribute : Attribute { }
}