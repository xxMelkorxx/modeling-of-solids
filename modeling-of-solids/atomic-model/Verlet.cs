namespace modeling_of_solids
{
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
            
            // Вычисление кинетической энергии.
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

            // Изменение координат с учётом периодических граничных условий.
            Atoms.ForEach(atom =>
            {
                var newPos = atom.Velocity * dt + 0.5 * atom.Acceleration * dt * dt;
                atom.Position = Periodic(atom.Position + newPos);
            });

            // Частичное изменение скорости, используя старое ускорение.
            Atoms.ForEach(atom => atom.Velocity += 0.5 * atom.Acceleration * dt);

            // Вычисление нового ускорения.
            Accel();

            Atoms.ForEach(atom =>
            {
                // Частичное изменение скорости, используя новое ускорение.
                atom.Velocity += 0.5 * atom.Acceleration * dt;
                // Вычисление кинетической энергии.
                Ke += 0.5 * atom.Velocity.SquaredMagnitude() * WeightAtom / eV;
            });
        }

        /// <summary>
        /// Вычисление ускорения, pe, virial.
        /// </summary>
        private void Accel()
        {
            Atoms.ForEach(atom => atom.Acceleration = Vector.Zero);

            for (var i = 0; i < CountAtoms - 1; i++)
            {
                var atomI = Atoms[i];

                var sumForce = Vector.Zero;
                for (var j = i + 1; j < CountAtoms; j++)
                {
                    var atomJ = Atoms[j];

                    // Вычисление расстояния между частицами.
                    var rij = Separation(atomI.Position, atomJ.Position, out var dxdydz);

					if (_potential != null)
                    {
						var force = (Vector)_potential.Force(new object[] { rij, dxdydz });
						sumForce += force;
						atomI.Acceleration += force / WeightAtom;
						atomJ.Acceleration -= force / WeightAtom;

						Pe += (double)_potential.PotentialEnergy(new object[] { rij });
					}
				}

                // Для вычисления давления.
                _virial += atomI.Position.X * sumForce.X + atomI.Position.Y * sumForce.Y + atomI.Position.Z * sumForce.Z;
            }
        }
    }
}