using System;
using System.Linq;

namespace modeling_of_solids
{
    public partial class AtomicModel
    {
        /// <summary>
        /// Вычисление расстояния между частицами с учётом периодических граничных условий. 
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <param name="dxdydz"></param>
        /// <returns></returns>
        private double Separation(Vector vec1, Vector vec2, out Vector dxdydz) => Math.Sqrt(SeparationSqured(vec1, vec2, out dxdydz));

        /// <summary>
        /// Вычисление квадрата расстояния между частицами с учётом периодических граничных условий. 
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <param name="dxdydz"></param>
        /// <returns></returns>
        private double SeparationSqured(Vector vec1, Vector vec2, out Vector dxdydz)
        {
            dxdydz = vec1 - vec2;

            // Обеспечивает, что расстояние между частицами никогда не будет больше L/2.
            if (Math.Abs(dxdydz.X) > 0.5 * L)
                dxdydz.X -= Math.Sign(dxdydz.X) * L;
            if (Math.Abs(dxdydz.Y) > 0.5 * L)
                dxdydz.Y -= Math.Sign(dxdydz.Y) * L;
            if (Math.Abs(dxdydz.Z) > 0.5 * L)
                dxdydz.Z -= Math.Sign(dxdydz.Z) * L;

            return dxdydz.SquaredMagnitude;
        }

        /// <summary>
        /// Учёт периодических граничных условий.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        private Vector Periodic(Vector pos)
        {
            var newPos = Vector.Zero;

            if (pos.X > L) newPos.X = pos.X - L;
            else if (pos.X < 0) newPos.X = L + pos.X;
            else newPos.X = pos.X;

            if (pos.Y > L) newPos.Y = pos.Y - L;
            else if (pos.Y < 0) newPos.Y = L + pos.Y;
            else newPos.Y = pos.Y;

            if (pos.Z > L) newPos.Z = pos.Z - L;
            else if (pos.Z < 0) newPos.Z = L + pos.Z;
            else newPos.Z = pos.Z;

            return newPos;
        }

        /// <summary>
        /// Начальное смещение атомов.
        /// </summary>
        /// <param name="k">Коэффициент смещения.</param>
        public void AtomsDisplacement(double k)
        {
            Atoms.ForEach(atom =>
            {
                var displacement = (-1 * Vector.One + 2 * new Vector(_rnd.NextDouble(), _rnd.NextDouble(), _rnd.NextDouble())) * k * Lattice;
                atom.Position = Periodic(atom.Position + displacement);
            });
        }

        /// <summary>
        /// Зануление импульса системы.
        /// </summary>
        /// <param name="eps">Точность.</param>
        public void PulseZeroing(double eps = 1e-5)
        {
            Vector sum;
            while (true)
            {
                sum = Vector.Zero;
                Atoms.ForEach(atom => sum += atom.Velocity);
                sum /= CountAtoms;

                if (Math.Abs(sum.X + sum.Y + sum.Z) > eps)
                    Atoms.ForEach(atom => atom.Velocity -= sum);
                else break;
            }
        }

        /// <summary>
        /// Начальная перенормировка скоростей.
        /// </summary>
        /// <param name="T"></param>
        public void InitVelocityNormalization(double T)
        {
            var vsqrt = Math.Sqrt(3 * kB * T / WeightAtom);
            const double pi2 = 2 * Math.PI;

            Atoms.ForEach(atom =>
            {
                var r1 = _rnd.NextDouble();
                var r2 = _rnd.NextDouble();
                atom.Velocity = new Vector(
                    Math.Sin(pi2 * r1) * Math.Cos(pi2 * r2),
                    Math.Sin(pi2 * r1) * Math.Sin(pi2 * r2),
                    Math.Sin(pi2 * r1)) * vsqrt;
            });
        }

        /// <summary>
        /// Перенормировка скоростей к заданной температуре.
        /// </summary>
        public void VelocityNormalization(double T)
        {
            var sum = Atoms.Sum(atom => WeightAtom * atom.Velocity.SquaredMagnitude);
            var beta = Math.Sqrt(3 * CountAtoms * kB * T / sum);
            Atoms.ForEach(atom => atom.Velocity *= beta);
        }
    }
}