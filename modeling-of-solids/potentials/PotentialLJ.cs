﻿namespace modeling_of_solids
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
					case AtomType.Ar: D = 0.01029; Sigma = 0.3408; break;
					case AtomType.Cu: D = 0.00102; Sigma = 0.34635; break;
					case AtomType.Fe: D = 0.00172; Sigma = 0.25; break;
					case AtomType.Au: D = 0.00198; Sigma = 0.288; break;
					case AtomType.Si: D = 0.00916; Sigma = 0.372; break;
					case AtomType.Ge: D = 0.0053; Sigma = 0.378; break;
				}
			}
		}

		private AtomType _type;

		/// <summary>
		/// Вектор силы парного взаимодействия.
		/// </summary>
		/// <param name="args">Массив аргументов: 0 - rij, 1 - dxdydz</param>
		/// <returns></returns>
		public override object Force(object[] args) => FLD((double)args[0]) * (Vector)args[1];

		/// <summary>
		/// Потенциальная энергия парного взаимодействия.
		/// </summary>
		/// <param name="args">Массив аргументов: 0 - rij.</param>
		/// <returns></returns>
		public override object PotentialEnergy(object[] args) => PLD((double)args[0]);

		/// <summary>
		/// Потенциал Леннарда-Джонса.
		/// </summary>
		/// <param name="r">Расстояние между частицами.</param>
		/// <returns></returns>
		private double PLD(double r)
		{
			var ri = Sigma / r;
			var ri3 = ri * ri * ri;
			var ri6 = ri3 * ri3;

			return 4 * D * ri6 * (ri6 - 1);
		}

		/// <summary>
		/// Cила в потенциале Леннарда-Джонса.
		/// </summary>
		/// <param name="r">Расстояние между частицами.</param>
		/// <returns></returns>
		private double FLD(double r)
		{
			var ri = Sigma / r;
			var ri3 = ri * ri * ri;
			var ri6 = ri3 * ri3;

			return 24 * D * eV * ri6 * (2 * ri6 - 1) / (r * r);
		}
	}
}