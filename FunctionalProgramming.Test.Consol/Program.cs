using System;
using System.Security.Cryptography.X509Certificates;
using FunctionalProgramming.Monad;

namespace FunctionalProgramming.Test.ConsoleApp
{
    class OOTurtle
    {
        public string Name { get; set; }
        public OOPosition Position { get; set; }

        public override string ToString()
        {
            return string.Format("OOTurtle(Name={0},Position={1})", Name, Position);
        }
    }

    class OOPosition
    {
        public int X { get; set; }
        public int Y { get; set; }

        public override string ToString()
        {
            return string.Format("(X={0},Y={1})", X, Y);
        }
    }

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
            return string.Format("(X={0},Y={1})", X, Y);
        }
    }

    class Program
    {
        private static readonly Lens<Turtle, string> TurtleName = new Lens<Turtle, string>((t, s) => t.Copy(name: s), t => t.Name);
        private static readonly Lens<Turtle, Position> TurtlePos = new Lens<Turtle, Position>((t, p) => t.Copy(position: p), t => t.Position);
        private static readonly Lens<Position, int> PosX = new Lens<Position, int>((p, x) => p.Copy(x: x), p => p.X);
        private static readonly Lens<Position, int> PosY = new Lens<Position, int>((p, y) => p.Copy(y: y), p => p.Y);
        private static readonly Lens<Turtle, int> TurtleX = TurtlePos.AndThen(PosX);
        private static readonly Lens<Turtle, int> TurtleY = TurtlePos.AndThen(PosY);

        static void Main(string[] args)
        {

        }

        static void MoveTurtleOO(OOTurtle t, int deltaX, int deltaY)
        {
            t.Position.X = t.Position.X + deltaX;
            t.Position.Y = t.Position.Y + deltaY;
        }

        static Turtle MoveTurtleCopy(Turtle t, int deltaX, int deltaY)
        {
            return t.Copy(position: t.Position.Copy(x: t.Position.X + deltaX, y: t.Position.Y + deltaY));
        }

        static Turtle MoveTurtle(Turtle t, int deltaX, int deltaY)
        {
            return (from x in TurtleX.ModS(x => x + deltaX)
                    from y in TurtleY.ModS(y => y + deltaY)
                    select Tuple.Create(x, y)).Run(t).Item1;
        }
    }
}
