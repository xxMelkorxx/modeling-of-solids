using modeling_of_solids.atomic_model;
using modeling_of_solids.potentials;

namespace modeling_of_solids.testing;

[TestClass]
public class VectorTesting
{
    private const double Eps = 1e-10;

    /// <summary>
    /// Тест на сложение вектора с целым числом.
    /// </summary>
    [TestMethod]
    public void TestAdditionOperations1()
    {
        var vector = new Vector(0.1, 2, -1) + 1;
        Assert.IsTrue(1.1 - Eps <= vector.X && vector.X <= 1.1 + Eps);
        Assert.IsTrue(3 - Eps <= vector.Y && vector.Y <= 3 + Eps);
        Assert.IsTrue(0 - Eps <= vector.Z && vector.Z <= 0 + Eps);
    }

    /// <summary>
    /// Тест на сложение вектора с числом с плавающей запятой.
    /// </summary>
    [TestMethod]
    public void TestAdditionOperations2()
    {
        var vector = new Vector(-0.25, 0.25, 0.35) + 0.4;
        Assert.IsTrue(0.15 - Eps <= vector.X && vector.X <= 0.15 + Eps);
        Assert.IsTrue(0.65 - Eps <= vector.Y && vector.Y <= 0.65 + Eps);
        Assert.IsTrue(0.75 - Eps <= vector.Z && vector.Z <= 0.75 + Eps);
    }

    /// <summary>
    /// Тест на сложение вектора с с вектором.
    /// </summary>
    [TestMethod]
    public void TestAdditionOperations3()
    {
        var vector = new Vector(-0.25, 0.25, 0.35) + new Vector(0.4, 0.4, 1);
        Assert.IsTrue(0.15 - Eps <= vector.X && vector.X <= 0.15 + Eps);
        Assert.IsTrue(0.65 - Eps <= vector.Y && vector.Y <= 0.65 + Eps);
        Assert.IsTrue(1.35 - Eps <= vector.Z && vector.Z <= 1.35 + Eps);
    }

    /// <summary>
    /// Тест на вычитание целого числа из вектора.
    /// </summary>
    [TestMethod]
    public void TestSubtractionOperation1()
    {
        var vector = new Vector(0.1, 2, -1) - 1;
        Assert.IsTrue(-0.9 - Eps <= vector.X && vector.X <= -0.9 + Eps);
        Assert.IsTrue(1 - Eps <= vector.Y && vector.Y <= 1 + Eps);
        Assert.IsTrue(-2 - Eps <= vector.Z && vector.Z <= -2 + Eps);
    }

    /// <summary>
    /// Тест на вычитание числа с плавающей запятой из вектора.
    /// </summary>
    [TestMethod]
    public void TestSubtractionOperation2()
    {
        var vector = new Vector(0.25, 0.75, 1.25) - 0.4;
        Assert.IsTrue(-0.15 - Eps <= vector.X && vector.X <= -0.15 + Eps);
        Assert.IsTrue(0.35 - Eps <= vector.Y && vector.Y <= 0.35 + Eps);
        Assert.IsTrue(0.85 - Eps <= vector.Z && vector.Z <= 0.85 + Eps);
    }

    /// <summary>
    /// Тест на вычитание вектора из вектора.
    /// </summary>
    [TestMethod]
    public void TestSubtractionOperation3()
    {
        var vector = new Vector(0.25, 0.25, 0.25) - new Vector(0.25, 0.4, 1);
        Assert.IsTrue(0 - Eps <= vector.X && vector.X <= 0 + Eps);
        Assert.IsTrue(-0.15 - Eps <= vector.Y && vector.Y <= -0.15 + Eps);
        Assert.IsTrue(-0.75 - Eps <= vector.Z && vector.Z <= -0.75 + Eps);
    }

    /// <summary>
    /// Тест на умножение вектора на целое число.
    /// </summary>
    [TestMethod]
    public void TestMultiplicationOperation1()
    {
        var vector = new Vector(1, 2, -1) * 2;
        Assert.IsTrue(2 - Eps <= vector.X && vector.X <= 2 + Eps);
        Assert.IsTrue(4 - Eps <= vector.Y && vector.Y <= 4 + Eps);
        Assert.IsTrue(-2 - Eps <= vector.Z && vector.Z <= -2 + Eps);
    }

