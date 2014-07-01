using System;
using System.Configuration;
using System.Security.Cryptography;
using FunctionalProgramming.Monad;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace FunctionalProgramming.Tests
{
    class Turtle
    {
        public readonly string Name;
        public readonly Position Position;

        public Turtle(string name, Position position)
        {
            Name = name;
            Position = position;
        }

        public Turtle Copy(string name = null, Position position = null)
        {
            return new Turtle(name ?? Name, position ?? Position);
        }

        public override string ToString()
        {
            return string.Format("Turtle(Name={0},Position={1})", Name, Position);
        }
    }

    class Position
    {
        public readonly int X;
        public readonly int Y;

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Position Copy(int? x = null, int? y = null)
        {
            return new Position(x.HasValue ? x.Value : X, y.HasValue ? y.Value : Y);
        }

        public override string ToString()
        {
            return string.Format("Position(X={0},Y={1})", X, Y);
        }
    }

    [TestFixture]
    public class LensTests
    {
        private static readonly Lens<Turtle, string> TurtleName = new Lens<Turtle, string>((t, s) => t.Copy(name: s), t => t.Name);
        private static readonly Lens<Turtle, Position> TurtlePos = new Lens<Turtle, Position>((t, p) => t.Copy(position: p), t => t.Position);
        private static readonly Lens<Position, int> PosX = new Lens<Position, int>((p, x) => p.Copy(x: x), p => p.X);
        private static readonly Lens<Position, int> PosY = new Lens<Position, int>((p, y) => p.Copy(y: y), p => p.Y);
        private static readonly Lens<Turtle, int> TurtleX = TurtlePos.AndThen(PosX);
        private static readonly Lens<Turtle, int> TurtleY = TurtlePos.AndThen(PosY);

        [Test]
        public void LensCompositionTet()
        {
            var moveTurtle = from x in TurtleX.ModS(x => x + 10)
                             from y in TurtleY.ModS(y => y + 10)
                             select Tuple.Create(x, y);

            var turtle1 = new Turtle("bob", new Position(x: 10, y: -2));

            var result = moveTurtle.Run(turtle1);

            Console.WriteLine(result);

            Assert.AreEqual(result.Item2.Item1, 20);
            Assert.AreEqual(result.Item2.Item2, 8);
        }

        [Test]
        public void LensReadTest()
        {
            var nameAndPosition = from name in TurtleName.GetS()
                                  from x in TurtleX.GetS()
                                  from y in TurtleY.GetS()
                                  select Tuple.Create(name, x, y);

            var turtle1 = new Turtle("bob", new Position(x: 10, y: -2));

            var result = nameAndPosition.Eval(turtle1);

            Console.WriteLine(result);

            Assert.AreEqual("bob", result.Item1);
            Assert.AreEqual(10, result.Item2);
            Assert.AreEqual(-2, result.Item3);
        }
    }
}
