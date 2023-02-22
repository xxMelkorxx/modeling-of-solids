namespace modeling_of_solids
{
	public enum PotentialType
	{
		LennardJones,
		ModifiedLennardJones
	}

	public abstract class Potential
	{
		/// <summary>
		/// 1 эВ в Дж с нм.
		/// </summary>
		public const double eV = 0.1602176634;

		/// <summary>
		/// Постоянная Больцмана (эВ/К).
		/// </summary>
		public const double kB = 8.61733262e-5;

		public abstract AtomType Type { get; set; }
		protected AtomType _type;

		/// <summary>
		/// Межатомная сила взаимодействия в потенциале.
		/// </summary>
		/// <param name="rij"></param>
		/// <returns></returns>
		public abstract object Force(object[] args);

		/// <summary>
		/// Потенциальная энергия двух атомов.
		/// </summary>
		/// <param name="rij"></param>
		/// <returns></returns>
		public abstract object PotentialEnergy(object[] args);
	}
}
