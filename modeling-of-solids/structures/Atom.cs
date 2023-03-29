namespace modeling_of_solids
{
	public enum AtomType
	{
		Ar
	}

	public class Atom
	{
		/// <summary>
		/// Идентификатор.
		/// </summary>
		public int ID;

		/// <summary>
		/// Вектор координаты.
		/// </summary>
		public Vector Position;

		/// <summary>
		/// Координаты атома без учёта периодичности границ.
		/// </summary>
		public Vector PositionNonePeriodic;

		/// <summary>
		/// Вектор скорости.
		/// </summary>
		public Vector Velocity;

		/// <summary>
		/// Вектор ускорения.
		/// </summary>
		public Vector Acceleration;

		/// <summary>
		/// Тип атома.
		/// </summary>
		public AtomType Type;

		/// <summary>
		/// Создание атома.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="atomType"></param>
		/// <param name="pos"></param>
		public Atom(int id, AtomType atomType, Vector pos)
		{
			ID = id;
			Type= atomType;
			Position = pos;
			PositionNonePeriodic = pos;
			Velocity = Vector.Zero;
			Acceleration = Vector.Zero;
		}
	}
}