    /// <summary>
    /// Тест на умножение вектора на число с плавабщей запятой.
    /// </summary>
    [TestMethod]
    public void TestMultiplicationOperation2()
    {
        var vector = new Vector(-0.3, 0.4, 0.25) * 0.5;
        Assert.IsTrue(-0.15 - Eps <= vector.X && vector.X <= -0.15 + Eps);
        Assert.IsTrue(0.2 - Eps <= vector.Y && vector.Y <= 0.2 + Eps);
        Assert.IsTrue(0.125 - Eps <= vector.Z && vector.Z <= 0.125 + Eps);
    }

    /// <summary>
    /// Тест на деление вектора на целое число.
    /// </summary>
    [TestMethod]
    public void TestDivisionOperation1()
    {
        var vector = new Vector(1, 2, -1) / 2;
        Assert.IsTrue(0.5 - Eps <= vector.X && vector.X <= 0.5 + Eps);
        Assert.IsTrue(1 - Eps <= vector.Y && vector.Y <= 1 + Eps);
        Assert.IsTrue(-0.5 - Eps <= vector.Z && vector.Z <= -0.5 + Eps);
        Assert.AreEqual(vector, new Vector(0.5, 1, -0.5));
    }

    /// <summary>
    /// Тест на деление вектора на число с плавабщей запятой.
    /// </summary>
    [TestMethod]
    public void TestDivisionOperation2()
    {
        var vector = new Vector(-0.3, 0.4, 0.25) / 0.5;
        Assert.IsTrue(-0.6 - Eps <= vector.X && vector.X <= -0.6 + Eps);
        Assert.IsTrue(0.8 - Eps <= vector.Y && vector.Y <= 0.8 + Eps);
        Assert.IsTrue(0.5 - Eps <= vector.Z && vector.Z <= 0.5 + Eps);
    }

    /// <summary>
    /// Тест на деление вектора ноль.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(DivideByZeroException))]
    public void TestDivisionOperation3()
    {
        _ = Vector.One / 0;
    }
}

[TestClass]
public class SeparationSquredTesting
{
    /// <summary>
    /// Тест при нулевых значениях параметров.
    /// </summary>
    [TestMethod]
    public void TestAtZeroValues()
    {
        AtomicModel model = new(3, AtomType.Ar, LatticeType.FCC, PotentialType.LJ);
        Vector vec1 = new(0, 0, 0);
        Vector vec2 = new(0, 0, 0);

        var result = model.SeparationSqured(vec1, vec2, out var dxdydz);
        Assert.AreEqual(result, 0);
        Assert.AreEqual(dxdydz, Vector.Zero);
    }
    
    /// <summary>
    /// Тест проверки на правильность работы, когда vec1 меньше boxSize.
    /// </summary>
    [TestMethod]
    public void Test1()
    {
        AtomicModel model = new(1, AtomType.Ar, LatticeType.FCC, PotentialType.LJ);
        Vector vec1 = new(model.BoxSize * 0.5, model.BoxSize * 0.5, model.BoxSize * 0.5);
        Vector vec2 = new(model.BoxSize * 0.25, model.BoxSize * 0.25, model.BoxSize * 0.25);

        var result = model.SeparationSqured(vec1, vec2, out var dxdydz);
        var diff = vec1 - vec2;
        Assert.AreEqual(result, diff.SquaredMagnitude());
        Assert.AreEqual(dxdydz, diff);
    }
    
    /// <summary>
    /// Тест проверки на правильность работы, когда pos больше boxSize, но меньше 2*boxSize.
    /// </summary>
    [TestMethod]
    public void Test2()
    {
        AtomicModel model = new(1, AtomType.Ar, LatticeType.FCC, PotentialType.LJ);
        Vector vec1 = new(model.BoxSize * 1.5, model.BoxSize * 1.5, model.BoxSize * 1.5);
        Vector vec2 = new(model.BoxSize * 0.25, model.BoxSize * 0.25, model.BoxSize * 0.25);

        var result = model.SeparationSqured(vec1, vec2, out var dxdydz);
        var diff = vec1 - vec2;
        Assert.AreEqual(result, diff.SquaredMagnitude());
        Assert.AreEqual(dxdydz, diff);
    }
    
