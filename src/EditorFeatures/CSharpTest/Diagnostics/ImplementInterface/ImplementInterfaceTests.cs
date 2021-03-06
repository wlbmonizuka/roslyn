// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeStyle;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.CodeFixes.ImplementInterface;
using Microsoft.CodeAnalysis.CSharp.CodeStyle;
using Microsoft.CodeAnalysis.CSharp.Test.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Options;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Diagnostics.ImplementInterface
{
    public partial class ImplementInterfaceTests : AbstractCSharpDiagnosticProviderBasedUserDiagnosticTest
    {
        internal override Tuple<DiagnosticAnalyzer, CodeFixProvider> CreateDiagnosticProviderAndFixer(Workspace workspace)
        {
            return new Tuple<DiagnosticAnalyzer, CodeFixProvider>(null, new ImplementInterfaceCodeFixProvider());
        }

        private static readonly Dictionary<OptionKey, object> AllOptionsOff =
            new Dictionary<OptionKey, object>
            {
                {  CSharpCodeStyleOptions.PreferExpressionBodiedConstructors, CodeStyleOptions.FalseWithNoneEnforcement },
                {  CSharpCodeStyleOptions.PreferExpressionBodiedMethods, CodeStyleOptions.FalseWithNoneEnforcement },
                {  CSharpCodeStyleOptions.PreferExpressionBodiedProperties, CodeStyleOptions.FalseWithNoneEnforcement },
                {  CSharpCodeStyleOptions.PreferExpressionBodiedIndexers, CodeStyleOptions.FalseWithNoneEnforcement },
                {  CSharpCodeStyleOptions.PreferExpressionBodiedAccessors, CodeStyleOptions.FalseWithNoneEnforcement },
                {  CSharpCodeStyleOptions.PreferExpressionBodiedOperators, CodeStyleOptions.FalseWithNoneEnforcement }
            };

        private static readonly Dictionary<OptionKey, object> AllOptionsOn =
            new Dictionary<OptionKey, object>
            {
                {  CSharpCodeStyleOptions.PreferExpressionBodiedConstructors, CodeStyleOptions.TrueWithNoneEnforcement },
                {  CSharpCodeStyleOptions.PreferExpressionBodiedMethods, CodeStyleOptions.TrueWithNoneEnforcement },
                {  CSharpCodeStyleOptions.PreferExpressionBodiedProperties, CodeStyleOptions.TrueWithNoneEnforcement },
                {  CSharpCodeStyleOptions.PreferExpressionBodiedIndexers, CodeStyleOptions.TrueWithNoneEnforcement },
                {  CSharpCodeStyleOptions.PreferExpressionBodiedAccessors, CodeStyleOptions.TrueWithNoneEnforcement },
                {  CSharpCodeStyleOptions.PreferExpressionBodiedOperators, CodeStyleOptions.TrueWithNoneEnforcement }
            };

        private static readonly Dictionary<OptionKey, object> AccessorOptionsOn =
            new Dictionary<OptionKey, object>
            {
                {  CSharpCodeStyleOptions.PreferExpressionBodiedConstructors, CodeStyleOptions.FalseWithNoneEnforcement },
                {  CSharpCodeStyleOptions.PreferExpressionBodiedMethods, CodeStyleOptions.FalseWithNoneEnforcement },
                {  CSharpCodeStyleOptions.PreferExpressionBodiedProperties, CodeStyleOptions.FalseWithNoneEnforcement },
                {  CSharpCodeStyleOptions.PreferExpressionBodiedIndexers, CodeStyleOptions.FalseWithNoneEnforcement },
                {  CSharpCodeStyleOptions.PreferExpressionBodiedAccessors, CodeStyleOptions.TrueWithNoneEnforcement },
                {  CSharpCodeStyleOptions.PreferExpressionBodiedOperators, CodeStyleOptions.FalseWithNoneEnforcement }
            };

        internal async Task TestWithAllCodeStyleOptionsOffAsync(
            string initialMarkup, string expectedMarkup,
            int index = 0, bool compareTokens = true,
            ParseOptions parseOptions = null,
            bool withScriptOption = false)
        {
            await TestAsync(initialMarkup, expectedMarkup, parseOptions, null,
                index, compareTokens, options: AllOptionsOff, withScriptOption: withScriptOption);
        }

        internal async Task TestWithAllCodeStyleOptionsOnAsync(
            string initialMarkup, string expectedMarkup,
            int index = 0, bool compareTokens = true,
            ParseOptions parseOptions = null,
            bool withScriptOption = false)
        {
            await TestAsync(initialMarkup, expectedMarkup, parseOptions, null,
                index, compareTokens, options: AllOptionsOn, withScriptOption: withScriptOption);
        }

        internal async Task TestWithAccessorCodeStyleOptionsOnAsync(
            string initialMarkup, string expectedMarkup,
            int index = 0, bool compareTokens = true,
            ParseOptions parseOptions = null,
            bool withScriptOption = false)
        {
            await TestAsync(initialMarkup, expectedMarkup, parseOptions, null,
                index, compareTokens, options: AccessorOptionsOn, withScriptOption: withScriptOption);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestMethod()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IInterface
{
    void Method1();
}

class Class : [|IInterface|]
{
}",
@"using System;

interface IInterface
{
    void Method1();
}

class Class : IInterface
{
    public void Method1()
    {
        throw new NotImplementedException();
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestExpressionBodiedMethod1()
        {
            await TestWithAllCodeStyleOptionsOnAsync(
@"interface IInterface
{
    void Method1();
}

class Class : [|IInterface|]
{
}",
@"using System;

interface IInterface
{
    void Method1();
}

class Class : IInterface
{
    public void Method1() => throw new NotImplementedException();
}");
        }

        private static readonly string s_tupleElementNamesAttribute =
@"namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Event )]
    public sealed class TupleElementNamesAttribute : Attribute { }
}
";

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface), Test.Utilities.CompilerTrait(Test.Utilities.CompilerFeature.Tuples)]
        public async Task TupleWithNamesInMethod()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IInterface
{
    [return: System.Runtime.CompilerServices.TupleElementNames(new[] { ""a"", ""b"" })]
    (int a, int b)[] Method1((int c, string) x);
}

class Class : [|IInterface|]
{
}" + s_tupleElementNamesAttribute,
@"using System;

interface IInterface
{
    [return: System.Runtime.CompilerServices.TupleElementNames(new[] { ""a"", ""b"" })]
    (int a, int b)[] Method1((int c, string) x);
}

class Class : IInterface
{
    public (int a, int b)[] Method1((int c, string) x)
    {
        throw new NotImplementedException();
    }
}" + s_tupleElementNamesAttribute);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface), Test.Utilities.CompilerTrait(Test.Utilities.CompilerFeature.Tuples)]
        public async Task TupleWithNamesInMethod_Explicitly()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IInterface
{
    [return: System.Runtime.CompilerServices.TupleElementNames(new[] { ""a"", ""b"" })]
    (int a, int b)[] Method1((int c, string) x);
}

class Class : [|IInterface|]
{
}" + s_tupleElementNamesAttribute,
@"using System;

interface IInterface
{
    [return: System.Runtime.CompilerServices.TupleElementNames(new[] { ""a"", ""b"" })]
    (int a, int b)[] Method1((int c, string) x);
}

class Class : IInterface
{
    (int a, int b)[] IInterface.Method1((int c, string) x)
    {
        throw new NotImplementedException();
    }
}" + s_tupleElementNamesAttribute,
index: 1);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface), Test.Utilities.CompilerTrait(Test.Utilities.CompilerFeature.Tuples)]
        public async Task TupleWithNamesInProperty()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IInterface
{
    [System.Runtime.CompilerServices.TupleElementNames(new[] { ""a"", ""b"" })]
    (int a, int b)[] Property1 { [System.Runtime.CompilerServices.TupleElementNames(new[] { ""a"", ""b"" })] get; [System.Runtime.CompilerServices.TupleElementNames(new[] { ""a"", ""b"" })] set; }
}

class Class : [|IInterface|]
{
}" + s_tupleElementNamesAttribute,
@"using System;

interface IInterface
{
    [System.Runtime.CompilerServices.TupleElementNames(new[] { ""a"", ""b"" })]
    (int a, int b)[] Property1 { [System.Runtime.CompilerServices.TupleElementNames(new[] { ""a"", ""b"" })] get; [System.Runtime.CompilerServices.TupleElementNames(new[] { ""a"", ""b"" })] set; }
}

class Class : IInterface
{
    public (int a, int b)[] Property1
    {
        get
        {
            throw new NotImplementedException();
        }

        set
        {
            throw new NotImplementedException();
        }
    }
}" + s_tupleElementNamesAttribute);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface), Test.Utilities.CompilerTrait(Test.Utilities.CompilerFeature.Tuples)]
        public async Task TupleWithNamesInEvent()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IInterface
{
    [System.Runtime.CompilerServices.TupleElementNames(new[] { ""a"", ""b"" })]
    event Func<(int a, int b)> Event1;
}

class Class : [|IInterface|]
{
}" + s_tupleElementNamesAttribute,
@"interface IInterface
{
    [System.Runtime.CompilerServices.TupleElementNames(new[] { ""a"", ""b"" })]
    event Func<(int a, int b)> Event1;
}

class Class : IInterface
{
    public event Func<(int a, int b)> Event1;
}" + s_tupleElementNamesAttribute);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task NoDynamicAttributeInMethod()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IInterface
{
    [return: System.Runtime.CompilerServices.DynamicAttribute()]
    object Method1();
}

class Class : [|IInterface|]
{
}",
@"using System;

interface IInterface
{
    [return: System.Runtime.CompilerServices.DynamicAttribute()]
    object Method1();
}

