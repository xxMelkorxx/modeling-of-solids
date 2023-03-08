namespace modeling_of_solids
{
    public class PotentialLJ : Potential
    {
        /// <summary>
        /// Модуль потенциальной энергии взаимодействия между атомами при равновесии.
        /// </summary>
        private double _d;

        /// <summary>
        /// Расстояние между центрами атомов при котором U(sigma) = 0.
        /// </summary>
        private double _sigma;

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
                        _d = 0.01029;
                        _sigma = 0.3408;
                        break;
                }
            }
        }

        private AtomType _type;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="type"></param>
        public PotentialLJ(AtomType type)
        {
            _type = type;
        }

        /// <summary>
        /// Cила в потенциале Леннарда-Джонса.
        /// </summary>
        /// <param name="args">Массив аргументов: 0 - rij, 1 - dxdydz</param>
        /// <returns></returns>
        public override object Force(object[] args)
        {
            var r = (double)args[0];
            var dxdydz = (Vector)args[1];

            var ri = _sigma / r;
            var ri3 = ri * ri * ri;
            var ri6 = ri3 * ri3;

            return 24 * _d * eV * ri6 * (2 * ri6 - 1) / (r * r) * dxdydz;
        }

        /// <summary>
        /// Потенциал Леннарда-Джонса.
        /// </summary>
        /// <param name="args">Массив аргументов: 0 - rij.</param>
        /// <returns></returns>
        public override object PotentialEnergy(object[] args)
        {
            var r = (double)args[0];

            var ri = _sigma / r;
            var ri3 = ri * ri * ri;
            var ri6 = ri3 * ri3;

            return 4 * _d * ri6 * (ri6 - 1);
        }
    }
}