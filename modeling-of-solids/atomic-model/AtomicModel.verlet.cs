using System;

namespace modeling_of_solids.atomic_model;

public partial class AtomicModel
{
    /// <summary>
    /// Вычисление начальных параметров системы (Acceleration, Pe, Ke, Press).
    /// </summary>
    public void InitCalculation()
    {
        Ke = 0;
        Pe = 0;
        _virial = 0;
        Accel();

        Atoms.ForEach(atom => Ke += 0.5 * atom.Velocity.SquaredMagnitude() * WeightAtom);
    }

    /// <summary>
    /// Алгоритм Верле.
    /// </summary>
    public void Verlet()
    {
        Ke = 0;
        Pe = 0;
        _virial = 0;
        //_flux = Vector.Zero;

        Atoms.ForEach(atom =>
        {
            var newPos = atom.Velocity * Dt + 0.5 * atom.Acceleration * Dt * Dt;
            atom.Position = Periodic(atom.Position + newPos, atom.Velocity * 1e-9 * WeightAtom);
            atom.PositionNonePeriodic += newPos;
        });

        Atoms.ForEach(atom => atom.Velocity += 0.5 * atom.Acceleration * Dt);

        // Рассчёт новых ускорений.
        Accel();

        Atoms.ForEach(atom =>
        {
            atom.Velocity += 0.5 * atom.Acceleration * Dt;
            Ke += 0.5 * atom.Velocity.SquaredMagnitude() * WeightAtom / Ev;
        });

        // Рассчёт АКФ скорости.
        if (CurrentStep == 1)
            _vtList.Clear();
        if (_vtList.Count < _countNumber + _countRepetition * _stepDelay)
            _vtList.Add(GetVelocitiesAtoms());

        CurrentStep++;
    }

    /// <summary>
    /// Вычисление ускорения, pe, virial.
    /// </summary>
    private void Accel()
    {
        Atoms.ForEach(atom => atom.Acceleration = Vector.Zero);

        for (var i = 0; i < CountAtoms - 1; i++)
        {
            var sumForce = Vector.Zero;

            for (var j = i + 1; j < CountAtoms; j++)
            {
                var rij = SeparationSqured(Atoms[i].Position, Atoms[j].Position, out var dxdydz);

                var force = (Vector)_potential.Force(new object[] { rij, dxdydz });
                sumForce += force;
                Atoms[i].Acceleration += force / WeightAtom;
                Atoms[j].Acceleration -= force / WeightAtom;

                Pe += (double)_potential.PotentialEnergy(new object[] { rij });
            }

            _virial += Atoms[i].Position.X * sumForce.X + Atoms[i].Position.Y * sumForce.Y + Atoms[i].Position.Z * sumForce.Z;
        }
    }

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
    public double SeparationSqured(Vector vec1, Vector vec2, out Vector dxdydz)
    {
        dxdydz = vec1 - vec2;

        // Обеспечивает, что расстояние между частицами никогда не будет больше L/2.
        if (Math.Abs(dxdydz.X) > 0.5 * BoxSize)
            dxdydz.X -= Math.Sign(dxdydz.X) * BoxSize;
        if (Math.Abs(dxdydz.Y) > 0.5 * BoxSize)
            dxdydz.Y -= Math.Sign(dxdydz.Y) * BoxSize;
        if (Math.Abs(dxdydz.Z) > 0.5 * BoxSize)
            dxdydz.Z -= Math.Sign(dxdydz.Z) * BoxSize;

        return dxdydz.SquaredMagnitude();
    }

    /// <summary>
    /// Учёт периодических граничных условий.
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    public Vector Periodic(Vector pos, Vector p)
    {
        var newPos = Vector.Zero;
        // Ось X.               
        if (pos.X > BoxSize)
        {
            newPos.X = pos.X - BoxSize;
            Flux.X += p.X;
        }
        else if (pos.X < 0)
        {
            newPos.X = pos.X + BoxSize;
            Flux.X -= p.X;
        }
        else newPos.X = pos.X;

        // Ось Y.
        if (pos.Y > BoxSize)
        {
            newPos.Y = pos.Y - BoxSize;
            Flux.Y += p.Y;
        }
        else if (pos.Y < 0)
        {
            newPos.Y = pos.Y + BoxSize;
            Flux.Y -= p.Y;
        }
        else newPos.Y = pos.Y;

        // Ось Z.
        if (pos.Z > BoxSize)
        {
            newPos.Z = pos.Z - BoxSize;
            Flux.Z += p.Z;
        }
        else if (pos.Z < 0)
        {
            newPos.Z = pos.Z + BoxSize;
            Flux.Z -= p.Z;
        }
        else newPos.Z = pos.Z;

        return newPos;
    }
}