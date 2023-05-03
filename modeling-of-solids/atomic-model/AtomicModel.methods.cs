using System;
using System.Collections.Generic;
using System.Linq;

namespace modeling_of_solids.atomic_model;

public partial class AtomicModel
{
    /// <summary>
    /// Начальное смещение атомов.
    /// </summary>
    /// <param name="k">Коэффициент смещения.</param>
    public void AtomsDisplacement(double k)
    {
        Atoms.ForEach(atom =>
        {
            var displacement = (-1 * Vector.One + 2 * new Vector(_rnd.NextDouble(), _rnd.NextDouble(), _rnd.NextDouble())) * k * Lattice;
            Flux = Vector.Zero;
            atom.Position = Periodic(atom.Position + displacement, atom.Velocity * WeightAtom);
            atom.PositionNonePeriodic += displacement;
        });
    }

    /// <summary>
    /// Начальная перенормировка скоростей.
    /// </summary>
    /// <param name="temp"></param>
    public void InitVelocityNormalization(double temp)
    {
        var vsqrt = Math.Sqrt(3 * Kb * temp / WeightAtom);
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
        var beta = Math.Sqrt(3 * CountAtoms * Kb * temp / sum);
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

    /// <summary>
    /// Получение радиального распределения атомов.
    /// </summary>
    /// <returns></returns>
    public PointD[] GetRadialDistribution()
    {
        var dr = 0.05 * Lattice * 0.726;
        var dr2 = dr * dr;
        var rd = new PointD[(int)(BoxSize / dr)];
        for (var i = 0; i < rd.Length; i++)
            rd[i] = new PointD(i * dr, 0);

        // Подсчёт числа атомов в центральной части расчётной ячейки.
        var countAtoms = Atoms.Where(atom =>
            atom.Position.X > 0.25 * BoxSize && atom.Position.X < 0.75 * BoxSize &&
            atom.Position.Y > 0.25 * BoxSize && atom.Position.Y < 0.75 * BoxSize &&
            atom.Position.Z > 0.25 * BoxSize && atom.Position.Z < 0.75 * BoxSize).Count();

        // Подсчёт n(r).
        foreach (var atomI in Atoms)
        foreach (var atomJ in Atoms)
        {
            if (atomJ.Equals(atomI)) continue;
            var r2 = Vector.SquaredMagnitudeDifference(atomI.Position, atomJ.Position);
            for (var k = 0; k < rd.Length; k++)
                if (r2 > k * k * dr2 && r2 < (k + 1) * (k + 1) * dr2)
                    rd[k].Y++;
        }

        // Усреднение.
        for (var i = 0; i < rd.Length; i++)
        {
            var coef = V / (CountAtoms * 4 * Math.PI * Math.PI * rd[i].X * rd[i].X * dr);
            rd[i].Y /= countAtoms == 0 ? 1 : countAtoms;
            rd[i].Y *= 1 / coef == 0 ? 1 : coef;
        }

        return rd;
    }

    /// <summary>
    /// Получение координат атомов.
    /// </summary>
    public List<Vector> GetPositionsAtoms() => Atoms.Select(atom => atom.Position).ToList();

    /// <summary>
    /// Получение координат атомов без учёта ПГУ.
    /// </summary>
    public List<Vector> GetPositionsNonePeriodicAtoms() => Atoms.Select(atom => atom.PositionNonePeriodic).ToList();

    /// <summary>
    /// Получение скоростей атомов.
    /// </summary>
    /// <returns></returns>
    public List<Vector> GetVelocitiesAtoms() => Atoms.Select(atom => atom.Velocity * 1e-9).ToList();

    /// <summary>
    /// Средний квадрат смещения.
    /// </summary>
    /// <returns></returns>
    public double GetAverageSquareOffset() => _rt1.Zip(GetPositionsNonePeriodicAtoms(), (vec1, vec2) => (vec2 - vec1).SquaredMagnitude()).Sum() / CountAtoms;

    /// <summary>
    /// Рассчёт автокорреляционной функции скорости атомов.
    /// </summary>
    /// <returns></returns>
    public double[] GetAcfs()
    {
        var zt = new double[CountNumberAcf];
        for (var i = 0; i < CountRepeatAcf; i++)
        for (var j = 0; j < CountNumberAcf; j++)
        {
            for (var k = 0; k < CountAtoms; k++)
                zt[j] += k != 0
                    ? (_vtList[i * StepRepeatAcf][k].X * _vtList[j + i * StepRepeatAcf][k].X +
                       _vtList[i * StepRepeatAcf][k].Y * _vtList[j + i * StepRepeatAcf][k].Y +
                       _vtList[i * StepRepeatAcf][k].Z * _vtList[j + i * StepRepeatAcf][k].Z)
                    : _vtList[i * StepRepeatAcf][k].Magnitude();
            // zt[j] /= CountRepeatAcf * CountAtoms;
        }

        var max = zt.Max();
        for (var i = 0; i < zt.Length; i++)
            zt[i] /= max;
            
        return zt;
    }

    public double GetSigma() => AtomsType switch
    {
        AtomType.Ar => 0.3408,
        AtomType.Cu => 0.34635,
        AtomType.Fe => 0.25,
        AtomType.Au => 0.288,
        AtomType.Si => 0.372,
        AtomType.Ge => 0.378,
        _ => throw new ArgumentNullException()
    };
}