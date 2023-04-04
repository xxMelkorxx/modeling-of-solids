namespace modeling_of_solids
{
    public class PotentialLJ : Potential
    {
        /// <summary>
        /// Модуль потенциальной энергии взаимодействия между атомами при равновесии.
        /// </summary>
        public double D;

        /// <summary>
        /// Расстояние между центрами атомов при котором U(sigma) = 0.
        /// </summary>
        public double Sigma;

        /// <summary>
        /// Тип атома.
        /// </summary>
        public override AtomType Type
        {
            get => _type;
            set
            {
                _type = value;
                switch (_type)
                {
                    case AtomType.Ar:
                        D = 0.01029;
                        Sigma = 0.3408;
                        break;
                }
            }
        }

        private AtomType _type;

        /// <summary>
        /// Cила в потенциале Леннарда-Джонса.
        /// </summary>
        /// <param name="args">Массив аргументов: 0 - rij, 1 - dxdydz</param>
        /// <returns></returns>
        public override object Force(object[] args)
        {
            var r = (double)args[0];
            var dxdydz = (Vector)args[1];

            var ri = Sigma / r;
            var ri3 = ri * ri * ri;
            var ri6 = ri3 * ri3;

            return 24 * D * eV * ri6 * (2 * ri6 - 1) / (r * r) * dxdydz;
        }

        /// <summary>
        /// Потенциал Леннарда-Джонса.
        /// </summary>
        /// <param name="args">Массив аргументов: 0 - rij.</param>
        /// <returns></returns>
        public override object PotentialEnergy(object[] args)
        {
            var r = (double)args[0];

            var ri = Sigma / r;
            var ri3 = ri * ri * ri;
            var ri6 = ri3 * ri3;

            return 4 * D * ri6 * (ri6 - 1);
        }
    }
}