class Class : IInterface
{
    public object Method1()
    {
        throw new NotImplementedException();
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestMethodWhenClassBracesAreMissing()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IInterface
{
    void Method1();
}

class Class : [|IInterface|]",
@"using System;

interface IInterface
{
    void Method1();
}

class Class : IInterface
{
    public void Method1()
    {
        throw new NotImplementedException();
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestInheritance1()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IInterface1
{
    void Method1();
}

interface IInterface2 : IInterface1
{
}

class Class : [|IInterface2|]
{
}",
@"using System;

interface IInterface1
{
    void Method1();
}

interface IInterface2 : IInterface1
{
}

class Class : IInterface2
{
    public void Method1()
    {
        throw new NotImplementedException();
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestInheritance2()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IInterface1
{
}

interface IInterface2 : IInterface1
{
    void Method1();
}

class Class : [|IInterface2|]
{
}",
@"using System;

interface IInterface1
{
}

interface IInterface2 : IInterface1
{
    void Method1();
}

class Class : IInterface2
{
    public void Method1()
    {
        throw new NotImplementedException();
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestInheritance3()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IInterface1
{
    void Method1();
}

interface IInterface2 : IInterface1
{
    void Method2();
}

class Class : [|IInterface2|]
{
}",
@"using System;

interface IInterface1
{
    void Method1();
}

interface IInterface2 : IInterface1
{
    void Method2();
}

class Class : IInterface2
{
    public void Method1()
    {
        throw new NotImplementedException();
    }

    public void Method2()
    {
        throw new NotImplementedException();
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestInheritanceMatchingMethod()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IInterface1
{
    void Method1();
}

interface IInterface2 : IInterface1
{
    void Method1();
}

class Class : [|IInterface2|]
{
}",
@"using System;

interface IInterface1
{
    void Method1();
}

interface IInterface2 : IInterface1
{
    void Method1();
}

class Class : IInterface2
{
    public void Method1()
    {
        throw new NotImplementedException();
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestExistingConflictingMethodReturnType()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IInterface1
{
    void Method1();
}

class Class : [|IInterface1|]
{
    public int Method1()
    {
        return 0;
    }
}",
@"using System;

interface IInterface1
{
    void Method1();
}

class Class : IInterface1
{
    public int Method1()
    {
        return 0;
    }

    void IInterface1.Method1()
    {
        throw new NotImplementedException();
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestExistingConflictingMethodParameters()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IInterface1
{
    void Method1(int i);
}

class Class : [|IInterface1|]
{
    public void Method1(string i)
    {
    }
}",
@"using System;

interface IInterface1
{
    void Method1(int i);
}

class Class : IInterface1
{
    public void Method1(int i)
    {
        throw new NotImplementedException();
    }

    public void Method1(string i)
    {
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementGenericType()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IInterface1<T>
{
    void Method1(T t);
}

class Class : [|IInterface1<int>|]
{
}",
@"using System;

interface IInterface1<T>
{
    void Method1(T t);
}

class Class : IInterface1<int>
{
    public void Method1(int t)
    {
        throw new NotImplementedException();
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementGenericTypeWithGenericMethod()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IInterface1<T>
{
    void Method1<U>(T t, U u);
}

class Class : [|IInterface1<int>|]
{
}",
@"using System;

interface IInterface1<T>
{
    void Method1<U>(T t, U u);
}

class Class : IInterface1<int>
{
    public void Method1<U>(int t, U u)
    {
        throw new NotImplementedException();
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementGenericTypeWithGenericMethodWithNaturalConstraint()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IInterface1<T>
{
    void Method1<U>(T t, U u) where U : IList<T>;
}

class Class : [|IInterface1<int>|]
{
}",
@"using System;

interface IInterface1<T>
{
    void Method1<U>(T t, U u) where U : IList<T>;
}

class Class : IInterface1<int>
{
    public void Method1<U>(int t, U u) where U : IList<int>
    {
        throw new NotImplementedException();
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementGenericTypeWithGenericMethodWithUnexpressibleConstraint()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IInterface1<T>
{
    void Method1<U>(T t, U u) where U : T;
}

class Class : [|IInterface1<int>|]
{
}",
@"using System;

interface IInterface1<T>
{
    void Method1<U>(T t, U u) where U : T;
}

class Class : IInterface1<int>
{
    void IInterface1<int>.Method1<U>(int t, U u)
    {
        throw new NotImplementedException();
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestArrayType()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    string[] M();
}

class C : [|I|]
{
}",
@"using System;

interface I
{
    string[] M();
}

class C : I
{
    public string[] M()
    {
        throw new NotImplementedException();
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementThroughFieldMember()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    void Method1();
}

class C : [|I|]
{
    I i;
}",
@"interface I
{
    void Method1();
}

class C : I
{
    I i;

    public void Method1()
    {
        i.Method1();
    }
}",
index: 1);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementThroughFieldMemberInterfaceWithIndexer()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IFoo
{
    int this[int x] { get; set; }
}

class Foo : [|IFoo|]
{
    IFoo f;
}",
@"interface IFoo
{
    int this[int x] { get; set; }
}

class Foo : IFoo
{
    IFoo f;

    public int this[int x]
    {
        get
        {
            return f[x];
        }

        set
        {
            f[x] = value;
        }
    }
}",
index: 1);
        }

        [WorkItem(472, "https://github.com/dotnet/roslyn/issues/472")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementThroughFieldMemberRemoveUnnecessaryCast()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System.Collections;

sealed class X : [|IComparer|]
{
    X x;
}",
@"using System.Collections;

sealed class X : IComparer
{
    X x;

    public int Compare(object x, object y)
    {
        return this.x.Compare(x, y);
    }
}",
index: 1);
        }

        [WorkItem(472, "https://github.com/dotnet/roslyn/issues/472")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementThroughFieldMemberRemoveUnnecessaryCastAndThis()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System.Collections;

sealed class X : [|IComparer|]
{
    X a;
}",
@"using System.Collections;

sealed class X : IComparer
{
    X a;

    public int Compare(object x, object y)
    {
        return a.Compare(x, y);
    }
}",
index: 1);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementAbstract()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    void Method1();
}

abstract class C : [|I|]
{
}",
@"interface I
{
    void Method1();
}

abstract class C : I
{
    public abstract void Method1();
}",
index: 1);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementInterfaceWithRefOutParameters()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"class C : [|I|]
{
    I foo;
}

interface I
{
    void Method1(ref int x, out int y, int z);
    int Method2();
}",
@"class C : I
{
    I foo;

    public void Method1(ref int x, out int y, int z)
    {
        foo.Method1(ref x, out y, z);
    }

    public int Method2()
    {
        return foo.Method2();
    }
}

interface I
{
    void Method1(ref int x, out int y, int z);
    int Method2();
}",
index: 1);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestConflictingMethods1()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"class B
{
    public int Method1()
    {
    }
}

class C : B, [|I|]
{
}

interface I
{
    void Method1();
}",
@"using System;

class B
{
    public int Method1()
    {
    }
}

class C : B, I
{
    void I.Method1()
    {
        throw new NotImplementedException();
    }
}

interface I
{
    void Method1();
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestConflictingProperties()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"class Test : [|I1|]
{
    int Prop { get; set; }
}

interface I1
{
    int Prop { get; set; }
}",
@"using System;

class Test : I1
{
    int I1.Prop
    {
        get
        {
            throw new NotImplementedException();
        }

        set
        {
            throw new NotImplementedException();
        }
    }

    int Prop { get; set; }
}

interface I1
{
    int Prop { get; set; }
}");
        }

        [WorkItem(539043, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/539043")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestExplicitProperties()
        {
            await TestMissingAsync(
@"interface I2
{
    decimal Calc { get; }
}

class C : [|I2|]
{
    protected decimal pay;

    decimal I2.Calc
    {
        get
        {
            return pay;
        }
    }
}");
        }

        [WorkItem(539489, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/539489")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestEscapedMethodName()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IInterface
{
    void @M();
}

class Class : [|IInterface|]
{
}",
@"using System;

interface IInterface
{
    void @M();
}

class Class : IInterface
{
    public void M()
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(539489, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/539489")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestEscapedMethodKeyword()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IInterface
{
    void @int();
}

class Class : [|IInterface|]
{
}",
@"using System;

interface IInterface
{
    void @int();
}

class Class : IInterface
{
    public void @int()
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(539489, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/539489")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestEscapedInterfaceName1()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface @IInterface
{
    void M();
}

class Class : [|@IInterface|]
{
    string M();
}",
@"using System;

interface @IInterface
{
    void M();
}

class Class : @IInterface
{
    void IInterface.M()
    {
        throw new NotImplementedException();
    }

    string M();
}");
        }

        [WorkItem(539489, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/539489")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestEscapedInterfaceName2()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface @IInterface
{
    void @M();
}

class Class : [|@IInterface|]
{
    string M();
}",
@"using System;

interface @IInterface
{
    void @M();
}

class Class : @IInterface
{
    void IInterface.M()
    {
        throw new NotImplementedException();
    }

    string M();
}");
        }

        [WorkItem(539489, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/539489")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestEscapedInterfaceKeyword1()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface @int
{
    void M();
}

class Class : [|@int|]
{
    string M();
}",
@"using System;

interface @int
{
    void M();
}

class Class : @int
{
    void @int.M()
    {
        throw new NotImplementedException();
    }

    string M();
}");
        }

        [WorkItem(539489, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/539489")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestEscapedInterfaceKeyword2()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface @int
{
    void @bool();
}

class Class : [|@int|]
{
    string @bool();
}",
@"using System;

interface @int
{
    void @bool();
}

class Class : @int
{
    void @int.@bool()
    {
        throw new NotImplementedException();
    }

    string @bool();
}");
        }

        [WorkItem(539522, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/539522")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestPropertyFormatting()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"public interface DD
{
    int Prop { get; set; }
}
public class A : [|DD|]
{
}",
@"using System;

public interface DD
{
    int Prop { get; set; }
}
public class A : DD
{
    public int Prop
    {
        get
        {
            throw new NotImplementedException();
        }

        set
        {
            throw new NotImplementedException();
        }
    }
}",
compareTokens: false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestProperty_PropertyCodeStyleOn1()
        {
            await TestWithAllCodeStyleOptionsOnAsync(
@"public interface DD
{
    int Prop { get; }
}

public class A : [|DD|]
{
}",
@"using System;

public interface DD
{
    int Prop { get; }
}

public class A : DD
{
    public int Prop => throw new NotImplementedException();
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestProperty_AccessorCodeStyleOn1()
        {
            await TestWithAccessorCodeStyleOptionsOnAsync(
@"public interface DD
{
    int Prop { get; }
}

public class A : [|DD|]
{
}",
@"using System;

public interface DD
{
    int Prop { get; }
}

public class A : DD
{
    public int Prop
    {
        get => throw new NotImplementedException();
        }
    }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestIndexer_IndexerCodeStyleOn1()
        {
            await TestWithAllCodeStyleOptionsOnAsync(
@"public interface DD
{
    int this[int i] { get; }
}

public class A : [|DD|]
{
}",
@"using System;

public interface DD
{
    int this[int i] { get; }
}

public class A : DD
{
    public int this[int i] => throw new NotImplementedException();
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestIndexer_AccessorCodeStyleOn1()
        {
            await TestWithAccessorCodeStyleOptionsOnAsync(
@"public interface DD
{
    int this[int i] { get; }
}

public class A : [|DD|]
{
}",
@"using System;

public interface DD
{
    int this[int i] { get; }
}

public class A : DD
{
    public int this[int i]
    {
        get => throw new NotImplementedException();
        }
    }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestMethod_AllCodeStyleOn1()
        {
            await TestWithAllCodeStyleOptionsOnAsync(
@"public interface DD
{
    int M();
}

public class A : [|DD|]
{
}",
@"using System;

public interface DD
{
    int M();
}

public class A : DD
{
    public int M() => throw new NotImplementedException();
}");
        }

        [WorkItem(539522, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/539522")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestReadonlyPropertyExpressionBodyYes1()
        {
            await TestWithAllCodeStyleOptionsOnAsync(
@"public interface DD
{
    int Prop { get; }
}
public class A : [|DD|]
{
}",
@"using System;

public interface DD
{
    int Prop { get; }
}
public class A : DD
{
    public int Prop => throw new NotImplementedException();
}", compareTokens: false);
        }

        [WorkItem(539522, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/539522")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestReadonlyPropertyAccessorBodyYes1()
        {
            await TestWithAccessorCodeStyleOptionsOnAsync(
@"public interface DD
{
    int Prop { get; }
}

public class A : [|DD|]
{
}",
@"using System;

public interface DD
{
    int Prop { get; }
}

public class A : DD
{
    public int Prop
    {
        get => throw new NotImplementedException();
        }
    }");
        }

        [WorkItem(539522, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/539522")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestReadonlyPropertyAccessorBodyYes2()
        {
            await TestWithAccessorCodeStyleOptionsOnAsync(
@"public interface DD
{
    int Prop { get; set; }
}

public class A : [|DD|]
{
}",
@"using System;

public interface DD
{
    int Prop { get; set; }
}

public class A : DD
{
    public int Prop
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
        }
    }");
        }

        [WorkItem(539522, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/539522")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestReadonlyPropertyExpressionBodyNo1()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"public interface DD
{
    int Prop { get; }
}

public class A : [|DD|]
{
}",
@"using System;

public interface DD
{
    int Prop { get; }
}

public class A : DD
{
    public int Prop
    {
        get
        {
            throw new NotImplementedException();
        }
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestIndexerExpressionBodyYes1()
        {
            await TestWithAllCodeStyleOptionsOnAsync(
@"public interface DD
{
    int this[int i] { get; }
}

public class A : [|DD|]
{
}",
@"using System;

public interface DD
{
    int this[int i] { get; }
}

public class A : DD
{
    public int this[int i] => throw new NotImplementedException();
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestIndexerExpressionBodyNo1()
        {
            await TestWithAllCodeStyleOptionsOnAsync(
@"public interface DD
{
    int this[int i] { get; set; }
}

public class A : [|DD|]
{
}",
@"using System;

public interface DD
{
    int this[int i] { get; set; }
}

public class A : DD
{
    public int this[int i]
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
        }
    }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestIndexerAccessorExpressionBodyYes1()
        {
            await TestWithAccessorCodeStyleOptionsOnAsync(
@"public interface DD
{
    int this[int i] { get; }
}

public class A : [|DD|]
{
}",
@"using System;

public interface DD
{
    int this[int i] { get; }
}

public class A : DD
{
    public int this[int i]
    {
        get => throw new NotImplementedException();
        }
    }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestIndexerAccessorExpressionBodyYes2()
        {
            await TestWithAllCodeStyleOptionsOnAsync(
@"public interface DD
{
    int this[int i] { get; set; }
}

public class A : [|DD|]
{
}",
@"using System;

public interface DD
{
    int this[int i] { get; set; }
}

public class A : DD
{
    public int this[int i]
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
        }
    }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestCommentPlacement()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"public interface DD
{
    void Foo();
}
public class A : [|DD|]
{
    //comments
}",
@"using System;

public interface DD
{
    void Foo();
}
public class A : DD
{
    //comments
    public void Foo()
    {
        throw new NotImplementedException();
    }
}",
compareTokens: false);
        }

        [WorkItem(539991, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/539991")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestBracePlacement()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;
class C : [|IServiceProvider|]",
@"using System;
class C : IServiceProvider
{
    public object GetService(Type serviceType)
    {
        throw new NotImplementedException();
    }
}
",
compareTokens: false);
        }

        [WorkItem(540318, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/540318")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestMissingWithIncompleteMember()
        {
            await TestMissingAsync(
@"interface ITest
{
    void Method();
}

class Test : [|ITest|]
{
    p public void Method()
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(541380, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541380")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestExplicitProperty()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface i1
{
    int p { get; set; }
}

class c1 : [|i1|]
{
}",
@"using System;

interface i1
{
    int p { get; set; }
}

class c1 : i1
{
    int i1.p
    {
        get
        {
            throw new NotImplementedException();
        }

        set
        {
            throw new NotImplementedException();
        }
    }
}",
index: 1);
        }

        [WorkItem(541981, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541981")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestNoDelegateThroughField1()
        {
            await TestActionCountAsync(
@"interface I
{
    void Method1();
}

class C : [|I|]
{
    I i { get; set; }
}",
count: 3);
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    void Method1();
}

class C : [|I|]
{
    I i { get; set; }
}",
@"using System;

interface I
{
    void Method1();
}

class C : I
{
    I i { get; set; }

    public void Method1()
    {
        throw new NotImplementedException();
    }
}",
index: 0);
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    void Method1();
}

class C : [|I|]
{
    I i { get; set; }
}",
@"interface I
{
    void Method1();
}

class C : I
{
    I i { get; set; }

    public void Method1()
    {
        i.Method1();
    }
}",
index: 1);
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    void Method1();
}

class C : [|I|]
{
    I i { get; set; }
}",
@"using System;

interface I
{
    void Method1();
}

class C : I
{
    I i { get; set; }

    void I.Method1()
    {
        throw new NotImplementedException();
    }
}",
index: 2);
        }

        [WorkItem(768799, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/768799")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementIReadOnlyListThroughField()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System.Collections.Generic;

class A : [|IReadOnlyList<int>|]
{
    int[] field;
}",
@"using System.Collections;
using System.Collections.Generic;

class A : IReadOnlyList<int>
{
    int[] field;

    public int this[int index]
    {
        get
        {
            return ((IReadOnlyList<int>)field)[index];
        }
    }

    public int Count
    {
        get
        {
            return ((IReadOnlyList<int>)field).Count;
        }
    }

    public IEnumerator<int> GetEnumerator()
    {
        return ((IReadOnlyList<int>)field).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IReadOnlyList<int>)field).GetEnumerator();
    }
}",
index: 1);
        }

        [WorkItem(768799, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/768799")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementIReadOnlyListThroughProperty()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System.Collections.Generic;

class A : [|IReadOnlyList<int>|]
{
    int[] field { get; set; }
}",
@"using System.Collections;
using System.Collections.Generic;

class A : IReadOnlyList<int>
{
    public int this[int index]
    {
        get
        {
            return ((IReadOnlyList<int>)field)[index];
        }
    }

    public int Count
    {
        get
        {
            return ((IReadOnlyList<int>)field).Count;
        }
    }

    int[] field { get; set; }

    public IEnumerator<int> GetEnumerator()
    {
        return ((IReadOnlyList<int>)field).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IReadOnlyList<int>)field).GetEnumerator();
    }
}",
index: 1);
        }

        [WorkItem(768799, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/768799")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementInterfaceThroughField()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    int M();
}

class A : I
{
    int I.M()
    {
        return 0;
    }
}

class B : [|I|]
{
    A a;
}",
@"interface I
{
    int M();
}

class A : I
{
    int I.M()
    {
        return 0;
    }
}

class B : I
{
    A a;

    public int M()
    {
        return ((I)a).M();
    }
}",
index: 1);
        }

        [WorkItem(768799, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/768799")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementInterfaceThroughField_FieldImplementsMultipleInterfaces()
        {
            await TestActionCountAsync(
@"interface I
{
    int M();
}

interface I2
{
    int M2() }

class A : I, I2
{
    int I.M()
    {
        return 0;
    }

    int I2.M2()
    {
        return 0;
    }
}

class B : [|I|], I2
{
    A a;
}",
count: 3);
            await TestActionCountAsync(
@"interface I
{
    int M();
}

interface I2
{
    int M2() }

class A : I, I2
{
    int I.M()
    {
        return 0;
    }

    int I2.M2()
    {
        return 0;
    }
}

class B : I, [|I2|]
{
    A a;
}",
count: 3);
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    int M();
}

interface I2
{
    int M2() }

class A : I, I2
{
    int I.M()
    {
        return 0;
    }

    int I2.M2()
    {
        return 0;
    }
}

class B : [|I|], I2
{
    A a;
}",
@"interface I
{
    int M();
}

interface I2
{
    int M2() }

class A : I, I2
{
    int I.M()
    {
        return 0;
    }

    int I2.M2()
    {
        return 0;
    }
}

class B : I, I2
{
    A a;

    public int M()
    {
        return ((I)a).M();
    }
}",
index: 1);
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    int M();
}

interface I2
{
    int M2() }

class A : I, I2
{
    int I.M()
    {
        return 0;
    }

    int I2.M2()
    {
        return 0;
    }
}

class B : I, [|I2|]
{
    A a;
}",
@"interface I
{
    int M();
}

interface I2
{
    int M2() }

class A : I, I2
{
    int I.M()
    {
        return 0;
    }

    int I2.M2()
    {
        return 0;
    }
}

class B : I, I2
{
    A a;

    public int M2()
    {
        return ((I2)a).M2();
    }
}",
index: 1);
        }

        [WorkItem(768799, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/768799")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementInterfaceThroughField_MultipleFieldsCanImplementInterface()
        {
            await TestActionCountAsync(
@"interface I
{
    int M();
}

class A : I
{
    int I.M()
    {
        return 0;
    }
}

class B : [|I|]
{
    A a;
    A aa;
}",
count: 4);
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    int M();
}

class A : I
{
    int I.M()
    {
        return 0;
    }
}

class B : [|I|]
{
    A a;
    A aa;
}",
@"interface I
{
    int M();
}

class A : I
{
    int I.M()
    {
        return 0;
    }
}

class B : I
{
    A a;
    A aa;

    public int M()
    {
        return ((I)a).M();
    }
}",
index: 1);
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    int M();
}

class A : I
{
    int I.M()
    {
        return 0;
    }
}

class B : [|I|]
{
    A a;
    A aa;
}",
@"interface I
{
    int M();
}

class A : I
{
    int I.M()
    {
        return 0;
    }
}

class B : I
{
    A a;
    A aa;

    public int M()
    {
        return ((I)aa).M();
    }
}",
index: 2);
        }

        [WorkItem(768799, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/768799")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementInterfaceThroughField_MultipleFieldsForMultipleInterfaces()
        {
            await TestActionCountAsync(
@"interface I
{
    int M();
}

interface I2
{
    int M2() }

class A : I
{
    int I.M()
    {
        return 0;
    }
}

class B : I2
{
    int I2.M2()
    {
        return 0;
    }
}

class C : [|I|], I2
{
    A a;
    B b;
}",
count: 3);
            await TestActionCountAsync(
@"interface I
{
    int M();
}

interface I2
{
    int M2() }

class A : I
{
    int I.M()
    {
        return 0;
    }
}

class B : I2
{
    int I2.M2()
    {
        return 0;
    }
}

class C : I, [|I2|]
{
    A a;
    B b;
}",
count: 3);
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    int M();
}

interface I2
{
    int M2() }

class A : I
{
    int I.M()
    {
        return 0;
    }
}

class B : I2
{
    int I2.M2()
    {
        return 0;
    }
}

class C : [|I|], I2
{
    A a;
    B b;
}",
@"interface I
{
    int M();
}

interface I2
{
    int M2() }

class A : I
{
    int I.M()
    {
        return 0;
    }
}

class B : I2
{
    int I2.M2()
    {
        return 0;
    }
}

class C : I, I2
{
    A a;
    B b;

    public int M()
    {
        return ((I)a).M();
    }
}",
index: 1);
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    int M();
}

interface I2
{
    int M2() }

class A : I
{
    int I.M()
    {
        return 0;
    }
}

class B : I2
{
    int I2.M2()
    {
        return 0;
    }
}

class C : I, [|I2|]
{
    A a;
    B b;
}",
@"interface I
{
    int M();
}

interface I2
{
    int M2() }

class A : I
{
    int I.M()
    {
        return 0;
    }
}

class B : I2
{
    int I2.M2()
    {
        return 0;
    }
}

class C : I, I2
{
    A a;
    B b;

    public int M2()
    {
        return ((I2)b).M2();
    }
}",
index: 1);
        }

        [WorkItem(768799, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/768799")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestNoImplementThroughIndexer()
        {
            await TestActionCountAsync(
@"interface I
{
    int M();
}

class A : I
{
    int I.M()
    {
        return 0;
    }
}

class B : [|I|]
{
    A this[int index]
    {
        get
        {
            return null;
        }
    };
}",
count: 2);
        }

        [WorkItem(768799, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/768799")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestNoImplementThroughWriteOnlyProperty()
        {
            await TestActionCountAsync(
@"interface I
{
    int M();
}

class A : I
{
    int I.M()
    {
        return 0;
    }
}

class B : [|I|]
{
    A a
    {
        set
        {
        }
    }
}",
count: 2);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementEvent()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IFoo
{
    event System.EventHandler E;
}

abstract class Foo : [|IFoo|]
{
}",
@"using System;

interface IFoo
{
    event System.EventHandler E;
}

abstract class Foo : IFoo
{
    public event EventHandler E;
}",
index: 0);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementEventAbstractly()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IFoo
{
    event System.EventHandler E;
}

abstract class Foo : [|IFoo|]
{
}",
@"using System;

interface IFoo
{
    event System.EventHandler E;
}

abstract class Foo : IFoo
{
    public abstract event EventHandler E;
}",
index: 1);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementEventExplicitly()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IFoo
{
    event System.EventHandler E;
}

abstract class Foo : [|IFoo|]
{
}",
@"using System;

interface IFoo
{
    event System.EventHandler E;
}

abstract class Foo : IFoo
{
    event EventHandler IFoo.E
    {
        add
        {
            throw new NotImplementedException();
        }

        remove
        {
            throw new NotImplementedException();
        }
    }
}",
index: 2);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestFaultToleranceInStaticMembers()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IFoo
{
    static string Name { set; get; }

    static int Foo(string s);
}

class Program : [|IFoo|]
{
}",
@"using System;

interface IFoo
{
    static string Name { set; get; }

    static int Foo(string s);
}

class Program : IFoo
{
    public string Name
    {
        get
        {
            throw new NotImplementedException();
        }

        set
        {
            throw new NotImplementedException();
        }
    }

    public int Foo(string s)
    {
        throw new NotImplementedException();
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestIndexers()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"public interface ISomeInterface
{
    int this[int index] { get; set; }
}

class IndexerClass : [|ISomeInterface|]
{
}",
@"using System;

public interface ISomeInterface
{
    int this[int index] { get; set; }
}

class IndexerClass : ISomeInterface
{
    public int this[int index]
    {
        get
        {
            throw new NotImplementedException();
        }

        set
        {
            throw new NotImplementedException();
        }
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestIndexersExplicit()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"public interface ISomeInterface
{
    int this[int index] { get; set; }
}

class IndexerClass : [|ISomeInterface|]
{
}",
@"using System;

public interface ISomeInterface
{
    int this[int index] { get; set; }
}

class IndexerClass : ISomeInterface
{
    int ISomeInterface.this[int index]
    {
        get
        {
            throw new NotImplementedException();
        }

        set
        {
            throw new NotImplementedException();
        }
    }
}",
index: 1);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestIndexersWithASingleAccessor()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"public interface ISomeInterface
{
    int this[int index] { get; }
}

class IndexerClass : [|ISomeInterface|]
{
}",
@"using System;

public interface ISomeInterface
{
    int this[int index] { get; }
}

class IndexerClass : ISomeInterface
{
    public int this[int index]
    {
        get
        {
            throw new NotImplementedException();
        }
    }
}");
        }

        [WorkItem(542357, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542357")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestConstraints1()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    void Foo<T>() where T : class;
}

class A : [|I|]
{
}",
@"using System;

interface I
{
    void Foo<T>() where T : class;
}

class A : I
{
    public void Foo<T>() where T : class
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(542357, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542357")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestConstraintsExplicit()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    void Foo<T>() where T : class;
}

class A : [|I|]
{
}",
@"using System;

interface I
{
    void Foo<T>() where T : class;
}

class A : I
{
    void I.Foo<T>()
    {
        throw new NotImplementedException();
    }
}",
index: 1);
        }

        [WorkItem(542357, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542357")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestUsingAddedForConstraint()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    void Foo<T>() where T : System.Attribute;
}

class A : [|I|]
{
}",
@"using System;

interface I
{
    void Foo<T>() where T : System.Attribute;
}

class A : I
{
    public void Foo<T>() where T : Attribute
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(542379, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542379")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestIndexer()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    int this[int x] { get; set; }
}

class C : [|I|]
{
}",
@"using System;

interface I
{
    int this[int x] { get; set; }
}

class C : I
{
    public int this[int x]
    {
        get
        {
            throw new NotImplementedException();
        }

        set
        {
            throw new NotImplementedException();
        }
    }
}");
        }

        [WorkItem(542588, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542588")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestRecursiveConstraint1()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

interface I
{
    void Foo<T>() where T : IComparable<T>;
}

class C : [|I|]
{
}",
@"using System;

interface I
{
    void Foo<T>() where T : IComparable<T>;
}

class C : I
{
    public void Foo<T>() where T : IComparable<T>
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(542588, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542588")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestRecursiveConstraint2()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

interface I
{
    void Foo<T>() where T : IComparable<T>;
}

class C : [|I|]
{
}",
@"using System;

interface I
{
    void Foo<T>() where T : IComparable<T>;
}

class C : I
{
    void I.Foo<T>()
    {
        throw new NotImplementedException();
    }
}",
index: 1);
        }

        [WorkItem(542587, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542587")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestUnexpressibleConstraint1()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I<S>
{
    void Foo<T>() where T : class, S;
}

class A : [|I<string>|]
{
}",
@"using System;

interface I<S>
{
    void Foo<T>() where T : class, S;
}

class A : I<string>
{
    void I<string>.Foo<T>()
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(542587, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542587")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestUnexpressibleConstraint2()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I<S>
{
    void Foo<T>() where T : class, S;
}

class A : [|I<object>|]
{
}",
@"using System;

interface I<S>
{
    void Foo<T>() where T : class, S;
}

class A : I<object>
{
    public void Foo<T>() where T : class
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(542587, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542587")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestUnexpressibleConstraint3()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I<S>
{
    void Foo<T>() where T : class, S;
}

class A : [|I<object>|]
{
}",
@"using System;

interface I<S>
{
    void Foo<T>() where T : class, S;
}

class A : I<object>
{
    void I<object>.Foo<T>()
    {
        throw new NotImplementedException();
    }
}",
index: 1);
        }

        [WorkItem(542587, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542587")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestUnexpressibleConstraint4()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

interface I<S>
{
    void Foo<T>() where T : class, S;
}

class A : [|I<Delegate>|]
{
}",
@"using System;

interface I<S>
{
    void Foo<T>() where T : class, S;
}

class A : I<Delegate>
{
    void I<Delegate>.Foo<T>()
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(542587, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542587")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestUnexpressibleConstraint5()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

interface I<S>
{
    void Foo<T>() where T : class, S;
}

class A : [|I<MulticastDelegate>|]
{
}",
@"using System;

interface I<S>
{
    void Foo<T>() where T : class, S;
}

class A : I<MulticastDelegate>
{
    void I<MulticastDelegate>.Foo<T>()
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(542587, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542587")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestUnexpressibleConstraint6()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

interface I<S>
{
    void Foo<T>() where T : class, S;
}

delegate void Bar();

class A : [|I<Bar>|]
{
}",
@"using System;

interface I<S>
{
    void Foo<T>() where T : class, S;
}

delegate void Bar();

class A : I<Bar>
{
    void I<Bar>.Foo<T>()
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(542587, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542587")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestUnexpressibleConstraint7()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

interface I<S>
{
    void Foo<T>() where T : class, S;
}

class A : [|I<Enum>|]
{
}",
@"using System;

interface I<S>
{
    void Foo<T>() where T : class, S;
}

class A : I<Enum>
{
    void I<Enum>.Foo<T>()
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(542587, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542587")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestUnexpressibleConstraint8()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

interface I<S>
{
    void Foo<T>() where T : class, S;
}

class A : [|I<int[]>|]
{
}",
@"using System;

interface I<S>
{
    void Foo<T>() where T : class, S;
}

class A : I<int[]>
{
    void I<int[]>.Foo<T>()
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(542587, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542587")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestUnexpressibleConstraint9()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

interface I<S>
{
    void Foo<T>() where T : class, S;
}

enum E
{
}

class A : [|I<E>|]
{
}",
@"using System;

interface I<S>
{
    void Foo<T>() where T : class, S;
}

enum E
{
}

class A : I<E>
{
    void I<E>.Foo<T>()
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(542621, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542621")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestUnexpressibleConstraint10()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

interface I<S>
{
    void Foo<T>() where T : S;
}

class A : [|I<ValueType>|]
{
}",
@"using System;

interface I<S>
{
    void Foo<T>() where T : S;
}

class A : I<ValueType>
{
    void I<ValueType>.Foo<T>()
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(542669, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542669")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestArrayConstraint()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

interface I<S>
{
    void Foo<T>() where T : S;
}

class C : [|I<Array>|]
{
}",
@"using System;

interface I<S>
{
    void Foo<T>() where T : S;
}

class C : I<Array>
{
    void I<Array>.Foo<T>()
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(542743, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542743")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestMultipleClassConstraints()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

interface I<S>
{
    void Foo<T>() where T : Exception, S;
}

class C : [|I<Attribute>|]
{
}",
@"using System;

interface I<S>
{
    void Foo<T>() where T : Exception, S;
}

class C : I<Attribute>
{
    void I<Attribute>.Foo<T>()
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(542751, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542751")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestClassConstraintAndRefConstraint()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

interface I<S>
{
    void Foo<T>() where T : class, S;
}

class C : [|I<Exception>|]
{
}",
@"using System;

interface I<S>
{
    void Foo<T>() where T : class, S;
}

class C : I<Exception>
{
    void I<Exception>.Foo<T>()
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(542505, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542505")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestRenameConflictingTypeParameters1()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;
using System.Collections.Generic;

interface I<T>
{
    void Foo<S>(T x, IList<S> list) where S : T;
}

class A<S> : [|I<S>|]
{
}",
@"using System;
using System.Collections.Generic;

interface I<T>
{
    void Foo<S>(T x, IList<S> list) where S : T;
}

class A<S> : I<S>
{
    public void Foo<S1>(S x, IList<S1> list) where S1 : S
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(542505, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542505")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestRenameConflictingTypeParameters2()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;
using System.Collections.Generic;

interface I<T>
{
    void Foo<S>(T x, IList<S> list) where S : T;
}

class A<S> : [|I<S>|]
{
}",
@"using System;
using System.Collections.Generic;

interface I<T>
{
    void Foo<S>(T x, IList<S> list) where S : T;
}

class A<S> : I<S>
{
    void I<S>.Foo<S1>(S x, IList<S1> list)
    {
        throw new NotImplementedException();
    }
}",
index: 1);
        }

        [WorkItem(542505, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542505")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestRenameConflictingTypeParameters3()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;
using System.Collections.Generic;

interface I<X, Y>
{
    void Foo<A, B>(X x, Y y, IList<A> list1, IList<B> list2)
        where A : IList<B>
        where B : IList<A>;
}

class C<A, B> : [|I<A, B>|]
{
}",
@"using System;
using System.Collections.Generic;

interface I<X, Y>
{
    void Foo<A, B>(X x, Y y, IList<A> list1, IList<B> list2)
        where A : IList<B>
        where B : IList<A>;
}

class C<A, B> : I<A, B>
{
    public void Foo<A1, B1>(A x, B y, IList<A1> list1, IList<B1> list2)
        where A1 : IList<B1>
        where B1 : IList<A1>
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(542505, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542505")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestRenameConflictingTypeParameters4()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;
using System.Collections.Generic;

interface I<X, Y>
{
    void Foo<A, B>(X x, Y y, IList<A> list1, IList<B> list2)
        where A : IList<B>
        where B : IList<A>;
}

class C<A, B> : [|I<A, B>|]
{
}",
@"using System;
using System.Collections.Generic;

interface I<X, Y>
{
    void Foo<A, B>(X x, Y y, IList<A> list1, IList<B> list2)
        where A : IList<B>
        where B : IList<A>;
}

class C<A, B> : I<A, B>
{
    void I<A, B>.Foo<A1, B1>(A x, B y, IList<A1> list1, IList<B1> list2)
    {
        throw new NotImplementedException();
    }
}",
index: 1);
        }

        [WorkItem(542506, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542506")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestNameSimplification()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

class A<T>
{
    class B
    {
    }

    interface I
    {
        void Foo(B x);
    }

    class C<U> : [|I|]
    {
    }
}",
@"using System;

class A<T>
{
    class B
    {
    }

    interface I
    {
        void Foo(B x);
    }

    class C<U> : I
    {
        public void Foo(B x)
        {
            throw new NotImplementedException();
        }
    }
}");
        }

        [WorkItem(542506, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542506")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestNameSimplification2()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"class A<T>
{
    class B
    {
    }

    interface I
    {
        void Foo(B[] x);
    }

    class C<U> : [|I|]
    {
    }
}",
@"using System;

class A<T>
{
    class B
    {
    }

    interface I
    {
        void Foo(B[] x);
    }

    class C<U> : I
    {
        public void Foo(B[] x)
        {
            throw new NotImplementedException();
        }
    }
}");
        }

        [WorkItem(542506, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542506")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestNameSimplification3()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"class A<T>
{
    class B
    {
    }

    interface I
    {
        void Foo(B[][,][,,][,,,] x);
    }

    class C<U> : [|I|]
    {
    }
}",
@"using System;

class A<T>
{
    class B
    {
    }

    interface I
    {
        void Foo(B[][,][,,][,,,] x);
    }

    class C<U> : I
    {
        public void Foo(B[][,][,,][,,,] x)
        {
            throw new NotImplementedException();
        }
    }
}");
        }

        [WorkItem(544166, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544166")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementAbstractProperty()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IFoo
{
    int Gibberish { get; set; }
}

abstract class Foo : [|IFoo|]
{
}",
@"interface IFoo
{
    int Gibberish { get; set; }
}

abstract class Foo : IFoo
{
    public abstract int Gibberish { get; set; }
}",
index: 1);
        }

        [WorkItem(544210, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544210")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestMissingOnWrongArity()
        {
            await TestMissingAsync(
@"interface I1<T>
{
    int X { get; set; }
}

class C : [|I1|]
{
}");
        }

        [WorkItem(544281, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544281")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplicitDefaultValue()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IOptional
{
    int Foo(int g = 0);
}

class Opt : [|IOptional|]
{
}",
@"using System;

interface IOptional
{
    int Foo(int g = 0);
}

class Opt : IOptional
{
    public int Foo(int g = 0)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(544281, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544281")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestExplicitDefaultValue()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IOptional
{
    int Foo(int g = 0);
}

class Opt : [|IOptional|]
{
}",
@"using System;

interface IOptional
{
    int Foo(int g = 0);
}

class Opt : IOptional
{
    int IOptional.Foo(int g)
    {
        throw new NotImplementedException();
    }
}",
index: 1);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestMissingInHiddenType()
        {
            await TestMissingAsync(
@"using System;

class Program : [|IComparable|]
{
#line hidden
}
#line default");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestGenerateIntoVisiblePart()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"#line default
using System;

partial class Program : [|IComparable|]
{
    void Foo()
    {
#line hidden
    }
}
#line default",
@"#line default
using System;

partial class Program : IComparable
{
    public int CompareTo(object obj)
    {
        throw new NotImplementedException();
    }

    void Foo()
    {
#line hidden
    }
}
#line default",
compareTokens: false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestGenerateIfAvailableRegionExists()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

partial class Program : [|IComparable|]
{
#line hidden
}
#line default

partial class Program
{
}",
@"using System;

partial class Program : IComparable
{
#line hidden
}
#line default

partial class Program
{
    public int CompareTo(object obj)
    {
        throw new NotImplementedException();
    }
}",
compareTokens: false);
        }

        [WorkItem(545334, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545334")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestNoGenerateInVenusCase1()
        {
            await TestMissingAsync(
@"using System;
#line 1 ""Bar""
class Foo : [|IComparable|]


#line default
#line hidden
// stuff");
        }

        [WorkItem(545476, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545476")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestOptionalDateTime1()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

interface IFoo
{
    void Foo([Optional][DateTimeConstant(100)] DateTime x);
}

public class C : [|IFoo|]
{
}",
@"using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

interface IFoo
{
    void Foo([Optional][DateTimeConstant(100)] DateTime x);
}

public class C : IFoo
{
    public void Foo([DateTimeConstant(100), Optional] DateTime x)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(545476, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545476")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestOptionalDateTime2()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

interface IFoo
{
    void Foo([Optional][DateTimeConstant(100)] DateTime x);
}

public class C : [|IFoo|]
{
}",
@"using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

interface IFoo
{
    void Foo([Optional][DateTimeConstant(100)] DateTime x);
}

public class C : IFoo
{
    void IFoo.Foo(DateTime x)
    {
        throw new NotImplementedException();
    }
}",
index: 1);
        }

        [WorkItem(545477, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545477")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestIUnknownIDispatchAttributes1()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

interface IFoo
{
    void Foo1([Optional][IUnknownConstant] object x);
    void Foo2([Optional][IDispatchConstant] object x);
}

public class C : [|IFoo|]
{
}",
@"using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

interface IFoo
{
    void Foo1([Optional][IUnknownConstant] object x);
    void Foo2([Optional][IDispatchConstant] object x);
}

public class C : IFoo
{
    public void Foo1([IUnknownConstant, Optional] object x)
    {
        throw new NotImplementedException();
    }

    public void Foo2([IDispatchConstant, Optional] object x)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(545477, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545477")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestIUnknownIDispatchAttributes2()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

interface IFoo
{
    void Foo1([Optional][IUnknownConstant] object x);
    void Foo2([Optional][IDispatchConstant] object x);
}

public class C : [|IFoo|]
{
}",
@"using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

interface IFoo
{
    void Foo1([Optional][IUnknownConstant] object x);
    void Foo2([Optional][IDispatchConstant] object x);
}

public class C : IFoo
{
    void IFoo.Foo1(object x)
    {
        throw new NotImplementedException();
    }

    void IFoo.Foo2(object x)
    {
        throw new NotImplementedException();
    }
}",
index: 1);
        }

        [WorkItem(545464, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545464")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestTypeNameConflict()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IFoo
{
    void Foo();
}

public class Foo : [|IFoo|]
{
}",
@"using System;

interface IFoo
{
    void Foo();
}

public class Foo : IFoo
{
    void IFoo.Foo()
    {
        throw new NotImplementedException();
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestStringLiteral()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IFoo { void Foo ( string s = ""\"""" ) ; } class B : [|IFoo|] { } ",
@"using System ; interface IFoo { void Foo ( string s = ""\"""" ) ; } class B : IFoo { public void Foo ( string s = ""\"""" ) { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(916114, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/916114")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestOptionalNullableStructParameter1()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"struct b
{
}

interface d
{
    void m(b? x = null, b? y = default(b?));
}

class c : [|d|]
{
}",
@"using System;

struct b
{
}

interface d
{
    void m(b? x = null, b? y = default(b?));
}

class c : d
{
    public void m(b? x = default(b?), b? y = default(b?))
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(916114, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/916114")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestOptionalNullableStructParameter2()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"struct b
{
}

interface d
{
    void m(b? x = null, b? y = default(b?));
}

class c : [|d|]
{
}",
@"using System;

struct b
{
}

interface d
{
    void m(b? x = null, b? y = default(b?));
}

class c : d
{
    void d.m(b? x, b? y)
    {
        throw new NotImplementedException();
    }
}", 1);
        }

        [WorkItem(916114, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/916114")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestOptionalNullableIntParameter()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface d
{
    void m(int? x = 5, int? y = null);
}

class c : [|d|]
{
}",
@"using System;

interface d
{
    void m(int? x = 5, int? y = null);
}

class c : d
{
    public void m(int? x = 5, int? y = default(int?))
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(545613, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545613")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestOptionalWithNoDefaultValue()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System.Runtime.InteropServices;

interface I
{
    void Foo([Optional] I o);
}

class C : [|I|]
{
}",
@"using System;
using System.Runtime.InteropServices;

interface I
{
    void Foo([Optional] I o);
}

class C : I
{
    public void Foo([Optional] I o)
    {
        throw new NotImplementedException();
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestIntegralAndFloatLiterals()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    void M01(short s = short.MinValue);
    void M02(short s = -1);
    void M03(short s = short.MaxValue);
    void M04(ushort s = ushort.MinValue);
    void M05(ushort s = 1);
    void M06(ushort s = ushort.MaxValue);
    void M07(int s = int.MinValue);
    void M08(int s = -1);
    void M09(int s = int.MaxValue);
    void M10(uint s = uint.MinValue);
    void M11(uint s = 1);
    void M12(uint s = uint.MaxValue);
    void M13(long s = long.MinValue);
    void M14(long s = -1);
    void M15(long s = long.MaxValue);
    void M16(ulong s = ulong.MinValue);
    void M17(ulong s = 1);
    void M18(ulong s = ulong.MaxValue);
    void M19(float s = float.MinValue);
    void M20(float s = 1);
    void M21(float s = float.MaxValue);
    void M22(double s = double.MinValue);
    void M23(double s = 1);
    void M24(double s = double.MaxValue);
}

class C : [|I|]
{
}",
@"using System;

interface I
{
    void M01(short s = short.MinValue);
    void M02(short s = -1);
    void M03(short s = short.MaxValue);
    void M04(ushort s = ushort.MinValue);
    void M05(ushort s = 1);
    void M06(ushort s = ushort.MaxValue);
    void M07(int s = int.MinValue);
    void M08(int s = -1);
    void M09(int s = int.MaxValue);
    void M10(uint s = uint.MinValue);
    void M11(uint s = 1);
    void M12(uint s = uint.MaxValue);
    void M13(long s = long.MinValue);
    void M14(long s = -1);
    void M15(long s = long.MaxValue);
    void M16(ulong s = ulong.MinValue);
    void M17(ulong s = 1);
    void M18(ulong s = ulong.MaxValue);
    void M19(float s = float.MinValue);
    void M20(float s = 1);
    void M21(float s = float.MaxValue);
    void M22(double s = double.MinValue);
    void M23(double s = 1);
    void M24(double s = double.MaxValue);
}

class C : I
{
    public void M01(short s = short.MinValue)
    {
        throw new NotImplementedException();
    }

    public void M02(short s = -1)
    {
        throw new NotImplementedException();
    }

    public void M03(short s = short.MaxValue)
    {
        throw new NotImplementedException();
    }

    public void M04(ushort s = 0)
    {
        throw new NotImplementedException();
    }

    public void M05(ushort s = 1)
    {
        throw new NotImplementedException();
    }

    public void M06(ushort s = ushort.MaxValue)
    {
        throw new NotImplementedException();
    }

    public void M07(int s = int.MinValue)
    {
        throw new NotImplementedException();
    }

    public void M08(int s = -1)
    {
        throw new NotImplementedException();
    }

    public void M09(int s = int.MaxValue)
    {
        throw new NotImplementedException();
    }

    public void M10(uint s = 0)
    {
        throw new NotImplementedException();
    }

    public void M11(uint s = 1)
    {
        throw new NotImplementedException();
    }

    public void M12(uint s = uint.MaxValue)
    {
        throw new NotImplementedException();
    }

    public void M13(long s = long.MinValue)
    {
        throw new NotImplementedException();
    }

    public void M14(long s = -1)
    {
        throw new NotImplementedException();
    }

    public void M15(long s = long.MaxValue)
    {
        throw new NotImplementedException();
    }

    public void M16(ulong s = 0)
    {
        throw new NotImplementedException();
    }

    public void M17(ulong s = 1)
    {
        throw new NotImplementedException();
    }

    public void M18(ulong s = ulong.MaxValue)
    {
        throw new NotImplementedException();
    }

    public void M19(float s = float.MinValue)
    {
        throw new NotImplementedException();
    }

    public void M20(float s = 1)
    {
        throw new NotImplementedException();
    }

    public void M21(float s = float.MaxValue)
    {
        throw new NotImplementedException();
    }

    public void M22(double s = double.MinValue)
    {
        throw new NotImplementedException();
    }

    public void M23(double s = 1)
    {
        throw new NotImplementedException();
    }

    public void M24(double s = double.MaxValue)
    {
        throw new NotImplementedException();
    }
}",
compareTokens: false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestEnumLiterals()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

enum E
{
   A = 1,
   B = 2  
}

[FlagsAttribute]
enum FlagE
{
   A = 1,
   B = 2
}

interface I
{
    void M1(E e = E.A | E.B);
    void M2(FlagE e = FlagE.A | FlagE.B);
}

class C : [|I|]
{
}",
@"using System;

enum E
{
   A = 1,
   B = 2  
}

[FlagsAttribute]
enum FlagE
{
   A = 1,
   B = 2
}

interface I
{
    void M1(E e = E.A | E.B);
    void M2(FlagE e = FlagE.A | FlagE.B);
}

class C : I
{
    public void M1(E e = (E)3)
    {
        throw new NotImplementedException();
    }

    public void M2(FlagE e = FlagE.A | FlagE.B)
    {
        throw new NotImplementedException();
    }
}",
compareTokens: false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestCharLiterals()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

interface I
{
    void M01(char c = '\0');
    void M02(char c = '\r');
    void M03(char c = '\n');
    void M04(char c = '\t');
    void M05(char c = '\b');
    void M06(char c = '\v');
    void M07(char c = '\'');
    void M08(char c = '“');
    void M09(char c = 'a');
    void M10(char c = '""');
    void M11(char c = '\u2029');
}

class C : [|I|]
{
}",
@"using System;

interface I
{
    void M01(char c = '\0');
    void M02(char c = '\r');
    void M03(char c = '\n');
    void M04(char c = '\t');
    void M05(char c = '\b');
    void M06(char c = '\v');
    void M07(char c = '\'');
    void M08(char c = '“');
    void M09(char c = 'a');
    void M10(char c = '""');
    void M11(char c = '\u2029');
}

class C : I
{
    public void M01(char c = '\0')
    {
        throw new NotImplementedException();
    }

    public void M02(char c = '\r')
    {
        throw new NotImplementedException();
    }

    public void M03(char c = '\n')
    {
        throw new NotImplementedException();
    }

    public void M04(char c = '\t')
    {
        throw new NotImplementedException();
    }

    public void M05(char c = '\b')
    {
        throw new NotImplementedException();
    }

    public void M06(char c = '\v')
    {
        throw new NotImplementedException();
    }

    public void M07(char c = '\'')
    {
        throw new NotImplementedException();
    }

    public void M08(char c = '“')
    {
        throw new NotImplementedException();
    }

    public void M09(char c = 'a')
    {
        throw new NotImplementedException();
    }

    public void M10(char c = '""')
    {
        throw new NotImplementedException();
    }

    public void M11(char c = '\u2029')
    {
        throw new NotImplementedException();
    }
}",
compareTokens: false);
        }

        [WorkItem(545695, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545695")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestRemoveParenthesesAroundTypeReference1()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

interface I
{
    void Foo(DayOfWeek x = DayOfWeek.Friday);
}

class C : [|I|]
{
    DayOfWeek DayOfWeek { get; set; }
}",
@"using System;

interface I
{
    void Foo(DayOfWeek x = DayOfWeek.Friday);
}

class C : I
{
    DayOfWeek DayOfWeek { get; set; }

    public void Foo(DayOfWeek x = DayOfWeek.Friday)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(545696, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545696")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestDecimalConstants1()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    void Foo(decimal x = decimal.MaxValue);
}

class C : [|I|]
{
}",
@"using System;

interface I
{
    void Foo(decimal x = decimal.MaxValue);
}

class C : I
{
    public void Foo(decimal x = decimal.MaxValue)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(545711, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545711")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestNullablePrimitiveLiteral()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    void Foo(decimal? x = decimal.MaxValue);
}

class C : [|I|]
{
}",
@"using System;

interface I
{
    void Foo(decimal? x = decimal.MaxValue);
}

class C : I
{
    public void Foo(decimal? x = decimal.MaxValue)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(545715, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545715")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestNullableEnumType()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

interface I
{
    void Foo(DayOfWeek? x = DayOfWeek.Friday);
}

class C : [|I|]
{
}",
@"using System;

interface I
{
    void Foo(DayOfWeek? x = DayOfWeek.Friday);
}

class C : I
{
    public void Foo(DayOfWeek? x = DayOfWeek.Friday)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(545752, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545752")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestByteLiterals()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    void Foo(byte x = 1);
}

class C : [|I|]
{
}",
@"using System;

interface I
{
    void Foo(byte x = 1);
}

class C : I
{
    public void Foo(byte x = 1)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(545736, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545736")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestCastedOptionalParameter1()
        {
            const string code = @"
using System;
interface I
{
    void Foo(ConsoleColor x = (ConsoleColor)(-1));
}

class C : [|I|]
{
}";

            const string expected = @"
using System;
interface I
{
    void Foo(ConsoleColor x = (ConsoleColor)(-1));
}

class C : I
{
    public void Foo(ConsoleColor x = (ConsoleColor)(-1))
    {
        throw new NotImplementedException();
    }
}";

            await TestWithAllCodeStyleOptionsOffAsync(code, expected, compareTokens: false);
        }

        [WorkItem(545737, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545737")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestCastedEnumValue()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

interface I
{
    void Foo(ConsoleColor x = (ConsoleColor)int.MaxValue);
}

class C : [|I|]
{
}",
@"using System;

interface I
{
    void Foo(ConsoleColor x = (ConsoleColor)int.MaxValue);
}

class C : I
{
    public void Foo(ConsoleColor x = (ConsoleColor)int.MaxValue)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(545785, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545785")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestNoCastFromZeroToEnum()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"enum E
{
    A = 1,
}

interface I
{
    void Foo(E x = 0);
}

class C : [|I|]
{
}",
@"using System;

enum E
{
    A = 1,
}

interface I
{
    void Foo(E x = 0);
}

class C : I
{
    public void Foo(E x = 0)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(545793, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545793")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestMultiDimArray()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System.Runtime.InteropServices;

interface I
{
    void Foo([Optional][DefaultParameterValue(1)] int x, int[,] y);
}

class C : [|I|]
{
}",
@"using System;
using System.Runtime.InteropServices;

interface I
{
    void Foo([Optional][DefaultParameterValue(1)] int x, int[,] y);
}

class C : I
{
    public void Foo([DefaultParameterValue(1), Optional] int x = 1, int[,] y = null)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(545794, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545794")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestParametersAfterOptionalParameter()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System.Runtime.InteropServices;

interface I
{
    void Foo([Optional, DefaultParameterValue(1)] int x, int[] y, int[] z);
}

class C : [|I|]
{
}",
@"using System;
using System.Runtime.InteropServices;

interface I
{
    void Foo([Optional, DefaultParameterValue(1)] int x, int[] y, int[] z);
}

class C : I
{
    public void Foo([DefaultParameterValue(1), Optional] int x = 1, int[] y = null, int[] z = null)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(545605, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545605")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestAttributeInParameter()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

interface I
{
    void Foo([Optional][DateTimeConstant(100)] DateTime d1, [Optional][IUnknownConstant] object d2);
}
class C : [|I|]
{
}
",
@"using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

interface I
{
    void Foo([Optional][DateTimeConstant(100)] DateTime d1, [Optional][IUnknownConstant] object d2);
}
class C : I
{
    public void Foo([DateTimeConstant(100), Optional] DateTime d1, [IUnknownConstant, Optional] object d2)
    {
        throw new NotImplementedException();
    }
}
",
compareTokens: false);
        }

        [WorkItem(545897, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545897")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestNameConflictBetweenMethodAndTypeParameter()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I<S>
{
    void T1<T>(S x, T y);
}

class C<T> : [|I<T>|]
{
}",
@"using System;

interface I<S>
{
    void T1<T>(S x, T y);
}

class C<T> : I<T>
{
    public void T1<T2>(T x, T2 y)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(545895, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545895")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestTypeParameterReplacementWithOuterType()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System.Collections.Generic;

interface I<S>
{
    void Foo<T>(S y, List<T>.Enumerator x);
}

class D<T> : [|I<T>|]
{
}",
@"using System;
using System.Collections.Generic;

interface I<S>
{
    void Foo<T>(S y, List<T>.Enumerator x);
}

class D<T> : I<T>
{
    public void Foo<T1>(T y, List<T1>.Enumerator x)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(545864, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545864")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestFloatConstant()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    void Foo(float x = 1E10F);
}

class C : [|I|]
{
}",
@"using System;

interface I
{
    void Foo(float x = 1E10F);
}

class C : I
{
    public void Foo(float x = 1E+10F)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(544640, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544640")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestKeywordForTypeParameterName()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    void Foo<@class>();
}

class C : [|I|]",
@"using System;

interface I
{
    void Foo<@class>();
}

class C : I
{
    public void Foo<@class>()
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(545922, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545922")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestExtremeDecimals()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    void Foo1(decimal x = 1E28M);
    void Foo2(decimal x = -1E28M);
}

class C : [|I|]
{
}",
@"using System;

interface I
{
    void Foo1(decimal x = 1E28M);
    void Foo2(decimal x = -1E28M);
}

class C : I
{
    public void Foo1(decimal x = 10000000000000000000000000000M)
    {
        throw new NotImplementedException();
    }

    public void Foo2(decimal x = -10000000000000000000000000000M)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(544659, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544659")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestNonZeroScaleDecimals()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    void Foo(decimal x = 0.1M);
}

class C : [|I|]
{
}",
@"using System;

interface I
{
    void Foo(decimal x = 0.1M);
}

class C : I
{
    public void Foo(decimal x = 0.1M)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(544639, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544639")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestUnterminatedComment()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;
 
// Implement interface
class C : [|IServiceProvider|] /*
",
@"using System;

// Implement interface
class C : IServiceProvider /*
*/
{
    public object GetService(Type serviceType)
    {
        throw new NotImplementedException();
    }
}
", compareTokens: false);
        }

        [WorkItem(529920, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/529920")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestNewLineBeforeDirective()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;
 
// Implement interface
class C : [|IServiceProvider|]
#pragma warning disable
",
@"using System;

// Implement interface
class C : IServiceProvider
{
    public object GetService(Type serviceType)
    {
        throw new NotImplementedException();
    }
}
#pragma warning disable
", compareTokens: false);
        }

        [WorkItem(529947, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/529947")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestCommentAfterInterfaceList1()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;
 
class C : [|IServiceProvider|] // Implement interface
",
@"using System;

class C : IServiceProvider // Implement interface
{
    public object GetService(Type serviceType)
    {
        throw new NotImplementedException();
    }
}
", compareTokens: false);
        }

        [WorkItem(529947, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/529947")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestCommentAfterInterfaceList2()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;
 
class C : [|IServiceProvider|] 
// Implement interface
",
@"using System;

class C : IServiceProvider
{
    public object GetService(Type serviceType)
    {
        throw new NotImplementedException();
    }
}
// Implement interface
", compareTokens: false);
        }

        [WorkItem(994456, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/994456")]
        [WorkItem(958699, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/958699")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementIDisposable_NoDisposePattern()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;
class C : [|IDisposable|]",
@"using System;
class C : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
", index: 0, compareTokens: false);
        }

        [WorkItem(994456, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/994456")]
        [WorkItem(958699, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/958699")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementIDisposable_DisposePattern()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;
class C : [|IDisposable|]",
$@"using System;
class C : IDisposable
{{
{DisposePattern("protected virtual ", "C", "public void ")}
}}
", index: 1, compareTokens: false);
        }

        [WorkItem(994456, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/994456")]
        [WorkItem(958699, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/958699")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementIDisposableExplicitly_NoDisposePattern()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;
class C : [|IDisposable|]",
@"using System;
class C : IDisposable
{
    void IDisposable.Dispose()
    {
        throw new NotImplementedException();
    }
}
", index: 2, compareTokens: false);
        }

        [WorkItem(994456, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/994456")]
        [WorkItem(941469, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/941469")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementIDisposableExplicitly_DisposePattern()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;
class C : [|System.IDisposable|]
{
    class IDisposable
    {
    }
}",
$@"using System;
class C : System.IDisposable
{{
    class IDisposable
    {{
    }}

{DisposePattern("protected virtual ", "C", "void System.IDisposable.")}
}}", index: 3, compareTokens: false);
        }

        [WorkItem(994456, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/994456")]
        [WorkItem(958699, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/958699")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementIDisposableAbstractly_NoDisposePattern()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;
abstract class C : [|IDisposable|]",
@"using System;
abstract class C : IDisposable
{
    public abstract void Dispose();
}
", index: 2, compareTokens: false);
        }

        [WorkItem(994456, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/994456")]
        [WorkItem(958699, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/958699")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementIDisposableThroughMember_NoDisposePattern()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;
class C : [|IDisposable|]
{
    private IDisposable foo;
}",
@"using System;
class C : IDisposable
{
    private IDisposable foo;

    public void Dispose()
    {
        foo.Dispose();
    }
}", index: 2, compareTokens: false);
        }

        [WorkItem(941469, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/941469")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementIDisposableExplicitly_NoNamespaceImportForSystem()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"class C : [|System.IDisposable|]",
$@"class C : System.IDisposable
{{
{DisposePattern("protected virtual ", "C", "void System.IDisposable.")}
}}
", index: 3, compareTokens: false);
        }

        [WorkItem(951968, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/951968")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementIDisposableViaBaseInterface_NoDisposePattern()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;
interface I : IDisposable
{
    void F();
}
class C : [|I|]
{
}",
@"using System;
interface I : IDisposable
{
    void F();
}
class C : I
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void F()
    {
        throw new NotImplementedException();
    }
}", index: 0, compareTokens: false);
        }

        [WorkItem(951968, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/951968")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementIDisposableViaBaseInterface()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;
interface I : IDisposable
{
    void F();
}
class C : [|I|]
{
}",
$@"using System;
interface I : IDisposable
{{
    void F();
}}
class C : I
{{
    public void F()
    {{
        throw new NotImplementedException();
    }}

{DisposePattern("protected virtual ", "C", "public void ")}
}}", index: 1, compareTokens: false);
        }

        [WorkItem(951968, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/951968")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementIDisposableExplicitlyViaBaseInterface()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;
interface I : IDisposable
{
    void F();
}
class C : [|I|]
{
}",
$@"using System;
interface I : IDisposable
{{
    void F();
}}
class C : I
{{
    void I.F()
    {{
        throw new NotImplementedException();
    }}

{DisposePattern("protected virtual ", "C", "void IDisposable.")}
}}", index: 3, compareTokens: false);
        }

        [WorkItem(941469, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/941469")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestDontImplementDisposePatternForLocallyDefinedIDisposable()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"namespace System
{
    interface IDisposable
    {
        void Dispose();
    }

    class C : [|IDisposable|]
}",
@"namespace System
{
    interface IDisposable
    {
        void Dispose();
    }

    class C : IDisposable
    {
        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }
    }
}", index: 1, compareTokens: false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestDontImplementDisposePatternForStructures1()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;
struct S : [|IDisposable|]",
@"using System;
struct S : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
", compareTokens: false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestDontImplementDisposePatternForStructures2()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;
struct S : [|IDisposable|]",
@"using System;
struct S : IDisposable
{
    void IDisposable.Dispose()
    {
        throw new NotImplementedException();
    }
}
", index: 1, compareTokens: false);
        }

        [WorkItem(545924, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545924")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestEnumNestedInGeneric()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"class C<T>
{
    public enum E
    {
        X
    }
}

interface I
{
    void Foo<T>(C<T>.E x = C<T>.E.X);
}

class D : [|I|]
{
}",
@"using System;

class C<T>
{
    public enum E
    {
        X
    }
}

interface I
{
    void Foo<T>(C<T>.E x = C<T>.E.X);
}

class D : I
{
    public void Foo<T>(C<T>.E x = C<T>.E.X)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(545939, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545939")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestUnterminatedString1()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

class C : [|IServiceProvider|] @""",
@"using System;

class C : IServiceProvider @"""" {
    public object GetService(Type serviceType)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(545939, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545939")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestUnterminatedString2()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

class C : [|IServiceProvider|] """,
@"using System;

class C : IServiceProvider """" {
    public object GetService(Type serviceType)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(545939, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545939")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestUnterminatedString3()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

class C : [|IServiceProvider|] @""",
@"using System;

class C : IServiceProvider @"""" {
    public object GetService(Type serviceType)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(545939, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545939")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestUnterminatedString4()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

class C : [|IServiceProvider|] """,
@"using System;

class C : IServiceProvider """" {
    public object GetService(Type serviceType)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(545940, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545940")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestDecimalENotation()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    void Foo1(decimal x = 1E-25M);
    void Foo2(decimal x = -1E-25M);
    void Foo3(decimal x = 1E-24M);
    void Foo4(decimal x = -1E-24M);
}

class C : [|I|]
{
}",
@"using System;

interface I
{
    void Foo1(decimal x = 1E-25M);
    void Foo2(decimal x = -1E-25M);
    void Foo3(decimal x = 1E-24M);
    void Foo4(decimal x = -1E-24M);
}

class C : I
{
    public void Foo1(decimal x = 0.0000000000000000000000001M)
    {
        throw new NotImplementedException();
    }

    public void Foo2(decimal x = -0.0000000000000000000000001M)
    {
        throw new NotImplementedException();
    }

    public void Foo3(decimal x = 0.000000000000000000000001M)
    {
        throw new NotImplementedException();
    }

    public void Foo4(decimal x = -0.000000000000000000000001M)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(545938, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545938")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestGenericEnumWithRenamedTypeParameters()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"class C<T>
{
    public enum E
    {
        X
    }
}

interface I<S>
{
    void Foo<T>(S y, C<T>.E x = C<T>.E.X);
}

class D<T> : [|I<T>|]
{
}",
@"using System;

class C<T>
{
    public enum E
    {
        X
    }
}

interface I<S>
{
    void Foo<T>(S y, C<T>.E x = C<T>.E.X);
}

class D<T> : I<T>
{
    public void Foo<T1>(T y, C<T1>.E x = C<T1>.E.X)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(545919, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545919")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestDoNotRenameTypeParameterToParameterName()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I<S>
{
    void Foo<T>(S T1);
}

class C<T> : [|I<T>|]
{
}",
@"using System;

interface I<S>
{
    void Foo<T>(S T1);
}

class C<T> : I<T>
{
    public void Foo<T2>(T T1)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(530265, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/530265")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestAttributes()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System.Runtime.InteropServices;

interface I
{
    [return: MarshalAs(UnmanagedType.U1)]
    bool Foo([MarshalAs(UnmanagedType.U1)] bool x);
}

class C : [|I|]
{
}",
@"using System;
using System.Runtime.InteropServices;

interface I
{
    [return: MarshalAs(UnmanagedType.U1)]
    bool Foo([MarshalAs(UnmanagedType.U1)] bool x);
}

class C : I
{
    [return: MarshalAs(UnmanagedType.U1)]
    public bool Foo([MarshalAs(UnmanagedType.U1)] bool x)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(530265, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/530265")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestAttributesExplicit()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System.Runtime.InteropServices;

interface I
{
    [return: MarshalAs(UnmanagedType.U1)]
    bool Foo([MarshalAs(UnmanagedType.U1)] bool x);
}

class C : [|I|]
{
}",
@"using System;
using System.Runtime.InteropServices;

interface I
{
    [return: MarshalAs(UnmanagedType.U1)]
    bool Foo([MarshalAs(UnmanagedType.U1)] bool x);
}

class C : I
{
    bool I.Foo(bool x)
    {
        throw new NotImplementedException();
    }
}",
index: 1);
        }

        [WorkItem(546443, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/546443")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestParameterNameWithTypeName()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

interface IFoo
{
    void Bar(DateTime DateTime);
}

class C : [|IFoo|]
{
}",
@"using System;

interface IFoo
{
    void Bar(DateTime DateTime);
}

class C : IFoo
{
    public void Bar(DateTime DateTime)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(530521, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/530521")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestUnboundGeneric()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System.Collections.Generic;
using System.Runtime.InteropServices;

interface I
{
    [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(List<>))]
    void Foo();
}

class C : [|I|]
{
}",
@"using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

interface I
{
    [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(List<>))]
    void Foo();
}

class C : I
{
    [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(List<>))]
    public void Foo()
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(752436, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/752436")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestQualifiedNameImplicitInterface()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"namespace N
{
    public interface I
    {
        void M();
    }
}

class C : [|N.I|]
{
}",
@"using System;

namespace N
{
    public interface I
    {
        void M();
    }
}

class C : N.I
{
    public void M()
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(752436, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/752436")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestQualifiedNameExplicitInterface()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"namespace N
{
    public interface I
    {
        void M();
    }
}

class C : [|N.I|]
{
}",
@"using System;
using N;

namespace N
{
    public interface I
    {
        void M();
    }
}

class C : N.I
{
    void I.M()
    {
        throw new NotImplementedException();
    }
}", index: 1);
        }

        [WorkItem(847464, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/847464")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementInterfaceForPartialType()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"public interface I
{
    void Foo();
}

partial class C
{
}

partial class C : [|I|]
{
}",
@"using System;

public interface I
{
    void Foo();
}

partial class C
{
}

partial class C : I
{
    void I.Foo()
    {
        throw new NotImplementedException();
    }
}", index: 1);
        }

        [WorkItem(847464, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/847464")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementInterfaceForPartialType2()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"public interface I
{
    void Foo();
}

partial class C : [|I|]
{
}

partial class C
{
}",
@"using System;

public interface I
{
    void Foo();
}

partial class C : I
{
    void I.Foo()
    {
        throw new NotImplementedException();
    }
}

partial class C
{
}", index: 1);
        }

        [WorkItem(847464, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/847464")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementInterfaceForPartialType3()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"public interface I
{
    void Foo();
}

public interface I2
{
    void Foo2();
}

partial class C : [|I|]
{
}

partial class C : I2
{
}",
@"using System;

public interface I
{
    void Foo();
}

public interface I2
{
    void Foo2();
}

partial class C : I
{
    void I.Foo()
    {
        throw new NotImplementedException();
    }
}

partial class C : I2
{
}", index: 1);
        }

        [WorkItem(752447, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/752447")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestExplicitImplOfIndexedProperty()
        {
            var initial = @"
<Workspace>
    <Project Language=""Visual Basic"" AssemblyName=""Assembly1"" CommonReferences=""true"">
        <Document>
Public Interface IFoo
    Property IndexProp(ByVal p1 As Integer) As String
End Interface
        </Document>
    </Project>
    <Project Language=""C#"" AssemblyName=""Assembly2"" CommonReferences=""true"">
        <ProjectReference>Assembly1</ProjectReference>
        <Document>
public class Test : [|IFoo|]
{
}
        </Document>
    </Project>
</Workspace>";

            var expected = @"
using System;

public class Test : IFoo
{
    string IFoo.get_IndexProp(int p1)
    {
        throw new NotImplementedException();
    }

    void IFoo.set_IndexProp(int p1, string Value)
    {
        throw new NotImplementedException();
    }
}
";

            await TestWithAllCodeStyleOptionsOffAsync(initial, expected, index: 1);
        }

        [WorkItem(602475, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/602475")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplicitImplOfIndexedProperty()
        {
            var initial = @"
<Workspace>
    <Project Language=""Visual Basic"" AssemblyName=""Assembly1"" CommonReferences=""true"">
        <Document>
Public Interface I
    Property P(x As Integer)
End Interface
        </Document>
    </Project>
    <Project Language=""C#"" AssemblyName=""Assembly2"" CommonReferences=""true"">
        <ProjectReference>Assembly1</ProjectReference>
        <Document>
using System;

class C : [|I|]
{
}
        </Document>
    </Project>
</Workspace>";

            var expected = @"
using System;

class C : I
{
    public object get_P(int x)
    {
        throw new NotImplementedException();
    }
 
    public void set_P(int x, object Value)
    {
        throw new NotImplementedException();
    }
}
";

            await TestWithAllCodeStyleOptionsOffAsync(initial, expected, index: 0);
        }

#if false
        [WorkItem(13677)]
        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestNoGenerateInVenusCase2()
        {
            await TestMissingAsync(
@"using System;
#line 1 ""Bar""
class Foo : [|IComparable|]
#line default
#line hidden");
        }
#endif

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementInterfaceForImplicitIDisposable()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

class Program : [|IDisposable|]
{
}",
$@"
using System;

class Program : IDisposable
{{
{DisposePattern("protected virtual ", "C", "public void ")}
}}
", index: 1);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementInterfaceForExplicitIDisposable()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

class Program : [|IDisposable|]
{
    private bool DisposedValue;
}",
$@"
using System;

class Program : IDisposable
{{
    private bool DisposedValue;
{DisposePattern("protected virtual ", "Program", "void IDisposable.")}
}}
", index: 3);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementInterfaceForIDisposableNonApplicable1()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

class Program : [|IDisposable|]
{
    private bool disposedValue;
}",
@"using System;

class Program : IDisposable
{
    private bool disposedValue;

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}", index: 0);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementInterfaceForIDisposableNonApplicable2()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

class Program : [|IDisposable|]
{
    public void Dispose(bool flag)
    {
    }
}",
@"using System;

class Program : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void Dispose(bool flag)
    {
    }
}", index: 0);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementInterfaceForExplicitIDisposableWithSealedClass()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

sealed class Program : [|IDisposable|]
{
}",
$@"
using System;

sealed class Program : IDisposable
{{
{DisposePattern("", "Program", "void IDisposable.")}
}}
", index: 3);
        }

        [WorkItem(939123, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/939123")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestNoComAliasNameAttributeOnMethodParameters()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    void M([System.Runtime.InteropServices.ComAliasName(""pAlias"")] int p);
}

class C : [|I|]
{
}",
@"using System;

interface I
{
    void M([System.Runtime.InteropServices.ComAliasName(""pAlias"")] int p);
}

class C : I
{
    public void M(int p)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(939123, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/939123")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestNoComAliasNameAttributeOnMethodReturnType()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System.Runtime.InteropServices;

interface I
{
    [return: ComAliasName(""pAlias1"")]
    long M([ComAliasName(""pAlias2"")] int p);
}

class C : [|I|]
{
}",
@"using System;
using System.Runtime.InteropServices;

interface I
{
    [return: ComAliasName(""pAlias1"")]
    long M([ComAliasName(""pAlias2"")] int p);
}

class C : I
{
    public long M(int p)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(939123, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/939123")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestNoComAliasNameAttributeOnIndexerParameters()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I
{
    long this[[System.Runtime.InteropServices.ComAliasName(""pAlias"")] int p] { get; }
}

class C : [|I|]
{
}",
@"using System;

interface I
{
    long this[[System.Runtime.InteropServices.ComAliasName(""pAlias"")] int p] { get; }
}

class C : I
{
    public long this[int p]
    {
        get
        {
            throw new NotImplementedException();
        }
    }
}");
        }

        [WorkItem(947819, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/947819")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestMissingOpenBrace()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"namespace Scenarios
{
    public interface TestInterface
    {
        void M1();
    }

    struct TestStruct1 : [|TestInterface|]


    // Comment
}",
@"using System;

namespace Scenarios
{
    public interface TestInterface
    {
        void M1();
    }

    struct TestStruct1 : TestInterface
    {
        public void M1()
        {
            throw new NotImplementedException();
        }
    }
    // Comment
}");
        }

        [WorkItem(994328, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/994328")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestDisposePatternWhenAdditionalUsingsAreIntroduced1()
        {
            //CSharpFeaturesResources.DisposePattern
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I<T, U> : System.IDisposable, System.IEquatable<int> where U : T
{
    System.Collections.Generic.List<U> M(System.Collections.Generic.Dictionary<T, System.Collections.Generic.List<U>> a, T b, U c);
    System.Collections.Generic.List<UU> M<TT, UU>(System.Collections.Generic.Dictionary<TT, System.Collections.Generic.List<UU>> a, TT b, UU c) where UU : TT;
}

partial class C
{
}

partial class C : [|I<System.Exception, System.AggregateException>|], System.IDisposable
{
}",
$@"using System;
using System.Collections.Generic;

interface I<T, U> : System.IDisposable, System.IEquatable<int> where U : T
{{
    System.Collections.Generic.List<U> M(System.Collections.Generic.Dictionary<T, System.Collections.Generic.List<U>> a, T b, U c);
    System.Collections.Generic.List<UU> M<TT, UU>(System.Collections.Generic.Dictionary<TT, System.Collections.Generic.List<UU>> a, TT b, UU c) where UU : TT;
}}

partial class C
{{
}}

partial class C : I<System.Exception, System.AggregateException>, System.IDisposable
{{
    public bool Equals(int other)
    {{
        throw new NotImplementedException();
    }}

    public List<AggregateException> M(Dictionary<Exception, List<AggregateException>> a, Exception b, AggregateException c)
    {{
        throw new NotImplementedException();
    }}

    public List<UU> M<TT, UU>(Dictionary<TT, List<UU>> a, TT b, UU c) where UU : TT
    {{
        throw new NotImplementedException();
    }}

{DisposePattern("protected virtual ", "C", "public void ")}
}}", index: 1, compareTokens: false);
        }

        [WorkItem(994328, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/994328")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestDisposePatternWhenAdditionalUsingsAreIntroduced2()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface I<T, U> : System.IDisposable, System.IEquatable<int> where U : T
{
    System.Collections.Generic.List<U> M(System.Collections.Generic.Dictionary<T, System.Collections.Generic.List<U>> a, T b, U c);
    System.Collections.Generic.List<UU> M<TT, UU>(System.Collections.Generic.Dictionary<TT, System.Collections.Generic.List<UU>> a, TT b, UU c) where UU : TT;
}

partial class C : [|I<System.Exception, System.AggregateException>|], System.IDisposable
{
}

partial class C
{
}",
$@"using System;
using System.Collections.Generic;

interface I<T, U> : System.IDisposable, System.IEquatable<int> where U : T
{{
    System.Collections.Generic.List<U> M(System.Collections.Generic.Dictionary<T, System.Collections.Generic.List<U>> a, T b, U c);
    System.Collections.Generic.List<UU> M<TT, UU>(System.Collections.Generic.Dictionary<TT, System.Collections.Generic.List<UU>> a, TT b, UU c) where UU : TT;
}}

partial class C : I<System.Exception, System.AggregateException>, System.IDisposable
{{
    bool IEquatable<int>.Equals(int other)
    {{
        throw new NotImplementedException();
    }}

    List<AggregateException> I<Exception, AggregateException>.M(Dictionary<Exception, List<AggregateException>> a, Exception b, AggregateException c)
    {{
        throw new NotImplementedException();
    }}

    List<UU> I<Exception, AggregateException>.M<TT, UU>(Dictionary<TT, List<UU>> a, TT b, UU c)
    {{
        throw new NotImplementedException();
    }}

{DisposePattern("protected virtual ", "C", "void IDisposable.")}
}}

partial class C
{{
}}", index: 3, compareTokens: false);
        }

        private static string DisposePattern(string disposeVisibility, string className, string implementationVisibility)
        {
            return $@"    #region IDisposable Support
    private bool disposedValue = false; // {FeaturesResources.To_detect_redundant_calls}

    {disposeVisibility}void Dispose(bool disposing)
    {{
        if (!disposedValue)
        {{
            if (disposing)
            {{
                // {FeaturesResources.TODO_colon_dispose_managed_state_managed_objects}
            }}

            // {CSharpFeaturesResources.TODO_colon_free_unmanaged_resources_unmanaged_objects_and_override_a_finalizer_below}
            // {FeaturesResources.TODO_colon_set_large_fields_to_null}

            disposedValue = true;
        }}
    }}

    // {CSharpFeaturesResources.TODO_colon_override_a_finalizer_only_if_Dispose_bool_disposing_above_has_code_to_free_unmanaged_resources}
    // ~{className}() {{
    //   // {CSharpFeaturesResources.Do_not_change_this_code_Put_cleanup_code_in_Dispose_bool_disposing_above}
    //   Dispose(false);
    // }}

    // {CSharpFeaturesResources.This_code_added_to_correctly_implement_the_disposable_pattern}
    {implementationVisibility}Dispose()
    {{
        // {CSharpFeaturesResources.Do_not_change_this_code_Put_cleanup_code_in_Dispose_bool_disposing_above}
        Dispose(true);
        // {CSharpFeaturesResources.TODO_colon_uncomment_the_following_line_if_the_finalizer_is_overridden_above}
        // GC.SuppressFinalize(this);
    }}
    #endregion";
        }

        [WorkItem(1132014, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1132014")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestInaccessibleAttributes()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;

public class Foo : [|Holder.SomeInterface|]
{
}

public class Holder
{
    public interface SomeInterface
    {
        void Something([SomeAttribute] string helloWorld);
    }

    private class SomeAttribute : Attribute
    {
    }
}",
@"using System;

public class Foo : Holder.SomeInterface
{
    public void Something(string helloWorld)
    {
        throw new NotImplementedException();
    }
}

public class Holder
{
    public interface SomeInterface
    {
        void Something([SomeAttribute] string helloWorld);
    }

    private class SomeAttribute : Attribute
    {
    }
}");
        }

        [WorkItem(2785, "https://github.com/dotnet/roslyn/issues/2785")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task TestImplementInterfaceThroughStaticMemberInGenericClass()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class Issue2785<T> : [|IList<object>|]
{
    private static List<object> innerList = new List<object>();
}",
@"using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class Issue2785<T> : IList<object>
{
    private static List<object> innerList = new List<object>();

    public object this[int index]
    {
        get
        {
            return ((IList<object>)innerList)[index];
        }

        set
        {
            ((IList<object>)innerList)[index] = value;
        }
    }

    public int Count
    {
        get
        {
            return ((IList<object>)innerList).Count;
        }
    }

    public bool IsReadOnly
    {
        get
        {
            return ((IList<object>)innerList).IsReadOnly;
        }
    }

    public void Add(object item)
    {
        ((IList<object>)innerList).Add(item);
    }

    public void Clear()
    {
        ((IList<object>)innerList).Clear();
    }

    public bool Contains(object item)
    {
        return ((IList<object>)innerList).Contains(item);
    }

    public void CopyTo(object[] array, int arrayIndex)
    {
        ((IList<object>)innerList).CopyTo(array, arrayIndex);
    }

    public IEnumerator<object> GetEnumerator()
    {
        return ((IList<object>)innerList).GetEnumerator();
    }

    public int IndexOf(object item)
    {
        return ((IList<object>)innerList).IndexOf(item);
    }

    public void Insert(int index, object item)
    {
        ((IList<object>)innerList).Insert(index, item);
    }

    public bool Remove(object item)
    {
        return ((IList<object>)innerList).Remove(item);
    }

    public void RemoveAt(int index)
    {
        ((IList<object>)innerList).RemoveAt(index);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IList<object>)innerList).GetEnumerator();
    }
}",
index: 1);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface), Test.Utilities.CompilerTrait(Test.Utilities.CompilerFeature.Tuples)]
        public async Task LongTuple()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IInterface
{
    (int, string, int, string, int, string, int, string) Method1((int, string, int, string, int, string, int, string) y);
}

class Class : [|IInterface|]
{
    (int, string) x;
}",
@"using System;

interface IInterface
{
    (int, string, int, string, int, string, int, string) Method1((int, string, int, string, int, string, int, string) y);
}

class Class : IInterface
{
    (int, string) x;

    public (int, string, int, string, int, string, int, string) Method1((int, string, int, string, int, string, int, string) y)
    {
        throw new NotImplementedException();
    }
}",
parseOptions: TestOptions.Regular, withScriptOption: true);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task LongTupleWithNames()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IInterface
{
    (int a, string b, int c, string d, int e, string f, int g, string h) Method1((int a, string b, int c, string d, int e, string f, int g, string h) y);
}

class Class : [|IInterface|]
{
    (int, string) x;
}",
@"using System;

interface IInterface
{
    (int a, string b, int c, string d, int e, string f, int g, string h) Method1((int a, string b, int c, string d, int e, string f, int g, string h) y);
}

class Class : IInterface
{
    (int, string) x;

    public (int a, string b, int c, string d, int e, string f, int g, string h) Method1((int a, string b, int c, string d, int e, string f, int g, string h) y)
    {
        throw new NotImplementedException();
    }
}",
parseOptions: TestOptions.Regular, withScriptOption: true);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task GenericWithTuple()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IInterface<TA, TB>
{
    (TA, TB) Method1((TA, TB) y);
}

class Class : [|IInterface<(int, string), int>|]
{
    (int, string) x;
}",
@"using System;

interface IInterface<TA, TB>
{
    (TA, TB) Method1((TA, TB) y);
}

class Class : IInterface<(int, string), int>
{
    (int, string) x;

    public ((int, string), int) Method1(((int, string), int) y)
    {
        throw new NotImplementedException();
    }
}",
parseOptions: TestOptions.Regular, withScriptOption: true);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsImplementInterface)]
        public async Task GenericWithTupleWithNamess()
        {
            await TestWithAllCodeStyleOptionsOffAsync(
@"interface IInterface<TA, TB>
{
    (TA a, TB b) Method1((TA a, TB b) y);
}

class Class : [|IInterface<(int, string), int>|]
{
    (int, string) x;
}",
@"using System;

interface IInterface<TA, TB>
{
    (TA a, TB b) Method1((TA a, TB b) y);
}

class Class : IInterface<(int, string), int>
{
    (int, string) x;

    public ((int, string) a, int b) Method1(((int, string) a, int b) y)
    {
        throw new NotImplementedException();
    }
}",
parseOptions: TestOptions.Regular, withScriptOption: true);
        }
    }
}
