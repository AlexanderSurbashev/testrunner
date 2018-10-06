﻿using TestRunner.Domain;
using System.Linq;

namespace TestRunner.Events
{

    /// <summary>
    /// Event handler that maintains <see cref="TestContext"/> state
    /// </summary>
    ///
    public class TestContextEventHandler : EventHandler
    {

        protected override void Handle(TestClassBeginEvent e)
        {
            TestContext.FullyQualifiedTestClassName = e.FullName;
        }


        protected override void Handle(TestClassEndEvent e)
        {
            TestContext.FullyQualifiedTestClassName = null;
        }


        protected override void Handle(TestBeginEvent e)
        {
            TestContext.TestName = e.Name;
        }


        protected override void Handle(TestEndEvent e)
        {
            TestContext.TestName = null;
            TestContext.CurrentTestOutcome = UnitTestOutcome.Unknown;
        }


        protected override void Handle(AssemblyInitializeMethodBeginEvent e)
        {
            TestContext.FullyQualifiedTestClassName = e.TestAssembly.TestClasses.First().FullName;
            TestContext.TestName = e.TestAssembly.TestClasses.First().TestMethods.First().Name;
            TestContext.CurrentTestOutcome = UnitTestOutcome.InProgress;
        }


        protected override void Handle(AssemblyInitializeMethodEndEvent e)
        {
            TestContext.FullyQualifiedTestClassName = null;
            TestContext.TestName = null;
            TestContext.CurrentTestOutcome = UnitTestOutcome.Unknown;
        }


        protected override void Handle(ClassInitializeMethodBeginEvent e)
        {
            TestContext.TestName = e.TestClass.TestMethods.First().Name;
            TestContext.CurrentTestOutcome = UnitTestOutcome.InProgress;
        }


        protected override void Handle(ClassInitializeMethodEndEvent e)
        {
            TestContext.TestName = null;
            TestContext.CurrentTestOutcome = UnitTestOutcome.Unknown;
        }


        protected override void Handle(TestInitializeMethodBeginEvent e)
        {
            TestContext.CurrentTestOutcome = UnitTestOutcome.InProgress;
        }


        protected override void Handle(TestMethodEndEvent e)
        {
            TestContext.CurrentTestOutcome = e.Success ? UnitTestOutcome.Passed : UnitTestOutcome.Failed;
        }

    }
}
