namespace modeling_of_solids
{
	public class PotentialLJ : Potential
	{
		public struct ParamsPotentialLJ
		{
			/// <summary>
			/// Модуль потенциальной энергии взаимодействия между атомами при равновесии.
			/// </summary>
			public double D;

			/// <summary>
			/// Расстояние между центрами атомов при котором U(sigma) = 0.
			/// </summary>
			public double sigma;

			public ParamsPotentialLJ(double D, double sigma) { this.D = D; this.sigma = sigma; }
		}

		/// <summary>
		/// Параметры для аргона в потенциале Леннарда-Джонса.
		/// </summary>
		private static ParamsPotentialLJ paramLJ_Ar = new(0.01029, 0.3408);


		/// <summary>
		/// Параметры потенциала.
		/// </summary>
		public ParamsPotentialLJ param;

		/// <summary>
		/// Тип атома.
		/// </summary>
		public override AtomType Type
		{
			get { return _type; }
			set
			{
				_type = value;
				switch (_type)
				{
					case AtomType.Ar: param = paramLJ_Ar; break;
				}
			}
		}

		public PotentialLJ(AtomType type) { Type = type; }

		/// <summary>
		/// Cила в потенциале Леннарда-Джонса.
		/// </summary>
		/// <param name="args">Массив аргументов: 0 - rij, 1 - dxdydz </param>
		/// <returns></returns>
		public override object Force(object[] args)
		{
			double r = (double)args[0];
			Vector dxdydz = (Vector)args[1];

			double ri = param.sigma / r;
			double ri3 = ri * ri * ri;
			double ri6 = ri3 * ri3;
			return 24 * param.D * eV * ri6 * (2 * ri6 - 1) / (r * r) * dxdydz;
		}

		/// <summary>
		/// Потенциал Леннарда-Джонса.
		/// </summary>
		/// <param name="args">Массив аргументов: 0 - rij.</param>
		/// <returns></returns>
		public override object PotentialEnergy(object[] args)
		{
			double r = (double)args[0];

			double ri = param.sigma / r;
			double ri3 = ri * ri * ri;
			double ri6 = ri3 * ri3;
			return 4 * param.D * ri6 * (ri6 - 1);
		}
	}
}