    /// <summary>
    /// Тест проверки на правильность работы, когда pos больше 2*boxSize.
    /// </summary>
    [TestMethod]
    public void Test3()
    {
        AtomicModel model = new(1, AtomType.Ar, LatticeType.FCC, PotentialType.LJ);
        Vector vec1 = new(model.BoxSize * 2.5, model.BoxSize * 2.5, model.BoxSize * 2.5);
        Vector vec2 = new(0, 0, 0);

        var result = model.SeparationSqured(vec1, vec2, out var dxdydz);
        var diff = vec1 - vec2;
        Assert.AreEqual(result, diff.SquaredMagnitude());
        Assert.AreEqual(dxdydz, diff);
    }
}

[TestClass]
public class PeriodicTesting
{
    /// <summary>
    /// Тест проверки на правильность работы, когда pos меньше boxSize.
    /// </summary>
    [TestMethod]
    public void Test1()
    {
        AtomicModel model = new(1, AtomType.Ar, LatticeType.FCC, PotentialType.LJ);
        Vector pos = new(model.BoxSize * 0.5, model.BoxSize * 0.5, model.BoxSize * 0.5);

        var result = model.Periodic(pos, Vector.Zero);
        Assert.AreEqual(result, new Vector(model.BoxSize * 0.5, model.BoxSize * 0.5, model.BoxSize * 0.5));
    }
    
    /// <summary>
    /// Тест проверки на правильность работы, когда pos больше boxSize, но меньше 2*boxSize.
    /// </summary>
    [TestMethod]
    public void Test2()
    {
        AtomicModel model = new(1, AtomType.Ar, LatticeType.FCC, PotentialType.LJ);
        Vector pos = new(model.BoxSize * 1.5, model.BoxSize * 1.5, model.BoxSize * 1.5);

        var result = model.Periodic(pos, Vector.Zero);
        Assert.AreEqual(result, new Vector(model.BoxSize * 0.5, model.BoxSize * 0.5, model.BoxSize * 0.5));
    }
    
    /// <summary>
    /// Тест проверки на правильность работы, когда pos больше 2*boxSize.
    /// </summary>
    [TestMethod]
    public void Test3()
    {
        AtomicModel model = new(1, AtomType.Ar, LatticeType.FCC, PotentialType.LJ);
        Vector pos = new(model.BoxSize * 2.5, model.BoxSize * 2.5, model.BoxSize * 2.5);

        var result = model.Periodic(pos, Vector.Zero);
        Assert.AreEqual(result, new Vector(model.BoxSize * 0.5, model.BoxSize * 0.5, model.BoxSize * 0.5));
    }
}

[TestClass]
public class PotentialsTesting
{
    /// <summary>
    /// Тест на деление ноль, когда r = 0, при расчёте энергии в потенциле Леннарда-Джонса.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(DivideByZeroException))]
    public void TestDivideByZeroExceptionPlj()
    {
        var potential = new PotentialLJ { Type = AtomType.Ar };
        var args = new object[] { 0d };
        _ = potential.PotentialEnergy(args);
    }
    
    /// <summary>
    /// Тест на деление ноль, когда r = 0, при расчёте силы в потенциле Леннарда-Джонса.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(DivideByZeroException))]
    public void TestDivideByZeroExceptionFlj()
    {
        var potential = new PotentialLJ { Type = AtomType.Ar };
        var args = new object[] { 0d, new Vector(0.1, 0.1, 0.1) };
        _ = potential.Force(args);
    }
    
    /// <summary>
    /// Тест на деление ноль, когда r = 0, при расчёте энергии в модифицированном потенциле Леннарда-Джонса.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(DivideByZeroException))]
    public void TestDivideByZeroExceptionPmlj()
    {
        var potential = new PotentialMLJ { Type = AtomType.Ar };
        var args = new object[] { 0d };
        _ = potential.PotentialEnergy(args);
    }
    
    /// <summary>
    /// Тест на деление ноль, когда r = 0, при расчёте силы в модифицированном потенциле Леннарда-Джонса.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(DivideByZeroException))]
    public void TestDivideByZeroExceptionFmlj()
    {
        var potential = new PotentialMLJ { Type = AtomType.Ar };
        var args = new object[] { 0d, new Vector(0.1, 0.1, 0.1) };
        _ = potential.Force(args);
    }
}