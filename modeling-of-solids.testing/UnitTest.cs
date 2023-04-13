namespace modeling_of_solids.testing;

[TestClass]
public class SeparationSquredTesting
{
	[TestMethod("Тест при нулевых значениях")]
	public void TestAtZeroValues()
	{
		AtomicModel _model = new(3, AtomType.Ar, LatticeType.FCC, PotentialType.LJ);
		Vector vec1 = new(0, 0, 0);
		Vector vec2 = new(0, 0, 0);

		double result = _model.SeparationSqured(vec1, vec2, out var dxdydz);
		Assert.AreEqual(result, 0);
		Assert.AreEqual(dxdydz, Vector.Zero);
	}
}