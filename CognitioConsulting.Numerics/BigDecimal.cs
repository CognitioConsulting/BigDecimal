using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace CognitioConsulting.Numerics
{
	[Serializable]
	public class BigDecimal : IComparable, IComparable<BigDecimal>, IEquatable<BigDecimal>, ISerializable
	{
		/// <summary>
		/// The decimal separator to use in current culture.
		/// </summary>
		private static string DecimalSeparator => CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

		//
		// The value fields 
		//
		private readonly BigInteger _bigIntValue;
		private readonly int _decimalCount;

		//
		// Static BigDecimal values
		//
		private static readonly BigDecimal _Zero = new BigDecimal(BigInteger.Zero);
		private static readonly BigDecimal _One = new BigDecimal(BigInteger.One);
		private static readonly BigDecimal _MinusOne = new BigDecimal(BigInteger.MinusOne);

		// 
		// Values for converting to/from double/decimal.
		//
		#region Values for converting to/from double/decimal.
		[StructLayout(LayoutKind.Explicit)]
		internal struct DoubleUlong
		{
			[FieldOffset(0)]
			public double dbl;
			[FieldOffset(0)]
			public ulong uu;
		}
		private const int DoubleMaxScale = 308;
		private static readonly BigInteger s_bnDoublePrecision = BigInteger.Pow(10, DoubleMaxScale);
		private static readonly BigInteger s_bnDoubleMaxValue = (BigInteger)Double.MaxValue;
		private static readonly BigInteger s_bnDoubleMinValue = (BigInteger)Double.MinValue;

		[StructLayout(LayoutKind.Explicit)]
		internal struct DecimalUInt32
		{
			[FieldOffset(0)]
			public Decimal dec;
			[FieldOffset(0)]
			public int flags;
		}
		private const int DecimalScaleMask = 0x00FF0000;
		private const int DecimalSignMask = unchecked((int)0x80000000);
		private const int DecimalMaxScale = 28;
		private static readonly BigInteger s_bnDecimalPrecision = BigInteger.Pow(10, DecimalMaxScale);
		private static readonly BigInteger s_bnDecimalMaxValue = (BigInteger)Decimal.MaxValue;
		private static readonly BigInteger s_bnDecimalMinValue = (BigInteger)Decimal.MinValue;

		#endregion Values for converting to/from double/decimal.


		// 
		// Public properties
		// 
		#region Public properties
		public static BigDecimal Zero
		{
			get
			{
				return _Zero;
			}
		}

		public static BigDecimal One
		{
			get
			{
				return _One;
			}
		}

		public static BigDecimal MinusOne
		{
			get
			{
				return _MinusOne;
			}
		}

		public Int32 Sign
		{
			get
			{
				return _bigIntValue.Sign;
			}
		}

		#endregion Public properties

		// 
		// Public Methods
		//
		#region Public Methods

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public BigInteger GetWholePart()
		{
			return BigInteger.Divide(_bigIntValue, BigInteger.Pow(10, _decimalCount));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public BigDecimal GetDecimals()
		{
			return this - GetWholePart();
		}

		public override bool Equals(Object obj)
		{
			if (obj == null)
				return false;

			if (!(obj is BigDecimal))
				return false;

			return this.Equals((BigDecimal)obj);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return _bigIntValue.GetHashCode() ^ _decimalCount.GetHashCode();
		}

		/// <summary>
		/// Implementation of IComparable.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		int IComparable.CompareTo(Object obj)
		{
			if (obj == null)
				return 1;

			if (!(obj is BigDecimal))
				throw new ArgumentException("Argument must be of type BigDecimal", "obj");

			return Compare(this, (BigDecimal)obj);
		}

		/// <summary>
		/// Implementation of IComparable<BigDecimal>
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo(BigDecimal other)
		{
			return Compare(this, other);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override String ToString()
		{
			var s = BigInteger.Abs(_bigIntValue).ToString("R");

			if (_decimalCount > 0)
			{
				s = s.PadLeft(_decimalCount + 1, '0');
				s = s.Insert(s.Length - _decimalCount, DecimalSeparator);
			}

			if (_bigIntValue.Sign < 0)
				s = "-" + s;

			return s;
		}

		/// <summary>
		/// Implementation of IEquatable<BigDecimal>
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public Boolean Equals(BigDecimal other)
		{
			if (Object.ReferenceEquals(other, null))
				return false;

			return this._bigIntValue == other._bigIntValue
				&& this._decimalCount == other._decimalCount;
		}

		#endregion Public Methods

		//
		// Constructors
		//
		#region Constructors

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public BigDecimal(BigInteger value)
			: this(value, 0)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="decimals"></param>
		private BigDecimal(BigInteger value, int decimals)
		{
			_bigIntValue = value;
			_decimalCount = decimals;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public BigDecimal(long value)
			: this((BigInteger)value, 0)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public BigDecimal(double value)
		{
			if (Double.IsNaN(value))
			{
				throw new ArgumentException("Argument is not a number", "value");
			}
			else if (Double.IsInfinity(value))
			{
				throw new ArgumentException("Argument is infinity", "value");
			}

			bool isFinite;
			int sign;
			int exponent;
			ulong significand;
			SplitDoubleIntoParts(value, out sign, out exponent, out significand, out isFinite);

			if (significand == 0)
			{
				// this = BigRational.Zero;
				return;
			}

			BigInteger numerator = significand;
			BigInteger denominator = BigInteger.One;

			if (exponent > 0)
			{
				numerator <<= exponent;
			}
			else if (exponent < 0)
			{
				denominator <<= -exponent;
			}

			if (sign < 0)
			{
				numerator = BigInteger.Negate(numerator);
			}

			var newBigDecimal = (BigDecimal)numerator / (BigDecimal)denominator;
			_bigIntValue = newBigDecimal._bigIntValue;
			_decimalCount = newBigDecimal._decimalCount;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public BigDecimal(decimal value)
		{
			int[] bits = decimal.GetBits(value);
			if (bits == null || bits.Length != 4 || (bits[3] & ~(DecimalSignMask | DecimalScaleMask)) != 0 || (bits[3] & DecimalScaleMask) > (28 << 16))
			{
				throw new ArgumentException("invalid Decimal", "value");
			}

			if (value == decimal.Zero)
			{
				return;
			}

			//
			// Build up the numerator
			//
			ulong ul = (((ulong)(uint)bits[2]) << 32) | ((ulong)(uint)bits[1]);   // (hi    << 32) | (mid)
			_bigIntValue = (new BigInteger(ul) << 32) | (uint)bits[0];             // (hiMid << 32) | (low)

			bool isNegative = (bits[3] & DecimalSignMask) != 0;
			if (isNegative)
			{
				_bigIntValue = BigInteger.Negate(_bigIntValue);
			}

			// 
			// Get the decimals
			// 
			_decimalCount = (bits[3] & DecimalScaleMask) >> 16;     // 0-28, power of 10 to divide numerator by
		}

		#endregion Constructors

		//
		// Public static methods
		//
		#region Public static methods

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static BigDecimal Parse(string value)
		{
			var bigInt = BigInteger.Parse(value.Replace(BigDecimal.DecimalSeparator, ""));
			var decimals = 0;

			if (value.Contains(BigDecimal.DecimalSeparator))
				decimals = value.Length - value.IndexOf(BigDecimal.DecimalSeparator) - 1;

			return new BigDecimal(bigInt, decimals);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="bd"></param>
		/// <returns></returns>
		public static BigDecimal Abs(BigDecimal bd)
		{
			return (bd._bigIntValue.Sign < 0 ? -bd : bd);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="bd"></param>
		/// <returns></returns>
		public static BigDecimal Negate(BigDecimal bd)
		{
			return new BigDecimal(BigInteger.Negate(bd._bigIntValue), bd._decimalCount);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public static BigDecimal Add(BigDecimal x, BigDecimal y)
		{
			return x + y;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public static BigDecimal Subtract(BigDecimal x, BigDecimal y)
		{
			return x - y;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public static BigDecimal Multiply(BigDecimal x, BigDecimal y)
		{
			return x * y;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dividend"></param>
		/// <param name="divisor"></param>
		/// <returns></returns>
		public static BigDecimal Divide(BigDecimal dividend, BigDecimal divisor)
		{
			return dividend / divisor;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="baseValue"></param>
		/// <param name="exponent"></param>
		/// <returns></returns>
		public static BigDecimal Pow(BigDecimal baseValue, BigInteger exponent)
		{
			if (exponent.Sign == 0)
			{
				//
				// Anyting to the power of zero is one.
				//
				return BigDecimal.One;
			}
			else if (exponent.Sign < 0)
			{
				if (baseValue == BigDecimal.Zero)
				{
					throw new ArgumentException("cannot raise zero to a negative power", "baseValue");
				}

				//
				// n^(-e) -> (1/n)^e
				//
				baseValue = BigDecimal.One / baseValue;
				exponent = BigInteger.Negate(exponent);
			}

			BigDecimal result = baseValue;
			while (exponent > BigInteger.One)
			{
				result = result * baseValue;
				exponent--;
			}

			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="r1"></param>
		/// <param name="r2"></param>
		/// <returns></returns>
		public static int Compare(BigDecimal r1, BigDecimal r2)
		{
			var res = r1 - r2;

			return res._bigIntValue.Sign;
		}
		#endregion Public static methods

		// 
		// Operator overloads
		//
		#region Operator overloads
		public static bool operator ==(BigDecimal x, BigDecimal y)
		{
			return x.Equals(y);
		}

		public static bool operator !=(BigDecimal x, BigDecimal y)
		{
			return !x.Equals(y);
		}

		public static bool operator <(BigDecimal x, BigDecimal y)
		{
			return Compare(x, y) < 0;
		}

		public static bool operator <=(BigDecimal x, BigDecimal y)
		{
			return Compare(x, y) <= 0;
		}

		public static bool operator >(BigDecimal x, BigDecimal y)
		{
			return Compare(x, y) > 0;
		}

		public static bool operator >=(BigDecimal x, BigDecimal y)
		{
			return Compare(x, y) >= 0;
		}

		public static BigDecimal operator +(BigDecimal bd)
		{
			return bd;
		}

		public static BigDecimal operator -(BigDecimal bd)
		{
			return new BigDecimal(-bd._bigIntValue, bd._decimalCount);
		}

		public static BigDecimal operator ++(BigDecimal bd)
		{
			return bd + BigDecimal.One;
		}

		public static BigDecimal operator --(BigDecimal bd)
		{
			return bd - BigDecimal.One;
		}

		public static BigDecimal operator +(BigDecimal bd1, BigDecimal bd2)
		{
			//
			// Ensure both values have the same number of decimals.
			//
			var v1 = bd1.IncreaseDecimals(bd2._decimalCount);
			var v2 = bd2.IncreaseDecimals(bd1._decimalCount);

			return new BigDecimal(v1._bigIntValue + v2._bigIntValue, v1._decimalCount);
		}

		public static BigDecimal operator -(BigDecimal bd1, BigDecimal bd2)
		{
			//
			// Ensure both values have the same number of decimals.
			//
			var v1 = bd1.IncreaseDecimals(bd2._decimalCount);
			var v2 = bd2.IncreaseDecimals(bd1._decimalCount);

			return new BigDecimal(v1._bigIntValue - v2._bigIntValue, v1._decimalCount);
		}

		public static BigDecimal operator *(BigDecimal bd1, BigDecimal bd2)
		{
			return new BigDecimal(bd1._bigIntValue * bd2._bigIntValue, bd1._decimalCount + bd2._decimalCount);
		}

		public static BigDecimal operator /(BigDecimal bd1, BigDecimal bd2)
		{
			var v1 = bd1.TrimDecimals();
			var v2 = bd2.TrimDecimals();

			//
			// Increase the values evenly until there are no decimals.
			//
			while (v1._decimalCount > 0 || v2._decimalCount > 0)
			{
				v1 = v1.MultiplyByTen();
				v2 = v2.MultiplyByTen();
			}

			//
			// Try to find the last decimal (will try up to 100 of them).
			// 
			int factor = 0;
			var v1Value = v1._bigIntValue;
			while (factor < 100 && v1Value % v2._bigIntValue != 0)
			{
				v1Value *= 10;
				factor++;
			}

			return new BigDecimal(v1Value / v2._bigIntValue, factor);
		}

		public static BigDecimal operator %(BigDecimal bd1, BigDecimal bd2)
		{
			var v1 = bd1.TrimDecimals();
			var v2 = bd2.TrimDecimals();

			while (v1._decimalCount > 0 || v2._decimalCount > 0)
			{
				v1 = v1.MultiplyByTen();
				v2 = v2.MultiplyByTen();
			}

			return new BigDecimal(v1._bigIntValue % v2._bigIntValue);
		}
		#endregion Operator overloads

		// 
		// Explicit conversions from BigDecimal 
		// 
		#region Explicit conversions from BigDecimal

		public static explicit operator SByte(BigDecimal value)
		{
			return (SByte)BigInteger.Divide(value._bigIntValue, (BigInteger)Math.Pow(10, value._decimalCount - 1));
		}

		public static explicit operator UInt16(BigDecimal value)
		{
			return (UInt16)BigInteger.Divide(value._bigIntValue, (BigInteger)Math.Pow(10, value._decimalCount - 1));
		}

		public static explicit operator UInt32(BigDecimal value)
		{
			return (UInt32)BigInteger.Divide(value._bigIntValue, (BigInteger)Math.Pow(10, value._decimalCount - 1));
		}

		public static explicit operator UInt64(BigDecimal value)
		{
			return (UInt64)BigInteger.Divide(value._bigIntValue, (BigInteger)Math.Pow(10, value._decimalCount - 1));
		}

		public static explicit operator Byte(BigDecimal value)
		{
			return (Byte)BigInteger.Divide(value._bigIntValue, (BigInteger)Math.Pow(10, value._decimalCount - 1));
		}

		public static explicit operator Int16(BigDecimal value)
		{
			return (Int16)BigInteger.Divide(value._bigIntValue, (BigInteger)Math.Pow(10, value._decimalCount - 1));
		}

		public static explicit operator Int32(BigDecimal value)
		{
			return (Int32)BigInteger.Divide(value._bigIntValue, (BigInteger)Math.Pow(10, value._decimalCount - 1));
		}

		public static explicit operator Int64(BigDecimal value)
		{
			return (Int64)BigInteger.Divide(value._bigIntValue, (BigInteger)Math.Pow(10, value._decimalCount - 1));
		}

		public static explicit operator BigInteger(BigDecimal value)
		{
			return BigInteger.Divide(value._bigIntValue, (BigInteger)Math.Pow(10, value._decimalCount));
		}

		public static explicit operator float(BigDecimal value)
		{
			//
			// The float value type represents a single-precision 32-bit number with
			// values ranging from negative 3.402823e38 to positive 3.402823e38      
			// values that do not fit into this range are returned as Infinity
			//
			return (float)((double)value);
		}

		public static explicit operator double(BigDecimal value)
		{
			//
			// The Double value type represents a double-precision 64-bit number with
			// values ranging from -1.79769313486232e308 to +1.79769313486232e308
			// values that do not fit into this range are returned as +/-Infinity
			var factor = BigInteger.Pow(10, value._decimalCount);
			if (SafeCastToDouble(value._bigIntValue) && SafeCastToDouble(factor))
			{
				return (double)value._bigIntValue / (double)factor;
			}

			//
			// Scale the numerator to preseve the fraction part through the integer division
			//
			BigInteger denormalized = (value._bigIntValue * s_bnDoublePrecision) / factor;
			if (denormalized.IsZero)
				return (value.Sign < 0) ? BitConverter.Int64BitsToDouble(unchecked((long)0x8000000000000000)) : 0d; // underflow to -+0

			double result = 0;
			bool isDouble = false;
			int scale = DoubleMaxScale;

			while (scale > 0)
			{
				if (!isDouble)
				{
					if (SafeCastToDouble(denormalized))
					{
						result = (double)denormalized;
						isDouble = true;
					}
					else
					{
						denormalized = denormalized / 10;
					}
				}
				result = result / 10;
				scale--;
			}

			if (!isDouble)
				return (value.Sign < 0) ? Double.NegativeInfinity : Double.PositiveInfinity;
			else
				return result;
		}

		public static explicit operator decimal(BigDecimal value)
		{
			// 
			// The decimal value type represents decimal numbers ranging
			// from +79,228,162,514,264,337,593,543,950,335 to -79,228,162,514,264,337,593,543,950,335
			// the binary representation of a Decimal value is of the form, ((-2^96 to 2^96) / 10^(0 to 28))
			// 
			var factor = BigInteger.Pow(10, value._decimalCount);
			if (SafeCastToDecimal(value._bigIntValue) && SafeCastToDecimal(factor))
			{
				return (Decimal)value._bigIntValue / (Decimal)factor;
			}

			// 
			// Scale the numerator to preseve the fraction part through the integer division
			// 
			BigInteger denormalized = (value._bigIntValue * s_bnDecimalPrecision) / factor;
			if (denormalized.IsZero)
			{
				// 
				// Underflow - fraction is too small to fit in a decimal
				//
				return Decimal.Zero; 
			}
			for (int scale = DecimalMaxScale; scale >= 0; scale--)
			{
				if (!SafeCastToDecimal(denormalized))
				{
					denormalized = denormalized / 10;
				}
				else
				{
					DecimalUInt32 dec = new DecimalUInt32();
					dec.dec = (Decimal)denormalized;
					dec.flags = (dec.flags & ~DecimalScaleMask) | (scale << 16);
					return dec.dec;
				}
			}

			throw new OverflowException("Value was either too large or too small for a Decimal.");
		}

		#endregion Explicit conversions from BigDecimal

		// 
		// Implicit conversions to BigDecimal 
		//
		#region Implicit conversions to BigDecimal

		public static implicit operator BigDecimal(sbyte value)
		{
			return new BigDecimal(value);
		}

		public static implicit operator BigDecimal(ushort value)
		{
			return new BigDecimal(value);
		}

		public static implicit operator BigDecimal(uint value)
		{
			return new BigDecimal(value);
		}

		public static implicit operator BigDecimal(ulong value)
		{
			return new BigDecimal((BigInteger)value);
		}

		public static implicit operator BigDecimal(byte value)
		{
			return new BigDecimal(value);
		}

		public static implicit operator BigDecimal(short value)
		{
			return new BigDecimal(value);
		}

		public static implicit operator BigDecimal(int value)
		{
			return new BigDecimal(value);
		}

		public static implicit operator BigDecimal(long value)
		{
			return new BigDecimal(value);
		}

		public static implicit operator BigDecimal(BigInteger value)
		{
			return new BigDecimal(value);
		}

		public static implicit operator BigDecimal(float value)
		{
			return new BigDecimal((double)value);
		}

		public static implicit operator BigDecimal(double value)
		{
			return new BigDecimal(value);
		}

		public static implicit operator BigDecimal(decimal value)
		{
			return new BigDecimal(value);
		}

		#endregion Implicit conversions to BigDecimal

		// 
		// 
		// 
		#region Serialization methods 
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}

			info.AddValue("BigIntValue", _bigIntValue);
			info.AddValue("DecimalCount", _decimalCount);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		BigDecimal(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}

			_bigIntValue = (BigInteger)info.GetValue("BigIntValue", typeof(BigInteger));
			_decimalCount = (int)info.GetValue("DecimalCount", typeof(int));
		}
		#endregion Serialization methods 

		// 
		// Private helper methods
		// 
		#region Private helper methods

		/// <summary>
		/// 
		/// </summary>
		/// <param name="decimals"></param>
		/// <returns></returns>
		private BigDecimal IncreaseDecimals(int decimals)
		{
			if (_decimalCount >= decimals)
				return this;

			var value = _bigIntValue * BigInteger.Pow(10, decimals - _decimalCount);

			return new BigDecimal(value, decimals);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private BigDecimal MultiplyByTen()
		{
			if (_decimalCount > 0)
				return new BigDecimal(_bigIntValue, _decimalCount - 1);
			else
				return new BigDecimal(_bigIntValue * 10, 0);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private BigDecimal TrimDecimals()
		{
			var value = _bigIntValue;
			int scale = _decimalCount;

			while (scale > 0 && value % 10 == 0)
			{
				value /= 10;
				scale--;
			}

			return new BigDecimal(value, scale);
		}

		#endregion Private helper methods

		// 
		// Private helper methods for converting to/from double/decimal
		//
		#region Private helper methods for converting to/from double/decimal

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private static bool SafeCastToDouble(BigInteger value)
		{
			return s_bnDoubleMinValue <= value && value <= s_bnDoubleMaxValue;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private static bool SafeCastToDecimal(BigInteger value)
		{
			return s_bnDecimalMinValue <= value && value <= s_bnDecimalMaxValue;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dbl"></param>
		/// <param name="sign"></param>
		/// <param name="exp"></param>
		/// <param name="man"></param>
		/// <param name="isFinite"></param>
		private static void SplitDoubleIntoParts(double dbl, out int sign, out int exp, out ulong man, out bool isFinite)
		{
			DoubleUlong du;
			du.uu = 0;
			du.dbl = dbl;

			sign = 1 - ((int)(du.uu >> 62) & 2);
			man = du.uu & 0x000FFFFFFFFFFFFF;
			exp = (int)(du.uu >> 52) & 0x7FF;
			if (exp == 0)
			{
				// Denormalized number.
				isFinite = true;
				if (man != 0)
					exp = -1074;
			}
			else if (exp == 0x7FF)
			{
				// NaN or Infinite.
				isFinite = false;
				exp = Int32.MaxValue;
			}
			else {
				isFinite = true;
				man |= 0x0010000000000000; // mask in the implied leading 53rd significand bit
				exp -= 1075;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sign"></param>
		/// <param name="exp"></param>
		/// <param name="man"></param>
		/// <returns></returns>
		private static double GetDoubleFromParts(int sign, int exp, ulong man)
		{
			DoubleUlong du;
			du.dbl = 0;

			if (man == 0)
			{
				du.uu = 0;
			}
			else {
				// Normalize so that 0x0010 0000 0000 0000 is the highest bit set
				int cbitShift = CbitHighZero(man) - 11;
				if (cbitShift < 0)
					man >>= -cbitShift;
				else
					man <<= cbitShift;

				// Move the point to just behind the leading 1: 0x001.0 0000 0000 0000
				// (52 bits) and skew the exponent (by 0x3FF == 1023)
				exp += 1075;

				if (exp >= 0x7FF)
				{
					// Infinity
					du.uu = 0x7FF0000000000000;
				}
				else if (exp <= 0)
				{
					// Denormalized
					exp--;
					if (exp < -52)
					{
						// Underflow to zero
						du.uu = 0;
					}
					else {
						du.uu = man >> -exp;
					}
				}
				else {
					// Mask off the implicit high bit
					du.uu = (man & 0x000FFFFFFFFFFFFF) | ((ulong)exp << 52);
				}
			}

			if (sign < 0)
			{
				du.uu |= 0x8000000000000000;
			}

			return du.dbl;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="uu"></param>
		/// <returns></returns>
		private static int CbitHighZero(ulong uu)
		{
			if ((uu & 0xFFFFFFFF00000000) == 0)
				return 32 + CbitHighZero((uint)uu);
			return CbitHighZero((uint)(uu >> 32));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="u"></param>
		/// <returns></returns>
		private static int CbitHighZero(uint u)
		{
			if (u == 0)
				return 32;

			int cbit = 0;
			if ((u & 0xFFFF0000) == 0)
			{
				cbit += 16;
				u <<= 16;
			}
			if ((u & 0xFF000000) == 0)
			{
				cbit += 8;
				u <<= 8;
			}
			if ((u & 0xF0000000) == 0)
			{
				cbit += 4;
				u <<= 4;
			}
			if ((u & 0xC0000000) == 0)
			{
				cbit += 2;
				u <<= 2;
			}
			if ((u & 0x80000000) == 0)
				cbit += 1;
			return cbit;
		}

		#endregion Private helper methods for converting to/from double/decimal
	} 
}