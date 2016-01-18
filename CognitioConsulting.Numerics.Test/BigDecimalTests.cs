using Microsoft.VisualStudio.TestTools.UnitTesting;
using CognitioConsulting.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace CognitioConsulting.Numerics.Tests
{
	[TestClass()]
	public class BigDecimalTests
	{
		[TestMethod()]
		public void ParseTest1()
		{
			var d = 1.0m;
			var tst = BigDecimal.Parse(d.ToString());

			Assert.AreEqual(d.ToString(), tst.ToString(), $"Parsing '{d.ToString()}' does not work.");
		}

		[TestMethod()]
		public void ParseTest2()
		{
			var d = 100m;
			var tst = BigDecimal.Parse(d.ToString());

			Assert.AreEqual(d.ToString(),tst.ToString(), $"Parsing '{d.ToString()}' does not work.");
		}

		[TestMethod()]
		public void ParseTest3()
		{
			var d = -100m;
			var tst = BigDecimal.Parse(d.ToString());

			Assert.AreEqual(d.ToString(), tst.ToString(), $"Parsing '{d.ToString()}' does not work.");
		}

		[TestMethod()]
		public void ParseTest4()
		{
			var d = 100.000m;
			var tst = BigDecimal.Parse(d.ToString());

			Assert.AreEqual(d.ToString(),tst.ToString(), $"Parsing '{d.ToString()}' does not work.");
		}

		[TestMethod()]
		public void GetWholePartTest()
		{
			var val = new BigDecimal(10.01m);
			var res = new BigDecimal(10m);

			Assert.AreEqual(res, val.GetWholePart(), $"GetWholePart() does not work with value '{val}'.");
		}

		[TestMethod()]
		public void GetDecimalsTest()
		{
			var val = new BigDecimal(10.01m);
			var res = new BigDecimal(0.01m);

			Assert.AreEqual(res, val.GetDecimals(), $"GetDecimals() does not work with value '{val}'.");
		}

		[TestMethod()]
		public void EqualsTest()
		{
			object v1 = new BigDecimal(100.001);
			object v2 = new BigDecimal(100.001);

			Assert.IsTrue(BigDecimal.Equals(v1, v2));
		}

		[TestMethod()]
		public void EqualsTest2()
		{
			object v1 = new BigDecimal(100.001);
			object v2 = new BigDecimal(100.002);

			Assert.IsFalse(BigDecimal.Equals(v1, v2));
		}

		[TestMethod()]
		public void EqualsTest3()
		{
			object v1 = new BigDecimal(100.001m);
			object v2 = new BigDecimal(1000.01m);

			Assert.IsFalse(BigDecimal.Equals(v1, v2));
		}

		[TestMethod()]
		public void CompareToTest()
		{
			var v1 = new BigDecimal(100.001m);
			var v2 = new BigDecimal(100.001m);

			Assert.IsTrue(v1.CompareTo(v2) == 0);
		}

		[TestMethod()]
		public void CompareToTest2()
		{
			var v1 = new BigDecimal(100.001m);
			var v2 = new BigDecimal(100.002m);

			Assert.IsTrue(v1.CompareTo(v2) < 0);
		}

		[TestMethod()]
		public void ToStringTest()
		{
			var d = 10.1111m;
			var bd = new BigDecimal(d);

			Assert.AreEqual(d.ToString(), bd.ToString());
		}

		[TestMethod()]
		public void ToStringTest2()
		{
			var d = -1234567890.123456789m;
			var bd = new BigDecimal(d);

			Assert.AreEqual(d.ToString(), bd.ToString());
		}

		[TestMethod()]
		public void ToStringTest3()
		{
			var d = -0.000001m;
			var bd = new BigDecimal(d);

			Assert.AreEqual(d.ToString(), bd.ToString());
		}

		[TestMethod()]
		public void Equals2Test()
		{
			var v1 = new BigDecimal(100.001);
			var v2 = new BigDecimal(100.001);

			Assert.IsTrue(v1.Equals(v2));
		}

		[TestMethod()]
		public void Equals2Test2()
		{
			var v1 = new BigDecimal(100.001);
			var v2 = new BigDecimal(100.002);

			Assert.IsFalse(v1.Equals(v2));
		}

		[TestMethod()]
		public void BigDecimalDoubleTest()
		{
			var d = 1.0;
			var tst = new BigDecimal(d);

			Assert.AreEqual(d.ToString(),  tst.ToString(), $"BigDecimal(double) '{d.ToString()}' does not work.");
		}

		[TestMethod()]
		public void BigDecimalDoubleTest2()
		{
			var d = -100.125;
			var tst = new BigDecimal(d);

			Assert.AreEqual(d.ToString(), tst.ToString(), $"BigDecimal(double) '{d.ToString()}' does not work.");
		}

		[TestMethod()]
		public void BigDecimalDoubleTest3()
		{
			var d = 1.25;
			var tst = new BigDecimal(d);

			Assert.AreEqual(d.ToString(), tst.ToString(), $"BigDecimal(double) '{d.ToString()}' does not work.");
		}

		[TestMethod()]
		public void BigDecimalDecimalTest()
		{
			var d = 1.0m;
			var tst = new BigDecimal(d);

			Assert.AreEqual(d.ToString(), tst.ToString(), $"BigDecimal(decimal) '{d.ToString()}' does not work.");
		}

		[TestMethod()]
		public void BigDecimalDecimalTest2()
		{
			var d = 1.1m;
			var tst = new BigDecimal(d);

			Assert.AreEqual(d.ToString(), tst.ToString(), $"BigDecimal(decimal) '{d.ToString()}' does not work.");
		}

		[TestMethod()]
		public void BigDecimalDecimalTest3()
		{
			var d = -100.01m;
			var tst = new BigDecimal(d);

			Assert.AreEqual(d.ToString(), tst.ToString(), $"BigDecimal(decimal) '{d.ToString()}' does not work.");
		}

		[TestMethod()]
		public void BigDecimalDecimalTest4()
		{
			string val = decimal.MaxValue.ToString();
			var tst = new BigDecimal(decimal.Parse(val));

			Assert.AreEqual(val, tst.ToString(), $"BigDecimal(decimal) '{val}' does not work.");
		}

		[TestMethod()]
		public void BigDecimalDecimalTest5()
		{
			string val = decimal.MinValue.ToString();
			var tst = new BigDecimal(decimal.Parse(val));

			Assert.AreEqual(val, tst.ToString(), $"BigDecimal(decimal) '{val}' does not work.");
		}

		[TestMethod()]
		public void AbsTest()
		{
			Assert.AreEqual(BigDecimal.One, BigDecimal.Abs(BigDecimal.MinusOne));
		}

		[TestMethod()]
		public void AbsTest2()
		{
			var v1 = new BigDecimal(-1.01m);
			var v2 = new BigDecimal(1.01m);

			Assert.AreEqual(v2, BigDecimal.Abs(v1));
		}

		public void AbsTest3()
		{
			var v1 = new BigDecimal(1.01m);
			var v2 = new BigDecimal(1.01m);

			Assert.AreEqual(v2, BigDecimal.Abs(v1));
		}

		[TestMethod()]
		public void NegateTest()
		{
			var v1= new BigDecimal(1.01m);
			var v2 = new BigDecimal(-1.01m);

			Assert.AreEqual(v2, BigDecimal.Negate(v1));
		}

		[TestMethod()]
		public void AddTest()
		{
			var res = BigDecimal.Add(BigDecimal.One, BigDecimal.One);

			Assert.AreEqual(new BigDecimal(2), res);
		}

		[TestMethod()]
		public void AddTest2()
		{
			var v1 = new BigDecimal(0.01m);
			var v2 = new BigDecimal(0.00001m);

			var res = BigDecimal.Add(v1, v2);

			Assert.AreEqual(new BigDecimal(0.01001m), res);
		}

		[TestMethod()]
		public void AddTest3()
		{
			var v1 = BigDecimal.MinusOne;
			var v2 = BigDecimal.MinusOne;

			var res = BigDecimal.Add(v1, v2);

			Assert.AreEqual(new BigDecimal(-2), res);
		}

		[TestMethod()]
		public void AddTest4()
		{
			var v1 = new BigDecimal((BigInteger)ulong.MaxValue);
			var v2 = BigDecimal.One;

			var res = BigDecimal.Add(v1, v2);

			var bigInt = new BigInteger(ulong.MaxValue) + 1;

			Assert.AreEqual(new BigDecimal(bigInt), res);
		}

		[TestMethod()]
		public void SubtractTest()
		{
			var res = BigDecimal.Subtract(BigDecimal.One, new BigDecimal(2));

			Assert.AreEqual(new BigDecimal(-1), res);
		}

		[TestMethod()]
		public void SubtractTest2()
		{
			var v1 = new BigDecimal(0.01m);
			var v2 = new BigDecimal(0.00001m);

			var res = BigDecimal.Subtract(v1, v2);

			Assert.AreEqual(new BigDecimal(0.00999m), res);
		}

		[TestMethod()]
		public void SubtractTest3()
		{
			var v1 = BigDecimal.MinusOne;
			var v2 = BigDecimal.One;

			var res = BigDecimal.Subtract(v1, v2);

			Assert.AreEqual(new BigDecimal(-2), res);
		}

		[TestMethod()]
		public void MultiplyTest()
		{
			var res = BigDecimal.Multiply( new BigDecimal(2), new BigDecimal(4));

			Assert.AreEqual(new BigDecimal(8), res);
		}

		[TestMethod()]
		public void DivideTest()
		{
			var res = BigDecimal.Divide(new BigDecimal(10), new BigDecimal(2));

			Assert.AreEqual(new BigDecimal(5), res);
		}

		[TestMethod()]
		public void DivideTest2()
		{
			var res = BigDecimal.Divide(new BigDecimal(10), new BigDecimal(8));

			Assert.AreEqual(new BigDecimal(1.25m), res);
		}

		[TestMethod()]
		public void DivideTest3()
		{
			var res = BigDecimal.Divide(new BigDecimal(101.0008m), new BigDecimal(0.05m));

			Assert.AreEqual(new BigDecimal(2020.016m), res);
		}

		[TestMethod()]
		public void PowTest()
		{
			var res = BigDecimal.Pow(2, 5);
			Assert.AreEqual(new BigDecimal(32), res, "BigDecimal.Pow() does not work.");
		}

		[TestMethod()]
		public void PowTest2()
		{
			var res = BigDecimal.Pow(2.5, 5);
			Assert.AreEqual(new BigDecimal(97.65625m), res, "BigDecimal.Pow() does not work.");
		}

		[TestMethod()]
		public void CompareTest()
		{
			var v1 = new BigDecimal(1);
			var v2 = new BigDecimal(1);

			Assert.IsTrue(BigDecimal.Compare(v1, v2) == 0);
		}

		[TestMethod()]
		public void CompareTest2()
		{
			var v1 = new BigDecimal(1);
			var v2 = new BigDecimal(2);

			Assert.IsTrue(BigDecimal.Compare(v1, v2) < 0);
		}

		[TestMethod()]
		public void CompareTest3()
		{
			var v1 = new BigDecimal(1.01m);
			var v2 = new BigDecimal(1.01m);

			Assert.IsTrue(BigDecimal.Compare(v1, v2) == 0);
		}

		[TestMethod()]
		public void CompareTest4()
		{
			var v1 = new BigDecimal(1.001m);
			var v2 = new BigDecimal(1.01m);

			Assert.IsTrue(BigDecimal.Compare(v1, v2) < 0);
		}

		[TestMethod()]
		public void CompareTest5()
		{
			var v1 = new BigDecimal(1.01m);
			var v2 = new BigDecimal(-1.01m);

			Assert.IsTrue(BigDecimal.Compare(v1, v2) == 1);
		}

		[TestMethod()]
		public void CompareTest6()
		{
			var v1 = new BigDecimal(-1.001m);
			var v2 = new BigDecimal(-1.01m);

			Assert.IsTrue(BigDecimal.Compare(v1, v2) > 0);
		}

		[TestMethod()]
		public void OperatorDouble()
		{
			var v = new BigDecimal(1.0);
			var d = (double)v;

			Assert.IsTrue(d == 1.0);
		}

		[TestMethod()]
		public void OperatorDouble2()
		{
			var v = new BigDecimal(1.25);
			var d = (double)v;

			Assert.IsTrue(d == 1.25);
		}

		[TestMethod()]
		public void OperatorDouble3()
		{
			var v = (BigDecimal.Pow(10, 350) + 1) / BigDecimal.Pow(10, 50);

			var d = (double)v;
			var ok = (double)Math.Pow(10, 300);

			Assert.AreEqual(ok, d);
		}

		[TestMethod()]
		public void OperatorDouble4()
		{
			var v = BigDecimal.Pow(10, -300);

			var d = (double)v;
			var ok = (double)Math.Pow(10, -300);

			Assert.AreEqual(ok, d);
		}

		[TestMethod()]
		public void OperatorDouble5()
		{
			var v = (BigDecimal.Pow(10, 600) + 1) / BigDecimal.Pow(10, 300);

			var d = (double)v;
			var ok = (double)Math.Pow(10, 300);

			Assert.AreEqual(ok, d);
		}

		[TestMethod()]
		public void OperatorDecimal()
		{
			var v = new BigDecimal(1.0);
			var d = (decimal)v;

			Assert.IsTrue(d == 1.0m);
		}

		[TestMethod()]
		public void OperatorDecimal2()
		{
			var v = new BigDecimal(1.25);
			var d = (decimal)v;

			Assert.IsTrue(d == 1.25m);
		}

		[TestMethod()]
		public void OperatorDecimal3()
		{
			var v = (BigDecimal.Pow(10, 30) + 1) / BigDecimal.Pow(10, 5);

			var d = (decimal)v;
			var ok = (decimal)Math.Pow(10, 25);

			Assert.AreEqual(ok, d);
		}

		[TestMethod()]
		public void OperatorDecimal4()
		{
			var v = BigDecimal.Pow(10, -25);

			var d = (decimal)v;
			var ok = (decimal)Math.Pow(10, -25);

			Assert.AreEqual(ok, d);
		}

		[TestMethod()]
		public void OperatorDecimal5()
		{
			var v = (BigDecimal.Pow(10, 50) + 1) / BigDecimal.Pow(10, 25);

			var d = (decimal)v;
			var ok = (decimal)Math.Pow(10, 25);

			Assert.AreEqual(ok, d);
		}

	}
}