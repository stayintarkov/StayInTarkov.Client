using JetBrains.Annotations;
using System;

namespace SIT.Tarkov.Core
{
    /// <summary>
    /// Code from: SPT-AKI
    /// https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.Reflection/Patching/Attributes.cs
    /// </summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class PatchPrefixAttribute : Attribute
    {
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class PatchPostfixAttribute : Attribute
    {
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class PatchTranspilerAttribute : Attribute
    {
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class PatchFinalizerAttribute : Attribute
    {
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class PatchILManipulatorAttribute : Attribute
    {
    }
}
