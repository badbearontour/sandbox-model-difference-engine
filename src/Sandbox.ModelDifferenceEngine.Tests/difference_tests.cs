﻿using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace Sandbox.ModelDifferenceEngine.Tests
{
    public class difference_tests
    {
        [Fact]
        public void simple_object()
        {
            var data = new A {Name = "Name"};

            var audit = new ModelDifference();
            var snapshot = audit.Snapshot(data);

            data.Name = "Change";

            var result = snapshot.GetChanges(data);
            Output.WriteLine(result);

            Assert.Equal(1, result.Count());
            Assert.Equal(".Name", result.ElementAt(0).Path);
        }

        [Fact]
        public void sub_object()
        {
            var data = new B
                {
                    A = new A {Name = "Name"}
                };

            var audit = new ModelDifference();
            var snapshot = audit.Snapshot(data);

            data.A.Name = "Change";

            var result = snapshot.GetChanges(data);
            Output.WriteLine(result);

            Assert.Equal(1, result.Count());
            Assert.Equal(".A.Name", result.ElementAt(0).Path);
        }

        [Fact]
        public void sub_collection()
        {
            var data = new C
                {
                    AList = new[] {new A {Name = "Name"}}
                };

            var audit = new ModelDifference();
            var snapshot = audit.Snapshot(data);

            data.AList.ElementAt(0).Name = "Change";

            var result = snapshot.GetChanges(data);
            Output.WriteLine(result);

            Assert.Equal(1, result.Count());
            Assert.Equal(".AList[0].Name", result.ElementAt(0).Path);
        }

        [Fact]
        public void looping_hierarchy()
        {
            var data = new D {Name = "Parent"};
            data.Children = new[] {new D {Name = "Child", Parent = data}};

            var audit = new ModelDifference();
            var snapshot = audit.Snapshot(data);

            data.Children.ElementAt(0).Name = "Change";

            var result = snapshot.GetChanges(data);
            Output.WriteLine(result);

            Assert.Equal(1, result.Count());
            Assert.Equal(".Children[0].Name", result.ElementAt(0).Path);
        }

        [Fact]
        public void remove_from_list()
        {
            var dataList = new List<A> {new A {Name = "one"}, new A {Name = "two"}};
            var data = new C
                {
                    AList = dataList
                };

            var audit = new ModelDifference();
            var snapshot = audit.Snapshot(data);

            dataList.RemoveAt(0);

            var result = snapshot.GetChanges(data);
            Output.WriteLine(result);

            Assert.Equal(1, result.Count());
            Assert.Equal(".AList[0]", result.ElementAt(0).Path);
        }

        [Fact]
        public void remove_from_list_change_other()
        {
            var dataList = new List<A> {new A {Name = "one"}, new A {Name = "two"}};
            var data = new C
                {
                    AList = dataList
                };

            var audit = new ModelDifference();
            var snapshot = audit.Snapshot(data);

            dataList.RemoveAt(0);
            dataList.ElementAt(0).Name = "Change";

            var result = snapshot.GetChanges(data);
            Output.WriteLine(result);

            Assert.Equal(2, result.Count());
            Assert.Equal(".AList[0]", result.ElementAt(0).Path);
            Assert.Equal(".AList[1].Name", result.ElementAt(1).Path);
        }

        [Fact]
        public void add_to_list()
        {
            var dataList = new List<A> {new A {Name = "one"}, new A {Name = "two"}};
            var data = new C
                {
                    AList = dataList
                };

            var audit = new ModelDifference();
            var snapshot = audit.Snapshot(data);

            dataList.Insert(0, new A {Name = "three"});

            var result = snapshot.GetChanges(data);
            Output.WriteLine(result);

            Assert.Equal(1, result.Count());
            Assert.Equal(".AList[2]", result.ElementAt(0).Path);
        }

        [Fact]
        public void add_to_list_self_referencing()
        {
            var data = new D {Name = "Parent"};
            data.Children = new[] {new D {Name = "one", Parent = data}};

            var audit = new ModelDifference();
            var snapshot = audit.Snapshot(data);

            data.Children = data.Children
                                .Concat(new[] {new D {Name = "two", Parent = data}});

            var result = snapshot.GetChanges(data);
            Output.WriteLine(result);

            Assert.Equal(1, result.Count());
            Assert.Equal(".Children[1]", result.ElementAt(0).Path);
        }

        [Fact]
        public void add_to_list_complex_type()
        {
            var dataList = new List<A> {new A {Name = "one"}, new A {Name = "two"}};
            var data = new C
                {
                    AList = dataList
                };

            var audit = new ModelDifference();
            var snapshot = audit.Snapshot(data);

            dataList.Insert(0, new B {Name = "three", A = new A {Name = "four"}});

            var result = snapshot.GetChanges(data);
            Output.WriteLine(result);

            Assert.Equal(1, result.Count());
            Assert.Equal(".AList[2]", result.ElementAt(0).Path);
        }

        [Fact]
        public void dictionaries()
        {
            var data = new Dictionary<string, A>
                {
                    {"one", new A {Name = "one"}}
                };

            var audit = new ModelDifference();
            var snapshot = audit.Snapshot(data);

            data["one"].Name = "Change";

            var result = snapshot.GetChanges(data);
            Output.WriteLine(result);

            Assert.Equal(1, result.Count());
            Assert.Equal("[0].Value.Name", result.ElementAt(0).Path);
        }

        class A
        {
            public string Name { get; set; }
        }

        class B : A
        {
            public A A { get; set; }
        }

        class C
        {
            public IEnumerable<A> AList { get; set; }
        }

        class D
        {
            public string Name { get; set; }

            public D Parent { get; set; }
            public IEnumerable<D> Children { get; set; }
        }
    }
}