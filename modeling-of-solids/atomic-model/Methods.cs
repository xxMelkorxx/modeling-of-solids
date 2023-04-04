using System;
using System.Collections.Generic;
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

            return dxdydz.SquaredMagnitude();
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
                atom.PositionNonePeriodic += displacement;
            });
        }

        /// <summary>
        /// Начальная перенормировка скоростей.
        /// </summary>
        /// <param name="temp"></param>
        public void InitVelocityNormalization(double temp)
        {
            var vsqrt = Math.Sqrt(3 * kB * temp / WeightAtom);
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
        /// <param name="temp">Заданная температура</param>
        public void VelocityNormalization(double temp)
        {
            var sum = Atoms.Sum(atom => WeightAtom * atom.Velocity.SquaredMagnitude());
            if (sum == 0)
                throw new DivideByZeroException();
            var beta = Math.Sqrt(3 * CountAtoms * kB * temp / sum);
            Atoms.ForEach(atom => atom.Velocity *= beta);
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

        public PointD[] GetRadialDistribution()
        {
            var dr = 0.05 * Lattice * 0.726;
            var dr2 = dr * dr;
            var rd = new PointD[(int)(L / dr)];
            for (var i = 0; i < rd.Length; i++)
                rd[i] = new(i * dr, 0);

            // Подсчёт числа атомов в центральной части расчётной ячейки.
            var countAtoms = 0;
            foreach (var atom in Atoms)
                if (atom.Position.X > 0.25 * L && atom.Position.X < 0.75 * L &&
                    atom.Position.Y > 0.25 * L && atom.Position.Y < 0.75 * L &&
                    atom.Position.Z > 0.25 * L && atom.Position.Z < 0.75 * L)
                    countAtoms++;

            // Подсчёт n(r).
            foreach (var atomI in Atoms)
            foreach (var atomJ in Atoms)
            {
                if (atomJ.Equals(atomI)) continue;
                var r2 = SeparationSqured(atomI.Position, atomJ.Position, out _);
                for (var k = 0; k < rd.Length; k++)
                    if (r2 > k * k * dr2 && r2 < (k + 1) * (k + 1) * dr2)
                        rd[k].Y++;
            }

            // Усреднение.
            for (var i = 0; i < rd.Length; i++)
            {
                var coef = V / (CountAtoms * 4 * Math.PI * Math.PI * rd[i].X * rd[i].X * dr);
                rd[i].Y /= (countAtoms == 0) ? 1 : countAtoms;
                rd[i].Y *= (1 / coef == 0) ? 1 : coef;
            }

            return rd;
        }

        /// <summary>
        /// Получение координат атомов.
        /// </summary>
        public List<Vector> GetPositionsAtoms()
        {
            List<Vector> positions = new();
            Atoms.ForEach(atom => positions.Add(atom.Position));
            return positions;
        }

        /// <summary>
        /// Получение координат атомов без учёта ПГУ.
        /// </summary>
        public List<Vector> GetPositionsNonePeriodicAtoms()
        {
            List<Vector> positions = new();
            Atoms.ForEach(atom => positions.Add(atom.PositionNonePeriodic));
            return positions;
        }

        public double GetSigma() => (_potential != null) ? ((PotentialLJ)_potential).Sigma : throw new NullReferenceException();

        /// <summary>
        /// Средний квадрат смещения.
        /// </summary>
        /// <param name="rt1"></param>
        /// <param name="rt2"></param>
        /// <returns></returns>
        public double AverageSquareOffset(List<Vector> rt1, List<Vector> rt2)
        {
            double sum = 0;
            for (var i = 0; i < rt1.Count; i++)
                sum += (rt2[i] - rt1[i]).SquaredMagnitude();
            return sum / CountAtoms;
        }
    }
}